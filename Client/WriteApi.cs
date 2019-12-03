using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Reactive.Threading.Tasks;
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
    public class WriteApi : IDisposable
    {
        private readonly Subject<List<BatchWriteData>> _flush = new Subject<List<BatchWriteData>>();

        private readonly InfluxDBClient _influxDbClient;
        private readonly MeasurementMapper _measurementMapper = new MeasurementMapper();
        private readonly InfluxDBClientOptions _options;
        private readonly Subject<BatchWriteData> _subject = new Subject<BatchWriteData>();

        protected internal WriteApi(InfluxDBClientOptions options, WriteService service, WriteOptions writeOptions,
            InfluxDBClient influxDbClient)
        {
            Arguments.CheckNotNull(service, nameof(service));
            Arguments.CheckNotNull(writeOptions, nameof(writeOptions));
            Arguments.CheckNotNull(influxDbClient, nameof(_influxDbClient));

            _options = options;
            _influxDbClient = influxDbClient;

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
                        .Aggregate(new StringBuilder(""), (builder, batchWrite) =>
                        {
                            var data = batchWrite.ToLineProtocol();

                            if (string.IsNullOrEmpty(data)) return builder;

                            if (builder.Length > 0)
                            {
                                builder.Append("\n");
                            }

                            return builder.Append(data);
                        }).Select(builder => builder.ToString());
                    
                    return aggregate.Select(records => new BatchWriteRecord(grouped.Key, records));
                })
                //
                // Jitter
                //
                .Select(source =>
                {
                    if (writeOptions.JitterInterval <= 0)
                    {
                        return source;
                    }

                    return source.Delay(_ => Observable.Timer(TimeSpan.FromMilliseconds(JitterDelay(writeOptions)), Scheduler.CurrentThread));
                })
                .Concat()
                //
                // Map to Async request
                //
                .Where(batchWriteItem => !string.IsNullOrEmpty(batchWriteItem.ToLineProtocol()))
                .Select(batchWriteItem =>
                {
                    var org = batchWriteItem.Options.OrganizationId;
                    var bucket = batchWriteItem.Options.Bucket;
                    var lineProtocol = batchWriteItem.ToLineProtocol();
                    var precision = batchWriteItem.Options.Precision;

                    return Observable
                        .Defer(() =>
                            service.PostWriteAsyncWithIRestResponse(org, bucket,
                                    Encoding.UTF8.GetBytes(lineProtocol), null,
                                    "identity", "text/plain; charset=utf-8", null, "application/json", null, precision)
                                .ToObservable())
                        .RetryWhen(f => f.SelectMany(e =>
                        {
                            if (e is HttpException httpException)
                            {
                                //
                                // This types is not able to retry
                                //
                                if (httpException.Status != 429 && httpException.Status != 503)
                                    throw httpException;

                                var retryInterval = (httpException.RetryAfter * 1000 ?? writeOptions.RetryInterval) +
                                                    JitterDelay(writeOptions);

                                var retryable = new WriteRetriableErrorEvent(org, bucket, precision, lineProtocol,
                                    httpException, retryInterval);
                                Publish(retryable);

                                return Observable.Timer(TimeSpan.FromMilliseconds(retryInterval));
                            }

                            throw e;
                        }))
                        .Select(result =>
                        {
                            // ReSharper disable once ConvertIfStatementToReturnStatement
                            if (result.IsSuccessful) return Notification.CreateOnNext(result);

                            return Notification.CreateOnError<IRestResponse>(HttpException.Create(result));
                        })
                        .Catch<Notification<IRestResponse>, Exception>(ex =>
                        {
                            var error = new WriteErrorEvent(org, bucket, precision, lineProtocol, ex);
                            Publish(error);

                            return Observable.Return(Notification.CreateOnError<IRestResponse>(ex));
                        }).Do(res =>
                        {
                            if (res.Kind == NotificationKind.OnNext)
                            {
                                var success = new WriteSuccessEvent(org, bucket, precision, lineProtocol);
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
            _influxDbClient.Apis.Remove(this);

            Trace.WriteLine("Flushing batches before shutdown.");

            if (!_subject.IsDisposed) _subject.OnCompleted();

            if (!_flush.IsDisposed) _flush.OnCompleted();

            _subject.Dispose();
            _flush.Dispose();
        }

        public event EventHandler EventHandler;

        /// <summary>
        /// Write Line Protocol record into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="record">
        ///     specifies the record in InfluxDB Line Protocol.
        ///     The <see cref="record" /> is considered as one batch unit.
        /// </param>
        public void WriteRecord(WritePrecision precision, string record)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            WriteRecord(_options.Bucket, _options.Org, precision, record);
        }

        /// <summary>
        /// Write Line Protocol record into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="record">
        ///     specifies the record in InfluxDB Line Protocol.
        ///     The <see cref="record" /> is considered as one batch unit.
        /// </param>
        public void WriteRecord(string bucket, string org, WritePrecision precision, string record)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            _subject.OnNext(new BatchWriteRecord(new BatchWriteOptions(bucket, org, precision), record));
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public void WriteRecords(WritePrecision precision, List<string> records)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            WriteRecords(_options.Bucket, _options.Org, precision, records);
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public void WriteRecords(string bucket, string org, WritePrecision precision, List<string> records)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            records.ForEach(record => WriteRecord(bucket, org, precision, record));
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public void WriteRecords(WritePrecision precision, params string[] records)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            WriteRecords(_options.Bucket, _options.Org, precision, records);
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public void WriteRecords(string bucket, string org, WritePrecision precision, params string[] records)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            foreach (var record in records) WriteRecord(bucket, org, precision, record);
        }

        /// <summary>
        /// Write a Data point into specified bucket.
        /// </summary>
        /// <param name="point">specifies the Data point to write into bucket</param>
        public void WritePoint(PointData point)
        {
            WritePoint(_options.Bucket, _options.Org, point);
        }

        /// <summary>
        /// Write a Data point into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="point">specifies the Data point to write into bucket</param>
        public void WritePoint(string bucket, string org, PointData point)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));

            if (point == null) return;

            _subject.OnNext(new BatchWritePoint(new BatchWriteOptions(bucket, org, point.Precision), _options, point));
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public void WritePoints(List<PointData> points)
        {
            WritePoints(_options.Bucket, _options.Org, points);
        }


        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public void WritePoints(string bucket, string org, List<PointData> points)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));

            foreach (var point in points) WritePoint(bucket, org, point);
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public void WritePoints(params PointData[] points)
        {
            WritePoints(_options.Bucket, _options.Org, points);
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public void WritePoints(string bucket, string org, params PointData[] points)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));

            WritePoints(bucket, org, points.ToList());
        }

        /// <summary>
        /// Write a Measurement into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurement">specifies the Measurement to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public void WriteMeasurement<TM>(WritePrecision precision, TM measurement)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            WriteMeasurement(_options.Bucket, _options.Org, precision, measurement);
        }

        /// <summary>
        /// Write a Measurement into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurement">specifies the Measurement to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public void WriteMeasurement<TM>(string bucket, string org, WritePrecision precision, TM measurement)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            if (measurement == null) return;

            var options = new BatchWriteOptions(bucket, org, precision);

            _subject.OnNext(new BatchWriteMeasurement<TM>(options, _options, measurement, _measurementMapper));
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public void WriteMeasurements<TM>(WritePrecision precision, List<TM> measurements)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            WriteMeasurements(_options.Bucket, _options.Org, precision, measurements);
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public void WriteMeasurements<TM>(string bucket, string org, WritePrecision precision, List<TM> measurements)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            foreach (var measurement in measurements) WriteMeasurement(bucket, org, precision, measurement);
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public void WriteMeasurements<TM>(WritePrecision precision, params TM[] measurements)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            WriteMeasurements(_options.Bucket, _options.Org, precision, measurements);
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public void WriteMeasurements<TM>(string bucket, string org, WritePrecision precision,
            params TM[] measurements)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            WriteMeasurements(bucket, org, precision, measurements.ToList());
        }

        /// <summary>
        /// Forces the client to flush all pending writes from the buffer to the InfluxDB via HTTP.
        /// </summary>
        public void Flush()
        {
            _flush.OnNext(new List<BatchWriteData>());
        }

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
        private readonly PointData _point;
        private readonly InfluxDBClientOptions _clientOptions;

        internal BatchWritePoint(BatchWriteOptions options, InfluxDBClientOptions clientOptions, PointData point) :
            base(options)
        {
            Arguments.CheckNotNull(point, nameof(point));

            _point = point;
            _clientOptions = clientOptions;
        }

        internal override string ToLineProtocol()
        {
            if (!_point.HasFields())
            {
                Trace.WriteLine($"The point: ${_point} doesn't contains any fields, skipping");

                return null;
            }

            return _point.ToLineProtocol(_clientOptions.PointSettings);
        }
    }

    internal class BatchWriteMeasurement<TM> : BatchWriteData
    {
        private readonly TM _measurement;
        private readonly MeasurementMapper _measurementMapper;
        private readonly InfluxDBClientOptions _clientOptions;

        internal BatchWriteMeasurement(BatchWriteOptions options, InfluxDBClientOptions clientOptions, TM measurement,
            MeasurementMapper measurementMapper) :
            base(options)
        {
            Arguments.CheckNotNull(measurement, nameof(measurement));

            _clientOptions = clientOptions;
            _measurement = measurement;
            _measurementMapper = measurementMapper;
        }

        internal override string ToLineProtocol()
        {
            var point = _measurementMapper.ToPoint(_measurement, Options.Precision);
            if (!point.HasFields())
            {
                Trace.WriteLine($"The point: ${point} doesn't contains any fields, skipping");

                return null;
            }

            return point.ToLineProtocol(_clientOptions.PointSettings);
        }
    }

    internal class BatchWriteOptions
    {
        internal readonly string Bucket;
        internal readonly string OrganizationId;
        internal readonly WritePrecision Precision;

        internal BatchWriteOptions(string bucket, string org, WritePrecision precision)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            Bucket = bucket;
            OrganizationId = org;
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