using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Internal;
using InfluxDB.Client.Writes;
using RestSharp;

namespace InfluxDB.Client
{
    // TODO gzip
    public class WriteApi : IDisposable
    {
        private readonly Subject<List<BatchWriteData>> _flush = new Subject<List<BatchWriteData>>();
        private readonly MeasurementMapper _measurementMapper = new MeasurementMapper();
        private readonly Subject<BatchWriteData> _subject = new Subject<BatchWriteData>();

        private readonly WriteService _service;
        
        protected internal WriteApi(WriteService service, WriteOptions writeOptions) 
        {
            Arguments.CheckNotNull(service, nameof(service));
            Arguments.CheckNotNull(writeOptions, nameof(writeOptions));

            _service = service;

            // backpreasure - is not implemented in C#
            // 
            // => use unbound buffer
            // 
            // https://github.com/dotnet/reactive/issues/19

            var observable = _subject.ObserveOn(writeOptions.WriteScheduler);

            var boundary = observable
                .Buffer(TimeSpan.FromMilliseconds(writeOptions.FlushInterval), writeOptions.BatchSize,
                    writeOptions.WriteScheduler)
                .Merge(_flush);

            observable
                //
                // Batching
                //
                .Window(boundary)
                //
                // Group by key - same bucket, same org
                //
                .SelectMany(it => it.GroupBy(batchWrite => batchWrite.Options))
                //
                // Create Write Point = bucket, org, ... + data
                //
                .Select(grouped =>
                {
                    var aggregate = grouped
                        .Aggregate("", (lineProtocol, batchWrite) =>
                        {
                            var data = batchWrite.ToLineProtocol();

                            if (string.IsNullOrEmpty(data)) return lineProtocol;

                            if (string.IsNullOrEmpty(lineProtocol)) return data;

                            return string.Join("\n", lineProtocol, data);
                        });

                    return aggregate.Select(records => new BatchWriteRecord(grouped.Key, records));
                })
                //
                // Jitter
                //
                .Delay(source =>
                {
                    var jitterDelay = JitterDelay(writeOptions);
                    
                    return Observable.Timer(TimeSpan.FromMilliseconds(jitterDelay), Scheduler.CurrentThread);
                })
                .Concat()
                //
                // Map to Async request
                //
                .Where(batchWriteItem => !string.IsNullOrEmpty(batchWriteItem.ToLineProtocol()))
                .Select(batchWriteItem =>
                {
                    var orgId = batchWriteItem.Options.OrganizationId;
                    var bucket = batchWriteItem.Options.Bucket;
                    var lineProtocol = batchWriteItem.ToLineProtocol();
                    var precision = batchWriteItem.Options.Precision;

                    return Observable
                        .Create<IRestResponse>(observer =>
                        {
                            var body = Encoding.UTF8.GetBytes(lineProtocol);
                            var response = _service.PostWriteWithIRestResponse(orgId, bucket, body, null, 
                                "utf-8", "text/plain", null, "application/json", precision);
                            observer.OnNext(response);
                            
                            return Disposable.Empty;
                        })
                        .RetryWhen(f => f.SelectMany(e =>
                        {
                            if (e is HttpException httpException)
                            {
                                //
                                // This types is not able to retry
                                //
                                if (httpException.Status == 400 || httpException.Status == 401 ||
                                    httpException.Status == 403 || httpException.Status == 413)
                                {
                                    throw httpException;
                                }

                                var retryInterval = (httpException.RetryAfter * 1000 ?? writeOptions.RetryInterval) +
                                                    JitterDelay(writeOptions);

                                var retriable = new WriteRetryableErrorEvent(orgId, bucket, precision, lineProtocol,
                                    httpException, retryInterval);
                                Publish(retriable);

                                return Observable.Timer(TimeSpan.FromMilliseconds(retryInterval));
                            }

                            throw e;
                        }))
                        .Select(result =>
                        {
                            // ReSharper disable once ConvertIfStatementToReturnStatement
                            if (result.IsSuccessful)
                            {
                                return Notification.CreateOnNext(result);
                            }

                            return Notification.CreateOnError<IRestResponse>(HttpException.Create(result));
                        })
                        .Catch<Notification<IRestResponse>, Exception>(ex =>
                        {
                            var error = new WriteErrorEvent(orgId, bucket, precision, lineProtocol, ex);
                            Publish(error);
                            
                            return Observable.Return(Notification.CreateOnError<IRestResponse>(ex));
                        }).Do(res =>
                        {
                            if (res.Kind == NotificationKind.OnNext)
                            {
                                var success = new WriteSuccessEvent(orgId, bucket, precision, lineProtocol);
                                Publish(success);
                            }
                        });
                })
                .Concat()
                .Subscribe(
                    notification =>
                    {
                        switch (notification.Kind)
                        {
                            case NotificationKind.OnNext:
                                Trace.WriteLine($"The batch item: {notification} was processed successfully.");
                                break;
                            case NotificationKind.OnError:
                                Trace.WriteLine(
                                    $"The batch item wasn't processed successfully because: {notification.Exception}");
                                break;
                            default:
                                Trace.WriteLine($"The batch item: {notification} was processed");
                                break;
                        }
                    },
                    exception => Trace.WriteLine($"The unhandled exception occurs: {exception}"),
                    () => Trace.WriteLine("The WriteApi was disposed."));
        }

        public void Dispose()
        {
            Trace.WriteLine("Flushing batches before shutdown.");

            if (!_subject.IsDisposed)
            {
                _subject.OnCompleted();
            }

            if (!_flush.IsDisposed)
            {
                _flush.OnCompleted();
            }

            _subject.Dispose();
            _flush.Dispose();
        }

        public event EventHandler EventHandler;

