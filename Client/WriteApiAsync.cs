using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using InfluxDB.Client.Internal;
using InfluxDB.Client.Writes;

namespace InfluxDB.Client
{
    public class WriteApiAsync
    {
        private readonly InfluxDBClient _influxDbClient;
        private readonly InfluxDBClientOptions _options;
        private readonly WriteService _service;
        private readonly MeasurementMapper _measurementMapper = new MeasurementMapper();

        protected internal WriteApiAsync(InfluxDBClientOptions options, WriteService service,
                        InfluxDBClient influxDbClient)
        {
            Arguments.CheckNotNull(service, nameof(service));
            Arguments.CheckNotNull(influxDbClient, nameof(_influxDbClient));

            _options = options;
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
        public Task WriteRecordAsync(WritePrecision precision, string record)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteRecordAsync(_options.Bucket, _options.Org, precision, record);
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
        public Task WriteRecordAsync(string bucket, string org, WritePrecision precision, string record)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteRecordsAsync(bucket, org, precision, new List<string> {record});
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public Task WriteRecordsAsync(WritePrecision precision, List<string> records)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteRecordsAsync(_options.Bucket, _options.Org, precision, records);
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public Task WriteRecordsAsync(string bucket, string org, WritePrecision precision, List<string> records)
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

            return WriteData(org, bucket, precision, list);
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
        /// Write a Data point into specified bucket.
        /// </summary>
        /// <param name="point">specifies the Data point to write into bucket</param>
        public Task WritePointAsync(PointData point)
        {
            return WritePointAsync(_options.Bucket, _options.Org, point);
        }

        /// <summary>
        /// Write a Data point into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="point">specifies the Data point to write into bucket</param>
        public Task WritePointAsync(string bucket, string org, PointData point)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));

            if (point == null) return Task.CompletedTask;

            return WritePointsAsync(bucket, org, new List<PointData> {point});
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public Task WritePointsAsync(List<PointData> points)
        {
            return WritePointsAsync(_options.Bucket, _options.Org, points);
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public async Task WritePointsAsync(string bucket, string org, List<PointData> points)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            
            foreach (var grouped in points.GroupBy(it => it.Precision))
            {
                var options = new BatchWriteOptions(bucket, org, grouped.Key);
                var groupedPoints = grouped
                    .Select(it => new BatchWritePoint(options, _options, it))
                    .ToList();

                await WriteData(org, bucket, grouped.Key, groupedPoints).ConfigureAwait(false);
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
        /// Write a Measurement into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurement">specifies the Measurement to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public Task WriteMeasurementAsync<TM>(WritePrecision precision, TM measurement)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteMeasurementAsync(_options.Bucket, _options.Org, precision, measurement);
        }

        /// <summary>
        /// Write a Measurement into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurement">specifies the Measurement to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public Task WriteMeasurementAsync<TM>(string bucket, string org, WritePrecision precision, TM measurement)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            if (measurement == null) return Task.CompletedTask;

            return WriteMeasurementsAsync(bucket, org, precision, new List<TM>() {measurement});
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public Task WriteMeasurementsAsync<TM>(WritePrecision precision, List<TM> measurements)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            return WriteMeasurementsAsync(_options.Bucket, _options.Org, precision, measurements);
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
            List<TM> measurements)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));
            
            var list = new List<BatchWriteData>();

            foreach (var measurement in measurements)
            {
                var options = new BatchWriteOptions(bucket, org, precision);

                BatchWriteData data = new BatchWriteMeasurement<TM>(options, _options, measurement, _measurementMapper);
                list.Add(data);
            }

            return WriteData(org, bucket, precision, list);
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

        private Task WriteData(string org, string bucket, WritePrecision precision, IEnumerable<BatchWriteData> data)
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
            
            if (sb.Length == 0)
            {
                Trace.WriteLine($"The writes: {data} doesn't contains any Line Protocol, skipping");
                return Task.CompletedTask;
            }
            
            // remove last \n
            sb.Remove(sb.Length - 1, 1);

            return _service.PostWriteAsync(org, bucket, Encoding.UTF8.GetBytes(sb.ToString()), null , 
                            "identity", "text/plain; charset=utf-8", null, "application/json", null, precision);
        }
    }
}