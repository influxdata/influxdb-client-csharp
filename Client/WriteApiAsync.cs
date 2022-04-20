using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Internal;
using InfluxDB.Client.Writes;
using RestSharp;

namespace InfluxDB.Client
{
    public class WriteApiAsync
    {
        private readonly InfluxDBClient _influxDbClient;
        private readonly InfluxDBClientOptions _options;
        private readonly WriteService _service;
        private readonly IDomainObjectMapper _mapper;
        private const string PostHeaderAccept = "application/json";
        private const string PostHeaderEncoding = "identity";
        private const string PostHeaderContentType = "text/plain; charset=utf-8";

        protected internal WriteApiAsync(InfluxDBClientOptions options, WriteService service,
            IDomainObjectMapper mapper,
            InfluxDBClient influxDbClient)
        {
            Arguments.CheckNotNull(service, nameof(service));
            Arguments.CheckNotNull(mapper, nameof(mapper));
            Arguments.CheckNotNull(influxDbClient, nameof(_influxDbClient));

            _options = options;
            _mapper = mapper;
            _influxDbClient = influxDbClient;
            _service = service;
        }

        /// <summary>
        /// Write Line Protocol record into specified bucket.
        /// </summary>
        /// <param name="record">specifies the record in InfluxDB Line Protocol. The <see cref="record" /> is considered as one batch unit. </param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol; default Nanoseconds</param>
        /// <param name="bucket">specifies the destination bucket for writes. If the bucket is not specified then is used config from <see cref="InfluxDBClientOptions.Bucket" />.</param>
        /// <param name="org">specifies the destination organization for writes. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        public Task WriteRecordAsync(string record, WritePrecision precision = WritePrecision.Ns, string bucket = null,
            string org = null, CancellationToken cancellationToken = default)
        {
            return WriteRecordsAsync(new List<string> { record }, precision, bucket, org, cancellationToken);
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol; default Nanoseconds</param>
        /// <param name="bucket">specifies the destination bucket for writes. If the bucket is not specified then is used config from <see cref="InfluxDBClientOptions.Bucket" />.</param>
        /// <param name="org">specifies the destination organization for writes. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        public Task WriteRecordsAsync(List<string> records, WritePrecision precision = WritePrecision.Ns,
            string bucket = null, string org = null, CancellationToken cancellationToken = default)
        {
            var options = new BatchWriteOptions(bucket ?? _options.Bucket, org ?? _options.Org, precision);
            var list = records.Select(record => new BatchWriteRecord(options, record)).ToList();

            return WriteData(options.OrganizationId, options.Bucket, precision, list, cancellationToken);
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol; default Nanoseconds</param>
        /// <param name="bucket">specifies the destination bucket for writes. If the bucket is not specified then is used config from <see cref="InfluxDBClientOptions.Bucket" />.</param>
        /// <param name="org">specifies the destination organization for writes. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        public Task WriteRecordsAsync(string[] records, WritePrecision precision = WritePrecision.Ns,
            string bucket = null, string org = null, CancellationToken cancellationToken = default)
        {
            return WriteRecordsAsync(records.ToList(), precision, bucket, org, cancellationToken);
        }

        /// <summary>
        /// Write Line Protocols records into specified bucket.
        /// </summary>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol; default Nanoseconds</param>
        /// <param name="bucket">specifies the destination bucket for writes. If the bucket is not specified then is used config from <see cref="InfluxDBClientOptions.Bucket" />.</param>
        /// <param name="org">specifies the destination organization for writes. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <returns>Write Task with IRestResponse</returns>
        public Task<RestResponse> WriteRecordsAsyncWithIRestResponse(IEnumerable<string> records,
            WritePrecision precision = WritePrecision.Ns, string bucket = null, string org = null,
            CancellationToken cancellationToken = default)
        {
            var options = new BatchWriteOptions(bucket ?? _options.Bucket, org ?? _options.Org, precision);
            var batch = records
                .Select(it => new BatchWriteRecord(options, it));

            return WriteDataAsyncWithIRestResponse(batch, options.Bucket, options.OrganizationId, precision,
                cancellationToken);
        }

