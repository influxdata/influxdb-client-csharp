using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    [TestFixture]
    public class ItBucketClientTest : AbstractItClientTest
    {
        private OrganizationClient _organizationClient;
        private BucketClient _bucketClient;
        private UserClient _userClient;
        
        private Organization _organization;

        [SetUp]
        public new async Task SetUp()
        {
            _organizationClient = PlatformClient.CreateOrganizationClient();
            _bucketClient = PlatformClient.CreateBucketClient();
            _userClient = PlatformClient.CreateUserClient();

            _organization = await _organizationClient.CreateOrganization(GenerateName("Org"));
        }

        [Test]
        public async Task CreateBucket()
        {
            var bucketName = GenerateName("robot sensor");

            var bucket = await _bucketClient.CreateBucket(bucketName, RetentionRule(), _organization);

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
            var bucketName = GenerateName("robot sensor");

            var bucket = await _bucketClient.CreateBucket(bucketName, _organization);

            Assert.IsNotNull(bucket);
            Assert.IsNotEmpty(bucket.Id);
            Assert.AreEqual(bucket.Name, bucketName);
            Assert.AreEqual(bucket.OrganizationId, _organization.Id);
            Assert.AreEqual(bucket.RetentionRules.Count, 0);
        }

        [Test]
        public async Task UpdateBucket()
        {
            var createBucket =
                await _bucketClient.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);
            createBucket.Name = "Therm sensor 2000";
            createBucket.RetentionRules[0].EverySeconds = 1000L;

            var updatedBucket = await _bucketClient.UpdateBucket(createBucket);

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
            var createBucket =
                await _bucketClient.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);
            Assert.IsNotNull(createBucket);

            var foundBucket = await _bucketClient.FindBucketById(createBucket.Id);
            Assert.IsNotNull(foundBucket);

            // delete task
            await _bucketClient.DeleteBucket(createBucket);

            foundBucket = await _bucketClient.FindBucketById(createBucket.Id);
            Assert.IsNull(foundBucket);
        }

        [Test]
        public async Task FindBucketById()
        {
            var bucketName = GenerateName("robot sensor");

            var bucket = await _bucketClient.CreateBucket(bucketName, RetentionRule(), _organization);

            var bucketById = await _bucketClient.FindBucketById(bucket.Id);
            
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
            var bucket = await _bucketClient.FindBucketById("020f755c3c082000");

            Assert.IsNull(bucket);
        }
        
        [Test]
        public async Task FindBuckets() {

            var size = (await _bucketClient.FindBuckets()).Count;

            await _bucketClient.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            var organization2 = await _organizationClient.CreateOrganization(GenerateName("Second"));
            await _bucketClient.CreateBucket(GenerateName("robot sensor"), organization2.Name);

            var buckets = await _bucketClient.FindBuckets();
            Assert.AreEqual(buckets.Count, size + 2);
        }

        [Test]
        public async Task FindBucketsByOrganization() {

            Assert.AreEqual((await  _bucketClient.FindBucketsByOrganization(_organization)).Count, 0);

            await _bucketClient.CreateBucket(GenerateName("robot sensor"), _organization);

            var organization2 = await _organizationClient.CreateOrganization(GenerateName("Second"));
            await _bucketClient.CreateBucket(GenerateName("robot sensor"), organization2);

            Assert.AreEqual((await  _bucketClient.FindBucketsByOrganization(_organization)).Count, 1);
        }
        
        [Test]
        public async Task Member() {

            var bucket = await _bucketClient.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            var members =  await _bucketClient.GetMembers(bucket);
            Assert.AreEqual(0, members.Count);

            var user = await _userClient.CreateUser(GenerateName("Luke Health"));

            var userResourceMapping = await _bucketClient.AddMember(user, bucket);
            Assert.IsNotNull(userResourceMapping);
            Assert.AreEqual(userResourceMapping.ResourceId, bucket.Id);
            Assert.AreEqual(userResourceMapping.ResourceType, ResourceType.BucketResourceType);
            Assert.AreEqual(userResourceMapping.UserId, user.Id);
            Assert.AreEqual(userResourceMapping.UserType, UserResourceMapping.MemberType.Member);

            members = await _bucketClient.GetMembers(bucket);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].ResourceId, bucket.Id);
            Assert.AreEqual(members[0].ResourceType, ResourceType.BucketResourceType);
            Assert.AreEqual(members[0].UserId, user.Id);
            Assert.AreEqual(members[0].UserType, UserResourceMapping.MemberType.Member);

            await _bucketClient.DeleteMember(user, bucket);

            members = await _bucketClient.GetMembers(bucket);
            Assert.AreEqual(0, members.Count);
        }
        
        [Test]
        public async Task Owner() {

            var bucket = await _bucketClient.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            var owners =  await _bucketClient.GetOwners(bucket);
            Assert.AreEqual(0, owners.Count);

            var user = await _userClient.CreateUser(GenerateName("Luke Health"));

            var userResourceMapping = await _bucketClient.AddOwner(user, bucket);
            Assert.IsNotNull(userResourceMapping);
            Assert.AreEqual(userResourceMapping.ResourceId, bucket.Id);
            Assert.AreEqual(userResourceMapping.ResourceType, ResourceType.BucketResourceType);
            Assert.AreEqual(userResourceMapping.UserId, user.Id);
            Assert.AreEqual(userResourceMapping.UserType, UserResourceMapping.MemberType.Owner);

            owners = await _bucketClient.GetOwners(bucket);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual(owners[0].ResourceId, bucket.Id);
            Assert.AreEqual(owners[0].ResourceType, ResourceType.BucketResourceType);
            Assert.AreEqual(owners[0].UserId, user.Id);
            Assert.AreEqual(owners[0].UserType, UserResourceMapping.MemberType.Owner);

            await _bucketClient.DeleteOwner(user, bucket);

            owners = await _bucketClient.GetOwners(bucket);
            Assert.AreEqual(0, owners.Count);
        }

        private static RetentionRule RetentionRule()
        {
            return new RetentionRule {Type = "expire", EverySeconds = 3600L};
        }
    }
}