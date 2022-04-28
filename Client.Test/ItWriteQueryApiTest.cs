using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;
using NodaTime;
using NUnit.Framework;
using Duration = NodaTime.Duration;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItWriteQueryApiTest : AbstractItClientTest
    {
        [SetUp]
        public new async Task SetUp()
        {
            _organization = await FindMyOrg();

            //bug: https://github.com/influxdata/influxdb/issues/19518
            //var retention = new BucketRetentionRules(BucketRetentionRules.TypeEnum.Expire, 3600);
            _bucket = await Client.GetBucketsApi()
                .CreateBucketAsync(GenerateName("h2o"), null, _organization);

            //
            // Add Permissions to read and write to the Bucket
            //
            var resource =
                new PermissionResource(PermissionResource.TypeBuckets, _bucket.Id, null, _organization.Id);

            var readBucket = new Permission(Permission.ActionEnum.Read, resource);
            var writeBucket = new Permission(Permission.ActionEnum.Write, resource);

            var loggedUser = await Client.GetUsersApi().MeAsync();
            Assert.IsNotNull(loggedUser);

            var authorization = await Client.GetAuthorizationsApi()
                .CreateAuthorizationAsync(await FindMyOrg(), new List<Permission> { readBucket, writeBucket });

            _token = authorization.Token;

            Client.Dispose();
            var options = new InfluxDBClientOptions.Builder().Url(InfluxDbUrl).AuthenticateToken(_token)
                .Org(_organization.Id).Bucket(_bucket.Id).Build();
            Client = InfluxDBClientFactory.Create(options);
            _queryApi = Client.GetQueryApi();
        }

        [TearDown]
        protected new void After()
        {
            _writeApi?.Dispose();
        }

        private Bucket _bucket;
        private QueryApi _queryApi;
        private WriteApi _writeApi;
        private Organization _organization;
        private string _token;


        [Measurement("h2o")]
        private class H20Measurement
        {
            [Column("location", IsTag = true)] public string Location { get; set; }

            [Column("level")] public double? Level { get; set; }

            [Column(IsTimestamp = true)] public DateTime Time { get; set; }
        }

        [Test]
        public async Task DefaultTagsConfiguration()
        {
            Client.Dispose();

            var options = new InfluxDBClientOptions.Builder()
                .LoadConfig()
                .Url(InfluxDbUrl)
                .AuthenticateToken(_token)
                .Build();

            Client = InfluxDBClientFactory.Create(options);

            var measurement1 = new H20Measurement
            {
                Location = "coyote_creek", Level = 2.927, Time = DateTime.UtcNow
            };

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WriteMeasurement(measurement1, WritePrecision.Ms, _bucket.Name, _organization.Id);
            _writeApi.Flush();

            listener.WaitToSuccess();

            _queryApi = Client.GetQueryApi();
            var tables = await _queryApi.QueryAsync(
                $"from(bucket:\"{_bucket.Name}\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")",
                _organization.Id);

            Assert.AreEqual(1, tables.Count);
            Assert.AreEqual(1, tables[0].Records.Count);
            Assert.AreEqual("h2o", tables[0].Records[0].GetMeasurement());
            Assert.AreEqual(2.927, tables[0].Records[0].GetValueByKey("level"));
            Assert.AreEqual("coyote_creek", tables[0].Records[0].GetValueByKey("location"));
            Assert.AreEqual("132-987-655", tables[0].Records[0].GetValueByKey("id"));
            Assert.AreEqual("California Miner", tables[0].Records[0].GetValueByKey("customer"));
            Assert.AreEqual("v1.00", tables[0].Records[0].GetValueByKey("version"));
        }

        [Test]
        public async Task DefaultTagsMeasurement()
        {
            Client.Dispose();

            Environment.SetEnvironmentVariable("measurement-datacenter", "LA");
            ConfigurationManager.AppSettings["measurement-sensor.version"] = "1.23a";

            var options = new InfluxDBClientOptions.Builder().Url(InfluxDbUrl)
                .AuthenticateToken(_token)
                .AddDefaultTag("id", "132-987-655")
                .AddDefaultTag("customer", "California Miner")
                .AddDefaultTag("env-var", "${env.measurement-datacenter}")
                .AddDefaultTag("sensor-version", "${measurement-sensor.version}")
                .Build();

            Client = InfluxDBClientFactory.Create(options);

            var measurement1 = new H20Measurement
            {
                Location = "coyote_creek", Level = 2.927, Time = DateTime.UtcNow
            };

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WriteMeasurement(measurement1, WritePrecision.Ms, _bucket.Name, _organization.Id);
            _writeApi.Flush();

            listener.WaitToSuccess();

            _queryApi = Client.GetQueryApi();
            var tables = await _queryApi.QueryAsync(
                $"from(bucket:\"{_bucket.Name}\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")",
                _organization.Id);

            Assert.AreEqual(1, tables.Count);
            Assert.AreEqual(1, tables[0].Records.Count);
            Assert.AreEqual("h2o", tables[0].Records[0].GetMeasurement());
            Assert.AreEqual(2.927, tables[0].Records[0].GetValueByKey("level"));
            Assert.AreEqual("coyote_creek", tables[0].Records[0].GetValueByKey("location"));
            Assert.AreEqual("132-987-655", tables[0].Records[0].GetValueByKey("id"));
            Assert.AreEqual("California Miner", tables[0].Records[0].GetValueByKey("customer"));
            Assert.AreEqual("1.23a", tables[0].Records[0].GetValueByKey("sensor-version"));
            Assert.AreEqual("LA", tables[0].Records[0].GetValueByKey("env-var"));
        }

        [Test]
        public async Task DefaultTagsPoint()
        {
            Client.Dispose();

            Environment.SetEnvironmentVariable("point-datacenter", "LA");
            ConfigurationManager.AppSettings["point-sensor.version"] = "1.23a";

            var options = new InfluxDBClientOptions.Builder().Url(InfluxDbUrl)
                .AuthenticateToken(_token)
                .AddDefaultTag("id", "132-987-655")
                .AddDefaultTag("customer", "California Miner")
                .AddDefaultTag("env-var", "${env.point-datacenter}")
                .AddDefaultTag("sensor-version", "${point-sensor.version}")
                .Build();

            Client = InfluxDBClientFactory.Create(options);

            var point = PointData.Measurement("h2o_feet").Tag("location", "west").Field("water_level", 1);

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WritePoint(point, _bucket.Name, _organization.Id);
            _writeApi.Flush();

            listener.WaitToSuccess();

            _queryApi = Client.GetQueryApi();
            var tables = await _queryApi.QueryAsync(
                $"from(bucket:\"{_bucket.Name}\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")",
                _organization.Id);

            Assert.AreEqual(1, tables.Count);
            Assert.AreEqual(1, tables[0].Records.Count);
            Assert.AreEqual("h2o_feet", tables[0].Records[0].GetMeasurement());
            Assert.AreEqual(1, tables[0].Records[0].GetValueByKey("water_level"));
            Assert.AreEqual("west", tables[0].Records[0].GetValueByKey("location"));
            Assert.AreEqual("132-987-655", tables[0].Records[0].GetValueByKey("id"));
            Assert.AreEqual("California Miner", tables[0].Records[0].GetValueByKey("customer"));
            Assert.AreEqual("1.23a", tables[0].Records[0].GetValueByKey("sensor-version"));
            Assert.AreEqual("LA", tables[0].Records[0].GetValueByKey("env-var"));
        }

        [Test]
        public async Task EnabledGzip()
        {
            Client.EnableGzip();

            var bucketName = _bucket.Name;

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WriteRecords(new List<string> { record1, record2 }, WritePrecision.Ns, bucketName,
                _organization.Id);
            _writeApi.Flush();

            listener.WaitToSuccess();

            var query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);

            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(2, records.Count);

            Assert.AreEqual("h2o_feet", records[0].GetMeasurement());
            Assert.AreEqual(1, records[0].GetValue());
            Assert.AreEqual("level water_level", records[0].GetField());

            Assert.AreEqual("h2o_feet", records[1].GetMeasurement());
            Assert.AreEqual(2, records[1].GetValue());
            Assert.AreEqual("level water_level", records[1].GetField());
        }

        [Test]
        public async Task Flush()
        {
            var bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(10).FlushInterval(100_000).Build();

            _writeApi = Client.GetWriteApi(writeOptions);
            var listener = new WriteApiTest.EventListener(_writeApi);

            var record = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";

            _writeApi.WriteRecord(record, WritePrecision.Ns, bucketName, _organization.Id);
            _writeApi.Flush();
            listener.WaitToSuccess();

            var query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);

            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(1, records.Count);
            Assert.AreEqual("h2o_feet", records[0].GetMeasurement());
            Assert.AreEqual(1, records[0].GetValue());
            Assert.AreEqual("level water_level", records[0].GetField());

            var instant = Instant.Add(Instant.FromUnixTimeMilliseconds(0), Duration.FromNanoseconds(1L));
            Assert.AreEqual(instant, records[0].GetTime());
        }

        [Test]
        public async Task FlushByCount()
        {
            var bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(6).FlushInterval(500_000).Build();

            _writeApi = Client.GetWriteApi(writeOptions);

            var listener = new WriteApiTest.EventListener(_writeApi);

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";
            const string record3 = "h2o_feet,location=coyote_creek level\\ water_level=3.0 3";
            const string record4 = "h2o_feet,location=coyote_creek level\\ water_level=4.0 4";
            const string record5 = "h2o_feet,location=coyote_creek level\\ water_level=5.0 5";
            const string record6 = "h2o_feet,location=coyote_creek level\\ water_level=6.0 6";

            _writeApi.WriteRecord(record1, WritePrecision.Ns, bucketName, _organization.Id);
            _writeApi.WriteRecord(record2, WritePrecision.Ns, bucketName, _organization.Id);
            _writeApi.WriteRecord(record3, WritePrecision.Ns, bucketName, _organization.Id);
            _writeApi.WriteRecord(record4, WritePrecision.Ns, bucketName, _organization.Id);
            _writeApi.WriteRecord(record5, WritePrecision.Ns, bucketName, _organization.Id);

            var query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);
            Assert.AreEqual(0, query.Count);

            _writeApi.WriteRecord(record6, WritePrecision.Ns, bucketName, _organization.Id);

            listener.Get<WriteSuccessEvent>();

            query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);
            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(6, records.Count);
        }

        [Test]
        public async Task FlushByOne()
        {
            var bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(1).FlushInterval(500_000).Build();

            _writeApi = Client.GetWriteApi(writeOptions);

            var eventListener = new WriteApiTest.EventListener(_writeApi);

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";
            const string record3 = "h2o_feet,location=coyote_creek level\\ water_level=3.0 3";
            const string record4 = "h2o_feet,location=coyote_creek level\\ water_level=4.0 4";
            const string record5 = "h2o_feet,location=coyote_creek level\\ water_level=5.0 5";

            _writeApi.WriteRecord(record1, WritePrecision.Ns, bucketName, _organization.Id);
            Thread.Sleep(100);
            _writeApi.WriteRecord(record2, WritePrecision.Ns, bucketName, _organization.Id);
            Thread.Sleep(100);
            _writeApi.WriteRecord(record3, WritePrecision.Ns, bucketName, _organization.Id);
            Thread.Sleep(100);
            _writeApi.WriteRecord(record4, WritePrecision.Ns, bucketName, _organization.Id);
            Thread.Sleep(100);
            _writeApi.WriteRecord(record5, WritePrecision.Ns, bucketName, _organization.Id);
            Thread.Sleep(100);

            Assert.AreEqual(record1, eventListener.Get<WriteSuccessEvent>().LineProtocol);
            Assert.AreEqual(record2, eventListener.Get<WriteSuccessEvent>().LineProtocol);
            Assert.AreEqual(record3, eventListener.Get<WriteSuccessEvent>().LineProtocol);
            Assert.AreEqual(record4, eventListener.Get<WriteSuccessEvent>().LineProtocol);
            Assert.AreEqual(record5, eventListener.Get<WriteSuccessEvent>().LineProtocol);

            var query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000000Z)",
                _organization.Id);
            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(5, records.Count);
            Assert.AreEqual(0, eventListener.EventCount());
        }

        [Test]
        public async Task FlushByTime()
        {
            var bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(10).FlushInterval(500).Build();

            _writeApi = Client.GetWriteApi(writeOptions);
            var listener = new WriteApiTest.EventListener(_writeApi);

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";
            const string record3 = "h2o_feet,location=coyote_creek level\\ water_level=3.0 3";
            const string record4 = "h2o_feet,location=coyote_creek level\\ water_level=4.0 4";
            const string record5 = "h2o_feet,location=coyote_creek level\\ water_level=5.0 5";

            _writeApi.WriteRecord(record1, WritePrecision.Ns, bucketName, _organization.Id);
            _writeApi.WriteRecord(record2, WritePrecision.Ns, bucketName, _organization.Id);
            _writeApi.WriteRecord(record3, WritePrecision.Ns, bucketName, _organization.Id);
            _writeApi.WriteRecord(record4, WritePrecision.Ns, bucketName, _organization.Id);
            _writeApi.WriteRecord(record5, WritePrecision.Ns, bucketName, _organization.Id);

            var query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);
            Assert.AreEqual(0, query.Count);

            listener.Get<WriteSuccessEvent>();

            query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);
            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(5, records.Count);
        }

        [Test]
        public async Task Jitter()
        {
            var bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(1).JitterInterval(5_000).Build();

            _writeApi = Client.GetWriteApi(writeOptions);

            var record = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";

            _writeApi.WriteRecord(record, WritePrecision.Ns, bucketName, _organization.Id);

            var query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);
            Assert.AreEqual(0, query.Count);

            Thread.Sleep(5_500);

            query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);

            Assert.AreEqual(1, query.Count);
        }

        [Test]
        public void ListenWriteErrorEvent()
        {
            var bucketName = _bucket.Name;

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);

            _writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ water_level=1.0 123456.789",
                WritePrecision.Ns, bucketName, _organization.Id);
            _writeApi.Flush();

            var error = listener.Get<WriteErrorEvent>();

            Assert.IsNotNull(error);
            Assert.AreEqual(
                "unable to parse 'h2o_feet,location=coyote_creek level\\ water_level=1.0 123456.789': bad timestamp",
                error.Exception.Message);
        }

        [Test]
        public void ListenWriteSuccessEvent()
        {
            var bucketName = _bucket.Name;

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);

            _writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ water_level=1.0 1", WritePrecision.Ns,
                bucketName, _organization.Id);
            _writeApi.Flush();

            var success = listener.Get<WriteSuccessEvent>();

            Assert.IsNotNull(success);
            Assert.AreEqual(success.Organization, _organization.Id);
            Assert.AreEqual(success.Bucket, bucketName);
            Assert.AreEqual(success.Precision, WritePrecision.Ns);
            Assert.AreEqual(success.LineProtocol, "h2o_feet,location=coyote_creek level\\ water_level=1.0 1");
        }

        [Test]
        public async Task PartialWrite()
        {
            var bucketName = _bucket.Name;

            _writeApi = Client.GetWriteApi(WriteOptions.CreateNew().BatchSize(2).Build());

            const string records = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1\n" +
                                   "h2o_feet,location=coyote_hill level\\ water_level=2.0 2x";

            _writeApi.WriteRecord(records, bucket: bucketName, org: _organization.Id);
            _writeApi.Flush();
            _writeApi.Dispose();

            var query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> last()",
                _organization.Id);
            Assert.AreEqual(0, query.Count);
        }

        [Test]
        public async Task SimpleWriteAndDisposing()
        {
            // Using WriteApi
            {
                var client = InfluxDBClientFactory.Create(InfluxDbUrl, _token);

                using (var writeApi = client.GetWriteApi())
                {
                    writeApi.WriteRecord("temperature,location=north value=60.0 1", WritePrecision.Ns, _bucket.Name,
                        _organization.Id);
                }

                client.Dispose();
            }

            // Using both
            {
                using (var client = InfluxDBClientFactory.Create(InfluxDbUrl, _token))
                {
                    using (var writeApi = client.GetWriteApi())
                    {
                        writeApi.WriteRecord("temperature,location=north value=70.0 2", WritePrecision.Ns, _bucket.Name,
                            _organization.Id);
                    }
                }
            }

            // Using without
            {
                var client = InfluxDBClientFactory.Create(InfluxDbUrl, _token);
                var writeApi = client.GetWriteApi();

                writeApi.WriteRecord("temperature,location=north value=80.0 3", WritePrecision.Ns, _bucket.Name,
                    _organization.Id);

                client.Dispose();
            }

            var tables = await _queryApi.QueryAsync(
                $"from(bucket:\"{_bucket.Name}\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);

            Assert.AreEqual(1, tables.Count);
            Assert.AreEqual(3, tables[0].Records.Count);
            Assert.AreEqual(60, tables[0].Records[0].GetValue());
            Assert.AreEqual(70, tables[0].Records[1].GetValue());
            Assert.AreEqual(80, tables[0].Records[2].GetValue());
        }

        [Test]
        public async Task Recovery()
        {
            var bucketName = _bucket.Name;

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);

            _writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ water_level=1.0 1x", WritePrecision.Ns,
                bucketName, _organization.Id);
            _writeApi.Flush();

            var error = listener.Get<WriteErrorEvent>();

            Assert.IsNotNull(error);
            Assert.AreEqual(
                "unable to parse 'h2o_feet,location=coyote_creek level\\ water_level=1.0 1x': bad timestamp",
                error.Exception.Message);

            _writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ water_level=1.0 1", WritePrecision.Ns,
                bucketName, _organization.Id);
            _writeApi.Flush();

            listener.Get<WriteSuccessEvent>();

            var query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);

            Assert.AreEqual(1, query.Count);
            Assert.AreEqual(1, query[0].Records.Count);
            Assert.AreEqual("coyote_creek", query[0].Records[0].GetValueByKey("location"));
            Assert.AreEqual("level water_level", query[0].Records[0].GetValueByKey("_field"));
            Assert.AreEqual(1, query[0].Records[0].GetValue());
        }

        [Test]
        public async Task WriteAndQueryByOrganizationName()
        {
            var bucketName = _bucket.Name;

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WriteRecords(new List<string> { record1, record2 }, WritePrecision.Ns, bucketName,
                _organization.Name);
            _writeApi.Flush();
            listener.WaitToSuccess();

            var query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Name);

            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(2, records.Count);

            Assert.AreEqual("h2o_feet", records[0].GetMeasurement());
            Assert.AreEqual(1, records[0].GetValue());
            Assert.AreEqual("level water_level", records[0].GetField());

            Assert.AreEqual("h2o_feet", records[1].GetMeasurement());
            Assert.AreEqual(2, records[1].GetValue());
            Assert.AreEqual("level water_level", records[1].GetField());
        }

        [Test]
        public async Task WriteMeasurements()
        {
            _writeApi = Client.GetWriteApi();

            var bucketName = _bucket.Name;

            var time = DateTime.UtcNow.Add(-TimeSpan.FromSeconds(10));

            var measurement1 = new H20Measurement
            {
                Location = "coyote_creek", Level = 2.927, Time = time.Add(-TimeSpan.FromSeconds(30))
            };
            var measurement2 = new H20Measurement
            {
                Location = "coyote_creek", Level = 1.927, Time = time
            };

            _writeApi.WriteMeasurements(new[] { measurement1, measurement2 }, WritePrecision.S, bucketName,
                _organization.Id);
            _writeApi.Dispose();

            var measurements = await _queryApi.QueryAsync<H20Measurement>(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000001Z) |> rename(columns:{{_value: \"level\"}})",
                _organization.Id);

            Assert.AreEqual(2, measurements.Count);

            Assert.AreEqual(2.927, measurements[0].Level);
            Assert.AreEqual("coyote_creek", measurements[0].Location);
            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)).Add(-TimeSpan.FromSeconds(30)),
                measurements[0].Time);

            Assert.AreEqual(1.927, measurements[1].Level);
            Assert.AreEqual("coyote_creek", measurements[1].Location);
            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)), measurements[1].Time);
        }

        [Test]
        public async Task WriteMeasurementsWithoutFields()
        {
            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);

            var bucketName = _bucket.Name;

            var time = DateTime.UtcNow;

            var measurement1 = new H20Measurement
            {
                Location = "coyote_creek", Level = 2.927, Time = time.Add(-TimeSpan.FromSeconds(30))
            };
            var measurement2 = new H20Measurement
            {
                Location = "coyote_creek", Level = null, Time = time
            };

            _writeApi.WriteMeasurements(new[] { measurement1, measurement2 }, WritePrecision.S, bucketName,
                _organization.Id);
            _writeApi.Flush();
            listener.WaitToSuccess();

            var measurements = await _queryApi.QueryAsync<H20Measurement>(
                $"from(bucket:\"{bucketName}\") |> range(start: 0) |> rename(columns:{{_value: \"level\"}})",
                _organization.Id);

            Assert.AreEqual(1, measurements.Count);

            Assert.AreEqual(2.927, measurements[0].Level);
            Assert.AreEqual("coyote_creek", measurements[0].Location);
            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)).Add(-TimeSpan.FromSeconds(30)),
                measurements[0].Time);
        }

        [Test]
        public async Task WritePoints()
        {
            var bucketName = _bucket.Name;

            var time = DateTime.UtcNow;

            var point1 = PointData
                .Measurement("h2o_feet")
                .Tag("location", "west")
                .Field("water_level", 1)
                .Timestamp(time, WritePrecision.S);

            var point2 = PointData
                .Measurement("h2o_feet").Tag("location", "west")
                .Field("water_level", 2)
                .Timestamp(time.AddSeconds(-10), WritePrecision.S);

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WritePoints(new[] { point1, point2 }, bucketName, _organization.Id);
            _writeApi.Flush();

            listener.WaitToSuccess();

            var query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);

            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(2, records.Count);

            Assert.AreEqual("h2o_feet", records[0].GetMeasurement());
            Assert.AreEqual(2, records[0].GetValue());
            Assert.AreEqual("water_level", records[0].GetField());

            Assert.AreEqual("h2o_feet", records[1].GetMeasurement());
            Assert.AreEqual(1, records[1].GetValue());
            Assert.AreEqual("water_level", records[1].GetField());

            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)).Add(-TimeSpan.FromSeconds(10)),
                records[0].GetTimeInDateTime());
            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)), records[1].GetTimeInDateTime());
        }

        [Test]
        public async Task WritePointsWithoutFields()
        {
            var bucketName = _bucket.Name;

            var time = DateTime.UtcNow;

            var point1 = PointData
                .Measurement("h2o_feet")
                .Tag("location", "west")
                .Timestamp(time, WritePrecision.S);

            var point2 = PointData
                .Measurement("h2o_feet").Tag("location", "west")
                .Field("water_level", 2)
                .Timestamp(time.AddSeconds(-10), WritePrecision.S);

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WritePoints(new[] { point1, point2 }, bucketName, _organization.Id);
            _writeApi.Flush();

            listener.WaitToSuccess();

            var query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 0)",
                _organization.Id);

            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(1, records.Count);

            Assert.AreEqual("h2o_feet", records[0].GetMeasurement());
            Assert.AreEqual(2, records[0].GetValue());
            Assert.AreEqual("water_level", records[0].GetField());

            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)).Add(TimeSpan.FromSeconds(-10)),
                records[0].GetTimeInDateTime());
        }

        [Test]
        public async Task WriteQueryWithDefaultOrgBucket()
        {
            var time = DateTime.UtcNow;

            var point1 = PointData
                .Measurement("h2o_feet")
                .Tag("location", "west")
                .Field("water_level", 1)
                .Timestamp(time, WritePrecision.S);

            var point2 = PointData
                .Measurement("h2o_feet").Tag("location", "west")
                .Field("water_level", 2)
                .Timestamp(time.AddSeconds(-10), WritePrecision.S);

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WritePoints(new[] { point1, point2 });
            _writeApi.Flush();

            listener.WaitToSuccess();

            var query = await _queryApi.QueryAsync(
                $"from(bucket:\"{_bucket.Name}\") |> range(start: 1970-01-01T00:00:00.000000001Z)");

            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(2, records.Count);

            Assert.AreEqual("h2o_feet", records[0].GetMeasurement());
            Assert.AreEqual(2, records[0].GetValue());
            Assert.AreEqual("water_level", records[0].GetField());

            Assert.AreEqual("h2o_feet", records[1].GetMeasurement());
            Assert.AreEqual(1, records[1].GetValue());
            Assert.AreEqual("water_level", records[1].GetField());

            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)).Add(-TimeSpan.FromSeconds(10)),
                records[0].GetTimeInDateTime());
            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)), records[1].GetTimeInDateTime());
        }

        [Test]
        public async Task WriteRecordsList()
        {
            var bucketName = _bucket.Name;

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WriteRecords(new List<string> { record1, record2 }, WritePrecision.Ns, bucketName,
                _organization.Id);
            _writeApi.Flush();
            listener.WaitToSuccess();

            var query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);

            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(2, records.Count);

            Assert.AreEqual("h2o_feet", records[0].GetMeasurement());
            Assert.AreEqual(1, records[0].GetValue());
            Assert.AreEqual("level water_level", records[0].GetField());

            Assert.AreEqual("h2o_feet", records[1].GetMeasurement());
            Assert.AreEqual(2, records[1].GetValue());
            Assert.AreEqual("level water_level", records[1].GetField());
        }

        [Test]
        public async Task WriteRecordsParams()
        {
            var bucketName = _bucket.Name;

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";

            _writeApi = Client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(_writeApi);
            _writeApi.WriteRecords(new[] { record1, record2 }, org: _organization.Id, bucket: bucketName);
            _writeApi.Flush();
            listener.WaitToSuccess();

            var query = await _queryApi.QueryAsync(
                $"from(bucket:\"{bucketName}\") |> range(start: 1970-01-01T00:00:00.000000001Z)",
                _organization.Id);

            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(2, records.Count);

            Assert.AreEqual("h2o_feet", records[0].GetMeasurement());
            Assert.AreEqual(1, records[0].GetValue());
            Assert.AreEqual("level water_level", records[0].GetField());

            Assert.AreEqual("h2o_feet", records[1].GetMeasurement());
            Assert.AreEqual(2, records[1].GetValue());
            Assert.AreEqual("level water_level", records[1].GetField());
        }

        [Test]
        public async Task WriteTooManyData()
        {
            _writeApi?.Dispose();

            const int count = 500_000;
            const int batchSize = 50_000;

            var measurements = new List<H20Measurement>();

            for (var i = 0; i < count; i++)
                measurements.Add(new H20Measurement
                    { Level = i, Time = DateTime.UnixEpoch.Add(TimeSpan.FromSeconds(i)), Location = "Europe" });

            var successEvents = new List<WriteSuccessEvent>();
            _writeApi = Client.GetWriteApi(WriteOptions.CreateNew().BatchSize(batchSize).FlushInterval(10_000).Build());
            _writeApi.EventHandler += (sender, args) => { successEvents.Add(args as WriteSuccessEvent); };

            var start = 0;
            for (;;)
            {
                var history = measurements.Skip(start).Take(batchSize).ToArray();
                if (history.Length == 0)
                {
                    break;
                }

                if (start != 0)
                {
                    Trace.WriteLine("Delaying...");
                    await Task.Delay(100);
                }

                start += batchSize;
                Trace.WriteLine(
                    $"Add measurement to buffer From: {history.First().Time}, To: {history.Last().Time}. Remaining {count - start}");
                _writeApi.WriteMeasurements(history, WritePrecision.S, _bucket.Name, _organization.Name);
            }

            Trace.WriteLine("Flushing data...");
            Client.Dispose();
            Trace.WriteLine("Finished");

            Assert.AreEqual(10, successEvents.Count);
            foreach (var successEvent in successEvents)
                Assert.AreEqual(50_000, successEvent.LineProtocol.Split("\n").Length);
        }

        [Test]
        public async Task GzipWithLargeAmountOfData()
        {
            Client.EnableGzip();

            var records = new List<string>();
            for (var i = 0; i < 1000; i++) records.Add($"mem{i},tag=a value={i}i {i}");
            await Client.GetWriteApiAsync().WriteRecordsAsync(records);

            var tables = await _queryApi.QueryAsync(
                $"from(bucket:\"{_bucket.Name}\") |> range(start: 0)");

            Assert.AreEqual(1000, tables.Count);
            Assert.AreEqual(1, tables[0].Records.Count);
        }
    }
}