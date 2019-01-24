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
        
        private Bucket _bucket;
        private Organization _organization;

        [SetUp]
        public new async Task SetUp()
        {
            _scraperClient = PlatformClient.CreateScraperClient();
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
    }
}