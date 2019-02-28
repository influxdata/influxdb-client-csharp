using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Client.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItOrganizationsApiTest : AbstractItClientTest
    {
        [SetUp]
        public new void SetUp()
        {
            _organizationsApi = Client.GetOrganizationsApi();
            _usersApi = Client.GetUsersApi();
        }

        private OrganizationsApi _organizationsApi;
        private UsersApi _usersApi;

        [Test]
        public async Task CreateOrganization()
        {
            var orgName = GenerateName("Constant Pro");

            var organization = await _organizationsApi.CreateOrganization(orgName);

            Assert.IsNotNull(organization);
            Assert.IsNotEmpty(organization.Id);
            Assert.AreEqual(organization.Name, orgName);

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
        public async Task DeleteOrganization()
        {
            var createdOrganization = await _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));
            Assert.IsNotNull(createdOrganization);

            var foundOrganization = await _organizationsApi.FindOrganizationById(createdOrganization.Id);
            Assert.IsNotNull(foundOrganization);

            // delete task
            await _organizationsApi.DeleteOrganization(createdOrganization);

            foundOrganization = await _organizationsApi.FindOrganizationById(createdOrganization.Id);
            Assert.IsNull(foundOrganization);
        }

        [Test]
        public async Task FindOrganizationById()
        {
            var orgName = GenerateName("Constant Pro");

            var organization = await _organizationsApi.CreateOrganization(orgName);

            var organizationById = await _organizationsApi.FindOrganizationById(organization.Id);

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
            var organization = await _organizationsApi.FindOrganizationById("020f755c3c082000");

            Assert.IsNull(organization);
        }

        [Test]
        public async Task FindOrganizationLogsFindOptionsNotFound()
        {
            var entries = await _organizationsApi.FindOrganizationLogs("020f755c3c082000", new FindOptions());

            Assert.IsNotNull(entries);
            Assert.AreEqual(0, entries.Logs.Count);
        }

        [Test]
        public async Task FindOrganizationLogsNotFound()
        {
            var logs = await _organizationsApi.FindOrganizationLogs("020f755c3c082000");

            Assert.AreEqual(0, logs.Count);
        }

        [Test]
        public async Task FindOrganizationLogsPaging()
        {
            var organization = await _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));

            foreach (var i in Enumerable.Range(0, 19))
            {
                organization.Name = $"{i}_{organization.Name}";

                await _organizationsApi.UpdateOrganization(organization);
            }

            var logs = await _organizationsApi.FindOrganizationLogs(organization);

            Assert.AreEqual(20, logs.Count);
            Assert.AreEqual("Organization Created", logs[0].Description);
            Assert.AreEqual("Organization Updated", logs[19].Description);

            var findOptions = new FindOptions {Limit = 5, Offset = 0};

            var entries = await _organizationsApi.FindOrganizationLogs(organization, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Organization Created", entries.Logs[0].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[1].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[2].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[3].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[4].Description);

            //TODO isNotNull FindOptions also in Log API? 
            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _organizationsApi.FindOrganizationLogs(organization, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Organization Updated", entries.Logs[0].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[1].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[2].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[3].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _organizationsApi.FindOrganizationLogs(organization, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Organization Updated", entries.Logs[0].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[1].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[2].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[3].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _organizationsApi.FindOrganizationLogs(organization, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Organization Updated", entries.Logs[0].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[1].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[2].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[3].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _organizationsApi.FindOrganizationLogs(organization, findOptions);
            Assert.AreEqual(0, entries.Logs.Count);
            Assert.IsNull(entries.GetNextPage());

            //
            // Order
            //
            findOptions = new FindOptions {Descending = false};
            entries = await _organizationsApi.FindOrganizationLogs(organization, findOptions);
            Assert.AreEqual(20, entries.Logs.Count);

            Assert.AreEqual("Organization Updated", entries.Logs[19].Description);
            Assert.AreEqual("Organization Created", entries.Logs[0].Description);
        }

        [Test]
        public async Task FindOrganizations()
        {
            var organizations = await _organizationsApi.FindOrganizations();

            await _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));

            var organizationsNew = await _organizationsApi.FindOrganizations();
            Assert.That(organizationsNew.Count == organizations.Count + 1);
        }

        [Test]
        public async Task Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var organization = await _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = await labelClient.CreateLabel(GenerateName("Cool Resource"), properties);

            var labels = await _organizationsApi.GetLabels(organization);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = await _organizationsApi.AddLabel(label, organization);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = await _organizationsApi.GetLabels(organization);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            await _organizationsApi.DeleteLabel(label, organization);

            labels = await _organizationsApi.GetLabels(organization);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        public async Task Member()
        {
            var organization = await _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));

            var members = await _organizationsApi.GetMembers(organization);
            Assert.AreEqual(0, members.Count);

            var user = await _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _organizationsApi.AddMember(user, organization);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.UserId, user.Id);
            Assert.AreEqual(resourceMember.UserName, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.UserType.Member);

            members = await _organizationsApi.GetMembers(organization);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].UserId, user.Id);
            Assert.AreEqual(members[0].UserName, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.UserType.Member);

            await _organizationsApi.DeleteMember(user, organization);

            members = await _organizationsApi.GetMembers(organization);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        public async Task Owner()
        {
            var organization = await _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));

            var owners = await _organizationsApi.GetOwners(organization);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual("my-user", owners[0].UserName);

            var user = await _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _organizationsApi.AddOwner(user, organization);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.UserId, user.Id);
            Assert.AreEqual(resourceMember.UserName, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.UserType.Owner);

            owners = await _organizationsApi.GetOwners(organization);
            Assert.AreEqual(2, owners.Count);
            Assert.AreEqual(owners[1].UserId, user.Id);
            Assert.AreEqual(owners[1].UserName, user.Name);
            Assert.AreEqual(owners[1].Role, ResourceMember.UserType.Owner);

            await _organizationsApi.DeleteOwner(user, organization);

            owners = await _organizationsApi.GetOwners(organization);
            Assert.AreEqual(1, owners.Count);
        }

        [Test]
        public async Task Secrets()
        {
            var organization = await _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));

            var secrets = await _organizationsApi.GetSecrets(organization);
            Assert.IsEmpty(secrets);

            var secretsKv = new Dictionary<string, string> {{"gh", "123456789"}, {"az", "987654321"}};

            await _organizationsApi.PutSecrets(secretsKv, organization);

            secrets = await _organizationsApi.GetSecrets(organization);
            Assert.AreEqual(2, secrets.Count);
            Assert.Contains("gh", secrets);
            Assert.Contains("az", secrets);

            await _organizationsApi.DeleteSecrets(new List<string> {"gh"}, organization);

            secrets = await _organizationsApi.GetSecrets(organization);
            Assert.AreEqual(1, secrets.Count);
            Assert.Contains("az", secrets);
        }

        [Test]
        public async Task UpdateOrganization()
        {
            var createdOrganization = await _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));
            createdOrganization.Name = "Master Pb";

            var updatedOrganization = await _organizationsApi.UpdateOrganization(createdOrganization);

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
        public async Task CloneOrganization()
        {
            var source = await _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = await Client.GetLabelsApi().CreateLabel(GenerateName("Cool Resource"), properties);
            await _organizationsApi.AddLabel(label, source);

            var name = GenerateName("cloned");
            
            var cloned = await _organizationsApi.CloneOrganization(name, source);
            
            Assert.AreEqual(name, cloned.Name);

            var labels = await _organizationsApi.GetLabels(cloned);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
        }

        [Test]
        public void CloneOrganizationNotFound()
        {
            var ioe = Assert.ThrowsAsync<InvalidOperationException>(async () => await _organizationsApi.CloneOrganization(GenerateName("bucket"),"020f755c3c082000"));
            
            Assert.AreEqual("NotFound Organization with ID: 020f755c3c082000", ioe.Message);
        }
    }
}