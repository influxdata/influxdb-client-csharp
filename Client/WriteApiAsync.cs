using System.Collections.Generic;
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
        public async Task WriteRecordAsync(WritePrecision precision, string record)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            await WriteRecordAsync(_options.Bucket, _options.Org, precision, record);
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
        public async Task WriteRecordAsync(string bucket, string org, WritePrecision precision, string record)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            BatchWriteData data = new BatchWriteRecord(new BatchWriteOptions(bucket, org, precision), record);
            await WriteData(org, bucket, data);
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public async Task WriteRecordsAsync(WritePrecision precision, List<string> records)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            await WriteRecordsAsync(_options.Bucket, _options.Org, precision, records);
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public async Task WriteRecordsAsync(string bucket, string org, WritePrecision precision, List<string> records)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            foreach (var record in records) await WriteRecordAsync(bucket, org, precision, record);
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public async Task WriteRecordsAsync(WritePrecision precision, params string[] records)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            await WriteRecordsAsync(_options.Bucket, _options.Org, precision, records);
        }

        /// <summary>
        /// Write Line Protocol records into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="records">specifies the record in InfluxDB Line Protocol</param>
        public async Task WriteRecordsAsync(string bucket, string org, WritePrecision precision,
                        params string[] records)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            foreach (var record in records) await WriteRecordAsync(bucket, org, precision, record);
        }

        /// <summary>
        /// Write a Data point into specified bucket.
        /// </summary>
        /// <param name="point">specifies the Data point to write into bucket</param>
        public async Task WritePointAsync(PointData point)
        {
            await WritePointAsync(_options.Bucket, _options.Org, point);
        }

        /// <summary>
        /// Write a Data point into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="point">specifies the Data point to write into bucket</param>
        public async Task WritePointAsync(string bucket, string org, PointData point)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));

            if (point == null) return;

            BatchWriteData data = new BatchWritePoint(new BatchWriteOptions(bucket, org, 
                            point.Precision), _options, point);

            await WriteData(org, bucket, data);
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public async Task WritePointsAsync(List<PointData> points)
        {
            await WritePointsAsync(_options.Bucket, _options.Org, points);
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

            foreach (var point in points) await WritePointAsync(bucket, org, point);
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public async Task WritePointsAsync(params PointData[] points)
        {
            await WritePointsAsync(_options.Bucket, _options.Org, points);
        }

        /// <summary>
        /// Write Data points into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="points">specifies the Data points to write into bucket</param>
        public async Task WritePointsAsync(string bucket, string org, params PointData[] points)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));

            await WritePointsAsync(bucket, org, points.ToList());
        }

        /// <summary>
        /// Write a Measurement into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurement">specifies the Measurement to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public async Task WriteMeasurementAsync<TM>(WritePrecision precision, TM measurement)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            await WriteMeasurementAsync(_options.Bucket, _options.Org, precision, measurement);
        }

        /// <summary>
        /// Write a Measurement into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurement">specifies the Measurement to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public async Task WriteMeasurementAsync<TM>(string bucket, string org, WritePrecision precision, TM measurement)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            if (measurement == null) return;

            var options = new BatchWriteOptions(bucket, org, precision);

            BatchWriteData data = new BatchWriteMeasurement<TM>(options, _options, measurement, _measurementMapper);

            await WriteData(org, bucket, data);
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public async Task WriteMeasurementsAsync<TM>(WritePrecision precision, List<TM> measurements)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            await WriteMeasurementsAsync(_options.Bucket, _options.Org, precision, measurements);
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public async Task WriteMeasurementsAsync<TM>(string bucket, string org, WritePrecision precision,
                        List<TM> measurements)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            foreach (var measurement in measurements) await WriteMeasurementAsync(bucket, org, precision, measurement);
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public async Task WriteMeasurementsAsync<TM>(WritePrecision precision, params TM[] measurements)
        {
            Arguments.CheckNotNull(precision, nameof(precision));

            await WriteMeasurementsAsync(_options.Bucket, _options.Org, precision, measurements);
        }

        /// <summary>
        /// Write Measurements into specified bucket.
        /// </summary>
        /// <param name="bucket">specifies the destination bucket for writes</param>
        /// <param name="org">specifies the destination organization for writes</param>
        /// <param name="precision">specifies the precision for the unix timestamps within the body line-protocol</param>
        /// <param name="measurements">specifies Measurements to write into bucket</param>
        /// <typeparam name="TM">measurement type</typeparam>
        public async Task WriteMeasurementsAsync<TM>(string bucket, string org, WritePrecision precision,
                        params TM[] measurements)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(precision, nameof(precision));

            await WriteMeasurementsAsync(bucket, org, precision, measurements.ToList());
        }

        private async Task WriteData(string org, string bucket, BatchWriteData data)
        {
            var lineProtocol = data.ToLineProtocol();
            var precision = data.Options.Precision;

            await _service.PostWriteAsync(org, bucket, Encoding.UTF8.GetBytes(lineProtocol), null , 
                            "identity", "text/plain; charset=utf-8", null, "application/json", null, precision);
        }
    }
}