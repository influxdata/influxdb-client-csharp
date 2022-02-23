using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;

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
            _user = await Client.GetUsersApi().MeAsync();
            _organization = await FindMyOrg();
        }

        [Test]
        public async Task CreateAuthorization()
        {
            var readUsers = new Permission(
                Permission.ActionEnum.Read,
                new PermissionResource { Type = PermissionResource.TypeUsers, OrgID = _organization.Id }
            );

            var writeOrganizations = new Permission
            (
                Permission.ActionEnum.Write,
                new PermissionResource { Type = PermissionResource.TypeOrgs, OrgID = _organization.Id }
            );

            var permissions = new List<Permission> { readUsers, writeOrganizations };

            var authorization = await _authorizationsApi.CreateAuthorizationAsync(_organization, permissions);

            Assert.IsNotNull(authorization);
            Assert.IsNotEmpty(authorization.Id);
            Assert.IsNotEmpty(authorization.Token);
            Assert.AreEqual(authorization.UserID, _user.Id);
            Assert.AreEqual(authorization.User, _user.Name);
            Assert.AreEqual(authorization.Status, AuthorizationUpdateRequest.StatusEnum.Active);

            Assert.AreEqual(authorization.Permissions.Count, 2);
            Assert.AreEqual(authorization.Permissions[0].Resource.Type, PermissionResource.TypeUsers);
            Assert.AreEqual(authorization.Permissions[0].Resource.OrgID, _organization.Id);
            Assert.AreEqual(authorization.Permissions[0].Action, Permission.ActionEnum.Read);

            Assert.AreEqual(authorization.Permissions[1].Resource.Type, PermissionResource.TypeOrgs);
            Assert.AreEqual(authorization.Permissions[1].Resource.OrgID, _organization.Id);
            Assert.AreEqual(authorization.Permissions[1].Action, Permission.ActionEnum.Write);

            var links = authorization.Links;

            Assert.IsNotNull(links);
            Assert.AreEqual(links.Self, $"/api/v2/authorizations/{authorization.Id}");
            Assert.AreEqual(links.User, $"/api/v2/users/{_user.Id}");
        }

        [Test]
        public async Task AuthorizationDescription()
        {
            var writeSources = new Permission(Permission.ActionEnum.Write,
                new PermissionResource { Type = PermissionResource.TypeSources, OrgID = _organization.Id }
            );

            var authorization = new AuthorizationPostRequest
            {
                OrgID = _organization.Id,
                Permissions = new List<Permission> { writeSources },
                Description = "My description!"
            };

            var created = await _authorizationsApi.CreateAuthorizationAsync(authorization);

            Assert.IsNotNull(created);
            Assert.AreEqual("My description!", created.Description);
        }

        [Test]
        public async Task UpdateAuthorizationStatus()
        {
            var readUsers = new Permission(Permission.ActionEnum.Read,
                new PermissionResource { Type = PermissionResource.TypeUsers, OrgID = _organization.Id }
            );

            var permissions = new List<Permission> { readUsers };

            var authorization = await _authorizationsApi.CreateAuthorizationAsync(_organization, permissions);

            Assert.AreEqual(authorization.Status, AuthorizationUpdateRequest.StatusEnum.Active);

            authorization.Status = AuthorizationUpdateRequest.StatusEnum.Inactive;
            authorization = await _authorizationsApi.UpdateAuthorizationAsync(authorization);

            Assert.AreEqual(authorization.Status, AuthorizationUpdateRequest.StatusEnum.Inactive);

            authorization.Status = AuthorizationUpdateRequest.StatusEnum.Active;
            authorization = await _authorizationsApi.UpdateAuthorizationAsync(authorization);

            Assert.AreEqual(authorization.Status, AuthorizationUpdateRequest.StatusEnum.Active);
        }

        [Test]
        public async Task FindAuthorizations()
        {
            var size = (await _authorizationsApi.FindAuthorizationsAsync()).Count;

            await _authorizationsApi.CreateAuthorizationAsync(_organization, NewPermissions());

            var authorizations = await _authorizationsApi.FindAuthorizationsAsync();

            Assert.AreEqual(size + 1, authorizations.Count);
        }

        [Test]
        public async Task FindAuthorizationsById()
        {
            var authorization = await _authorizationsApi.CreateAuthorizationAsync(_organization, NewPermissions());

            var foundAuthorization = await _authorizationsApi.FindAuthorizationByIdAsync(authorization.Id);

            Assert.IsNotNull(foundAuthorization);
            Assert.AreEqual(authorization.Id, foundAuthorization.Id);
            Assert.AreEqual(authorization.Token, foundAuthorization.Token);
            Assert.AreEqual(authorization.UserID, foundAuthorization.UserID);
            Assert.AreEqual(authorization.User, foundAuthorization.User);
            Assert.AreEqual(authorization.Status, foundAuthorization.Status);
        }

        [Test]
        public void FindAuthorizationsByIdNull()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _authorizationsApi.FindAuthorizationByIdAsync("020f755c3c082000"));

            Assert.AreEqual("authorization not found", ioe.Message);
        }

        [Test]
        public async Task DeleteAuthorization()
        {
            var createdAuthorization =
                await _authorizationsApi.CreateAuthorizationAsync(_organization, NewPermissions());
            Assert.IsNotNull(createdAuthorization);

            var foundAuthorization = await _authorizationsApi.FindAuthorizationByIdAsync(createdAuthorization.Id);
            Assert.IsNotNull(foundAuthorization);

            // delete authorization
            await _authorizationsApi.DeleteAuthorizationAsync(createdAuthorization);

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _authorizationsApi.FindAuthorizationByIdAsync(createdAuthorization.Id));

            Assert.AreEqual("authorization not found", ioe.Message);
        }

        [Test]
        public async Task FindAuthorizationsByUser()
        {
            var size = (await _authorizationsApi.FindAuthorizationsByUserAsync(_user)).Count;

            await _authorizationsApi.CreateAuthorizationAsync(_organization, NewPermissions());

            var authorizations = await _authorizationsApi.FindAuthorizationsByUserAsync(_user);
            Assert.AreEqual(size + 1, authorizations.Count);
        }

        [Test]
        public async Task FindAuthorizationsByUserName()
        {
            var size = (await _authorizationsApi.FindAuthorizationsByUserAsync(_user)).Count;

            await _authorizationsApi.CreateAuthorizationAsync(_organization, NewPermissions());

            var authorizations = await _authorizationsApi.FindAuthorizationsByUserNameAsync(_user.Name);
            Assert.AreEqual(size + 1, authorizations.Count);
        }

        [Test]
        public async Task CloneAuthorization()
        {
            var source = await _authorizationsApi.CreateAuthorizationAsync(_organization, NewPermissions());

            var cloned = await _authorizationsApi.CloneAuthorizationAsync(source);

            Assert.IsNotEmpty(cloned.Token);
            Assert.AreNotEqual(cloned.Token, source.Token);
            Assert.AreEqual(source.UserID, cloned.UserID);
            Assert.AreEqual(source.User, cloned.User);
            Assert.AreEqual(_organization.Id, cloned.OrgID);
            Assert.AreEqual(_organization.Name, cloned.Org);
            Assert.AreEqual(AuthorizationUpdateRequest.StatusEnum.Active, cloned.Status);
            Assert.AreEqual(source.Description, cloned.Description);
            Assert.AreEqual(1, cloned.Permissions.Count);
            Assert.AreEqual(Permission.ActionEnum.Read, cloned.Permissions[0].Action);
            Assert.AreEqual(PermissionResource.TypeUsers, cloned.Permissions[0].Resource.Type);
            Assert.AreEqual(_organization.Id, cloned.Permissions[0].Resource.OrgID);
        }

        [Test]
        public void CloneAuthorizationNotFound()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _authorizationsApi.CloneAuthorizationAsync("020f755c3c082000"));

            Assert.AreEqual("authorization not found", ioe.Message);
            Assert.AreEqual(typeof(NotFoundException), ioe.GetType());
        }

        private List<Permission> NewPermissions()
        {
            var resource = new PermissionResource
                { Type = PermissionResource.TypeUsers, OrgID = _organization.Id };

            var permission = new Permission(Permission.ActionEnum.Read, resource);

            var permissions = new List<Permission> { permission };

            return permissions;
        }
    }
}