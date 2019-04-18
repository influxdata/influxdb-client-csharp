using System;
using System.Collections.Generic;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Internal;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;

namespace InfluxDB.Client
{
    public class AuthorizationsApi : AbstractClient
    {
        private readonly AuthorizationsService _service;

        protected internal AuthorizationsApi(AuthorizationsService service)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
        }

        /// <summary>
        /// Create an authorization with defined permissions.
        /// </summary>
        /// <param name="organization">the owner of the authorization</param>
        /// <param name="permissions">the permissions for the authorization</param>
        /// <returns>the created authorization</returns>
        public Authorization CreateAuthorization(Organization organization, List<Permission> permissions)
        {
            Arguments.CheckNotNull(organization, "organization");
            Arguments.CheckNotNull(permissions, "permissions");

            return CreateAuthorization(organization.Id, permissions);
        }

        /// <summary>
        /// Create an authorization with defined permissions.
        /// </summary>
        /// <param name="orgId">the owner id of the authorization</param>
        /// <param name="permissions">the permissions for the authorization</param>
        /// <returns>the created authorization</returns>
        public Authorization CreateAuthorization(string orgId, List<Permission> permissions)
        {
            Arguments.CheckNonEmptyString(orgId, "orgId");
            Arguments.CheckNotNull(permissions, "permissions");

            var authorization =
                new Authorization(orgId, permissions, null, AuthorizationUpdateRequest.StatusEnum.Active);

            return CreateAuthorization(authorization);
        }

        /// <summary>
        /// Create an authorization with defined permissions.
        /// </summary>
        /// <param name="authorization">authorization to create</param>
        /// <returns>the created authorization</returns>
        public Authorization CreateAuthorization(Authorization authorization)
        {
            Arguments.CheckNotNull(authorization, nameof(authorization));

            return _service.AuthorizationsPost(authorization);
        }

        /// <summary>
        /// Updates the status of the authorization. Useful for setting an authorization to inactive or active.
        /// </summary>
        /// <param name="authorization">the authorization with updated status</param>
        /// <returns>the updated authorization</returns>
        public Authorization UpdateAuthorization(Authorization authorization)
        {
            Arguments.CheckNotNull(authorization, nameof(authorization));

            return _service.AuthorizationsPost(authorization);
        }

        /// <summary>
        /// Delete an authorization.
        /// </summary>
        /// <param name="authorization">authorization to delete</param>
        /// <returns>authorization deleted</returns>
        public void DeleteAuthorization(Authorization authorization)
        {
            Arguments.CheckNotNull(authorization, nameof(authorization));

            DeleteAuthorization(authorization.Id);
        }

        /// <summary>
        /// Delete an authorization.
        /// </summary>
        /// <param name="authorizationId">ID of authorization to delete</param>
        /// <returns>authorization deleted</returns>
        public void DeleteAuthorization(string authorizationId)
        {
            Arguments.CheckNonEmptyString(authorizationId, nameof(authorizationId));

            _service.AuthorizationsAuthIDDelete(authorizationId);
        }

        /// <summary>
        /// Clone an authorization.
        /// </summary>
        /// <param name="authorizationId">ID of authorization to clone</param>
        /// <returns>cloned authorization</returns>
        public Authorization CloneAuthorization(string authorizationId)
        {
            Arguments.CheckNonEmptyString(authorizationId, nameof(authorizationId));

            var authorization = FindAuthorizationById(authorizationId);
            if (authorization == null)
            {
                throw new InvalidOperationException($"NotFound Authorization with ID: {authorizationId}");
            }

            return CloneAuthorization(authorization);
        }

        /// <summary>
        /// Clone an authorization.
        /// </summary>
        /// <param name="authorization">authorization to clone</param>
        /// <returns>cloned authorization</returns>
        public Authorization CloneAuthorization(Authorization authorization)
        {
            Arguments.CheckNotNull(authorization, nameof(authorization));

            var cloned = new Authorization(authorization.OrgID, authorization.Permissions, authorization.Links,
                authorization.Status, authorization.Description);

            return CreateAuthorization(cloned);
        }

        /// <summary>
        /// Retrieve an authorization.
        /// </summary>
        /// <param name="authorizationId">ID of authorization to get</param>
        /// <returns>authorization details</returns>
        public Authorization FindAuthorizationById(string authorizationId)
        {
            Arguments.CheckNonEmptyString(authorizationId, nameof(authorizationId));

            return _service.AuthorizationsAuthIDGet(authorizationId);
        }

        /// <summary>
        /// List all authorizations.
        /// </summary>
        /// <returns>List all authorizations.</returns>
        public List<Authorization> FindAuthorizations()
        {
            return FindAuthorizationsBy(null, null);
        }

        /// <summary>
        /// List all authorizations belonging to specified user.
        /// </summary>
        /// <param name="user">user</param>
        /// <returns>A list of authorizations</returns>
        public List<Authorization> FindAuthorizationsByUser(User user)
        {
            Arguments.CheckNotNull(user, "user");

            return FindAuthorizationsByUserId(user.Id);
        }

        /// <summary>
        /// List all authorizations belonging to specified user.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <returns>A list of authorizations</returns>
        public List<Authorization> FindAuthorizationsByUserId(string userId)
        {
            Arguments.CheckNonEmptyString(userId, "User ID");

            return FindAuthorizationsBy(userId, null);
        }

        /// <summary>
        /// List all authorizations belonging to specified user.
        /// </summary>
        /// <param name="userName">Name of User</param>
        /// <returns>A list of authorizations</returns>
        public List<Authorization> FindAuthorizationsByUserName(string userName)
        {
            Arguments.CheckNonEmptyString(userName, "User name");

            return FindAuthorizationsBy(null, userName);
        }

        private List<Authorization> FindAuthorizationsBy(string userId, string userName)
        {
            return _service.AuthorizationsGet(null, userId, userName)._Authorizations;
        }
    }
}