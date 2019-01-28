using System;
using System.Collections.Generic;
using System.Threading;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Client.Event;
using InfluxData.Platform.Client.Domain;
using InfluxData.Platform.Client.Option;
using InfluxData.Platform.Client.Write;
using NodaTime;
using NUnit.Framework;
using Platform.Common.Platform;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    [TestFixture]
    public class ItWriteQueryClientTest : AbstractItClientTest
    {
        private Bucket _bucket;
        private QueryClient _queryClient;
        private WriteClient _writeClient;
        private Organization _organization;

        [SetUp]
        public new async Task SetUp()
        {
            _organization = await FindMyOrg();

            var retention = new RetentionRule {Type = "expire", EverySeconds = 3600L};

            _bucket = await PlatformClient.CreateBucketClient()
                .CreateBucket(GenerateName("h2o"), retention, _organization);

            //
            // Add Permissions to read and write to the Bucket
            //
            var resource = new PermissionResource
                {Type = ResourceType.Buckets, OrgId = _organization.Id, Id = _bucket.Id};
            
            var readBucket = new Permission
            {
                Resource = resource,
                Action = Permission.ReadAction
            };
            var writeBucket = new Permission
            {
                Resource = resource,
                Action = Permission.WriteAction
            };

            var loggedUser = await PlatformClient.CreateUserClient().Me();
            Assert.IsNotNull(loggedUser);

            var authorization = await PlatformClient.CreateAuthorizationClient()
                .CreateAuthorization(await FindMyOrg(), new List<Permission> {readBucket, writeBucket});

            var token = authorization.Token;

            PlatformClient.Dispose();
            PlatformClient = PlatformClientFactory.Create(PlatformUrl, token.ToCharArray());
            _queryClient = PlatformClient.CreateQueryClient();
        }

        [TearDown]
        protected new void After()
        {
            _writeClient.Dispose();
        }

        [Test]
        public async Task WriteRecordsList()
        {
            var bucketName = _bucket.Name;

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";

            _writeClient = PlatformClient.CreateWriteClient();
            _writeClient.WriteRecords(bucketName, _organization.Id, TimeUnit.Nanos,
                new List<string> {record1, record2});
            _writeClient.Flush();

            var query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0)",
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

            _writeClient = PlatformClient.CreateWriteClient();
            _writeClient.WriteRecords(bucketName, _organization.Id, TimeUnit.Nanos, record1, record2);
            _writeClient.Flush();

            var query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0)",
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
        public async Task WritePoints()
        {
            var bucketName = _bucket.Name;

            var time = DateTime.UtcNow;

            var point1 = Point
                .Measurement("h2o_feet")
                .Tag("location", "west")
                .Field("water_level", 1)
                .Timestamp(time, TimeUnit.Seconds);

            var point2 = Point
                .Measurement("h2o_feet").Tag("location", "west")
                .Field("water_level", 2)
                .Timestamp(time.AddSeconds(-10), TimeUnit.Seconds);

            _writeClient = PlatformClient.CreateWriteClient();
            _writeClient.WritePoints(bucketName, _organization.Id, point1, point2);
            _writeClient.Flush();
            Thread.Sleep(1000);

            var query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0)",
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
        public async Task WriteMeasurements()
        {
            _writeClient = PlatformClient.CreateWriteClient();

            var bucketName = _bucket.Name;

            var time = DateTime.UtcNow;

            var measurement1 = new H20Measurement
            {
                Location = "coyote_creek", Level = 2.927, Time = time.Add(-TimeSpan.FromSeconds(30))
            };
            var measurement2 = new H20Measurement
            {
                Location = "coyote_creek", Level = 1.927, Time = time
            };

            _writeClient.WriteMeasurements(bucketName, _organization.Id, TimeUnit.Seconds, measurement1, measurement2);
            _writeClient.Flush();

            var measurements = await _queryClient.Query<H20Measurement>(
                "from(bucket:\"" + bucketName + "\") |> range(start: 0) |> rename(columns:{_value: \"level\"})",
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
        public async Task Flush()
        {
            var bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(10).FlushInterval(100_000).Build();

            _writeClient = PlatformClient.CreateWriteClient(writeOptions);

            var record = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";

            _writeClient.WriteRecord(bucketName, _organization.Id, TimeUnit.Nanos, record);
            _writeClient.Flush();

            var query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0) |> last()",
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
        public async Task FlushByTime()
        {
            var bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(10).FlushInterval(500).Build();

            _writeClient = PlatformClient.CreateWriteClient(writeOptions);

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";
            const string record3 = "h2o_feet,location=coyote_creek level\\ water_level=3.0 3";
            const string record4 = "h2o_feet,location=coyote_creek level\\ water_level=4.0 4";
            const string record5 = "h2o_feet,location=coyote_creek level\\ water_level=5.0 5";

            _writeClient.WriteRecord(bucketName, _organization.Id, TimeUnit.Nanos, record1);
            _writeClient.WriteRecord(bucketName, _organization.Id, TimeUnit.Nanos, record2);
            _writeClient.WriteRecord(bucketName, _organization.Id, TimeUnit.Nanos, record3);
            _writeClient.WriteRecord(bucketName, _organization.Id, TimeUnit.Nanos, record4);
            _writeClient.WriteRecord(bucketName, _organization.Id, TimeUnit.Nanos, record5);

            var query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0)",
                _organization.Id);
            Assert.AreEqual(0, query.Count);

            Thread.Sleep(550);

            query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0)",
                _organization.Id);
            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(5, records.Count);
        }

        [Test]
        public async Task FlushByCount()
        {
            var bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(6).FlushInterval(500_000).Build();

            _writeClient = PlatformClient.CreateWriteClient(writeOptions);

            const string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            const string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";
            const string record3 = "h2o_feet,location=coyote_creek level\\ water_level=3.0 3";
            const string record4 = "h2o_feet,location=coyote_creek level\\ water_level=4.0 4";
            const string record5 = "h2o_feet,location=coyote_creek level\\ water_level=5.0 5";
            const string record6 = "h2o_feet,location=coyote_creek level\\ water_level=6.0 6";

            _writeClient.WriteRecord(bucketName, _organization.Id, TimeUnit.Nanos, record1);
            _writeClient.WriteRecord(bucketName, _organization.Id, TimeUnit.Nanos, record2);
            _writeClient.WriteRecord(bucketName, _organization.Id, TimeUnit.Nanos, record3);
            _writeClient.WriteRecord(bucketName, _organization.Id, TimeUnit.Nanos, record4);
            _writeClient.WriteRecord(bucketName, _organization.Id, TimeUnit.Nanos, record5);

            var query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0)",
                _organization.Id);
            Assert.AreEqual(0, query.Count);

            _writeClient.WriteRecord(bucketName, _organization.Id, TimeUnit.Nanos, record6);

            Thread.Sleep(10);

            query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0)",
                _organization.Id);
            Assert.AreEqual(1, query.Count);

            var records = query[0].Records;
            Assert.AreEqual(6, records.Count);
        }

        [Test]
        public async Task Jitter()
        {
            var bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(1).JitterInterval(5_000).Build();

            _writeClient = PlatformClient.CreateWriteClient(writeOptions);

            var record = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";

            _writeClient.WriteRecord(bucketName, _organization.Id, TimeUnit.Nanos, record);

            var query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0) |> last()",
                _organization.Id);
            Assert.AreEqual(0, query.Count);

            Thread.Sleep(5_000);

            query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0) |> last()",
                _organization.Id);

            Assert.AreEqual(1, query.Count);
        }

        [Test]
        public void ListenWriteSuccessEvent()
        {
            var bucketName = _bucket.Name;

            WriteSuccessEvent success = null;

            _writeClient = PlatformClient.CreateWriteClient();
            _writeClient.EventHandler += (sender, args) => { success = args as WriteSuccessEvent; };

            _writeClient.WriteRecord(bucketName, _organization.Id, TimeUnit.Nanos,
                "h2o_feet,location=coyote_creek level\\ water_level=1.0 1");
            _writeClient.Flush();

            Thread.Sleep(100);

            Assert.IsNotNull(success);
            Assert.AreEqual(success.Organization, _organization.Id);
            Assert.AreEqual(success.Bucket, bucketName);
            Assert.AreEqual(success.Precision, TimeUnit.Nanos);
            Assert.AreEqual(success.LineProtocol, "h2o_feet,location=coyote_creek level\\ water_level=1.0 1");
        }

        [Test]
        public void ListenWriteErrorEvent()
        {
            var bucketName = _bucket.Name;

            WriteErrorEvent error = null;

            _writeClient = PlatformClient.CreateWriteClient();
            _writeClient.EventHandler += (sender, args) => { error = args as WriteErrorEvent; };

            _writeClient.WriteRecord(bucketName, _organization.Id, TimeUnit.Nanos,
                "h2o_feet,location=coyote_creek level\\ water_level=1.0 123456.789");
            _writeClient.Flush();

            Thread.Sleep(100);

            Assert.IsNotNull(error);
            Assert.AreEqual("unable to parse points: unable to parse 'h2o_feet,location=coyote_creek level\\ water_level=1.0 123456.789': bad timestamp", error.Exception.Message);
        }

        [Measurement("h2o")]
        private class H20Measurement
        {
            [Column("location", IsTag = true)] public string Location { get; set; }

            [Column("level")] public Double Level { get; set; }

            [Column(IsTimestamp = true)] public DateTime Time { get; set; }
        }
    }
}