using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Domain;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItBucketsApiTest : AbstractItClientTest
    {
        [SetUp]
        public new async Task SetUp()
        {
            _organizationsApi = Client.GetOrganizationsApi();
            _bucketsApi = Client.GetBucketsApi();
            _usersApi = Client.GetUsersApi();

            _organization = await _organizationsApi.CreateOrganizationAsync(GenerateName("Org"));

            foreach (var bucket in (await _bucketsApi.FindBucketsAsync()).Where(bucket => bucket.Name.EndsWith("-IT")))
                await _bucketsApi.DeleteBucketAsync(bucket);
        }

        private OrganizationsApi _organizationsApi;
        private BucketsApi _bucketsApi;
        private UsersApi _usersApi;

        private Organization _organization;

        private static BucketRetentionRules RetentionRule()
        {
            return new BucketRetentionRules(BucketRetentionRules.TypeEnum.Expire, 3600);
        }

        [Test]
        public async Task CloneBucket()
        {
            var source =
                await _bucketsApi.CreateBucketAsync(GenerateName("robot sensor"), RetentionRule(), _organization);

            var properties = new Dictionary<string, string> { { "color", "green" }, { "location", "west" } };

            var label = await Client.GetLabelsApi()
                .CreateLabelAsync(GenerateName("Cool Resource"), properties, _organization.Id);
            await _bucketsApi.AddLabelAsync(label, source);

            var name = GenerateName("cloned");

            var cloned = await _bucketsApi.CloneBucketAsync(name, source);

            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual(_organization.Id, cloned.OrgID);
            Assert.IsNull(cloned.Rp);
            Assert.AreEqual(1, cloned.RetentionRules.Count);
            Assert.AreEqual(3600, cloned.RetentionRules[0].EverySeconds);
            Assert.AreEqual(BucketRetentionRules.TypeEnum.Expire, cloned.RetentionRules[0].Type);

            var labels = await _bucketsApi.GetLabelsAsync(cloned);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
        }

        [Test]
        public void CloneBucketNotFound()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _bucketsApi.CloneBucketAsync(GenerateName("bucket"), "020f755c3c082000"));

            Assert.AreEqual("bucket not found", ioe.Message);
        }

        [Test]
        public async Task CreateBucket()
        {
            var bucketName = GenerateName("robot sensor");

            var bucket = await _bucketsApi.CreateBucketAsync(bucketName, RetentionRule(), _organization);

            Assert.IsNotNull(bucket);
            Assert.IsNotEmpty(bucket.Id);
            Assert.AreEqual(bucket.Name, bucketName);
            Assert.AreEqual(bucket.OrgID, _organization.Id);
            Assert.AreEqual(bucket.RetentionRules.Count, 1);
            Assert.AreEqual(bucket.RetentionRules[0].EverySeconds, 3600L);
            Assert.AreEqual(bucket.RetentionRules[0].Type, BucketRetentionRules.TypeEnum.Expire);
            Assert.IsNotNull(bucket.Links);
            Assert.AreEqual(bucket.Links.Org, $"/api/v2/orgs/{_organization.Id}");
            Assert.AreEqual(bucket.Links.Self, $"/api/v2/buckets/{bucket.Id}");
            Assert.AreEqual(bucket.Links.Labels, $"/api/v2/buckets/{bucket.Id}/labels");
        }

        [Test]
        public async Task CreateBucketWithoutRetentionRule()
        {
            var bucketName = GenerateName("robot sensor");

            var bucket = await _bucketsApi.CreateBucketAsync(bucketName, _organization);

            Assert.IsNotNull(bucket);
            Assert.IsNotEmpty(bucket.Id);
            Assert.AreEqual(bucket.Name, bucketName);
            Assert.AreEqual(bucket.OrgID, _organization.Id);
        }

        [Test]
        public async Task DeleteBucket()
        {
            var createBucket =
                await _bucketsApi.CreateBucketAsync(GenerateName("robot sensor"), RetentionRule(), _organization);
            Assert.IsNotNull(createBucket);

            var foundBucket = await _bucketsApi.FindBucketByIdAsync(createBucket.Id);
            Assert.IsNotNull(foundBucket);

            // delete task
            await _bucketsApi.DeleteBucketAsync(createBucket);

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _bucketsApi.FindBucketByIdAsync(createBucket.Id));

            Assert.AreEqual("bucket not found", ioe.Message);
        }

        [Test]
        public async Task FindBucketById()
        {
            var bucketName = GenerateName("robot sensor");

            var bucket = await _bucketsApi.CreateBucketAsync(bucketName, RetentionRule(), _organization);

            var bucketById = await _bucketsApi.FindBucketByIdAsync(bucket.Id);

            Assert.IsNotNull(bucketById);
            Assert.AreEqual(bucketById.Id, bucket.Id);
            Assert.AreEqual(bucketById.Name, bucket.Name);
            Assert.AreEqual(bucketById.OrgID, bucket.OrgID);
            Assert.AreEqual(bucketById.RetentionRules.Count, bucket.RetentionRules.Count);
            Assert.AreEqual(bucketById.Links.Self, bucket.Links.Self);
        }

        [Test]
        public void FindBucketByIdNull()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _bucketsApi.FindBucketByIdAsync("020f755c3c082000"));

            Assert.AreEqual("bucket not found", ioe.Message);
        }

        [Test]
        public async Task FindBucketByName()
        {
            var bucket = await _bucketsApi.FindBucketByNameAsync("my-bucket");

            Assert.IsNotNull(bucket);
            Assert.AreEqual("my-bucket", bucket.Name);
            Assert.AreEqual((await FindMyOrg()).Id, bucket.OrgID);
        }

        [Test]
        public async Task FindBucketByNameNotFound()
        {
            var bucket = await _bucketsApi.FindBucketByNameAsync("my-bucket-not-found");

            Assert.IsNull(bucket);
        }

        [Test]
        [Ignore("TODO https://github.com/influxdata/influxdb/issues/14900")]
        public async Task FindBuckets()
        {
            var size = (await _bucketsApi.FindBucketsAsync()).Count;

            await _bucketsApi.CreateBucketAsync(GenerateName("robot sensor"), RetentionRule(), _organization);

            var organization2 = await _organizationsApi.CreateOrganizationAsync(GenerateName("Second"));
            await _bucketsApi.CreateBucketAsync(GenerateName("robot sensor"), organization2.Id);

            var buckets = await _bucketsApi.FindBucketsAsync();
            Assert.AreEqual(buckets.Count, size + 2);
        }

        [Test]
        public async Task FindBucketsByOrganization()
        {
            var organization2 = await _organizationsApi.CreateOrganizationAsync(GenerateName("Second"));
            Assert.AreEqual(2, (await _bucketsApi.FindBucketsByOrganizationAsync(organization2)).Count);

            await _bucketsApi.CreateBucketAsync(GenerateName("robot sensor"), organization2);
            Assert.AreEqual((await _bucketsApi.FindBucketsByOrganizationAsync(organization2)).Count, 3);
        }

        [Test]
        [Ignore("TODO https://github.com/influxdata/influxdb/issues/14900")]
        public async Task FindBucketsPaging()
        {
            foreach (var i in Enumerable.Range(0, 20 - (await _bucketsApi.FindBucketsAsync()).Count))
                await _bucketsApi.CreateBucketAsync(GenerateName($"{i}"), RetentionRule(), _organization);

            var findOptions = new FindOptions { Limit = 5 };

            var buckets = await _bucketsApi.FindBucketsAsync(findOptions);
            Assert.AreEqual(5, buckets._Buckets.Count);
            Assert.AreEqual("/api/v2/buckets?descending=false&limit=5&offset=5", buckets.Links.Next);

            buckets = await _bucketsApi.FindBucketsAsync(FindOptions.GetFindOptions(buckets.Links.Next));
            Assert.AreEqual(5, buckets._Buckets.Count);
            Assert.AreEqual("/api/v2/buckets?descending=false&limit=5&offset=10", buckets.Links.Next);

            buckets = await _bucketsApi.FindBucketsAsync(FindOptions.GetFindOptions(buckets.Links.Next));
            Assert.AreEqual(5, buckets._Buckets.Count);
            Assert.AreEqual("/api/v2/buckets?descending=false&limit=5&offset=15", buckets.Links.Next);

            buckets = await _bucketsApi.FindBucketsAsync(FindOptions.GetFindOptions(buckets.Links.Next));
            Assert.AreEqual(5, buckets._Buckets.Count);
            Assert.AreEqual("/api/v2/buckets?descending=false&limit=5&offset=20", buckets.Links.Next);

            buckets = await _bucketsApi.FindBucketsAsync(FindOptions.GetFindOptions(buckets.Links.Next));
            Assert.AreEqual(0, buckets._Buckets.Count);
            Assert.IsNull(buckets.Links.Next);
        }

        [Test]
        public async Task Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var bucket =
                await _bucketsApi.CreateBucketAsync(GenerateName("robot sensor"), RetentionRule(), _organization);

            var properties = new Dictionary<string, string> { { "color", "green" }, { "location", "west" } };

            var label = await labelClient.CreateLabelAsync(GenerateName("Cool Resource"), properties, _organization.Id);

            var labels = await _bucketsApi.GetLabelsAsync(bucket);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = await _bucketsApi.AddLabelAsync(label, bucket);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = await _bucketsApi.GetLabelsAsync(bucket);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            await _bucketsApi.DeleteLabelAsync(label, bucket);

            labels = await _bucketsApi.GetLabelsAsync(bucket);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        public async Task Member()
        {
            var bucket =
                await _bucketsApi.CreateBucketAsync(GenerateName("robot sensor"), RetentionRule(), _organization);

            var members = await _bucketsApi.GetMembersAsync(bucket);
            Assert.AreEqual(0, members.Count);

            var user = await _usersApi.CreateUserAsync(GenerateName("Luke Health"));

            var resourceMember = await _bucketsApi.AddMemberAsync(user, bucket);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.RoleEnum.Member);

            members = await _bucketsApi.GetMembersAsync(bucket);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].Id, user.Id);
            Assert.AreEqual(members[0].Name, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.RoleEnum.Member);

            await _bucketsApi.DeleteMemberAsync(user, bucket);

            members = await _bucketsApi.GetMembersAsync(bucket);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        [Ignore("TODO https://github.com/influxdata/influxdb/issues/17244")]
        public async Task Owner()
        {
            var bucket =
                await _bucketsApi.CreateBucketAsync(GenerateName("robot sensor"), RetentionRule(), _organization);

            var owners = await _bucketsApi.GetOwnersAsync(bucket);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual("my-user", owners[0].Name);

            var user = await _usersApi.CreateUserAsync(GenerateName("Luke Health"));

            var resourceMember = await _bucketsApi.AddOwnerAsync(user, bucket);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceOwner.RoleEnum.Owner);

            owners = await _bucketsApi.GetOwnersAsync(bucket);
            Assert.AreEqual(2, owners.Count);
            Assert.AreEqual(owners[1].Id, user.Id);
            Assert.AreEqual(owners[1].Name, user.Name);
            Assert.AreEqual(owners[1].Role, ResourceOwner.RoleEnum.Owner);

            await _bucketsApi.DeleteOwnerAsync(user, bucket);

            owners = await _bucketsApi.GetOwnersAsync(bucket);
            Assert.AreEqual(1, owners.Count);
        }

        [Test]
        [Ignore("https://github.com/influxdata/influxdb/issues/19518")]
        public async Task UpdateBucket()
        {
            var createBucket =
                await _bucketsApi.CreateBucketAsync(GenerateName("robot sensor"), RetentionRule(), _organization);
            createBucket.Name = "Therm sensor 2000";
            createBucket.RetentionRules[0].EverySeconds = 1000;

            var updatedBucket = await _bucketsApi.UpdateBucketAsync(createBucket);

            Assert.IsNotNull(updatedBucket);
            Assert.IsNotEmpty(updatedBucket.Id);
            Assert.AreEqual(updatedBucket.Id, createBucket.Id);
            Assert.AreEqual(updatedBucket.Name, "Therm sensor 2000");
            Assert.AreEqual(updatedBucket.OrgID, createBucket.OrgID);
            Assert.AreEqual(updatedBucket.RetentionRules[0].EverySeconds, 1000);
        }
    }
}