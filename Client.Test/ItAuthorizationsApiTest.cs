using System;
using System.Collections.Generic;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Api.Domain;
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
        public new void SetUp()
        {
            _authorizationsApi = Client.GetAuthorizationsApi();
            _user = Client.GetUsersApi().Me();
            _organization = FindMyOrg();
        }

        [Test]
        public void CreateAuthorization()
        {
            var readUsers = new Permission(
                Permission.ActionEnum.Read,
                new PermissionResource {Type = PermissionResource.TypeEnum.Users, OrgID = _organization.Id}
            );

            var writeOrganizations = new Permission
            (
                Permission.ActionEnum.Write,
                new PermissionResource {Type = PermissionResource.TypeEnum.Orgs, OrgID = _organization.Id}
            );

            var permissions = new List<Permission> {readUsers, writeOrganizations};

            var authorization = _authorizationsApi.CreateAuthorization(_organization, permissions);

            Assert.IsNotNull(authorization);
            Assert.IsNotEmpty(authorization.Id);
            Assert.IsNotEmpty(authorization.Token);
            Assert.AreEqual(authorization.UserID, _user.Id);
            Assert.AreEqual(authorization.User, _user.Name);
            Assert.AreEqual(authorization.Status, AuthorizationUpdateRequest.StatusEnum.Active);

            Assert.AreEqual(authorization.Permissions.Count, 2);
            Assert.AreEqual(authorization.Permissions[0].Resource.Type, PermissionResource.TypeEnum.Users);
            Assert.AreEqual(authorization.Permissions[0].Resource.OrgID, _organization.Id);
            Assert.AreEqual(authorization.Permissions[0].Action, Permission.ActionEnum.Read);

            Assert.AreEqual(authorization.Permissions[1].Resource.Type, PermissionResource.TypeEnum.Orgs);
            Assert.AreEqual(authorization.Permissions[1].Resource.OrgID, _organization.Id);
            Assert.AreEqual(authorization.Permissions[1].Action, Permission.ActionEnum.Write);

            var links = authorization.Links;

            Assert.IsNotNull(links);
            Assert.AreEqual(links.Self, $"/api/v2/authorizations/{authorization.Id}");
            Assert.AreEqual(links.User, $"/api/v2/users/{_user.Id}");
        }

        [Test]
        public void AuthorizationDescription()
        {
            var writeSources = new Permission(Permission.ActionEnum.Write,
                new PermissionResource {Type = PermissionResource.TypeEnum.Sources, OrgID = _organization.Id}
            );

            var authorization = new Authorization
            {
                OrgID = _organization.Id,
                Permissions = new List<Permission> {writeSources},
                Description = "My description!"
            };

            var created = _authorizationsApi.CreateAuthorization(authorization);

            Assert.IsNotNull(created);
            Assert.AreEqual("My description!", created.Description);
        }

        [Test]
        public void UpdateAuthorizationStatus()
        {
            var readUsers = new Permission(Permission.ActionEnum.Read,
                new PermissionResource {Type = PermissionResource.TypeEnum.Users, OrgID = _organization.Id}
            );

            var permissions = new List<Permission> {readUsers};

            var authorization = _authorizationsApi.CreateAuthorization(_organization, permissions);

            Assert.AreEqual(authorization.Status, AuthorizationUpdateRequest.StatusEnum.Active);

            authorization.Status = AuthorizationUpdateRequest.StatusEnum.Inactive;
            authorization = _authorizationsApi.UpdateAuthorization(authorization);

            Assert.AreEqual(authorization.Status, AuthorizationUpdateRequest.StatusEnum.Inactive);

            authorization.Status = AuthorizationUpdateRequest.StatusEnum.Active;
            authorization = _authorizationsApi.UpdateAuthorization(authorization);

            Assert.AreEqual(authorization.Status, AuthorizationUpdateRequest.StatusEnum.Active);
        }

        [Test]
        public void FindAuthorizations()
        {
            var size = _authorizationsApi.FindAuthorizations().Count;

            _authorizationsApi.CreateAuthorization(_organization, NewPermissions());

            var authorizations = _authorizationsApi.FindAuthorizations();

            Assert.AreEqual(size + 1, authorizations.Count);
        }

        [Test]
        public void FindAuthorizationsById()
        {
            var authorization = _authorizationsApi.CreateAuthorization(_organization, NewPermissions());

            var foundAuthorization = _authorizationsApi.FindAuthorizationById(authorization.Id);

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
            var ioe = Assert.Throws<HttpException>(() =>
                _authorizationsApi.FindAuthorizationById("020f755c3c082000"));

            Assert.AreEqual("authorization not found", ioe.Message);
        }

        [Test]
        public void DeleteAuthorization()
        {
            var createdAuthorization = _authorizationsApi.CreateAuthorization(_organization, NewPermissions());
            Assert.IsNotNull(createdAuthorization);

            var foundAuthorization = _authorizationsApi.FindAuthorizationById(createdAuthorization.Id);
            Assert.IsNotNull(foundAuthorization);

            // delete authorization
            _authorizationsApi.DeleteAuthorization(createdAuthorization);

            var ioe = Assert.Throws<HttpException>(() =>
                _authorizationsApi.FindAuthorizationById(createdAuthorization.Id));

            Assert.AreEqual("authorization not found", ioe.Message);
        }

        [Test]
        public void FindAuthorizationsByUser()
        {
            var size = _authorizationsApi.FindAuthorizationsByUser(_user).Count;

            _authorizationsApi.CreateAuthorization(_organization, NewPermissions());

            var authorizations = _authorizationsApi.FindAuthorizationsByUser(_user);
            Assert.AreEqual(size + 1, authorizations.Count);
        }

        [Test]
        public void FindAuthorizationsByUserName()
        {
            var size = _authorizationsApi.FindAuthorizationsByUser(_user).Count;

            _authorizationsApi.CreateAuthorization(_organization, NewPermissions());

            var authorizations = _authorizationsApi.FindAuthorizationsByUserName(_user.Name);
            Assert.AreEqual(size + 1, authorizations.Count);
        }

        [Test]
        public void CloneAuthorization()
        {
            var source = _authorizationsApi.CreateAuthorization(_organization, NewPermissions());

            var cloned = _authorizationsApi.CloneAuthorization(source);

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
            Assert.AreEqual(PermissionResource.TypeEnum.Users, cloned.Permissions[0].Resource.Type);
            Assert.AreEqual(_organization.Id, cloned.Permissions[0].Resource.OrgID);
        }

        [Test]
        public void CloneAuthorizationNotFound()
        {
            var ioe = Assert.Throws<HttpException>(() =>
                _authorizationsApi.CloneAuthorization("020f755c3c082000"));

            Assert.AreEqual("authorization not found", ioe.Message);
        }

        private List<Permission> NewPermissions()
        {
            var resource = new PermissionResource {Type = PermissionResource.TypeEnum.Users, OrgID = _organization.Id};

            var permission = new Permission(Permission.ActionEnum.Read, resource);

            var permissions = new List<Permission> {permission};

            return permissions;
        }
    }
}