using System;
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
        public void CloneOrganization()
        {
            var source = _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = Client.GetLabelsApi().CreateLabel(GenerateName("Cool Resource"), properties, source.Id);
            _organizationsApi.AddLabel(label, source);

            var name = GenerateName("cloned");

            var cloned = _organizationsApi.CloneOrganization(name, source);

            Assert.AreEqual(name, cloned.Name);

            var labels = _organizationsApi.GetLabels(cloned);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
        }

        [Test]
        public void CloneOrganizationNotFound()
        {
            var ioe = Assert.Throws<HttpException>(() =>
                _organizationsApi.CloneOrganization(GenerateName("bucket"), "020f755c3c082000"));

            Assert.AreEqual("organization not found", ioe.Message);
        }

        [Test]
        public void CreateOrganization()
        {
            var now = DateTime.UtcNow;
            
            var orgName = GenerateName("Constant Pro");

            var organization = _organizationsApi.CreateOrganization(orgName);

            Assert.IsNotNull(organization);
            Assert.IsNotEmpty(organization.Id);
            Assert.AreEqual(organization.Name, orgName);
            Assert.Greater(organization.CreatedAt, now);
            Assert.Greater(organization.UpdatedAt, now);
            
            var links = organization.Links;

            Assert.IsNotNull(links);
            Assert.IsNotNull(links.Buckets);
            Assert.IsNotNull(links.Dashboards);
            Assert.IsNotNull(links.Logs);
            Assert.IsNotNull(links.Members);
            Assert.IsNotNull(links.Self);
            Assert.IsNotNull(links.Tasks);
            Assert.IsNotNull(links.Labels);
            Assert.IsNotNull(links.Secrets);
        }

        [Test]
        public void DeleteOrganization()
        {
            var createdOrganization = _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));
            Assert.IsNotNull(createdOrganization);

            var foundOrganization = _organizationsApi.FindOrganizationById(createdOrganization.Id);
            Assert.IsNotNull(foundOrganization);

            // delete task
            _organizationsApi.DeleteOrganization(createdOrganization);

            var ioe = Assert.Throws<HttpException>(() =>
                _organizationsApi.FindOrganizationById(createdOrganization.Id));

            Assert.AreEqual("organization not found", ioe.Message);
        }

        [Test]
        public void FindOrganizationById()
        {
            var orgName = GenerateName("Constant Pro");

            var organization = _organizationsApi.CreateOrganization(orgName);

            var organizationById = _organizationsApi.FindOrganizationById(organization.Id);

            Assert.IsNotNull(organizationById);
            Assert.AreEqual(organizationById.Id, organization.Id);
            Assert.AreEqual(organizationById.Name, organization.Name);

            var links = organization.Links;

            Assert.IsNotNull(links);
            Assert.IsNotNull(links.Buckets);
            Assert.IsNotNull(links.Dashboards);
            Assert.IsNotNull(links.Logs);
            Assert.IsNotNull(links.Members);
            Assert.IsNotNull(links.Self);
            Assert.IsNotNull(links.Tasks);
            Assert.IsNotNull(links.Labels);
            Assert.IsNotNull(links.Secrets);
        }

        [Test]
        public void FindOrganizationByIdNull()
        {
            var ioe = Assert.Throws<HttpException>(() => _organizationsApi.FindOrganizationById("020f755c3c082000"));

            Assert.AreEqual("organization not found", ioe.Message);
        }

        [Test]
        public void FindOrganizationLogsFindOptionsNotFound()
        {
            var entries = _organizationsApi.FindOrganizationLogs("020f755c3c082000", new FindOptions());

            Assert.IsNotNull(entries);
            Assert.AreEqual(0, entries.Logs.Count);
        }

        [Test]
        public void FindOrganizationLogsNotFound()
        {
            var logs = _organizationsApi.FindOrganizationLogs("020f755c3c082000");

            Assert.AreEqual(0, logs.Count);
        }

        [Test]
        public void FindOrganizationLogsPaging()
        {
            var organization = _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));

            foreach (var i in Enumerable.Range(0, 19))
            {
                organization.Name = $"{i}_{organization.Name}";

                _organizationsApi.UpdateOrganization(organization);
            }

            var logs = _organizationsApi.FindOrganizationLogs(organization);

            Assert.AreEqual(20, logs.Count);
            Assert.AreEqual("Organization Created", logs[0].Description);
            Assert.AreEqual("Organization Updated", logs[19].Description);

            var findOptions = new FindOptions {Limit = 5, Offset = 0};

            var entries = _organizationsApi.FindOrganizationLogs(organization, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Organization Created", entries.Logs[0].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[1].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[2].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[3].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;

            entries = _organizationsApi.FindOrganizationLogs(organization, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Organization Updated", entries.Logs[0].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[1].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[2].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[3].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;

            entries = _organizationsApi.FindOrganizationLogs(organization, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Organization Updated", entries.Logs[0].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[1].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[2].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[3].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;

            entries = _organizationsApi.FindOrganizationLogs(organization, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("Organization Updated", entries.Logs[0].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[1].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[2].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[3].Description);
            Assert.AreEqual("Organization Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;

            entries = _organizationsApi.FindOrganizationLogs(organization, findOptions);
            Assert.AreEqual(0, entries.Logs.Count);

            //
            // Order
            //
            findOptions = new FindOptions {Descending = false};
            entries = _organizationsApi.FindOrganizationLogs(organization, findOptions);
            Assert.AreEqual(20, entries.Logs.Count);

            Assert.AreEqual("Organization Updated", entries.Logs[19].Description);
            Assert.AreEqual("Organization Created", entries.Logs[0].Description);
        }

        [Test]
        public void FindOrganizations()
        {
            var organizations = _organizationsApi.FindOrganizations();

            _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));

            var organizationsNew = _organizationsApi.FindOrganizations();
            Assert.That(organizationsNew.Count == organizations.Count + 1);
        }

        [Test]
        public void Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var organization = _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = labelClient.CreateLabel(GenerateName("Cool Resource"), properties, organization.Id);

            var labels = _organizationsApi.GetLabels(organization);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = _organizationsApi.AddLabel(label, organization);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = _organizationsApi.GetLabels(organization);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            _organizationsApi.DeleteLabel(label, organization);

            labels = _organizationsApi.GetLabels(organization);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        public void Member()
        {
            var organization = _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));

            var members = _organizationsApi.GetMembers(organization);
            Assert.AreEqual(0, members.Count);

            var user = _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = _organizationsApi.AddMember(user, organization);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.RoleEnum.Member);

            members = _organizationsApi.GetMembers(organization);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].Id, user.Id);
            Assert.AreEqual(members[0].Name, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.RoleEnum.Member);

            _organizationsApi.DeleteMember(user, organization);

            members = _organizationsApi.GetMembers(organization);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        public void Owner()
        {
            var organization = _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));

            var owners = _organizationsApi.GetOwners(organization);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual("my-user", owners[0].Name);

            var user = _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = _organizationsApi.AddOwner(user, organization);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceOwner.RoleEnum.Owner);

            owners = _organizationsApi.GetOwners(organization);
            Assert.AreEqual(2, owners.Count);
            Assert.AreEqual(owners[1].Id, user.Id);
            Assert.AreEqual(owners[1].Name, user.Name);
            Assert.AreEqual(owners[1].Role, ResourceOwner.RoleEnum.Owner);

            _organizationsApi.DeleteOwner(user, organization);

            owners = _organizationsApi.GetOwners(organization);
            Assert.AreEqual(1, owners.Count);
        }

        [Test]
        public void Secrets()
        {
            var organization = _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));

            var secrets = _organizationsApi.GetSecrets(organization);
            Assert.IsEmpty(secrets);

            var secretsKv = new Dictionary<string, string> {{"gh", "123456789"}, {"az", "987654321"}};

            _organizationsApi.PutSecrets(secretsKv, organization);

            secrets = _organizationsApi.GetSecrets(organization);
            Assert.AreEqual(2, secrets.Count);
            Assert.Contains("gh", secrets);
            Assert.Contains("az", secrets);

            _organizationsApi.DeleteSecrets(new List<string> {"gh"}, organization);

            secrets = _organizationsApi.GetSecrets(organization);
            Assert.AreEqual(1, secrets.Count);
            Assert.Contains("az", secrets);
        }

        [Test]
        public void UpdateOrganization()
        {
            var createdOrganization = _organizationsApi.CreateOrganization(GenerateName("Constant Pro"));
            var newName = GenerateName("Master Pb");
            createdOrganization.Name = newName;

            var updatedAt = createdOrganization.UpdatedAt;
            var updatedOrganization = _organizationsApi.UpdateOrganization(createdOrganization);

            Assert.IsNotNull(updatedOrganization);
            Assert.AreEqual(updatedOrganization.Id, createdOrganization.Id);
            Assert.AreEqual(updatedOrganization.Name,  newName);
            Assert.Greater(updatedOrganization.UpdatedAt, updatedAt);

            var links = updatedOrganization.Links;

            Assert.IsNotNull(links);
            Assert.AreEqual(links.Buckets, $"/api/v2/buckets?org={newName}");
            Assert.AreEqual(links.Dashboards, $"/api/v2/dashboards?org={newName}");
            Assert.AreEqual(links.Self, "/api/v2/orgs/" + updatedOrganization.Id);
            Assert.AreEqual(links.Tasks, $"/api/v2/tasks?org={newName}");
            Assert.AreEqual(links.Members, "/api/v2/orgs/" + updatedOrganization.Id + "/members");
            Assert.AreEqual(links.Logs, "/api/v2/orgs/" + updatedOrganization.Id + "/logs");
            Assert.AreEqual(links.Labels, "/api/v2/orgs/" + updatedOrganization.Id + "/labels");
            Assert.AreEqual(links.Secrets, "/api/v2/orgs/" + updatedOrganization.Id + "/secrets");
        }
    }
}