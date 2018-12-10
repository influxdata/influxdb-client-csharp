using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using InfluxData.Platform.Client.Option;
using InfluxData.Platform.Client.Write;
using NodaTime;
using NUnit.Framework;
using Platform.Common.Flux.Domain;
using Platform.Common.Platform;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    public class ItWriteQueryClientTest : AbstractItClientTest
    {
        private Bucket _bucket;
        private QueryClient _queryClient;
        private WriteClient _writeClient;

        [SetUp]
        public new async Task SetUp()
        {
            var retention = new RetentionRule {Type = "expire", EverySeconds = 3600L};
            
            _bucket = await PlatformClient.CreateBucketClient()
                .CreateBucket(GenerateName("h2o"), retention, "my-org");

            //
            // Add Permissions to read and write to the Bucket
            //
            String bucketResource = Permission.BucketResource(_bucket.Id);

            Permission readBucket = new Permission {Resource = bucketResource, Action = Permission.ReadAction};
            Permission writeBucket = new Permission {Resource = bucketResource, Action = Permission.WriteAction};

            User loggedUser = await PlatformClient.CreateUserClient().Me();
            Assert.IsNotNull(loggedUser);

            Authorization authorization =  await PlatformClient.CreateAuthorizationClient()
                .CreateAuthorization(loggedUser, new List<Permission> {readBucket, writeBucket});

            String token = authorization.Token;

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
            String bucketName = _bucket.Name;

            string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";

            _writeClient = PlatformClient.CreateWriteClient();
            _writeClient.WriteRecords(bucketName, "my-org", TimeUnit.Nanos, new List<string>{record1, record2});
            _writeClient.Flush();
            
            List<FluxTable> query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0)", "my-org");

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
            String bucketName = _bucket.Name;

            string record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            string record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";

            _writeClient = PlatformClient.CreateWriteClient();
            _writeClient.WriteRecords(bucketName, "my-org", TimeUnit.Nanos, record1, record2);
            _writeClient.Flush();
            
            List<FluxTable> query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0)", "my-org");

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
        public async Task WritePoints() {

            String bucketName = _bucket.Name;

            var time = DateTime.UtcNow;

            Point point1 = Point
                .Measurement("h2o_feet")
                .Tag("location", "west")
                .Field("water_level", 1)
                .Timestamp(time, TimeUnit.Seconds);
            
            Point point2 = Point
                .Measurement("h2o_feet").Tag("location", "west")
                .Field("water_level", 2)
                .Timestamp(time.AddSeconds(-10), TimeUnit.Seconds);
            
            _writeClient = PlatformClient.CreateWriteClient();
            _writeClient.WritePoints(bucketName, "my-org", point1, point2);
            _writeClient.Flush();
            Thread.Sleep(1000);

            List<FluxTable> query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0)", "my-org");

            Assert.AreEqual(1, query.Count);
            
            var records = query[0].Records;
            Assert.AreEqual(2, records.Count);
            
            Assert.AreEqual("h2o_feet", records[0].GetMeasurement());
            Assert.AreEqual(2, records[0].GetValue());
            Assert.AreEqual("water_level", records[0].GetField());

            Assert.AreEqual("h2o_feet", records[1].GetMeasurement());
            Assert.AreEqual(1, records[1].GetValue());
            Assert.AreEqual("water_level", records[1].GetField());

            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)).Add(-TimeSpan.FromSeconds(10)), records[0].GetTimeInDateTime());
            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)), records[1].GetTimeInDateTime());
        }

        [Test]
        public async Task WriteMeasurements()
        {
            _writeClient = PlatformClient.CreateWriteClient();
            
            String bucketName = _bucket.Name;

            var time = DateTime.UtcNow;

            var measurement1 = new H20Measurement
            {
                Location = "coyote_creek", Level = 2.927, Time = time.Add(-TimeSpan.FromSeconds(30))
            };
            var measurement2 = new H20Measurement
            {
                Location = "coyote_creek", Level = 1.927, Time = time
            };
            
            _writeClient.WriteMeasurements(bucketName, "my-org", TimeUnit.Seconds, measurement1, measurement2);
            _writeClient.Flush();
            
            List<H20Measurement> measurements = await _queryClient.Query<H20Measurement>("from(bucket:\"" + bucketName + "\") |> range(start: 0) |> rename(columns:{_value: \"level\"})", "my-org");
            
            Assert.AreEqual(2, measurements.Count);
            
            Assert.AreEqual(2.927, measurements[0].Level);
            Assert.AreEqual("coyote_creek", measurements[0].Location);
            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)).Add(-TimeSpan.FromSeconds(30)), measurements[0].Time);
            
            Assert.AreEqual(1.927, measurements[1].Level);
            Assert.AreEqual("coyote_creek", measurements[1].Location);
            Assert.AreEqual(time.AddTicks(-(time.Ticks % TimeSpan.TicksPerSecond)), measurements[1].Time);
        }
        
        [Test]
        public async Task Flush() {

            String bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(10).FlushInterval(100_000).Build();
            
            _writeClient = PlatformClient.CreateWriteClient(writeOptions);

            String record = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";

            _writeClient.WriteRecord(bucketName, "my-org", TimeUnit.Nanos, record);
            _writeClient.Flush();
            
            List<FluxTable> query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0) |> last()", "my-org");

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
        public async Task FlushByTime() {

            String bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(10).FlushInterval(500).Build();
            
            _writeClient = PlatformClient.CreateWriteClient(writeOptions);

            String record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            String record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";
            String record3 = "h2o_feet,location=coyote_creek level\\ water_level=3.0 3";
            String record4 = "h2o_feet,location=coyote_creek level\\ water_level=4.0 4";
            String record5 = "h2o_feet,location=coyote_creek level\\ water_level=5.0 5";

            _writeClient.WriteRecord(bucketName, "my-org", TimeUnit.Nanos, record1);
            _writeClient.WriteRecord(bucketName, "my-org", TimeUnit.Nanos, record2);
            _writeClient.WriteRecord(bucketName, "my-org", TimeUnit.Nanos, record3);
            _writeClient.WriteRecord(bucketName, "my-org", TimeUnit.Nanos, record4);
            _writeClient.WriteRecord(bucketName, "my-org", TimeUnit.Nanos, record5);

            List<FluxTable> query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0)", "my-org");
            Assert.AreEqual(0, query.Count);
            
            Thread.Sleep(550);
            
            query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0)", "my-org");
            Assert.AreEqual(1, query.Count);
            
            var records = query[0].Records;
            Assert.AreEqual(5, records.Count);
        }
        
        [Test]
        public async Task FlushByCount() {

            String bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(6).FlushInterval(500_000).Build();
            
            _writeClient = PlatformClient.CreateWriteClient(writeOptions);

            String record1 = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";
            String record2 = "h2o_feet,location=coyote_creek level\\ water_level=2.0 2";
            String record3 = "h2o_feet,location=coyote_creek level\\ water_level=3.0 3";
            String record4 = "h2o_feet,location=coyote_creek level\\ water_level=4.0 4";
            String record5 = "h2o_feet,location=coyote_creek level\\ water_level=5.0 5";
            String record6 = "h2o_feet,location=coyote_creek level\\ water_level=6.0 6";

            _writeClient.WriteRecord(bucketName, "my-org", TimeUnit.Nanos, record1);
            _writeClient.WriteRecord(bucketName, "my-org", TimeUnit.Nanos, record2);
            _writeClient.WriteRecord(bucketName, "my-org", TimeUnit.Nanos, record3);
            _writeClient.WriteRecord(bucketName, "my-org", TimeUnit.Nanos, record4);
            _writeClient.WriteRecord(bucketName, "my-org", TimeUnit.Nanos, record5);

            List<FluxTable> query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0)", "my-org");
            Assert.AreEqual(0, query.Count);
            
            _writeClient.WriteRecord(bucketName, "my-org", TimeUnit.Nanos, record6);

            Thread.Sleep(10);
            
            query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0)", "my-org");
            Assert.AreEqual(1, query.Count);
            
            var records = query[0].Records;
            Assert.AreEqual(6, records.Count);
        }

        [Test]
        public async Task Jitter()
        {
            String bucketName = _bucket.Name;

            var writeOptions = WriteOptions.CreateNew().BatchSize(1).JitterInterval(5_000).Build();
            
            _writeClient = PlatformClient.CreateWriteClient(writeOptions);

            String record = "h2o_feet,location=coyote_creek level\\ water_level=1.0 1";

            _writeClient.WriteRecord(bucketName, "my-org", TimeUnit.Nanos, record);

            List<FluxTable> query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0) |> last()", "my-org");
            Assert.AreEqual(0, query.Count);
            
            Thread.Sleep(5_000);
            
            query = await _queryClient.Query("from(bucket:\"" + bucketName + "\") |> range(start: 0) |> last()", "my-org");

            Assert.AreEqual(1, query.Count);
        }
        
        [Measurement("h2o")]
        private class H20Measurement
        {
            [Column("location", IsTag = true)] 
            public string Location { get; set; }

            [Column("level")]
            public Double Level { get; set; }
 
            [Column(IsTimestamp = true)]
            public DateTime Time { get; set; }
        }
    }
}