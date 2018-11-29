using System.Collections.Generic;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    public class ItAuthorizationClient : AbstractItClientTest
    {
        private AuthorizationClient _authorizationClient;
        private User _user;

        [SetUp]
        public new async Task SetUp()
        {
            _authorizationClient = PlatformClient.CreateAuthorizationClient();
            _user = await PlatformClient.CreateUserClient().CreateUser(GenerateName("Auth User"));
        }

        [Test]
        public async Task CreateAuthorization()
        {
            Permission readUsers = new Permission
            {
                Action = Permission.ReadAction,
                Resource = Permission.UserResource
            };

            Permission writeOrganizations = new Permission
            {
                Action = Permission.WriteAction,
                Resource = Permission.OrganizationResource
            };

            List<Permission> permissions = new List<Permission> {readUsers, writeOrganizations};

            Authorization authorization = await _authorizationClient.CreateAuthorization(_user, permissions);

            Assert.IsNotNull(authorization);
            Assert.IsNotEmpty(authorization.Id);
            Assert.IsNotEmpty(authorization.Token);
            Assert.AreEqual(authorization.UserId, _user.Id);
            Assert.AreEqual(authorization.UserName, _user.Name);
            Assert.AreEqual(authorization.Status, Status.Active);

            Assert.AreEqual(authorization.Permissions.Count, 2);
            Assert.AreEqual(authorization.Permissions[0].Resource, "user");
            Assert.AreEqual(authorization.Permissions[0].Action, "read");
            Assert.AreEqual(authorization.Permissions[1].Resource, "org");
            Assert.AreEqual(authorization.Permissions[1].Action, "write");

            var links = authorization.Links;

            Assert.That(links.Count == 2);
            Assert.AreEqual($"/api/v2/authorizations/{authorization.Id}", links["self"]);
            Assert.AreEqual($"/api/v2/users/{_user.Id}", links["user"]);
        }
    }
}