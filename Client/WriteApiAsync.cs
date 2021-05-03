using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
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
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="record">
        ///     specifies the record in InfluxDB Line Protocol.
        ///     The <see cref="record" /> is considered as one batch unit.
        /// </param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        public Task WriteRecordAsync(WritePrecision precision, string record, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteRecordAsync(_options.Bucket, _options.Org, precision, record, cancellationToken);
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
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        public Task WriteRecordAsync(string bucket, string org, WritePrecision precision, string record, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteRecordsAsync(bucket, org, precision, new List<string> {record}, cancellationToken);
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        public Task WriteRecordsAsync(WritePrecision precision, List<string> records, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteRecordsAsync(_options.Bucket, _options.Org, precision, records, cancellationToken);
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        public Task WriteRecordsAsync(string bucket, string org, WritePrecision precision, List<string> records, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));
            
            var list = new List<BatchWriteData>();

            foreach (var record in records)
            {
                BatchWriteData data = new BatchWriteRecord(new BatchWriteOptions(bucket, org, precision), record);
                list.Add(data);
            }

            return WriteData(org, bucket, precision, list, cancellationToken);
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public Task WriteRecordsAsync(WritePrecision precision, params string[] records)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteRecordsAsync(_options.Bucket, _options.Org, precision, records);
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public Task WriteRecordsAsync(WritePrecision precision, CancellationToken cancellationToken, params string[] records)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteRecordsAsync(_options.Bucket, _options.Org, precision, cancellationToken, records);
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public Task WriteRecordsAsync(string bucket, string org, WritePrecision precision,
                        params string[] records)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteRecordsAsync(bucket, org, precision, records.ToList());
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public Task WriteRecordsAsync(string bucket, string org, WritePrecision precision, 
            CancellationToken cancellationToken, params string[] records)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteRecordsAsync(bucket, org, precision, records.ToList(), cancellationToken);
        }

        /// <summary>
        /// Write Line Protocols records into specified bucket.
        /// </summary>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        /// <param name="bucket">specifies the destination bucket for writes.
        /// If the bucket is not specified than is used config from <see cref="InfluxDBClientOptions.Bucket" />.</param>
        /// <param name="org">specifies the destination organization for writes.
        /// If the org is not specified than is used config from <see cref="InfluxDBClientOptions.Org" />.
        /// </param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <returns>Write Task with IRestResponse</returns>
        public Task<IRestResponse> WriteRecordsAsyncWithIRestResponse(IEnumerable<string> records, string bucket = null,
            string org = null, WritePrecision precision = WritePrecision.Ns,
            CancellationToken cancellationToken = default)
        {
            var batch = records
                .Select(it => new BatchWriteRecord(new BatchWriteOptions(bucket, org, precision), it));

            return WriteDataAsyncWithIRestResponse(batch, bucket, org, precision, cancellationToken);
        }

        /// <summary>
        /// Write a Data point into specified bucket.
        /// </summary>
        /// <param name="point">specifies the Data point to write into bucket</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        public Task WritePointAsync(PointData point, CancellationToken cancellationToken = default)
        {
            return WritePointAsync(_options.Bucket, _options.Org, point, cancellationToken);
        }

        /// <summary>
        /// Write a Data point into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="point">specifies the Data point to write into bucket</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        public Task WritePointAsync(string bucket, string org, PointData point, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));

            if (point == null) return Task.CompletedTask;

            return WritePointsAsync(bucket, org, new List<PointData> {point}, cancellationToken);
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="points">specifies the Data points to write into bucket</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        public Task WritePointsAsync(List<PointData> points, CancellationToken cancellationToken = default)
        {
            return WritePointsAsync(_options.Bucket, _options.Org, points, cancellationToken);
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="points">specifies the Data points to write into bucket</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        public async Task WritePointsAsync(string bucket, string org, List<PointData> points, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            
            foreach (var grouped in points.GroupBy(it => it.Precision))
            {
                var options = new BatchWriteOptions(bucket, org, grouped.Key);
                var groupedPoints = grouped
                    .Select(it => new BatchWritePoint(options, _options, it))
                    .ToList();

                await WriteData(org, bucket, grouped.Key, groupedPoints, cancellationToken).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public Task WritePointsAsync(params PointData[] points)
        {
            return WritePointsAsync(_options.Bucket, _options.Org, points);
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public Task WritePointsAsync(CancellationToken cancellationToken, params PointData[] points)
        {
            return WritePointsAsync(_options.Bucket, _options.Org, cancellationToken, points);
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public Task WritePointsAsync(string bucket, string org, params PointData[] points)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return WritePointsAsync(bucket, org, points.ToList());
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public Task WritePointsAsync(string bucket, string org, CancellationToken cancellationToken, params PointData[] points)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return WritePointsAsync(bucket, org, points.ToList(), cancellationToken);
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="points">specifies the Data points to write into bucket</param>
        /// <param name="bucket">specifies the destination bucket for writes.
        /// If the bucket is not specified than is used config from <see cref="InfluxDBClientOptions.Bucket" />.</param>
        /// <param name="org">specifies the destination organization for writes.
        /// If the org is not specified than is used config from <see cref="InfluxDBClientOptions.Org" />.
        /// </param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <returns>Write Tasks with IRestResponses.</returns>
        public Task<IRestResponse[]> WritePointsAsyncWithIRestResponse(IEnumerable<PointData> points,
            string bucket = null, string org = null, CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task<IRestResponse>>();
            foreach (var grouped in points.GroupBy(it => it.Precision))
            {
                var options = new BatchWriteOptions(bucket, org, grouped.Key);
                var groupedPoints = grouped
                    .Select(it => new BatchWritePoint(options, _options, it))
                    .ToList();

                tasks.Add(WriteDataAsyncWithIRestResponse(groupedPoints, bucket, org, grouped.Key, cancellationToken));
            }

            return Task.WhenAll(tasks);
        }

        /// <summary>
        /// Write a Measurement into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurement">specifies the Measurement to write into bucket</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public Task WriteMeasurementAsync<TM>(WritePrecision precision, TM measurement, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteMeasurementAsync(_options.Bucket, _options.Org, precision, measurement, cancellationToken);
        }

        /// <summary>
        /// Write a Measurement into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurement">specifies the Measurement to write into bucket</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public Task WriteMeasurementAsync<TM>(string bucket, string org, WritePrecision precision, TM measurement, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            if (measurement == null) return Task.CompletedTask;

            return WriteMeasurementsAsync(bucket, org, precision, new List<TM>() {measurement}, cancellationToken);
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public Task WriteMeasurementsAsync<TM>(WritePrecision precision, List<TM> measurements, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteMeasurementsAsync(_options.Bucket, _options.Org, precision, measurements, cancellationToken);
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public Task WriteMeasurementsAsync<TM>(string bucket, string org, WritePrecision precision,
            List<TM> measurements, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));
            
            var list = new List<BatchWriteData>();

            var options = new BatchWriteOptions(bucket, org, precision);
            foreach (var measurement in measurements)
            {
                BatchWriteData data = new BatchWriteMeasurement<TM>(options, _options, measurement, _mapper);
                list.Add(data);
            }

            return WriteData(org, bucket, precision, list, cancellationToken);
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public Task WriteMeasurementsAsync<TM>(WritePrecision precision, params TM[] measurements)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteMeasurementsAsync(_options.Bucket, _options.Org, precision, measurements);
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public Task WriteMeasurementsAsync<TM>(WritePrecision precision, CancellationToken cancellationToken, params TM[] measurements)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteMeasurementsAsync(_options.Bucket, _options.Org, precision, cancellationToken, measurements);
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public Task WriteMeasurementsAsync<TM>(string bucket, string org, WritePrecision precision,
                        params TM[] measurements)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteMeasurementsAsync(bucket, org, precision, measurements.ToList());
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public Task WriteMeasurementsAsync<TM>(string bucket, string org, WritePrecision precision,
            CancellationToken cancellationToken, params TM[] measurements)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteMeasurementsAsync(bucket, org, precision, measurements.ToList(), cancellationToken);
        }
        
        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <param name="bucket">specifies the destination bucket for writes.
        /// If the bucket is not specified than is used config from <see cref="InfluxDBClientOptions.Bucket" />.</param>
        /// <param name="org">specifies the destination organization for writes.
        /// If the org is not specified than is used config from <see cref="InfluxDBClientOptions.Org" />.
        /// </param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="cancellationToken">specifies the token to monitor for cancellation requests</param>
        /// <typeparam name="TM">measurement type</typeparam>
        /// <returns>Write Task with IRestResponse</returns>
        public Task<IRestResponse> WriteMeasurementsAsyncWithIRestResponse<TM>(IEnumerable<TM> measurements, string bucket = null,
            string org = null, WritePrecision precision = WritePrecision.Ns,
            CancellationToken cancellationToken = default)
        {
            
            var options = new BatchWriteOptions(bucket, org, precision);
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

        private Task<IRestResponse> WriteDataAsyncWithIRestResponse(IEnumerable<BatchWriteData> batch,string bucket = null, string org = null,
            WritePrecision precision = WritePrecision.Ns, CancellationToken cancellationToken = default)
        {
            var localBucket = bucket ?? _options.Bucket;
            var localOrg = org ?? _options.Org;
            
            Arguments.CheckNonEmptyString(localBucket, nameof(bucket));
            Arguments.CheckNonEmptyString(localOrg, nameof(org));
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