        /// <summary>
        /// Write a Data point into specified bucket.
        /// </summary>
        /// <param name="point">specifies the Data point to write into bucket</param>
        /// <param name="bucket">specifies the destination bucket for writes. If the bucket is not specified then is used config from <see cref="InfluxDBClientOptions.Bucket" />.</param>
        /// <param name="org">specifies the destination organization for writes. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        public Task WritePointAsync(PointData point, string bucket = null, string org = null,
            CancellationToken cancellationToken = default)
        {
            if (point == null)
            {
                return Task.CompletedTask;
            }

            return WritePointsAsync(new List<PointData> { point }, bucket, org, cancellationToken);
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="points">specifies the Data points to write into bucket</param>
        /// <param name="bucket">specifies the destination bucket for writes. If the bucket is not specified then is used config from <see cref="InfluxDBClientOptions.Bucket" />.</param>
        /// <param name="org">specifies the destination organization for writes. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        public async Task WritePointsAsync(List<PointData> points, string bucket = null, string org = null,
            CancellationToken cancellationToken = default)
        {
            foreach (var grouped in points.GroupBy(it => it.Precision))
            {
                var options = new BatchWriteOptions(bucket ?? _options.Bucket, org ?? _options.Org, grouped.Key);
                var groupedPoints = grouped
                    .Select(it => new BatchWritePoint(options, _options, it))
                    .ToList();

                await WriteData(options.OrganizationId, options.Bucket, grouped.Key, groupedPoints, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="points">specifies the Data points to write into bucket</param>
        /// <param name="bucket">specifies the destination bucket for writes. If the bucket is not specified then is used config from <see cref="InfluxDBClientOptions.Bucket" />.</param>
        /// <param name="org">specifies the destination organization for writes. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        public Task WritePointsAsync(PointData[] points, string bucket = null, string org = null,
            CancellationToken cancellationToken = default)
        {
            return WritePointsAsync(points.ToList(), bucket, org, cancellationToken);
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="points">specifies the Data points to write into bucket</param>
        /// <param name="bucket">specifies the destination bucket for writes.
        /// If the bucket is not specified then is used config from <see cref="InfluxDBClientOptions.Bucket" />.</param>
        /// <param name="org">specifies the destination organization for writes. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />. </param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <returns>Write Tasks with IRestResponses.</returns>
        public Task<RestResponse[]> WritePointsAsyncWithIRestResponse(IEnumerable<PointData> points,
            string bucket = null, string org = null, CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task<RestResponse>>();
            foreach (var grouped in points.GroupBy(it => it.Precision))
            {
                var options = new BatchWriteOptions(bucket ?? _options.Bucket, org ?? _options.Org, grouped.Key);
                var groupedPoints = grouped
                    .Select(it => new BatchWritePoint(options, _options, it))
                    .ToList();

                tasks.Add(WriteDataAsyncWithIRestResponse(groupedPoints, options.Bucket, options.OrganizationId,
                    grouped.Key, cancellationToken));
            }

            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Write a Measurement into specified bucket.
        /// </summary>
        /// <param name="measurement">specifies the Measurement to write into bucket</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol; default Nanoseconds</param>
        /// <param name="bucket">specifies the destination bucket for writes. If the bucket is not specified then is used config from <see cref="InfluxDBClientOptions.Bucket" />.</param>
        /// <param name="org">specifies the destination organization for writes. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public Task WriteMeasurementAsync<TM>(TM measurement, WritePrecision precision = WritePrecision.Ns,
            string bucket = null, string org = null, CancellationToken cancellationToken = default)
        {
            if (measurement == null)
            {
                return Task.CompletedTask;
            }

            return WriteMeasurementsAsync(new List<TM> { measurement }, precision, bucket, org, cancellationToken);
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol; default Nanoseconds</param>
        /// <param name="bucket">specifies the destination bucket for writes. If the bucket is not specified then is used config from <see cref="InfluxDBClientOptions.Bucket" />.</param>
        /// <param name="org">specifies the destination organization for writes. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public Task WriteMeasurementsAsync<TM>(List<TM> measurements, WritePrecision precision = WritePrecision.Ns,
            string bucket = null, string org = null, CancellationToken cancellationToken = default)
        {
            var list = new List<BatchWriteData>();

            var options = new BatchWriteOptions(bucket ?? _options.Bucket, org ?? _options.Org, precision);
            foreach (var measurement in measurements)
            {
                BatchWriteData data = new BatchWriteMeasurement<TM>(options, _options, measurement, _mapper);
                list.Add(data);
            }

            return WriteData(options.OrganizationId, options.Bucket, precision, list, cancellationToken);
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol; default Nanoseconds</param>
        /// <param name="bucket">specifies the destination bucket for writes. If the bucket is not specified then is used config from <see cref="InfluxDBClientOptions.Bucket" />.</param>
        /// <param name="org">specifies the destination organization for writes. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public Task WriteMeasurementsAsync<TM>(TM[] measurements, WritePrecision precision = WritePrecision.Ns,
            string bucket = null, string org = null, CancellationToken cancellationToken = default)
        {
            return WriteMeasurementsAsync(measurements.ToList(), precision, bucket, org, cancellationToken);
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol; default Nanoseconds</param>
        /// <param name="bucket">specifies the destination bucket for writes. If the bucket is not specified then is used config from <see cref="InfluxDBClientOptions.Bucket" />.</param>
        /// <param name="org">specifies the destination organization for writes. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <typeparam name="TM">measurement type</typeparam>
        /// <returns>Write Task with IRestResponse</returns>
        public Task<RestResponse> WriteMeasurementsAsyncWithIRestResponse<TM>(IEnumerable<TM> measurements,
            WritePrecision precision = WritePrecision.Ns, string bucket = null, string org = null,
            CancellationToken cancellationToken = default)
        {
            var options = new BatchWriteOptions(bucket ?? _options.Bucket, org ?? _options.Org, precision);
            var batch = measurements
                .Select(it => new BatchWriteMeasurement<TM>(options, _options, it, _mapper));

            return WriteDataAsyncWithIRestResponse(batch, bucket, org, precision, cancellationToken);
        }


        private Task WriteData(string org, string bucket, WritePrecision precision, IEnumerable<BatchWriteData> data,
            CancellationToken cancellationToken)
        {
            var sb = ToLineProtocolBody(data);
            if (sb.Length == 0)
            {
                Trace.WriteLine($"The writes: {data} doesn't contains any Line Protocol, skipping");
                return Task.CompletedTask;
            }

            return _service.PostWriteAsync(org, bucket, Encoding.UTF8.GetBytes(sb.ToString()), null,
                PostHeaderEncoding, PostHeaderContentType, null, PostHeaderAccept, null, precision,
                cancellationToken);
        }

        private Task<RestResponse> WriteDataAsyncWithIRestResponse(IEnumerable<BatchWriteData> batch,
            string bucket = null, string org = null,
            WritePrecision precision = WritePrecision.Ns, CancellationToken cancellationToken = default)
        {
            var localBucket = bucket ?? _options.Bucket;
            var localOrg = org ?? _options.Org;

            Arguments.CheckNonEmptyString(localBucket, AbstractRestClient.BucketArgumentValidation);
            Arguments.CheckNonEmptyString(localOrg, AbstractRestClient.OrgArgumentValidation);
            Arguments.CheckNotNull(precision, nameof(precision));

            var sb = ToLineProtocolBody(batch);

            return _service.PostWriteAsyncWithIRestResponse(org, bucket, Encoding.UTF8.GetBytes(sb.ToString()), null,
                PostHeaderEncoding, PostHeaderContentType, null, PostHeaderAccept, null, precision,
                cancellationToken);
        }

        private static StringBuilder ToLineProtocolBody(IEnumerable<BatchWriteData> data)
        {
            var sb = new StringBuilder("");

            foreach (var item in data)
            {
                var lineProtocol = item.ToLineProtocol();

                if (string.IsNullOrEmpty(lineProtocol))
                {
                    continue;
                }

                sb.Append(lineProtocol);
                sb.Append("\n");
            }

            if (sb.Length != 0)
            {
                // remove last \n
                sb.Remove(sb.Length - 1, 1);
            }

            return sb;
        }
    }
}