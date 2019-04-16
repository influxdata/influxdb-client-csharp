using System.Collections.Generic;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Generated.Domain;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItScraperTargetsApiTest : AbstractItClientTest
    {
        [SetUp]
        public new void SetUp()
        {
            _scraperTargetsApi = Client.GetScraperTargetsApi();
            _usersApi = Client.GetUsersApi();
            _bucket = Client.GetBucketsApi().FindBucketByName("my-bucket");
            _organization = FindMyOrg();
        }

        private ScraperTargetsApi _scraperTargetsApi;
        private UsersApi _usersApi;

        private Bucket _bucket;
        private Organization _organization;

        [Test]
        public void CloneScraper()
        {
            var source = _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = Client.GetLabelsApi().CreateLabel(GenerateName("Cool Resource"), properties, _organization.Id);
            _scraperTargetsApi.AddLabel(label, source);

            var name = GenerateName("cloned");

            var cloned = _scraperTargetsApi.CloneScraperTarget(name, source);

            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual(ScraperTargetRequest.TypeEnum.Prometheus, cloned.Type);
            Assert.AreEqual(source.Url, cloned.Url);
            Assert.AreEqual(source.OrgID, cloned.OrgID);
            Assert.AreEqual(source.BucketID, cloned.BucketID);

            var labels = _scraperTargetsApi.GetLabels(cloned);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
        }

        [Test]
        public void CloneScraperNotFound()
        {
            var ioe = Assert.Throws<HttpException>(() =>
                _scraperTargetsApi.CloneScraperTarget(GenerateName("bucket"), "020f755c3c082000"));

            Assert.AreEqual("scraper target is not found", ioe.Message);
        }

        [Test]
        public void CreateScraperTarget()
        {
            var scraper = _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            Assert.IsNotNull(scraper);
            Assert.IsNotEmpty(scraper.Id);
            Assert.AreEqual(_organization.Name, scraper.Organization);
            Assert.AreEqual(_bucket.Name, scraper.Bucket);

            var links = scraper.Links;

            Assert.IsNotNull(links);
            Assert.AreEqual(links.Bucket, $"/api/v2/buckets/{_bucket.Id}");
            Assert.AreEqual(links.Organization, $"/api/v2/orgs/{_organization.Id}");
            Assert.AreEqual(links.Self, $"/api/v2/scrapers/{scraper.Id}");
        }

        [Test]
        public void DeleteScraper()
        {
            var createdScraper = _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);
            Assert.IsNotNull(createdScraper);

            var foundScraper = _scraperTargetsApi.FindScraperTargetById(createdScraper.Id);
            Assert.IsNotNull(foundScraper);

            // delete scraper
            _scraperTargetsApi.DeleteScraperTarget(createdScraper);

            var ioe = Assert.Throws<HttpException>(() =>
                _scraperTargetsApi.FindScraperTargetById(createdScraper.Id));

            Assert.AreEqual("scraper target is not found", ioe.Message);
        }

        [Test]
        public void FindScraperById()
        {
            var scraper = _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var scraperById = _scraperTargetsApi.FindScraperTargetById(scraper.Id);

            Assert.IsNotNull(scraperById);
            Assert.AreEqual(scraper.Id, scraperById.Id);
            Assert.AreEqual(scraper.Name, scraperById.Name);
        }

        [Test]
        public void FindScraperByIdNull()
        {
            var ioe = Assert.Throws<HttpException>(() =>
                _scraperTargetsApi.FindScraperTargetById("020f755c3c082000"));

            Assert.AreEqual("scraper target is not found", ioe.Message);
        }

        [Test]
        public void FindScrapers()
        {
            var size = (_scraperTargetsApi.FindScraperTargets()).Count;

            _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var scrapers = _scraperTargetsApi.FindScraperTargets();

            Assert.AreEqual(scrapers.Count, size + 1);
        }

        [Test]
        public void Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var scraper = _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = labelClient.CreateLabel(GenerateName("Cool Resource"), properties, _organization.Id);

            var labels = _scraperTargetsApi.GetLabels(scraper);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = _scraperTargetsApi.AddLabel(label, scraper);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = _scraperTargetsApi.GetLabels(scraper);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            _scraperTargetsApi.DeleteLabel(label, scraper);

            labels = _scraperTargetsApi.GetLabels(scraper);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        public void Member()
        {
            var scraper = _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var members = _scraperTargetsApi.GetMembers(scraper);
            Assert.AreEqual(0, members.Count);

            var user = _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = _scraperTargetsApi.AddMember(user, scraper);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.RoleEnum.Member);

            members = _scraperTargetsApi.GetMembers(scraper);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].Id, user.Id);
            Assert.AreEqual(members[0].Name, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.RoleEnum.Member);

            _scraperTargetsApi.DeleteMember(user, scraper);

            members = _scraperTargetsApi.GetMembers(scraper);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        public void Owner()
        {
            var scraper = _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var owners = _scraperTargetsApi.GetOwners(scraper);
            Assert.AreEqual(1, owners.Count);

            var user = _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = _scraperTargetsApi.AddOwner(user, scraper);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceOwner.RoleEnum.Owner);

            owners = _scraperTargetsApi.GetOwners(scraper);
            Assert.AreEqual(2, owners.Count);
            Assert.AreEqual(owners[1].Id, user.Id);
            Assert.AreEqual(owners[1].Name, user.Name);
            Assert.AreEqual(owners[1].Role, ResourceOwner.RoleEnum.Owner);

            _scraperTargetsApi.DeleteOwner(user, scraper);

            owners = _scraperTargetsApi.GetOwners(scraper);
            Assert.AreEqual(1, owners.Count);
        }

        [Test]
        public void UpdateScraper()
        {
            var scraper = _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            scraper.Name = "Changed name";

            var scraperUpdated = _scraperTargetsApi.UpdateScraperTarget(scraper);

            Assert.AreEqual("Changed name", scraperUpdated.Name);
        }
    }
}