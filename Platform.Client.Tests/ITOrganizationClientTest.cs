using System.Collections.Generic;
using System.Linq;
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
        
        [Test]
        public async Task FindOrganizationById() 
        {
            string organizationName = GenerateName("Constant Pro");

            Organization organization = await _organizationClient.CreateOrganization(organizationName);

            Organization organizationById = await _organizationClient.FindOrganizationById(organization.Id);

            Assert.IsNotNull(organizationById);
            Assert.AreEqual(organizationById.Id, organization.Id);
            Assert.AreEqual(organizationById.Name, organization.Name);
            
            var links = organization.Links;
            
            Assert.That(links.Count == 6);
            Assert.That(links.ContainsKey("buckets"));
            Assert.That(links.ContainsKey("dashboards"));
            Assert.That(links.ContainsKey("log"));
            Assert.That(links.ContainsKey("members"));
            Assert.That(links.ContainsKey("self"));
            Assert.That(links.ContainsKey("tasks"));
        }
        
        [Test]
        public async Task FindOrganizationByIdNull() 
        {
            Organization organization = await _organizationClient.FindOrganizationById("020f755c3c082000");

            Assert.IsNull(organization);
        }
        
        [Test]
        public async Task FindOrganizations() 
        {
            List<Organization> organizations = await _organizationClient.FindOrganizations();
            
            await _organizationClient.CreateOrganization(GenerateName("Constant Pro"));

            List<Organization> organizationsNew = await _organizationClient.FindOrganizations();
            Assert.That(organizationsNew.Count == organizations.Count + 1);
        }
        
        [Test]
        public async Task DeleteOrganization() 
        {
            Organization createdOrganization = await _organizationClient.CreateOrganization(GenerateName("Constant Pro"));
            Assert.IsNotNull(createdOrganization);

            Organization foundOrganization = await _organizationClient.FindOrganizationById(createdOrganization.Id);
            Assert.IsNotNull(foundOrganization);
                            
            // delete task
            await _organizationClient.DeleteOrganization(createdOrganization);

            foundOrganization = await _organizationClient.FindOrganizationById(createdOrganization.Id);
            Assert.IsNull(foundOrganization);
        }
        
        [Test]
        public async Task UpdateOrganization() 
        {
            Organization createdOrganization = await _organizationClient.CreateOrganization(GenerateName("Constant Pro"));
            createdOrganization.Name = "Master Pb";

            Organization updatedOrganization = await _organizationClient.UpdateOrganization(createdOrganization);

            Assert.IsNotNull(updatedOrganization);
            Assert.AreEqual(updatedOrganization.Id, createdOrganization.Id);
            Assert.AreEqual(updatedOrganization.Name, "Master Pb");
            
            var links = updatedOrganization.Links;
            
            Assert.That(links.Count == 6);
            Assert.AreEqual("/api/v2/buckets?org=Master Pb", links["buckets"]);
            Assert.AreEqual("/api/v2/dashboards?org=Master Pb", links["dashboards"]);
            Assert.AreEqual("/api/v2/orgs/" + updatedOrganization.Id, links["self"]);
            Assert.AreEqual("/api/v2/tasks?org=Master Pb", links["tasks"]);
            Assert.AreEqual("/api/v2/orgs/" + updatedOrganization.Id + "/members", links["members"]);
            Assert.AreEqual("/api/v2/orgs/" + updatedOrganization.Id + "/log", links["log"]);
        }
    }
}