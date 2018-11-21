using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    public class ItOrganizationClientTest : AbstractItClientTest
    {
        private OrganizationClient _organizationClient;
        
        [SetUp]
        public new void SetUp()
        {
            _organizationClient = PlatformClient.CreateOrganizationClient();
        }
        
        [Test]
        public async Task CreateOrganization() 
        {
            string organizationName = GenerateName("Constant Pro");

            Organization organization = await _organizationClient.CreateOrganization(organizationName);

            Assert.IsNotNull(organization);
            Assert.IsNotEmpty(organization.Id);
            Assert.AreEqual(organization.Name, organizationName);

            var links = organization.Links;
            
            Assert.That(links.Count == 6);
            Assert.That(links.ContainsKey("buckets"));
            Assert.That(links.ContainsKey("dashboards"));
            Assert.That(links.ContainsKey("log"));
            Assert.That(links.ContainsKey("members"));
            Assert.That(links.ContainsKey("self"));
            Assert.That(links.ContainsKey("tasks"));
        }
    }
}