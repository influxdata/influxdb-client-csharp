using System.Collections.Generic;
using System.Linq;
using InfluxDB.Client.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

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

            _organization = await _organizationsApi.CreateOrganization(GenerateName("Org"));

            foreach (var bucket in (await _bucketsApi.FindBuckets()).Where(bucket => bucket.Name.EndsWith("-IT")))
                await _bucketsApi.DeleteBucket(bucket);
        }

        private OrganizationsApi _organizationsApi;
        private BucketsApi _bucketsApi;
        private UsersApi _usersApi;

        private Organization _organization;

        private static RetentionRule RetentionRule()
        {
            return new RetentionRule {Type = "expire", EverySeconds = 3600L};
        }

        [Test]
        public async Task CreateBucket()
        {
            var bucketName = GenerateName("robot sensor");

            var bucket = await _bucketsApi.CreateBucket(bucketName, RetentionRule(), _organization);

            Assert.IsNotNull(bucket);
            Assert.IsNotEmpty(bucket.Id);
            Assert.AreEqual(bucket.Name, bucketName);
            Assert.AreEqual(bucket.OrgId, _organization.Id);
            Assert.AreEqual(bucket.OrganizationName, _organization.Name);
            Assert.AreEqual(bucket.RetentionRules.Count, 1);
            Assert.AreEqual(bucket.RetentionRules[0].EverySeconds, 3600L);
            Assert.AreEqual(bucket.RetentionRules[0].Type, "expire");
            Assert.AreEqual(bucket.Links.Count, 4);
            Assert.AreEqual(bucket.Links["org"], $"/api/v2/orgs/{_organization.Id}");
            Assert.AreEqual(bucket.Links["self"], $"/api/v2/buckets/{bucket.Id}");
            Assert.AreEqual(bucket.Links["log"], $"/api/v2/buckets/{bucket.Id}/log");
            Assert.AreEqual(bucket.Links["labels"], $"/api/v2/buckets/{bucket.Id}/labels");
        }

        [Test]
        public async Task CreateBucketWithoutRetentionRule()
        {
            var bucketName = GenerateName("robot sensor");

            var bucket = await _bucketsApi.CreateBucket(bucketName, _organization);

            Assert.IsNotNull(bucket);
            Assert.IsNotEmpty(bucket.Id);
            Assert.AreEqual(bucket.Name, bucketName);
            Assert.AreEqual(bucket.OrgId, _organization.Id);
            Assert.AreEqual(bucket.RetentionRules.Count, 0);
        }

        [Test]
        public async Task DeleteBucket()
        {
            var createBucket =
                await _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);
            Assert.IsNotNull(createBucket);

            var foundBucket = await _bucketsApi.FindBucketById(createBucket.Id);
            Assert.IsNotNull(foundBucket);

            // delete task
            await _bucketsApi.DeleteBucket(createBucket);

            foundBucket = await _bucketsApi.FindBucketById(createBucket.Id);
            Assert.IsNull(foundBucket);
        }

        [Test]
        public async Task FindBucketById()
        {
            var bucketName = GenerateName("robot sensor");

            var bucket = await _bucketsApi.CreateBucket(bucketName, RetentionRule(), _organization);

            var bucketById = await _bucketsApi.FindBucketById(bucket.Id);

            Assert.IsNotNull(bucketById);
            Assert.AreEqual(bucketById.Id, bucket.Id);
            Assert.AreEqual(bucketById.Name, bucket.Name);
            Assert.AreEqual(bucketById.OrgId, bucket.OrgId);
            Assert.AreEqual(bucketById.OrganizationName, bucket.OrganizationName);
            Assert.AreEqual(bucketById.RetentionRules.Count, bucket.RetentionRules.Count);
            Assert.AreEqual(bucketById.Links.Count, bucket.Links.Count);
        }

        [Test]
        public async Task FindBucketByIdNull()
        {
            var bucket = await _bucketsApi.FindBucketById("020f755c3c082000");

            Assert.IsNull(bucket);
        }

        [Test]
        public async Task FindBucketByName()
        {
            var bucket = await _bucketsApi.FindBucketByName("my-bucket");

            Assert.IsNotNull(bucket);
            Assert.AreEqual("my-bucket", bucket.Name);
            Assert.AreEqual((await FindMyOrg()).Id, bucket.OrgId);
        }

        [Test]
        public async Task FindBucketByNameNotFound()
        {
            var bucket = await _bucketsApi.FindBucketByName("my-bucket-not-found");

            Assert.IsNull(bucket);
        }

        [Test]
        public async Task FindBucketLogsFindOptionsNotFound()
        {
            var entries = await _bucketsApi.FindBucketLogs("020f755c3c082000", new FindOptions());

            Assert.IsNotNull(entries);
            Assert.AreEqual(0, entries.Logs.Count);
        }

        [Test]
        public async Task FindBucketLogsNotFound()
        {
            var logs = await _bucketsApi.FindBucketLogs("020f755c3c082000");

            Assert.AreEqual(0, logs.Count);
        }

        [Test]
        public async Task FindBucketLogsPaging()
        {
            var bucket = await _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            foreach (var i in Enumerable.Range(0, 19))
            {
                bucket.Name = $"{i}_{bucket.Name}";

                await _bucketsApi.UpdateBucket(bucket);
            }

            var logs = await _bucketsApi.FindBucketLogs(bucket);

            Assert.AreEqual(20, logs.Count);
            Assert.AreEqual("Bucket Created", logs[0].Description);
            Assert.AreEqual("Bucket Updated", logs[19].Description);

            var findOptions = new FindOptions {Limit = 5, Offset = 0};

            var entries = await _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Bucket Created", entries.Logs[0].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[1].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[2].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[3].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[4].Description);

            //TODO isNotNull FindOptions also in Log API? 
            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Bucket Updated", entries.Logs[0].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[1].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[2].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[3].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Bucket Updated", entries.Logs[0].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[1].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[2].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[3].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Bucket Updated", entries.Logs[0].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[1].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[2].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[3].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(0, entries.Logs.Count);
            Assert.IsNull(entries.GetNextPage());

            //
            // Order
            //
            findOptions = new FindOptions {Descending = false};
            entries = await _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(20, entries.Logs.Count);

            Assert.AreEqual("Bucket Updated", entries.Logs[19].Description);
            Assert.AreEqual("Bucket Created", entries.Logs[0].Description);
        }

        [Test]
        public async Task FindBuckets()
        {
            var size = (await _bucketsApi.FindBuckets()).Count;

            await _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            var organization2 = await _organizationsApi.CreateOrganization(GenerateName("Second"));
            await _bucketsApi.CreateBucket(GenerateName("robot sensor"), organization2.Id);

            var buckets = await _bucketsApi.FindBuckets();
            Assert.AreEqual(buckets.Count, size + 2);
        }

        [Test]
        public async Task FindBucketsByOrganization()
        {
            Assert.AreEqual((await _bucketsApi.FindBucketsByOrganization(_organization)).Count, 0);

            await _bucketsApi.CreateBucket(GenerateName("robot sensor"), _organization);

            var organization2 = await _organizationsApi.CreateOrganization(GenerateName("Second"));
            await _bucketsApi.CreateBucket(GenerateName("robot sensor"), organization2);

            Assert.AreEqual((await _bucketsApi.FindBucketsByOrganization(_organization)).Count, 1);
        }

        [Test]
        public async Task FindBucketsPaging()
        {
            foreach (var i in Enumerable.Range(0, 20 - (await _bucketsApi.FindBuckets()).Count))
                await _bucketsApi.CreateBucket(GenerateName($"{i}"), RetentionRule(), _organization);

            var findOptions = new FindOptions {Limit = 5};

            var buckets = await _bucketsApi.FindBuckets(findOptions);
            Assert.AreEqual(5, buckets.BucketList.Count);
            Assert.IsNotNull(buckets.GetNextPage());

            buckets = await _bucketsApi.FindBuckets(buckets.GetNextPage());
            Assert.AreEqual(5, buckets.BucketList.Count);
            Assert.IsNotNull(buckets.GetNextPage());

            buckets = await _bucketsApi.FindBuckets(buckets.GetNextPage());
            Assert.AreEqual(5, buckets.BucketList.Count);
            Assert.IsNotNull(buckets.GetNextPage());

            buckets = await _bucketsApi.FindBuckets(buckets.GetNextPage());
            Assert.AreEqual(5, buckets.BucketList.Count);
            Assert.IsNotNull(buckets.GetNextPage());

            buckets = await _bucketsApi.FindBuckets(buckets.GetNextPage());
            Assert.AreEqual(0, buckets.BucketList.Count);
            Assert.IsNull(buckets.GetNextPage());
        }

        [Test]
        public async Task Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var bucket = await _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = await labelClient.CreateLabel(GenerateName("Cool Resource"), properties);

            var labels = await _bucketsApi.GetLabels(bucket);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = await _bucketsApi.AddLabel(label, bucket);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = await _bucketsApi.GetLabels(bucket);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            await _bucketsApi.DeleteLabel(label, bucket);

            labels = await _bucketsApi.GetLabels(bucket);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        public async Task Member()
        {
            var bucket = await _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            var members = await _bucketsApi.GetMembers(bucket);
            Assert.AreEqual(0, members.Count);

            var user = await _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _bucketsApi.AddMember(user, bucket);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.UserId, user.Id);
            Assert.AreEqual(resourceMember.UserName, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.UserType.Member);

            members = await _bucketsApi.GetMembers(bucket);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].UserId, user.Id);
            Assert.AreEqual(members[0].UserName, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.UserType.Member);

            await _bucketsApi.DeleteMember(user, bucket);

            members = await _bucketsApi.GetMembers(bucket);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        public async Task Owner()
        {
            var bucket = await _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            var owners = await _bucketsApi.GetOwners(bucket);
            Assert.AreEqual(0, owners.Count);

            var user = await _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _bucketsApi.AddOwner(user, bucket);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.UserId, user.Id);
            Assert.AreEqual(resourceMember.UserName, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.UserType.Owner);

            owners = await _bucketsApi.GetOwners(bucket);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual(owners[0].UserId, user.Id);
            Assert.AreEqual(owners[0].UserName, user.Name);
            Assert.AreEqual(owners[0].Role, ResourceMember.UserType.Owner);

            await _bucketsApi.DeleteOwner(user, bucket);

            owners = await _bucketsApi.GetOwners(bucket);
            Assert.AreEqual(0, owners.Count);
        }

        [Test]
        public async Task UpdateBucket()
        {
            var createBucket =
                await _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);
            createBucket.Name = "Therm sensor 2000";
            createBucket.RetentionRules[0].EverySeconds = 1000L;

            var updatedBucket = await _bucketsApi.UpdateBucket(createBucket);

            Assert.IsNotNull(updatedBucket);
            Assert.IsNotEmpty(updatedBucket.Id);
            Assert.AreEqual(updatedBucket.Id, createBucket.Id);
            Assert.AreEqual(updatedBucket.Name, "Therm sensor 2000");
            Assert.AreEqual(updatedBucket.OrgId, createBucket.OrgId);
            Assert.AreEqual(updatedBucket.OrganizationName, createBucket.OrganizationName);
            Assert.AreEqual(updatedBucket.RetentionRules[0].EverySeconds, 1000L);
        }
    }
}