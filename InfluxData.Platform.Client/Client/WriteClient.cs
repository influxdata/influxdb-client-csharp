using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Option;
using InfluxData.Platform.Client.Write;
using Platform.Common;
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

        protected internal WriteClient(DefaultClientIo client, WriteOptions writeOptions) : base(client)
        {
            Arguments.CheckNotNull(writeOptions, "writeOptions");

            // backpreasure - is not implemented in C#
            // 
            // => use unbound buffer
            // 
            // https://github.com/dotnet/reactive/issues/19

            var observable = _subject.ObserveOn(writeOptions.WriteScheduler);

            IObservable<IList<BatchWriteData>> boundary = observable
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
                    IObservable<string> aggregate = grouped
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
                    string org = batchWriteItem.Options.Organization;
                    string bucket = batchWriteItem.Options.Bucket;
                    string precision;

                    switch (batchWriteItem.Options.Precision)
                    {
                        case TimeUnit.Nanos:
                            precision = "n";
                            break;
                        case TimeUnit.Micros:
                            precision = "u";
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

                    var path = $"/api/v2/write?org={org}&bucket={bucket}&precision={precision}";
                    var request = new HttpRequestMessage(new HttpMethod(HttpMethodKind.Post.Name()), path)
                    {
                        Content = new StringContent(batchWriteItem.ToLineProtocol(), Encoding.UTF8, "text/plain")
                    };

                    Task<RequestResult> doRequest = Client.DoRequest(request);

                    return Observable.FromAsync(async () => await doRequest);
                })
                //
                // TODO retry
                //
                .Concat()
                //
                // TODO events
                //
                .Subscribe(
                    requestResult => Trace.WriteLine($"Observed: {requestResult.StatusCode}"),
                    () => Trace.WriteLine("Completed"));
        }

        /// <summary>
        /// Write Line Protocol record into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="organization">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="record">specifies the record in InfluxDB Line Protocol.
        /// The <see cref="record"/> is considered as one batch unit.</param>
        public void WriteRecord(string bucket, string organization, TimeUnit precision, string record)
        {
            Arguments.CheckNonEmptyString(bucket, "bucket");
            Arguments.CheckNonEmptyString(organization, "organization");
            Arguments.CheckNotNull(precision, "TimeUnit.precision is required");

            _subject.OnNext(new BatchWriteRecord(new BatchWriteOptions(bucket, organization, precision), record));
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="organization">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public void WriteRecords(string bucket, string organization, TimeUnit precision, List<string> records)
        {
            Arguments.CheckNonEmptyString(bucket, "bucket");
            Arguments.CheckNonEmptyString(organization, "organization");
            Arguments.CheckNotNull(precision, "TimeUnit.precision is required");

            records.ForEach(record => WriteRecord(bucket, organization, precision, record));
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="organization">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public void WriteRecords(string bucket, string organization, TimeUnit precision, params string[] records)
        {
            Arguments.CheckNonEmptyString(bucket, "bucket");
            Arguments.CheckNonEmptyString(organization, "organization");
            Arguments.CheckNotNull(precision, "TimeUnit.precision is required");

            foreach (var record in records)
            {
                WriteRecord(bucket, organization, precision, record);
            }
        }

        /// <summary>
        /// Write a Data point into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="organization">specifies the destination organization for writes</param>
        /// <param name="point">specifies the Data point to write into bucket</param>
        public void WritePoint(string bucket, string organization, Point point)
        {
            Arguments.CheckNonEmptyString(bucket, "bucket");
            Arguments.CheckNonEmptyString(organization, "organization");

            if (point == null)
            {
                return;
            }

            _subject.OnNext(new BatchWritePoint(new BatchWriteOptions(bucket, organization, point.Precision), point));
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="organization">specifies the destination organization for writes</param>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public void WritePoints(string bucket, string organization, List<Point> points)
        {
            Arguments.CheckNonEmptyString(bucket, "bucket");
            Arguments.CheckNonEmptyString(organization, "organization");

            foreach (var point in points)
            {
                WritePoint(bucket, organization, point);
            }
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="organization">specifies the destination organization for writes</param>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public void WritePoints(string bucket, string organization, params Point[] points)
        {
            Arguments.CheckNonEmptyString(bucket, "bucket");
            Arguments.CheckNonEmptyString(organization, "organization");

            WritePoints(bucket, organization, points.ToList());
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
            _subject.OnCompleted();
            _flush.OnCompleted();

            _subject.Dispose();
            _flush.Dispose();
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


    internal class BatchWriteOptions
    {
        internal readonly string Bucket;
        internal readonly string Organization;
        internal readonly TimeUnit Precision;

        internal BatchWriteOptions(String bucket, String organization, TimeUnit precision)
        {
            Arguments.CheckNonEmptyString(bucket, "bucket");
            Arguments.CheckNonEmptyString(organization, "organization");
            Arguments.CheckNotNull(precision, "TimeUnit.precision is required");

            Bucket = bucket;
            Organization = organization;
            Precision = precision;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((BatchWriteOptions) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Bucket != null ? Bucket.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Organization != null ? Organization.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (int) Precision;
                return hashCode;
            }
        }

        private bool Equals(BatchWriteOptions other)
        {
            return string.Equals(Bucket, other.Bucket) && string.Equals(Organization, other.Organization) &&
                   Precision == other.Precision;
        }
    }
}