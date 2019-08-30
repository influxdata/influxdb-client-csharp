using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
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

        private static BucketRetentionRules RetentionRule()
        {
            return new BucketRetentionRules(BucketRetentionRules.TypeEnum.Expire, 3600);
        }

        [Test]
        public async Task CloneBucket()
        {
            var source = await _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = await Client.GetLabelsApi()
                .CreateLabel(GenerateName("Cool Resource"), properties, _organization.Id);
            await _bucketsApi.AddLabel(label, source);

            var name = GenerateName("cloned");

            var cloned = await _bucketsApi.CloneBucket(name, source);

            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual(_organization.Id, cloned.OrgID);
            Assert.IsNull(cloned.Rp);
            Assert.AreEqual(1, cloned.RetentionRules.Count);
            Assert.AreEqual(3600, cloned.RetentionRules[0].EverySeconds);
            Assert.AreEqual(BucketRetentionRules.TypeEnum.Expire, cloned.RetentionRules[0].Type);

            var labels = await _bucketsApi.GetLabels(cloned);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
        }

        [Test]
        public void CloneBucketNotFound()
        {
            var ioe = Assert.ThrowsAsync<AggregateException>(async () =>
                await _bucketsApi.CloneBucket(GenerateName("bucket"), "020f755c3c082000"));

            Assert.AreEqual(typeof(HttpException), ioe.InnerException.InnerException.GetType());
            Assert.AreEqual("bucket not found", ioe.InnerException.InnerException.Message);
        }

        [Test]
        public async Task CreateBucket()
        {
            var bucketName = GenerateName("robot sensor");

            var bucket = await _bucketsApi.CreateBucket(bucketName, RetentionRule(), _organization);

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
            Assert.AreEqual(bucket.Links.Logs, $"/api/v2/buckets/{bucket.Id}/logs");
            Assert.AreEqual(bucket.Links.Labels, $"/api/v2/buckets/{bucket.Id}/labels");
        }

        [Test]
        public async Task CreateBucketWithoutRetentionRule()
        {
            var bucketName = GenerateName("robot sensor");

            var bucket = await _bucketsApi.CreateBucket(bucketName, _organization);

            Assert.IsNotNull(bucket);
            Assert.IsNotEmpty(bucket.Id);
            Assert.AreEqual(bucket.Name, bucketName);
            Assert.AreEqual(bucket.OrgID, _organization.Id);
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

            var ioe = Assert.ThrowsAsync<HttpException>(async () => await _bucketsApi.FindBucketById(createBucket.Id));

            Assert.AreEqual("bucket not found", ioe.Message);
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
            Assert.AreEqual(bucketById.OrgID, bucket.OrgID);
            Assert.AreEqual(bucketById.RetentionRules.Count, bucket.RetentionRules.Count);
            Assert.AreEqual(bucketById.Links.Self, bucket.Links.Self);
        }

        [Test]
        public void FindBucketByIdNull()
        {
            var ioe = Assert.ThrowsAsync<HttpException>(async () =>
                await _bucketsApi.FindBucketById("020f755c3c082000"));

            Assert.AreEqual("bucket not found", ioe.Message);
        }

        [Test]
        public async Task FindBucketByName()
        {
            var bucket = await _bucketsApi.FindBucketByName("my-bucket");

            Assert.IsNotNull(bucket);
            Assert.AreEqual("my-bucket", bucket.Name);
            Assert.AreEqual((await FindMyOrg()).Id, bucket.OrgID);
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

            findOptions.Offset += 5;
            entries = await _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Bucket Updated", entries.Logs[0].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[1].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[2].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[3].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            entries = await _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Bucket Updated", entries.Logs[0].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[1].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[2].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[3].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            entries = await _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Bucket Updated", entries.Logs[0].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[1].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[2].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[3].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            entries = await _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(0, entries.Logs.Count);

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
            var organization2 = await _organizationsApi.CreateOrganization(GenerateName("Second"));
            Assert.AreEqual(2, (await _bucketsApi.FindBucketsByOrganization(organization2)).Count);

            await _bucketsApi.CreateBucket(GenerateName("robot sensor"), organization2);
            Assert.AreEqual((await _bucketsApi.FindBucketsByOrganization(organization2)).Count, 3);
        }

        [Test]
        public async Task FindBucketsPaging()
        {
            foreach (var i in Enumerable.Range(0, 20 - (await _bucketsApi.FindBuckets()).Count))
                await _bucketsApi.CreateBucket(GenerateName($"{i}"), RetentionRule(), _organization);

            var findOptions = new FindOptions {Limit = 5};

            var buckets = await _bucketsApi.FindBuckets(findOptions);
            Assert.AreEqual(5, buckets._Buckets.Count);
            Assert.AreEqual("/api/v2/buckets?descending=false&limit=5&offset=5", buckets.Links.Next);

            buckets = await _bucketsApi.FindBuckets(FindOptions.GetFindOptions(buckets.Links.Next));
            Assert.AreEqual(5, buckets._Buckets.Count);
            Assert.AreEqual("/api/v2/buckets?descending=false&limit=5&offset=10", buckets.Links.Next);

            buckets = await _bucketsApi.FindBuckets(FindOptions.GetFindOptions(buckets.Links.Next));
            Assert.AreEqual(5, buckets._Buckets.Count);
            Assert.AreEqual("/api/v2/buckets?descending=false&limit=5&offset=15", buckets.Links.Next);

            buckets = await _bucketsApi.FindBuckets(FindOptions.GetFindOptions(buckets.Links.Next));
            Assert.AreEqual(5, buckets._Buckets.Count);
            Assert.AreEqual("/api/v2/buckets?descending=false&limit=5&offset=20", buckets.Links.Next);

            buckets = await _bucketsApi.FindBuckets(FindOptions.GetFindOptions(buckets.Links.Next));
            Assert.AreEqual(0, buckets._Buckets.Count);
            Assert.IsNull(buckets.Links.Next);
        }

        [Test]
        public async Task Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var bucket = await _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = await labelClient.CreateLabel(GenerateName("Cool Resource"), properties, _organization.Id);

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
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.RoleEnum.Member);

            members = await _bucketsApi.GetMembers(bucket);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].Id, user.Id);
            Assert.AreEqual(members[0].Name, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.RoleEnum.Member);

            await _bucketsApi.DeleteMember(user, bucket);

            members = await _bucketsApi.GetMembers(bucket);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        public async Task Owner()
        {
            var bucket = await _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            var owners = await _bucketsApi.GetOwners(bucket);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual("my-user", owners[0].Name);

            var user = await _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _bucketsApi.AddOwner(user, bucket);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceOwner.RoleEnum.Owner);

            owners = await _bucketsApi.GetOwners(bucket);
            Assert.AreEqual(2, owners.Count);
            Assert.AreEqual(owners[1].Id, user.Id);
            Assert.AreEqual(owners[1].Name, user.Name);
            Assert.AreEqual(owners[1].Role, ResourceOwner.RoleEnum.Owner);

            await _bucketsApi.DeleteOwner(user, bucket);

            owners = await _bucketsApi.GetOwners(bucket);
            Assert.AreEqual(1, owners.Count);
        }

        [Test]
        public async Task UpdateBucket()
        {
            var createBucket =
                await _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);
            createBucket.Name = "Therm sensor 2000";
            createBucket.RetentionRules[0].EverySeconds = 1000;

            var updatedBucket = await _bucketsApi.UpdateBucket(createBucket);

            Assert.IsNotNull(updatedBucket);
            Assert.IsNotEmpty(updatedBucket.Id);
            Assert.AreEqual(updatedBucket.Id, createBucket.Id);
            Assert.AreEqual(updatedBucket.Name, "Therm sensor 2000");
            Assert.AreEqual(updatedBucket.OrgID, createBucket.OrgID);
            Assert.AreEqual(updatedBucket.RetentionRules[0].EverySeconds, 1000);
        }
    }
}