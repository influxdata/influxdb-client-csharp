using System.Collections.Generic;
using InfluxDB.Client.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItScraperTargetsApiTest : AbstractItClientTest
    {
        [SetUp]
        public new async Task SetUp()
        {
            _scraperTargetsApi = Client.GetScraperTargetsApi();
            _usersApi = Client.GetUsersApi();
            _bucket = await Client.GetBucketsApi().FindBucketByName("my-bucket");
            _organization = await FindMyOrg();
        }

        private ScraperTargetsApi _scraperTargetsApi;
        private UsersApi _usersApi;

        private Bucket _bucket;
        private Organization _organization;

        [Test]
        public async Task CreateScraperTarget()
        {
            var scraper = await _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            Assert.IsNotNull(scraper);
            Assert.IsNotEmpty(scraper.Id);
            Assert.AreEqual(_organization.Name, scraper.OrganizationName);
            Assert.AreEqual(_bucket.Name, scraper.BucketName);

            var links = scraper.Links;

            Assert.That(links.Count == 3);
            Assert.AreEqual(links["bucket"], $"/api/v2/buckets/{_bucket.Id}");
            Assert.AreEqual(links["organization"], $"/api/v2/orgs/{_organization.Id}");
            Assert.AreEqual(links["self"], $"/api/v2/scrapers/{scraper.Id}");
        }

        [Test]
        public async Task DeleteScraper()
        {
            var createdScraper = await _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);
            Assert.IsNotNull(createdScraper);

            var foundScraper = await _scraperTargetsApi.FindScraperTargetById(createdScraper.Id);
            Assert.IsNotNull(foundScraper);

            // delete scraper
            await _scraperTargetsApi.DeleteScraperTarget(createdScraper);

            foundScraper = await _scraperTargetsApi.FindScraperTargetById(createdScraper.Id);
            Assert.IsNull(foundScraper);
        }

        [Test]
        public async Task FindScraperById()
        {
            ScraperTarget scraper = await _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var scraperById = await _scraperTargetsApi.FindScraperTargetById(scraper.Id);

            Assert.IsNotNull(scraperById);
            Assert.AreEqual(scraper.Id, scraperById.Id);
            Assert.AreEqual(scraper.Name, scraperById.Name);
        }

        [Test]
        public async Task FindScraperByIdNull()
        {
            var scraper = await _scraperTargetsApi.FindScraperTargetById("020f755c3c082000");

            Assert.IsNull(scraper);
        }

        [Test]
        public async Task FindScrapers()
        {
            var size = (await _scraperTargetsApi.FindScraperTargets()).Count;

            await _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var scrapers = await _scraperTargetsApi.FindScraperTargets();

            Assert.AreEqual(scrapers.Count, size + 1);
        }

        [Test]
        //TODO
        [Ignore("https://github.com/influxdata/influxdb/issues/11767")]
        public async Task Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var scraper = await _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = await labelClient.CreateLabel(GenerateName("Cool Resource"), properties);

            var labels = await _scraperTargetsApi.GetLabels(scraper);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = await _scraperTargetsApi.AddLabel(label, scraper);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = await _scraperTargetsApi.GetLabels(scraper);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            await _scraperTargetsApi.DeleteLabel(label, scraper);

            labels = await _scraperTargetsApi.GetLabels(scraper);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        //TODO
        [Ignore("https://github.com/influxdata/influxdb/issues/11767")]
        public async Task Member()
        {
            var scraper = await _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var members = await _scraperTargetsApi.GetMembers(scraper);
            Assert.AreEqual(0, members.Count);

            var user = await _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _scraperTargetsApi.AddMember(user, scraper);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.UserId, user.Id);
            Assert.AreEqual(resourceMember.UserName, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.UserType.Member);

            members = await _scraperTargetsApi.GetMembers(scraper);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].UserId, user.Id);
            Assert.AreEqual(members[0].UserName, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.UserType.Member);

            await _scraperTargetsApi.DeleteMember(user, scraper);

            members = await _scraperTargetsApi.GetMembers(scraper);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        //TODO
        [Ignore("https://github.com/influxdata/influxdb/issues/11767")]
        public async Task Owner()
        {
            var scraper = await _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var owners = await _scraperTargetsApi.GetOwners(scraper);
            Assert.AreEqual(1, owners.Count);

            var user = await _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _scraperTargetsApi.AddOwner(user, scraper);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.UserId, user.Id);
            Assert.AreEqual(resourceMember.UserName, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.UserType.Owner);

            owners = await _scraperTargetsApi.GetOwners(scraper);
            Assert.AreEqual(2, owners.Count);
            Assert.AreEqual(owners[1].UserId, user.Id);
            Assert.AreEqual(owners[1].UserName, user.Name);
            Assert.AreEqual(owners[1].Role, ResourceMember.UserType.Owner);

            await _scraperTargetsApi.DeleteOwner(user, scraper);

            owners = await _scraperTargetsApi.GetOwners(scraper);
            Assert.AreEqual(1, owners.Count);
        }

        [Test]
        public async Task UpdateScraper()
        {
            ScraperTarget scraper = await _scraperTargetsApi
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            scraper.Name = "Changed name";

            var scraperUpdated = await _scraperTargetsApi.UpdateScraperTarget(scraper);

            Assert.AreEqual("Changed name", scraperUpdated.Name);
        }
    }
}