        /// <summary>
        ///     Write Line Protocol record into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="record">
        ///     specifies the record in InfluxDB Line Protocol.
        ///     The <see cref="record" /> is considered as one batch unit.
        /// </param>
        public void WriteRecord(string bucket, string orgId, WritePrecision precision, string record)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(precision, nameof(precision));

            _subject.OnNext(new BatchWriteRecord(new BatchWriteOptions(bucket, orgId, precision), record));
        }

        /// <summary>
        ///     Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public void WriteRecords(string bucket, string orgId, WritePrecision precision, List<string> records)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(precision, nameof(precision));

            records.ForEach(record => WriteRecord(bucket, orgId, precision, record));
        }

        /// <summary>
        ///     Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public void WriteRecords(string bucket, string orgId, WritePrecision precision, params string[] records)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(precision, nameof(precision));

            foreach (var record in records) WriteRecord(bucket, orgId, precision, record);
        }

        /// <summary>
        ///     Write a Data point into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="point">specifies the Data point to write into bucket</param>
        public void WritePoint(string bucket, string orgId, Point point)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            if (point == null) return;

            _subject.OnNext(new BatchWritePoint(new BatchWriteOptions(bucket, orgId, point.Precision), point));
        }

        /// <summary>
        ///     Write Data points into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public void WritePoints(string bucket, string orgId, List<Point> points)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            foreach (var point in points) WritePoint(bucket, orgId, point);
        }

        /// <summary>
        ///     Write Data points into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public void WritePoints(string bucket, string orgId, params Point[] points)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            WritePoints(bucket, orgId, points.ToList());
        }

        /// <summary>
        ///     Write a Measurement into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurement">specifies the Measurement to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public void WriteMeasurement<TM>(string bucket, string orgId, WritePrecision precision, TM measurement)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(precision, nameof(precision));

            if (measurement == null) return;

            var options = new BatchWriteOptions(bucket, orgId, precision);

            _subject.OnNext(new BatchWriteMeasurement<TM>(options, measurement, _measurementMapper));
        }

        /// <summary>
        ///     Write Measurements into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public void WriteMeasurements<TM>(string bucket, string orgId, WritePrecision precision, List<TM> measurements)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(precision, nameof(precision));

            foreach (var measurement in measurements) WriteMeasurement(bucket, orgId, precision, measurement);
        }

        /// <summary>
        ///     Write Measurements into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public void WriteMeasurements<TM>(string bucket, string orgId, WritePrecision precision,
            params TM[] measurements)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(precision, nameof(precision));

            WriteMeasurements(bucket, orgId, precision, measurements.ToList());
        }

        /// <summary>
        ///     Forces the client to flush all pending writes from the buffer to the InfluxDB via HTTP.
        /// </summary>
        public void Flush()
        {
            _flush.OnNext(new List<BatchWriteData>());
        }

//        private IRestResponse DoRequest(Func<HttpRequestMessage> request)
//        {
//            retvar writePostAsyncWithHttpInfo = _service.WritePostWithIRestResponse(null, null, null);
//            return writePostAsyncWithHttpInfo;
//        }
        
        private int JitterDelay(WriteOptions writeOptions)
        {
            return (int) (new Random().NextDouble() * writeOptions.JitterInterval);
        }

        private void Publish(InfluxDBEventArgs eventArgs)
        {
            eventArgs.LogEvent();

            EventHandler?.Invoke(this, eventArgs);
        }
    }

    internal abstract class BatchWriteData
    {
        internal readonly BatchWriteOptions Options;

        protected BatchWriteData(BatchWriteOptions options)
        {
            Arguments.CheckNotNull(options, "options");

            Options = options;
        }

        internal abstract string ToLineProtocol();
    }

    internal class BatchWriteRecord : BatchWriteData
    {
        private readonly string _record;

        internal BatchWriteRecord(BatchWriteOptions options, string record) : base(options)
        {
            Arguments.CheckNotNull(record, nameof(record));

            _record = record;
        }

        internal override string ToLineProtocol()
        {
            return _record;
        }
    }

    internal class BatchWritePoint : BatchWriteData
    {
        private readonly Point _point;

        internal BatchWritePoint(BatchWriteOptions options, Point point) : base(options)
        {
            Arguments.CheckNotNull(point, nameof(point));

            _point = point;
        }

        internal override string ToLineProtocol()
        {
            return _point.ToLineProtocol();
        }
    }

    internal class BatchWriteMeasurement<TM> : BatchWriteData
    {
        private readonly TM _measurement;
        private readonly MeasurementMapper _measurementMapper;

        internal BatchWriteMeasurement(BatchWriteOptions options, TM measurement, MeasurementMapper measurementMapper) :
            base(options)
        {
            Arguments.CheckNotNull(measurement, nameof(measurement));

            _measurement = measurement;
            _measurementMapper = measurementMapper;
        }

        internal override string ToLineProtocol()
        {
            return _measurementMapper.ToPoint(_measurement, Options.Precision).ToLineProtocol();
        }
    }

    internal class BatchWriteOptions
    {
        internal readonly string Bucket;
        internal readonly string OrganizationId;
        internal readonly WritePrecision Precision;

        internal BatchWriteOptions(string bucket, string orgId, WritePrecision precision)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(precision, nameof(precision));

            Bucket = bucket;
            OrganizationId = orgId;
            Precision = precision;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((BatchWriteOptions) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Bucket != null ? Bucket.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ (OrganizationId != null ? OrganizationId.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Precision;
                return hashCode;
            }
        }

        private bool Equals(BatchWriteOptions other)
        {
            return string.Equals(Bucket, other.Bucket) && string.Equals(OrganizationId, other.OrganizationId) &&
                   Precision == other.Precision;
        }
    }
}