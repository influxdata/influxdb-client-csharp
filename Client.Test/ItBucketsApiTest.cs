using System.Collections.Generic;
using System.Linq;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Api.Domain;
using NUnit.Framework;
using ResourceMember = InfluxDB.Client.Api.Domain.ResourceMember;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItBucketsApiTest : AbstractItClientTest
    {
        [SetUp]
        public new void SetUp()
        {
            _organizationsApi = Client.GetOrganizationsApi();
            _bucketsApi = Client.GetBucketsApi();
            _usersApi = Client.GetUsersApi();

            _organization = _organizationsApi.CreateOrganization(GenerateName("Org"));

            foreach (var bucket in _bucketsApi.FindBuckets().Where(bucket => bucket.Name.EndsWith("-IT")))
                _bucketsApi.DeleteBucket(bucket);
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
        public void CloneBucket()
        {
            var source = _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = Client.GetLabelsApi().CreateLabel(GenerateName("Cool Resource"), properties, _organization.Id);
            _bucketsApi.AddLabel(label, source);

            var name = GenerateName("cloned");

            var cloned = _bucketsApi.CloneBucket(name, source);

            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual(_organization.Id, cloned.OrgID);
            Assert.IsNull(cloned.Rp);
            Assert.AreEqual(1, cloned.RetentionRules.Count);
            Assert.AreEqual(3600, cloned.RetentionRules[0].EverySeconds);
            Assert.AreEqual(BucketRetentionRules.TypeEnum.Expire, cloned.RetentionRules[0].Type);

            var labels = _bucketsApi.GetLabels(cloned);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
        }

        [Test]
        public void CloneBucketNotFound()
        {
            var ioe = Assert.Throws<HttpException>(() =>
                _bucketsApi.CloneBucket(GenerateName("bucket"), "020f755c3c082000"));

            Assert.AreEqual("bucket not found", ioe.Message);
        }

        [Test]
        public void CreateBucket()
        {
            var bucketName = GenerateName("robot sensor");

            var bucket = _bucketsApi.CreateBucket(bucketName, RetentionRule(), _organization);

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
        public void CreateBucketWithoutRetentionRule()
        {
            var bucketName = GenerateName("robot sensor");

            var bucket = _bucketsApi.CreateBucket(bucketName, _organization);

            Assert.IsNotNull(bucket);
            Assert.IsNotEmpty(bucket.Id);
            Assert.AreEqual(bucket.Name, bucketName);
            Assert.AreEqual(bucket.OrgID, _organization.Id);
            Assert.AreEqual(bucket.RetentionRules.Count, 0);
        }

        [Test]
        public void DeleteBucket()
        {
            var createBucket =
                _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);
            Assert.IsNotNull(createBucket);

            var foundBucket = _bucketsApi.FindBucketById(createBucket.Id);
            Assert.IsNotNull(foundBucket);

            // delete task
            _bucketsApi.DeleteBucket(createBucket);

            var ioe = Assert.Throws<HttpException>(() => _bucketsApi.FindBucketById(createBucket.Id));

            Assert.AreEqual("bucket not found", ioe.Message);
        }

        [Test]
        public void FindBucketById()
        {
            var bucketName = GenerateName("robot sensor");

            var bucket = _bucketsApi.CreateBucket(bucketName, RetentionRule(), _organization);

            var bucketById = _bucketsApi.FindBucketById(bucket.Id);

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
            var ioe = Assert.Throws<HttpException>(() =>
                _bucketsApi.FindBucketById("020f755c3c082000"));

            Assert.AreEqual("bucket not found", ioe.Message);
        }

        [Test]
        public void FindBucketByName()
        {
            var bucket = _bucketsApi.FindBucketByName("my-bucket");

            Assert.IsNotNull(bucket);
            Assert.AreEqual("my-bucket", bucket.Name);
            Assert.AreEqual( FindMyOrg().Id, bucket.OrgID);
        }

        [Test]
        public void FindBucketByNameNotFound()
        {
            var bucket = _bucketsApi.FindBucketByName("my-bucket-not-found");

            Assert.IsNull(bucket);
        }

        [Test]
        public void FindBucketLogsFindOptionsNotFound()
        {
            var entries = _bucketsApi.FindBucketLogs("020f755c3c082000", new FindOptions());

            Assert.IsNotNull(entries);
            Assert.AreEqual(0, entries.Logs.Count);
        }

        [Test]
        public void FindBucketLogsNotFound()
        {
            var logs = _bucketsApi.FindBucketLogs("020f755c3c082000");

            Assert.AreEqual(0, logs.Count);
        }

        [Test]
        public void FindBucketLogsPaging()
        {
            var bucket = _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            foreach (var i in Enumerable.Range(0, 19))
            {
                bucket.Name = $"{i}_{bucket.Name}";

                _bucketsApi.UpdateBucket(bucket);
            }

            var logs = _bucketsApi.FindBucketLogs(bucket);

            Assert.AreEqual(20, logs.Count);
            Assert.AreEqual("Bucket Created", logs[0].Description);
            Assert.AreEqual("Bucket Updated", logs[19].Description);

            var findOptions = new FindOptions {Limit = 5, Offset = 0};

            var entries = _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Bucket Created", entries.Logs[0].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[1].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[2].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[3].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            entries = _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Bucket Updated", entries.Logs[0].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[1].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[2].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[3].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            entries = _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Bucket Updated", entries.Logs[0].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[1].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[2].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[3].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            entries = _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Bucket Updated", entries.Logs[0].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[1].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[2].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[3].Description);
            Assert.AreEqual("Bucket Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            entries = _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(0, entries.Logs.Count);

            //
            // Order
            //
            findOptions = new FindOptions {Descending = false};
            entries = _bucketsApi.FindBucketLogs(bucket, findOptions);
            Assert.AreEqual(20, entries.Logs.Count);

            Assert.AreEqual("Bucket Updated", entries.Logs[19].Description);
            Assert.AreEqual("Bucket Created", entries.Logs[0].Description);
        }

        [Test]
        public void FindBuckets()
        {
            var size = _bucketsApi.FindBuckets().Count;

            _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            var organization2 = _organizationsApi.CreateOrganization(GenerateName("Second"));
            _bucketsApi.CreateBucket(GenerateName("robot sensor"), organization2.Id);

            var buckets = _bucketsApi.FindBuckets();
            Assert.AreEqual(buckets.Count, size + 2);
        }

        [Test]
        public void FindBucketsByOrganization()
        {
            Assert.AreEqual(_bucketsApi.FindBucketsByOrganization(_organization).Count, 0);

            _bucketsApi.CreateBucket(GenerateName("robot sensor"), _organization);

            var organization2 = _organizationsApi.CreateOrganization(GenerateName("Second"));
            _bucketsApi.CreateBucket(GenerateName("robot sensor"), organization2);

            Assert.AreEqual(_bucketsApi.FindBucketsByOrganization(_organization).Count, 1);
        }

        [Test]
        public void FindBucketsPaging()
        {
            foreach (var i in Enumerable.Range(0, 20 - _bucketsApi.FindBuckets().Count))
                _bucketsApi.CreateBucket(GenerateName($"{i}"), RetentionRule(), _organization);

            var findOptions = new FindOptions {Limit = 5};

            var buckets = _bucketsApi.FindBuckets(findOptions);
            Assert.AreEqual(5, buckets._Buckets.Count);
            Assert.AreEqual("/api/v2/buckets?descending=false&limit=5&offset=5", buckets.Links.Next);

            buckets = _bucketsApi.FindBuckets(FindOptions.GetFindOptions(buckets.Links.Next));
            Assert.AreEqual(5, buckets._Buckets.Count);
            Assert.AreEqual("/api/v2/buckets?descending=false&limit=5&offset=10", buckets.Links.Next);

            buckets = _bucketsApi.FindBuckets(FindOptions.GetFindOptions(buckets.Links.Next));
            Assert.AreEqual(5, buckets._Buckets.Count);
            Assert.AreEqual("/api/v2/buckets?descending=false&limit=5&offset=15", buckets.Links.Next);

            buckets = _bucketsApi.FindBuckets(FindOptions.GetFindOptions(buckets.Links.Next));
            Assert.AreEqual(5, buckets._Buckets.Count);
            Assert.AreEqual("/api/v2/buckets?descending=false&limit=5&offset=20", buckets.Links.Next);

            buckets = _bucketsApi.FindBuckets(FindOptions.GetFindOptions(buckets.Links.Next));
            Assert.AreEqual(0, buckets._Buckets.Count);
            Assert.IsNull(buckets.Links.Next);
        }

        [Test]
        public void Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var bucket = _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = labelClient.CreateLabel(GenerateName("Cool Resource"), properties, _organization.Id);

            var labels = _bucketsApi.GetLabels(bucket);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = _bucketsApi.AddLabel(label, bucket);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = _bucketsApi.GetLabels(bucket);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            _bucketsApi.DeleteLabel(label, bucket);

            labels = _bucketsApi.GetLabels(bucket);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        public void Member()
        {
            var bucket = _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            var members = _bucketsApi.GetMembers(bucket);
            Assert.AreEqual(0, members.Count);

            var user = _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = _bucketsApi.AddMember(user, bucket);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.RoleEnum.Member);

            members = _bucketsApi.GetMembers(bucket);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].Id, user.Id);
            Assert.AreEqual(members[0].Name, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.RoleEnum.Member);

            _bucketsApi.DeleteMember(user, bucket);

            members = _bucketsApi.GetMembers(bucket);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        public void Owner()
        {
            var bucket = _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);

            var owners = _bucketsApi.GetOwners(bucket);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual("my-user", owners[0].Name);

            var user = _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = _bucketsApi.AddOwner(user, bucket);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceOwner.RoleEnum.Owner);

            owners = _bucketsApi.GetOwners(bucket);
            Assert.AreEqual(2, owners.Count);
            Assert.AreEqual(owners[1].Id, user.Id);
            Assert.AreEqual(owners[1].Name, user.Name);
            Assert.AreEqual(owners[1].Role, ResourceOwner.RoleEnum.Owner);

            _bucketsApi.DeleteOwner(user, bucket);

            owners = _bucketsApi.GetOwners(bucket);
            Assert.AreEqual(1, owners.Count);
        }

        [Test]
        public void UpdateBucket()
        {
            var createBucket =
                _bucketsApi.CreateBucket(GenerateName("robot sensor"), RetentionRule(), _organization);
            createBucket.Name = "Therm sensor 2000";
            createBucket.RetentionRules[0].EverySeconds = 1000;

            var updatedBucket = _bucketsApi.UpdateBucket(createBucket);

            Assert.IsNotNull(updatedBucket);
            Assert.IsNotEmpty(updatedBucket.Id);
            Assert.AreEqual(updatedBucket.Id, createBucket.Id);
            Assert.AreEqual(updatedBucket.Name, "Therm sensor 2000");
            Assert.AreEqual(updatedBucket.OrgID, createBucket.OrgID);
            Assert.AreEqual(updatedBucket.RetentionRules[0].EverySeconds, 1000);
        }
    }
}