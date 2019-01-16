using System.Collections.Generic;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    [TestFixture]
    public class ItAuthorizationClient : AbstractItClientTest
    {
        private AuthorizationClient _authorizationClient;
        private User _user;
        private Organization _organization;

        [SetUp]
        public new async Task SetUp()
        {
            _authorizationClient = PlatformClient.CreateAuthorizationClient();
            _user = await PlatformClient.CreateUserClient().Me();
            _organization = await FindMyOrg();
        }

        [Test]
        public async Task CreateAuthorization()
        {
            var readUsers = new Permission
            {
                Action = Permission.ReadAction,
                Resource = new PermissionResource {Type = PermissionResourceType.User, OrgId = _organization.Id}
            };

            var writeOrganizations = new Permission
            {
                Action = Permission.WriteAction,
                Resource = new PermissionResource {Type = PermissionResourceType.Org, OrgId = _organization.Id}
            };

            var permissions = new List<Permission> {readUsers, writeOrganizations};

            var authorization = await _authorizationClient.CreateAuthorization(_organization, permissions);

            Assert.IsNotNull(authorization);
            Assert.IsNotEmpty(authorization.Id);
            Assert.IsNotEmpty(authorization.Token);
            Assert.AreEqual(authorization.UserId, _user.Id);
            Assert.AreEqual(authorization.UserName, _user.Name);
            Assert.AreEqual(authorization.Status, Status.Active);

            Assert.AreEqual(authorization.Permissions.Count, 2);
            Assert.AreEqual(authorization.Permissions[0].Resource.Type, PermissionResourceType.User);
            Assert.AreEqual(authorization.Permissions[0].Resource.OrgId, _organization.Id);
            Assert.AreEqual(authorization.Permissions[0].Action, "read");
            
            Assert.AreEqual(authorization.Permissions[1].Resource.Type, PermissionResourceType.Org);
            Assert.AreEqual(authorization.Permissions[1].Resource.OrgId, _organization.Id);
            Assert.AreEqual(authorization.Permissions[1].Action, "write");

            var links = authorization.Links;

            Assert.That(links.Count == 2);
            Assert.AreEqual($"/api/v2/authorizations/{authorization.Id}", links["self"]);
            Assert.AreEqual($"/api/v2/users/{_user.Id}", links["user"]);
        }
        
        [Test]
        public async Task AuthorizationDescription() {

            var writeSources = new Permission
            {
                Action = Permission.WriteAction,
                Resource = new PermissionResource {Type = PermissionResourceType.Source, OrgId = _organization.Id}
            };

            var authorization = new Authorization
            {
                OrgId = _organization.Id,
                Permissions = new List<Permission> {writeSources},
                Description = "My description!"
            };

            var created = await _authorizationClient.CreateAuthorization(authorization);

            Assert.IsNotNull(created);
            Assert.AreEqual("My description!", created.Description);
        }
        
        [Test]
        public async Task UpdateAuthorizationStatus() {

            var readUsers = new Permission
            {
                Action = Permission.ReadAction,
                Resource = new PermissionResource {Type = PermissionResourceType.User, OrgId = _organization.Id}
            };

            var permissions = new List<Permission> {readUsers};

            var authorization = await _authorizationClient.CreateAuthorization(_organization, permissions);

            Assert.AreEqual(authorization.Status, Status.Active);

            authorization.Status = Status.Inactive;
            authorization = await _authorizationClient.UpdateAuthorization(authorization);

            Assert.AreEqual(authorization.Status, Status.Inactive);

            authorization.Status = Status.Active;
            authorization = await _authorizationClient.UpdateAuthorization(authorization);

            Assert.AreEqual(authorization.Status, Status.Active);
        }
        
        [Test]
        public async Task FindAuthorizations() {

            var size = (await _authorizationClient.FindAuthorizations()).Count;

            await _authorizationClient.CreateAuthorization(_organization, NewPermissions());

            var authorizations = await _authorizationClient.FindAuthorizations();
            
            Assert.AreEqual(size + 1, authorizations.Count);
        }

        [Test]
        public async Task FindAuthorizationsById() {

            var authorization = await _authorizationClient.CreateAuthorization(_organization, NewPermissions());

            var foundAuthorization = await _authorizationClient.FindAuthorizationById(authorization.Id);

            Assert.IsNotNull(foundAuthorization);
            Assert.AreEqual(authorization.Id, foundAuthorization.Id);
            Assert.AreEqual(authorization.Token, foundAuthorization.Token);
            Assert.AreEqual(authorization.UserId, foundAuthorization.UserId);
            Assert.AreEqual(authorization.UserName, foundAuthorization.UserName);
            Assert.AreEqual(authorization.Status, foundAuthorization.Status);
        }

        [Test]
        public async Task FindAuthorizationsByIdNull() {

            var authorization = await _authorizationClient.FindAuthorizationById("020f755c3c082000");

            Assert.IsNull(authorization);
        }
        
        [Test]
        public async Task DeleteAuthorization() {

            var createdAuthorization = await _authorizationClient.CreateAuthorization(_organization, NewPermissions());
            Assert.IsNotNull(createdAuthorization);

            var foundAuthorization = await _authorizationClient.FindAuthorizationById(createdAuthorization.Id);
            Assert.IsNotNull(foundAuthorization);

            // delete authorization
            await _authorizationClient.DeleteAuthorization(createdAuthorization);

            foundAuthorization = await _authorizationClient.FindAuthorizationById(createdAuthorization.Id);
            Assert.IsNull(foundAuthorization);
        }
        
        [Test]
        public async Task  FindAuthorizationsByUser()
        {
            var size = (await _authorizationClient.FindAuthorizationsByUser(_user)).Count;

            await _authorizationClient.CreateAuthorization(_organization, NewPermissions());

            var authorizations = await _authorizationClient.FindAuthorizationsByUser(_user);
            Assert.AreEqual(size + 1, authorizations.Count);
        }

        [Test]
        public async Task  FindAuthorizationsByUserName() {

            var size = (await _authorizationClient.FindAuthorizationsByUser(_user)).Count;

            await _authorizationClient.CreateAuthorization(_organization, NewPermissions());

            var authorizations = await _authorizationClient.FindAuthorizationsByUserName(_user.Name);
            Assert.AreEqual(size + 1, authorizations.Count);
        }
        
        private List<Permission> NewPermissions()
        {
            var resource = new PermissionResource {Type = PermissionResourceType.User, OrgId = _organization.Id};

            var permission = new Permission
            {
                Action = Permission.ReadAction, 
                Resource = resource
            };

            var permissions = new List<Permission> {permission};

            return permissions;
        }
    }
}