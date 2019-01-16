using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using InfluxData.Platform.Client.Client.Event;
using InfluxData.Platform.Client.Option;
using InfluxData.Platform.Client.Write;
using Platform.Common;
using Platform.Common.Flux.Error;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;

namespace InfluxData.Platform.Client.Client
{
    /// <summary>
    /// Precision of Dates that are written to InfluxData Platform.
    /// </summary>
    public enum TimeUnit
    {
        /// <summary>
        /// Nanosecond precision.
        /// </summary>
        Nanos,

        /// <summary>
        /// Microsecond precision.
        /// </summary>
        Micros,

        /// <summary>
        /// Millisecond precision.
        /// </summary>
        Millis,

        /// <summary>
        /// Second precision.
        /// </summary>
        Seconds
    }

    public class WriteClient : AbstractClient, IDisposable
    {
        private readonly Subject<BatchWriteData> _subject = new Subject<BatchWriteData>();
        private readonly Subject<List<BatchWriteData>> _flush = new Subject<List<BatchWriteData>>();
        private readonly MeasurementMapper _measurementMapper = new MeasurementMapper();
        public event EventHandler EventHandler;

        protected internal WriteClient(DefaultClientIo client, WriteOptions writeOptions) : base(client)
        {
            Arguments.CheckNotNull(writeOptions, "writeOptions");

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

                            if (string.IsNullOrEmpty(data))
                            {
                                return lineProtocol;
                            }

                            if (string.IsNullOrEmpty(lineProtocol))
                            {
                                return data;
                            }

                            return String.Join("\n", lineProtocol, data);
                        });

                    return aggregate.Select(records => new BatchWriteRecord(grouped.Key, records));
                })
                //
                // Jitter
                //
                .Delay(source =>
                {
                    var delay = new Random().NextDouble() * writeOptions.JitterInterval;

                    return Observable.Timer(TimeSpan.FromMilliseconds(delay), Scheduler.CurrentThread);
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
                    string precision;

                    switch (batchWriteItem.Options.Precision)
                    {
                        case TimeUnit.Nanos:
                            precision = "ns";
                            break;
                        case TimeUnit.Micros:
                            precision = "us";
                            break;
                        case TimeUnit.Millis:
                            precision = "ms";
                            break;
                        case TimeUnit.Seconds:
                            precision = "s";
                            break;
                        default:
                            throw new InvalidOperationException();
                    }

                    var path = $"/api/v2/write?org={orgId}&bucket={bucket}&precision={precision}";
                    var request = new HttpRequestMessage(new HttpMethod(HttpMethodKind.Post.Name()), path)
                    {
                        Content = new StringContent(lineProtocol, Encoding.UTF8, "text/plain")
                    };

                    var doRequest = Client.DoRequest(request);
                    doRequest.ContinueWith(task =>
                    {
                        if (task.Result.IsSuccessful())
                        {
                            Publish(new WriteSuccessEvent(orgId, bucket, batchWriteItem.Options.Precision, lineProtocol));
                        }
                        else
                        {
                            var exception = new HttpException(InfluxException.GetErrorMessage(task.Result), task.Result.StatusCode);

                            Publish(new WriteErrorEvent(orgId, bucket, batchWriteItem.Options.Precision, lineProtocol, exception));
                        }
                    });
                    
                    return Observable.FromAsync(async () => await doRequest);
                })
                //
                // TODO retry
                //
                .Concat()
                .Subscribe(
                    requestResult => Trace.WriteLine($"Observed: {requestResult.StatusCode}"),
                    () => Trace.WriteLine("Completed"));
        }

        /// <summary>
        /// Write Line Protocol record into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="record">specifies the record in InfluxDB Line Protocol.
        /// The <see cref="record"/> is considered as one batch unit.</param>
        public void WriteRecord(string bucket, string orgId, TimeUnit precision, string record)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(precision, nameof(precision));

            _subject.OnNext(new BatchWriteRecord(new BatchWriteOptions(bucket, orgId, precision), record));
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public void WriteRecords(string bucket, string orgId, TimeUnit precision, List<string> records)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(precision, nameof(precision));

            records.ForEach(record => WriteRecord(bucket, orgId, precision, record));
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public void WriteRecords(string bucket, string orgId, TimeUnit precision, params string[] records)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(precision, nameof(precision));

            foreach (var record in records)
            {
                WriteRecord(bucket, orgId, precision, record);
            }
        }

        /// <summary>
        /// Write a Data point into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="point">specifies the Data point to write into bucket</param>
        public void WritePoint(string bucket, string orgId, Point point)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            if (point == null)
            {
                return;
            }

            _subject.OnNext(new BatchWritePoint(new BatchWriteOptions(bucket, orgId, point.Precision), point));
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public void WritePoints(string bucket, string orgId, List<Point> points)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            foreach (var point in points)
            {
                WritePoint(bucket, orgId, point);
            }
        }

        /// <summary>
        /// Write Data points into specified bucket.
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
        /// Write a Measurement into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurement">specifies the Measurement to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public void WriteMeasurement<TM>(string bucket, string orgId, TimeUnit precision, TM measurement)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(precision, nameof(precision));

            if (measurement == null)
            {
                return;
            }

            var options = new BatchWriteOptions(bucket, orgId, precision);

            _subject.OnNext(new BatchWriteMeasurement<TM>(options, measurement, _measurementMapper));
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public void WriteMeasurements<TM>(string bucket, string orgId, TimeUnit precision, List<TM> measurements)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(precision, nameof(precision));

            foreach (var measurement in measurements)
            {
                WriteMeasurement(bucket, orgId, precision, measurement);
            }
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="orgId">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public void WriteMeasurements<TM>(string bucket, string orgId, TimeUnit precision,
            params TM[] measurements)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(precision, nameof(precision));

            WriteMeasurements(bucket, orgId, precision, measurements.ToList());
        }

        /// <summary>
        /// Forces the client to flush all pending writes from the buffer to InfluxData Platform via HTTP.
        /// </summary>
        public void Flush()
        {
            _flush.OnNext(new List<BatchWriteData>());
        }

        public void Dispose()
        {
            Trace.WriteLine("Flushing batches before shutdown.");
            
            _subject.OnCompleted();
            _flush.OnCompleted();

            _subject.Dispose();
            _flush.Dispose();
        }
        
        private void Publish(PlatformEventArgs eventArgs)
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

        internal BatchWriteMeasurement(BatchWriteOptions options, TM measurement, MeasurementMapper measurementMapper) : base(options)
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
        internal readonly TimeUnit Precision;

        internal BatchWriteOptions(string bucket, string orgId, TimeUnit precision)
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
                var hashCode = (Bucket != null ? Bucket.GetHashCode() : 0);
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