using System.Collections.Generic;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    public class ItBucketClientTest : AbstractItClientTest
    {
        private OrganizationClient _organizationClient;
        private BucketClient _bucketClient;
        private Organization _organization;

        [SetUp]
        public new async Task SetUp()
        {
            _organizationClient = PlatformClient.CreateOrganizationClient();
            _bucketClient = PlatformClient.CreateBucketClient();

            _organization = await _organizationClient.CreateOrganization(GenerateName("Org"));
        }

        [Test]
        public async Task CreateBucket()
        {
            string bucketName = GenerateName("robot sensor");

            Bucket bucket = await _bucketClient.CreateBucket(bucketName, RetentionRule(), _organization);

            Assert.IsNotNull(bucket);
            Assert.IsNotEmpty(bucket.Id);
            Assert.AreEqual(bucket.Name, bucketName);
            Assert.AreEqual(bucket.OrganizationId, _organization.Id);
            Assert.AreEqual(bucket.OrganizationName, _organization.Name);
            Assert.AreEqual(bucket.RetentionRules.Count, 1);
            Assert.AreEqual(bucket.RetentionRules[0].EverySeconds, 3600L);
            Assert.AreEqual(bucket.RetentionRules[0].Type, "expire");
            Assert.AreEqual(bucket.Links.Count, 3);
            Assert.AreEqual(bucket.Links["org"], $"/api/v2/orgs/{_organization.Id}");
            Assert.AreEqual(bucket.Links["self"], $"/api/v2/buckets/{bucket.Id}");
            Assert.AreEqual(bucket.Links["log"], $"/api/v2/buckets/{bucket.Id}/log");
        }

        [Test]
        public async Task CreateBucketWithoutRetentionRule()
        {
            string bucketName = GenerateName("robot sensor");

            Bucket bucket = await _bucketClient.CreateBucket(bucketName, _organization);

            Assert.IsNotNull(bucket);
            Assert.IsNotEmpty(bucket.Id);
            Assert.AreEqual(bucket.Name, bucketName);
            Assert.AreEqual(bucket.OrganizationId, _organization.Id);
            Assert.AreEqual(bucket.RetentionRules.Count, 0);
        }

        [Test]
        public async Task UpdateBucket()
        {
            Bucket createBucket =
                await _bucketClient.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);
            createBucket.Name = "Therm sensor 2000";
            createBucket.RetentionRules[0].EverySeconds = 1000L;

            Bucket updatedBucket = await _bucketClient.UpdateBucket(createBucket);

            Assert.IsNotNull(updatedBucket);
            Assert.IsNotEmpty(updatedBucket.Id);
            Assert.AreEqual(updatedBucket.Id, createBucket.Id);
            Assert.AreEqual(updatedBucket.Name, "Therm sensor 2000");
            Assert.AreEqual(updatedBucket.OrganizationId, createBucket.OrganizationId);
            Assert.AreEqual(updatedBucket.OrganizationName, createBucket.OrganizationName);
            Assert.AreEqual(updatedBucket.RetentionRules[0].EverySeconds, 1000L);
        }

        [Test]
        public async Task DeleteBucket()
        {
            Bucket createBucket =
                await _bucketClient.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);
            Assert.IsNotNull(createBucket);

            Bucket foundBucket = await _bucketClient.FindBucketById(createBucket.Id);
            Assert.IsNotNull(foundBucket);

            // delete task
            await _bucketClient.DeleteBucket(createBucket);

            foundBucket = await _bucketClient.FindBucketById(createBucket.Id);
            Assert.IsNull(foundBucket);
        }

        [Test]
        public async Task FindBucketById()
        {
            string bucketName = GenerateName("robot sensor");

            Bucket bucket = await _bucketClient.CreateBucket(bucketName, RetentionRule(), _organization);

            Bucket bucketById = await _bucketClient.FindBucketById(bucket.Id);
            
            Assert.IsNotNull(bucketById);
            Assert.AreEqual(bucketById.Id, bucket.Id);
            Assert.AreEqual(bucketById.Name, bucket.Name);
            Assert.AreEqual(bucketById.OrganizationId, bucket.OrganizationId);
            Assert.AreEqual(bucketById.OrganizationName, bucket.OrganizationName);
            Assert.AreEqual(bucketById.RetentionRules.Count, bucket.RetentionRules.Count);
            Assert.AreEqual(bucketById.Links.Count, bucket.Links.Count);
        }

        [Test]
        public async Task FindBucketByIdNull()
        {
            Bucket bucket = await _bucketClient.FindBucketById("020f755c3c082000");

            Assert.IsNull(bucket);
        }
        
        [Test]
        public async Task FindBuckets() {

            int size = (await _bucketClient.FindBuckets()).Count;

            await _bucketClient.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            Organization organization2 = await _organizationClient.CreateOrganization(GenerateName("Second"));
            await _bucketClient.CreateBucket(GenerateName("robot sensor"), organization2.Name);

            List<Bucket> buckets = await _bucketClient.FindBuckets();
            Assert.AreEqual(buckets.Count, size + 2);
        }

        [Test]
        public async Task FindBucketsByOrganization() {

            Assert.AreEqual((await  _bucketClient.FindBucketsByOrganization(_organization)).Count, 0);

            await _bucketClient.CreateBucket(GenerateName("robot sensor"), _organization);

            Organization organization2 = await _organizationClient.CreateOrganization(GenerateName("Second"));
            await _bucketClient.CreateBucket(GenerateName("robot sensor"), organization2);

            Assert.AreEqual((await  _bucketClient.FindBucketsByOrganization(_organization)).Count, 1);
        }

        private static RetentionRule RetentionRule()
        {
            return new RetentionRule {Type = "expire", EverySeconds = 3600L};
        }
    }
}