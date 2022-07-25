using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;

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
        public async Task CloneOrganization()
        {
            var source = await _organizationsApi.CreateOrganizationAsync(GenerateName("Constant Pro"));

            var name = GenerateName("cloned");

            var cloned = await _organizationsApi.CloneOrganizationAsync(name, source);

            Assert.AreEqual(name, cloned.Name);
        }

        [Test]
        public void CloneOrganizationNotFound()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _organizationsApi.CloneOrganizationAsync(GenerateName("bucket"), "020f755c3c082000"));

            Assert.AreEqual("organization not found", ioe.Message);
            Assert.AreEqual(typeof(NotFoundException), ioe.GetType());
        }

        [Test]
        public async Task CreateOrganization()
        {
            var now = DateTime.UtcNow;

            var orgName = GenerateName("Constant Pro");

            var organization = await _organizationsApi.CreateOrganizationAsync(orgName);

            Assert.IsNotNull(organization);
            Assert.IsNotEmpty(organization.Id);
            Assert.AreEqual(organization.Name, orgName);
            Assert.Greater(organization.CreatedAt, now);
            Assert.Greater(organization.UpdatedAt, now);

            var links = organization.Links;

            Assert.IsNotNull(links);
            Assert.IsNotNull(links.Buckets);
            Assert.IsNotNull(links.Dashboards);
            Assert.IsNotNull(links.Members);
            Assert.IsNotNull(links.Self);
            Assert.IsNotNull(links.Tasks);
            Assert.IsNotNull(links.Labels);
            Assert.IsNotNull(links.Secrets);
        }

        [Test]
        public async Task DeleteOrganization()
        {
            var createdOrganization = await _organizationsApi.CreateOrganizationAsync(GenerateName("Constant Pro"));
            Assert.IsNotNull(createdOrganization);

            var foundOrganization = await _organizationsApi.FindOrganizationByIdAsync(createdOrganization.Id);
            Assert.IsNotNull(foundOrganization);

            // delete task
            await _organizationsApi.DeleteOrganizationAsync(createdOrganization);

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _organizationsApi.FindOrganizationByIdAsync(createdOrganization.Id));

            Assert.AreEqual("organization not found", ioe.Message);
        }

        [Test]
        public async Task FindOrganizationById()
        {
            var orgName = GenerateName("Constant Pro");

            var organization = await _organizationsApi.CreateOrganizationAsync(orgName);

            var organizationById = await _organizationsApi.FindOrganizationByIdAsync(organization.Id);

            Assert.IsNotNull(organizationById);
            Assert.AreEqual(organizationById.Id, organization.Id);
            Assert.AreEqual(organizationById.Name, organization.Name);

            var links = organization.Links;

            Assert.IsNotNull(links);
            Assert.IsNotNull(links.Buckets);
            Assert.IsNotNull(links.Dashboards);
            Assert.IsNotNull(links.Members);
            Assert.IsNotNull(links.Self);
            Assert.IsNotNull(links.Tasks);
            Assert.IsNotNull(links.Labels);
            Assert.IsNotNull(links.Secrets);
        }

        [Test]
        public void FindOrganizationByIdNull()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _organizationsApi.FindOrganizationByIdAsync("020f755c3c082000"));

            Assert.AreEqual("organization not found", ioe.Message);
        }

        [Test]
        [Ignore("TODO https://github.com/influxdata/influxdb/issues/18048")]
        public async Task FindOrganizations()
        {
            var organizations = (await _organizationsApi.FindOrganizationsAsync()).Count;

            await _organizationsApi.CreateOrganizationAsync(GenerateName("Constant Pro"));

            var organizationsNew = await _organizationsApi.FindOrganizationsAsync();
            Assert.AreEqual(organizationsNew.Count, organizations + 1);
        }

        [Test]
        public async Task Member()
        {
            var organization = await _organizationsApi.CreateOrganizationAsync(GenerateName("Constant Pro"));

            var members = await _organizationsApi.GetMembersAsync(organization);
            Assert.AreEqual(0, members.Count);

            var user = await _usersApi.CreateUserAsync(GenerateName("Luke Health"));

            var resourceMember = await _organizationsApi.AddMemberAsync(user, organization);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.RoleEnum.Member);

            members = await _organizationsApi.GetMembersAsync(organization);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].Id, user.Id);
            Assert.AreEqual(members[0].Name, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.RoleEnum.Member);

            await _organizationsApi.DeleteMemberAsync(user, organization);

            members = await _organizationsApi.GetMembersAsync(organization);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        public async Task Owner()
        {
            var organization = await _organizationsApi.CreateOrganizationAsync(GenerateName("Constant Pro"));

            var owners = await _organizationsApi.GetOwnersAsync(organization);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual("my-user", owners[0].Name);

            var user = await _usersApi.CreateUserAsync(GenerateName("Luke Health"));

            var resourceMember = await _organizationsApi.AddOwnerAsync(user, organization);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceOwner.RoleEnum.Owner);

            owners = await _organizationsApi.GetOwnersAsync(organization);
            Assert.AreEqual(2, owners.Count);
            Assert.AreEqual(owners[1].Id, user.Id);
            Assert.AreEqual(owners[1].Name, user.Name);
            Assert.AreEqual(owners[1].Role, ResourceOwner.RoleEnum.Owner);

            await _organizationsApi.DeleteOwnerAsync(user, organization);

            owners = await _organizationsApi.GetOwnersAsync(organization);
            Assert.AreEqual(1, owners.Count);
        }

        [Test]
        public async Task Secrets()
        {
            var organization = await _organizationsApi.CreateOrganizationAsync(GenerateName("Constant Pro"));

            var secrets = await _organizationsApi.GetSecretsAsync(organization);
            Assert.That(secrets, Is.Null.Or.Empty);

            var secretsKv = new Dictionary<string, string> { { "gh", "123456789" }, { "az", "987654321" } };

            await _organizationsApi.PutSecretsAsync(secretsKv, organization);

            secrets = await _organizationsApi.GetSecretsAsync(organization);
            Assert.AreEqual(2, secrets.Count);
            Assert.Contains("gh", secrets);
            Assert.Contains("az", secrets);

            await _organizationsApi.DeleteSecretsAsync(new List<string> { "gh" }, organization);

            secrets = await _organizationsApi.GetSecretsAsync(organization);
            Assert.AreEqual(1, secrets.Count);
            Assert.Contains("az", secrets);
        }

        [Test]
        public async Task UpdateOrganization()
        {
            var createdOrganization = await _organizationsApi.CreateOrganizationAsync(GenerateName("Constant Pro"));
            var newName = GenerateName("Master Pb");
            createdOrganization.Name = newName;

            var updatedAt = createdOrganization.UpdatedAt;
            var updatedOrganization = await _organizationsApi.UpdateOrganizationAsync(createdOrganization);

            Assert.IsNotNull(updatedOrganization);
            Assert.AreEqual(updatedOrganization.Id, createdOrganization.Id);
            Assert.AreEqual(updatedOrganization.Name, newName);
            Assert.Greater(updatedOrganization.UpdatedAt, updatedAt);

            var links = updatedOrganization.Links;

            Assert.IsNotNull(links);
            Assert.AreEqual(links.Buckets, $"/api/v2/buckets?org={newName}");
            Assert.AreEqual(links.Dashboards, $"/api/v2/dashboards?org={newName}");
            Assert.AreEqual(links.Self, "/api/v2/orgs/" + updatedOrganization.Id);
            Assert.AreEqual(links.Tasks, $"/api/v2/tasks?org={newName}");
            Assert.AreEqual(links.Members, "/api/v2/orgs/" + updatedOrganization.Id + "/members");
            Assert.AreEqual(links.Labels, "/api/v2/orgs/" + updatedOrganization.Id + "/labels");
            Assert.AreEqual(links.Secrets, "/api/v2/orgs/" + updatedOrganization.Id + "/secrets");
        }
    }
}