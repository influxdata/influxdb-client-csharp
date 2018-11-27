using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    public class ItBucketClientTest: AbstractItClientTest
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
        public async Task CreateBucket() {

            string bucketName = GenerateName("robot sensor");

            var retentionRule = new RetentionRule {Type = "expire", EverySeconds = 3600L};

            Bucket bucket = await _bucketClient.CreateBucket(bucketName, retentionRule, _organization);

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
    }
}