using System.Collections.Generic;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    [TestFixture]
    public class ItOrganizationClientTest : AbstractItClientTest
    {
        private OrganizationClient _organizationClient;
        private UserClient _userClient;
        
        [SetUp]
        public new void SetUp()
        {
            _organizationClient = PlatformClient.CreateOrganizationClient();
            _userClient = PlatformClient.CreateUserClient();
        }
        
        [Test]
        public async Task CreateOrganization() 
        {
            var organizationName = GenerateName("Constant Pro");

            var organization = await _organizationClient.CreateOrganization(organizationName);

            Assert.IsNotNull(organization);
            Assert.IsNotEmpty(organization.Id);
            Assert.AreEqual(organization.Name, organizationName);

            var links = organization.Links;
            
            Assert.That(links.Count == 8);
            Assert.That(links.ContainsKey("buckets"));
            Assert.That(links.ContainsKey("dashboards"));
            Assert.That(links.ContainsKey("log"));
            Assert.That(links.ContainsKey("members"));
            Assert.That(links.ContainsKey("self"));
            Assert.That(links.ContainsKey("tasks"));
            Assert.That(links.ContainsKey("labels"));
            Assert.That(links.ContainsKey("secrets"));
        }
        
        [Test]
        public async Task FindOrganizationById() 
        {
            var organizationName = GenerateName("Constant Pro");

            var organization = await _organizationClient.CreateOrganization(organizationName);

            var organizationById = await _organizationClient.FindOrganizationById(organization.Id);

            Assert.IsNotNull(organizationById);
            Assert.AreEqual(organizationById.Id, organization.Id);
            Assert.AreEqual(organizationById.Name, organization.Name);
            
            var links = organization.Links;
            
            Assert.That(links.Count == 8);
            Assert.That(links.ContainsKey("buckets"));
            Assert.That(links.ContainsKey("dashboards"));
            Assert.That(links.ContainsKey("log"));
            Assert.That(links.ContainsKey("members"));
            Assert.That(links.ContainsKey("self"));
            Assert.That(links.ContainsKey("tasks"));
            Assert.That(links.ContainsKey("labels"));
            Assert.That(links.ContainsKey("secrets"));
        }
        
        [Test]
        public async Task FindOrganizationByIdNull() 
        {
            var organization = await _organizationClient.FindOrganizationById("020f755c3c082000");

            Assert.IsNull(organization);
        }
        
        [Test]
        public async Task FindOrganizations() 
        {
            var organizations = await _organizationClient.FindOrganizations();
            
            await _organizationClient.CreateOrganization(GenerateName("Constant Pro"));

            var organizationsNew = await _organizationClient.FindOrganizations();
            Assert.That(organizationsNew.Count == organizations.Count + 1);
        }
        
        [Test]
        public async Task DeleteOrganization() 
        {
            var createdOrganization = await _organizationClient.CreateOrganization(GenerateName("Constant Pro"));
            Assert.IsNotNull(createdOrganization);

            var foundOrganization = await _organizationClient.FindOrganizationById(createdOrganization.Id);
            Assert.IsNotNull(foundOrganization);
                            
            // delete task
            await _organizationClient.DeleteOrganization(createdOrganization);

            foundOrganization = await _organizationClient.FindOrganizationById(createdOrganization.Id);
            Assert.IsNull(foundOrganization);
        }
        
        [Test]
        public async Task UpdateOrganization() 
        {
            var createdOrganization = await _organizationClient.CreateOrganization(GenerateName("Constant Pro"));
            createdOrganization.Name = "Master Pb";

            var updatedOrganization = await _organizationClient.UpdateOrganization(createdOrganization);

            Assert.IsNotNull(updatedOrganization);
            Assert.AreEqual(updatedOrganization.Id, createdOrganization.Id);
            Assert.AreEqual(updatedOrganization.Name, "Master Pb");
            
            var links = updatedOrganization.Links;
            
            Assert.That(links.Count == 8);
            Assert.AreEqual("/api/v2/buckets?org=Master Pb", links["buckets"]);
            Assert.AreEqual("/api/v2/dashboards?org=Master Pb", links["dashboards"]);
            Assert.AreEqual("/api/v2/orgs/" + updatedOrganization.Id, links["self"]);
            Assert.AreEqual("/api/v2/tasks?org=Master Pb", links["tasks"]);
            Assert.AreEqual("/api/v2/orgs/" + updatedOrganization.Id + "/members", links["members"]);
            Assert.AreEqual("/api/v2/orgs/" + updatedOrganization.Id + "/log", links["log"]);
            Assert.AreEqual("/api/v2/orgs/" + updatedOrganization.Id + "/labels", links["labels"]);
            Assert.AreEqual("/api/v2/orgs/" + updatedOrganization.Id + "/secrets", links["secrets"]);
        }
        
        [Test]
        public async Task Member() {

            var organization = await _organizationClient.CreateOrganization(GenerateName("Constant Pro"));

            var members =  await _organizationClient.GetMembers(organization);
            Assert.AreEqual(0, members.Count);

            var user = await _userClient.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _organizationClient.AddMember(user, organization);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.UserId, user.Id);
            Assert.AreEqual(resourceMember.UserName, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.UserType.Member);

            members = await _organizationClient.GetMembers(organization);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].UserId, user.Id);
            Assert.AreEqual(members[0].UserName, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.UserType.Member);

            await _organizationClient.DeleteMember(user, organization);

            members = await _organizationClient.GetMembers(organization);
            Assert.AreEqual(0, members.Count);
        }
        
        [Test]
        public async Task Owner() {

            var organization = await _organizationClient.CreateOrganization(GenerateName("Constant Pro"));

            var owners =  await _organizationClient.GetOwners(organization);
            Assert.AreEqual(0, owners.Count);

            var user = await _userClient.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _organizationClient.AddOwner(user, organization);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.UserId, user.Id);
            Assert.AreEqual(resourceMember.UserName, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.UserType.Owner);

            owners = await _organizationClient.GetOwners(organization);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual(owners[0].UserId, user.Id);
            Assert.AreEqual(owners[0].UserName, user.Name);
            Assert.AreEqual(owners[0].Role, ResourceMember.UserType.Owner);

            await _organizationClient.DeleteOwner(user, organization);

            owners = await _organizationClient.GetOwners(organization);
            Assert.AreEqual(0, owners.Count);
        }
        
        [Test]
        public async Task Secrets() {

            var organization = await _organizationClient.CreateOrganization(GenerateName("Constant Pro"));

            var secrets = await _organizationClient.GetSecrets(organization);
            Assert.IsEmpty(secrets);

            var secretsKv = new Dictionary<string, string> {{"gh", "123456789"}, {"az", "987654321"}};

            await _organizationClient.PutSecrets(secretsKv, organization);

            secrets = await _organizationClient.GetSecrets(organization);
            Assert.AreEqual(2, secrets.Count);
            Assert.Contains("gh", secrets);    
            Assert.Contains("az", secrets);

            await _organizationClient.DeleteSecrets(new List<string> {"gh"}, organization);

            secrets = await _organizationClient.GetSecrets(organization);
            Assert.AreEqual(1, secrets.Count);
            Assert.Contains("az", secrets);
        }
        
        [Test]
        public async Task Labels() {

            var labelClient = PlatformClient.CreateLabelClient();

            var organization = await _organizationClient.CreateOrganization(GenerateName("Constant Pro"));

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = await labelClient.CreateLabel(GenerateName("Cool Resource"), properties);

            var labels = await _organizationClient.GetLabels(organization);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = await _organizationClient.AddLabel(label, organization);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels =  await _organizationClient.GetLabels(organization);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            await _organizationClient.DeleteLabel(label, organization);

            labels = await _organizationClient.GetLabels(organization);
            Assert.AreEqual(0, labels.Count);
        }
    }
}