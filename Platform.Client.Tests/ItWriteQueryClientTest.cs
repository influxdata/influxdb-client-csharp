using System;
using System.Collections.Generic;
using System.Threading;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using InfluxData.Platform.Client.Option;
using NodaTime;
using NUnit.Framework;
using Platform.Common.Flux.Domain;
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
    }
}