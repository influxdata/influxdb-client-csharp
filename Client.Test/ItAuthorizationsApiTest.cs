using System;
using System.Collections.Generic;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Generated.Domain;
using NUnit.Framework;
using Authorization = InfluxDB.Client.Domain.Authorization;
using Organization = InfluxDB.Client.Domain.Organization;
using Permission = InfluxDB.Client.Domain.Permission;
using PermissionResource = InfluxDB.Client.Domain.PermissionResource;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItAuthorizationsApiTest : AbstractItClientTest
    {
        private AuthorizationsApi _authorizationsApi;
        private User _user;
        private Organization _organization;

        [SetUp]
        public new async Task SetUp()
        {
            _authorizationsApi = Client.GetAuthorizationsApi();
            _user = Client.GetUsersApi().Me();
            _organization = await FindMyOrg();
        }

        [Test]
        public async Task CreateAuthorization()
        {
            var readUsers = new Permission
            {
                Action = Permission.ReadAction,
                Resource = new PermissionResource {Type = ResourceType.Users, OrgId = _organization.Id}
            };

            var writeOrganizations = new Permission
            {
                Action = Permission.WriteAction,
                Resource = new PermissionResource {Type = ResourceType.Orgs, OrgId = _organization.Id}
            };

            var permissions = new List<Permission> {readUsers, writeOrganizations};

            var authorization = await _authorizationsApi.CreateAuthorization(_organization, permissions);

            Assert.IsNotNull(authorization);
            Assert.IsNotEmpty(authorization.Id);
            Assert.IsNotEmpty(authorization.Token);
            Assert.AreEqual(authorization.UserId, _user.Id);
            Assert.AreEqual(authorization.UserName, _user.Name);
            Assert.AreEqual(authorization.Status, Status.Active);

            Assert.AreEqual(authorization.Permissions.Count, 2);
            Assert.AreEqual(authorization.Permissions[0].Resource.Type, ResourceType.Users);
            Assert.AreEqual(authorization.Permissions[0].Resource.OrgId, _organization.Id);
            Assert.AreEqual(authorization.Permissions[0].Action, "read");
            
            Assert.AreEqual(authorization.Permissions[1].Resource.Type, ResourceType.Orgs);
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
                Resource = new PermissionResource {Type = ResourceType.Sources, OrgId = _organization.Id}
            };

            var authorization = new Authorization
            {
                OrgId = _organization.Id,
                Permissions = new List<Permission> {writeSources},
                Description = "My description!"
            };

            var created = await _authorizationsApi.CreateAuthorization(authorization);

            Assert.IsNotNull(created);
            Assert.AreEqual("My description!", created.Description);
        }
        
        [Test]
        public async Task UpdateAuthorizationStatus() {

            var readUsers = new Permission
            {
                Action = Permission.ReadAction,
                Resource = new PermissionResource {Type = ResourceType.Users, OrgId = _organization.Id}
            };

            var permissions = new List<Permission> {readUsers};

            var authorization = await _authorizationsApi.CreateAuthorization(_organization, permissions);

            Assert.AreEqual(authorization.Status, Status.Active);

            authorization.Status = Status.Inactive;
            authorization = await _authorizationsApi.UpdateAuthorization(authorization);

            Assert.AreEqual(authorization.Status, Status.Inactive);

            authorization.Status = Status.Active;
            authorization = await _authorizationsApi.UpdateAuthorization(authorization);

            Assert.AreEqual(authorization.Status, Status.Active);
        }
        
        [Test]
        public async Task FindAuthorizations() {

            var size = (await _authorizationsApi.FindAuthorizations()).Count;

            await _authorizationsApi.CreateAuthorization(_organization, NewPermissions());

            var authorizations = await _authorizationsApi.FindAuthorizations();
            
            Assert.AreEqual(size + 1, authorizations.Count);
        }

        [Test]
        public async Task FindAuthorizationsById() {

            var authorization = await _authorizationsApi.CreateAuthorization(_organization, NewPermissions());

            var foundAuthorization = await _authorizationsApi.FindAuthorizationById(authorization.Id);

            Assert.IsNotNull(foundAuthorization);
            Assert.AreEqual(authorization.Id, foundAuthorization.Id);
            Assert.AreEqual(authorization.Token, foundAuthorization.Token);
            Assert.AreEqual(authorization.UserId, foundAuthorization.UserId);
            Assert.AreEqual(authorization.UserName, foundAuthorization.UserName);
            Assert.AreEqual(authorization.Status, foundAuthorization.Status);
        }

        [Test]
        public async Task FindAuthorizationsByIdNull() {

            var authorization = await _authorizationsApi.FindAuthorizationById("020f755c3c082000");

            Assert.IsNull(authorization);
        }
        
        [Test]
        public async Task DeleteAuthorization() {

            var createdAuthorization = await _authorizationsApi.CreateAuthorization(_organization, NewPermissions());
            Assert.IsNotNull(createdAuthorization);

            var foundAuthorization = await _authorizationsApi.FindAuthorizationById(createdAuthorization.Id);
            Assert.IsNotNull(foundAuthorization);

            // delete authorization
            await _authorizationsApi.DeleteAuthorization(createdAuthorization);

            foundAuthorization = await _authorizationsApi.FindAuthorizationById(createdAuthorization.Id);
            Assert.IsNull(foundAuthorization);
        }
        
        [Test]
        public async Task  FindAuthorizationsByUser()
        {
            var size = (await _authorizationsApi.FindAuthorizationsByUser(_user)).Count;

            await _authorizationsApi.CreateAuthorization(_organization, NewPermissions());

            var authorizations = await _authorizationsApi.FindAuthorizationsByUser(_user);
            Assert.AreEqual(size + 1, authorizations.Count);
        }

        [Test]
        public async Task  FindAuthorizationsByUserName() {

            var size = (await _authorizationsApi.FindAuthorizationsByUser(_user)).Count;

            await _authorizationsApi.CreateAuthorization(_organization, NewPermissions());

            var authorizations = await _authorizationsApi.FindAuthorizationsByUserName(_user.Name);
            Assert.AreEqual(size + 1, authorizations.Count);
        }

        [Test]
        public async Task CloneAuthorization()
        {
            var source = await _authorizationsApi.CreateAuthorization(_organization, NewPermissions());

            var cloned = await _authorizationsApi.CloneAuthorization(source);
            
            Assert.IsNotEmpty(cloned.Token);
            Assert.AreNotEqual(cloned.Token, source.Token);
            Assert.AreEqual(source.UserId, cloned.UserId);
            Assert.AreEqual(source.UserName, cloned.UserName);
            Assert.AreEqual(_organization.Id, cloned.OrgId);
            Assert.AreEqual(_organization.Name, cloned.OrgName);
            Assert.AreEqual(Status.Active, cloned.Status);
            Assert.AreEqual(source.Description, cloned.Description);
            Assert.AreEqual(1, cloned.Permissions.Count);
            Assert.AreEqual(Permission.ReadAction, cloned.Permissions[0].Action);
            Assert.AreEqual(ResourceType.Users, cloned.Permissions[0].Resource.Type);
            Assert.AreEqual(_organization.Id, cloned.Permissions[0].Resource.OrgId);
        }

        [Test]
        public void CloneAuthorizationNotFound()
        {
            var ioe = Assert.ThrowsAsync<InvalidOperationException>(async () => await _authorizationsApi.CloneAuthorization("020f755c3c082000"));
            
            Assert.AreEqual("NotFound Authorization with ID: 020f755c3c082000", ioe.Message);
        }

        private List<Permission> NewPermissions()
        {
            var resource = new PermissionResource {Type = ResourceType.Users, OrgId = _organization.Id};

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