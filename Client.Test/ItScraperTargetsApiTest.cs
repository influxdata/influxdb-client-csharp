using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;

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
            _bucket = await Client.GetBucketsApi().FindBucketByNameAsync("my-bucket");
            _organization = await FindMyOrg();
        }

        private ScraperTargetsApi _scraperTargetsApi;
        private UsersApi _usersApi;

        private Bucket _bucket;
        private Organization _organization;

        [Test]
        public async Task CloneScraper()
        {
            var source = await _scraperTargetsApi
                .CreateScraperTargetAsync(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var properties = new Dictionary<string, string> { { "color", "green" }, { "location", "west" } };

            var label = await Client.GetLabelsApi()
                .CreateLabelAsync(GenerateName("Cool Resource"), properties, _organization.Id);
            await _scraperTargetsApi.AddLabelAsync(label, source);

            var name = GenerateName("cloned");

            var cloned = await _scraperTargetsApi.CloneScraperTargetAsync(name, source);

            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual(ScraperTargetRequest.TypeEnum.Prometheus, cloned.Type);
            Assert.AreEqual(source.Url, cloned.Url);
            Assert.AreEqual(source.OrgID, cloned.OrgID);
            Assert.AreEqual(source.BucketID, cloned.BucketID);

            var labels = await _scraperTargetsApi.GetLabelsAsync(cloned);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
        }

        [Test]
        public void CloneScraperNotFound()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _scraperTargetsApi.CloneScraperTargetAsync(GenerateName("bucket"), "020f755c3c082000"));

            Assert.AreEqual("scraper target is not found", ioe.Message);
            Assert.AreEqual(typeof(NotFoundException), ioe.GetType());
        }

        [Test]
        public async Task CreateScraperTarget()
        {
            var scraper = await _scraperTargetsApi
                .CreateScraperTargetAsync(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            Assert.IsNotNull(scraper);
            Assert.IsNotEmpty(scraper.Id);
            Assert.AreEqual(_organization.Id, scraper.OrgID);
            Assert.AreEqual(_bucket.Name, scraper.Bucket);

            var links = scraper.Links;

            Assert.IsNotNull(links);
            Assert.AreEqual(links.Bucket, $"/api/v2/buckets/{_bucket.Id}");
            Assert.AreEqual(links.Organization, $"/api/v2/orgs/{_organization.Id}");
            Assert.AreEqual(links.Self, $"/api/v2/scrapers/{scraper.Id}");
        }

        [Test]
        public async Task DeleteScraper()
        {
            var createdScraper = await _scraperTargetsApi
                .CreateScraperTargetAsync(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);
            Assert.IsNotNull(createdScraper);

            var foundScraper = await _scraperTargetsApi.FindScraperTargetByIdAsync(createdScraper.Id);
            Assert.IsNotNull(foundScraper);

            // delete scraper
            await _scraperTargetsApi.DeleteScraperTargetAsync(createdScraper);

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _scraperTargetsApi.FindScraperTargetByIdAsync(createdScraper.Id));

            Assert.AreEqual("scraper target is not found", ioe.Message);
        }

        [Test]
        public async Task FindScraperById()
        {
            var scraper = await _scraperTargetsApi
                .CreateScraperTargetAsync(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var scraperById = await _scraperTargetsApi.FindScraperTargetByIdAsync(scraper.Id);

            Assert.IsNotNull(scraperById);
            Assert.AreEqual(scraper.Id, scraperById.Id);
            Assert.AreEqual(scraper.Name, scraperById.Name);
        }

        [Test]
        public void FindScraperByIdNull()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _scraperTargetsApi.FindScraperTargetByIdAsync("020f755c3c082000"));

            Assert.AreEqual("scraper target is not found", ioe.Message);
        }

        [Test]
        public async Task FindScrapers()
        {
            var size = (await _scraperTargetsApi.FindScraperTargetsAsync()).Count;

            await _scraperTargetsApi
                .CreateScraperTargetAsync(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var scrapers = await _scraperTargetsApi.FindScraperTargetsAsync();

            Assert.AreEqual(scrapers.Count, size + 1);
        }

        [Test]
        public async Task Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var scraper = await _scraperTargetsApi
                .CreateScraperTargetAsync(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var properties = new Dictionary<string, string> { { "color", "green" }, { "location", "west" } };

            var label = await labelClient.CreateLabelAsync(GenerateName("Cool Resource"), properties, _organization.Id);

            var labels = await _scraperTargetsApi.GetLabelsAsync(scraper);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = await _scraperTargetsApi.AddLabelAsync(label, scraper);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = await _scraperTargetsApi.GetLabelsAsync(scraper);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            await _scraperTargetsApi.DeleteLabelAsync(label, scraper);

            labels = await _scraperTargetsApi.GetLabelsAsync(scraper);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        public async Task Member()
        {
            var scraper = await _scraperTargetsApi
                .CreateScraperTargetAsync(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var members = await _scraperTargetsApi.GetMembersAsync(scraper);
            Assert.AreEqual(0, members.Count);

            var user = await _usersApi.CreateUserAsync(GenerateName("Luke Health"));

            var resourceMember = await _scraperTargetsApi.AddMemberAsync(user, scraper);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.RoleEnum.Member);

            members = await _scraperTargetsApi.GetMembersAsync(scraper);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].Id, user.Id);
            Assert.AreEqual(members[0].Name, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.RoleEnum.Member);

            await _scraperTargetsApi.DeleteMemberAsync(user, scraper);

            members = await _scraperTargetsApi.GetMembersAsync(scraper);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        [Ignore("//TODO https://github.com/influxdata/influxdb/issues/19540")]
        public async Task Owner()
        {
            var scraper = await _scraperTargetsApi
                .CreateScraperTargetAsync(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            var owners = await _scraperTargetsApi.GetOwnersAsync(scraper);
            Assert.AreEqual(1, owners.Count);

            var user = await _usersApi.CreateUserAsync(GenerateName("Luke Health"));

            var resourceMember = await _scraperTargetsApi.AddOwnerAsync(user, scraper);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceOwner.RoleEnum.Owner);

            owners = await _scraperTargetsApi.GetOwnersAsync(scraper);
            Assert.AreEqual(2, owners.Count);
            Assert.AreEqual(owners[1].Id, user.Id);
            Assert.AreEqual(owners[1].Name, user.Name);
            Assert.AreEqual(owners[1].Role, ResourceOwner.RoleEnum.Owner);

            await _scraperTargetsApi.DeleteOwnerAsync(user, scraper);

            owners = await _scraperTargetsApi.GetOwnersAsync(scraper);
            Assert.AreEqual(1, owners.Count);
        }

        [Test]
        public async Task UpdateScraper()
        {
            var scraper = await _scraperTargetsApi
                .CreateScraperTargetAsync(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id,
                    _organization.Id);

            scraper.Name = "Changed name";

            var scraperUpdated = await _scraperTargetsApi.UpdateScraperTargetAsync(scraper);

            Assert.AreEqual("Changed name", scraperUpdated.Name);
        }
    }
}