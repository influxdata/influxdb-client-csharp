using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    [TestFixture]
    public class ItScraperClientTest: AbstractItClientTest
    {
        private ScraperClient _scraperClient;
        private UserClient _userClient;
        
        private Bucket _bucket;
        private Organization _organization;

        [SetUp]
        public new async Task SetUp()
        {
            _scraperClient = PlatformClient.CreateScraperClient();
            _userClient = PlatformClient.CreateUserClient();
            _bucket = await PlatformClient.CreateBucketClient().FindBucketByName("my-bucket");
            _organization = await FindMyOrg();
        }

        [Test]
        public async Task CreateScraperTarget()
        {
            var scraper = await _scraperClient
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id, _organization.Id);
            
            Assert.IsNotNull(scraper);
            Assert.IsNotEmpty(scraper.Id);
            Assert.AreEqual(_organization.Name, scraper.OrganizationName);
            Assert.AreEqual(_bucket.Name, scraper.BucketName);
            
            var links = scraper.Links;

            Assert.That(links.Count ==3);
            Assert.AreEqual(links["bucket"], $"/api/v2/buckets/{_bucket.Id}");
            Assert.AreEqual(links["organization"], $"/api/v2/orgs/{_organization.Id}");
            Assert.AreEqual(links["self"], $"/api/v2/scrapers/{scraper.Id}");
        }
        
        [Test]
        public async Task UpdateScraper()
        {
            ScraperTarget scraper = await _scraperClient
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id, _organization.Id);

            scraper.Name = "Changed name";

            var scraperUpdated = await _scraperClient.UpdateScraperTarget(scraper);
            
            Assert.AreEqual("Changed name", scraperUpdated.Name);
        }
        
        [Test]
        public async Task FindScrapers()
        {
            var size = (await _scraperClient.FindScraperTargets()).Count;

            await _scraperClient
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id, _organization.Id);

            var scrapers = await _scraperClient.FindScraperTargets();

            Assert.AreEqual(scrapers.Count, size + 1);
        }
        
        [Test]
        public async Task FindScraperById()
        {
            ScraperTarget scraper = await _scraperClient
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id, _organization.Id);

            var scraperById = await _scraperClient.FindScraperTargetById(scraper.Id);

            Assert.IsNotNull(scraperById);
            Assert.AreEqual(scraper.Id, scraperById.Id);
            Assert.AreEqual(scraper.Name, scraperById.Name);
        }

        [Test]
        public async Task FindScraperByIdNull()
        {
            var scraper = await _scraperClient.FindScraperTargetById("020f755c3c082000");

            Assert.IsNull(scraper);
        }
        
        [Test]
        public async Task DeleteScraper()
        {
            var createdScraper = await _scraperClient
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id, _organization.Id);
            Assert.IsNotNull(createdScraper);

            var foundScraper = await _scraperClient.FindScraperTargetById(createdScraper.Id);
            Assert.IsNotNull(foundScraper);

            // delete scraper
            await _scraperClient.DeleteScraperTarget(createdScraper);

            foundScraper = await _scraperClient.FindScraperTargetById(createdScraper.Id);
            Assert.IsNull(foundScraper);
        }
        
        [Test]
        public async Task Member() {

            var scraper = await _scraperClient
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id, _organization.Id);

            var members =  await _scraperClient.GetMembers(scraper);
            Assert.AreEqual(0, members.Count);

            var user = await _userClient.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _scraperClient.AddMember(user, scraper);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.UserId, user.Id);
            Assert.AreEqual(resourceMember.UserName, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.UserType.Member);

            members = await _scraperClient.GetMembers(scraper);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].UserId, user.Id);
            Assert.AreEqual(members[0].UserName, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.UserType.Member);

            await _scraperClient.DeleteMember(user, scraper);

            members = await _scraperClient.GetMembers(scraper);
            Assert.AreEqual(0, members.Count);
        }
        
        [Test]
        public async Task Owner() {

            var scraper = await _scraperClient
                .CreateScraperTarget(GenerateName("InfluxDB scraper"), "http://localhost:9999", _bucket.Id, _organization.Id);

            var owners =  await _scraperClient.GetOwners(scraper);
            Assert.AreEqual(1, owners.Count);

            var user = await _userClient.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _scraperClient.AddOwner(user, scraper);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.UserId, user.Id);
            Assert.AreEqual(resourceMember.UserName, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.UserType.Owner);

            owners = await _scraperClient.GetOwners(scraper);
            Assert.AreEqual(2, owners.Count);
            Assert.AreEqual(owners[1].UserId, user.Id);
            Assert.AreEqual(owners[1].UserName, user.Name);
            Assert.AreEqual(owners[1].Role, ResourceMember.UserType.Owner);

            await _scraperClient.DeleteOwner(user, scraper);

            owners = await _scraperClient.GetOwners(scraper);
            Assert.AreEqual(1, owners.Count);
        }
    }
}