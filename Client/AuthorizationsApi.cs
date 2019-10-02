using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client
{
    public class AuthorizationsApi
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
        public async Task<Authorization> CreateAuthorizationAsync(Organization organization, List<Permission> permissions)
        {
            Arguments.CheckNotNull(organization, "organization");
            Arguments.CheckNotNull(permissions, "permissions");

            return await CreateAuthorizationAsync(organization.Id, permissions);
        }

        /// <summary>
        /// Create an authorization with defined permissions.
        /// </summary>
        /// <param name="orgId">the owner id of the authorization</param>
        /// <param name="permissions">the permissions for the authorization</param>
        /// <returns>the created authorization</returns>
        public async Task<Authorization> CreateAuthorizationAsync(string orgId, List<Permission> permissions)
        {
            Arguments.CheckNonEmptyString(orgId, "orgId");
            Arguments.CheckNotNull(permissions, "permissions");

            var authorization =
                new Authorization(orgId, permissions, null, AuthorizationUpdateRequest.StatusEnum.Active);

            return await CreateAuthorizationAsync(authorization);
        }

        /// <summary>
        /// Create an authorization with defined permissions.
        /// </summary>
        /// <param name="authorization">authorization to create</param>
        /// <returns>the created authorization</returns>
        public async Task<Authorization> CreateAuthorizationAsync(Authorization authorization)
        {
            Arguments.CheckNotNull(authorization, nameof(authorization));

            return await _service.PostAuthorizationsAsync(authorization);
        }

        /// <summary>
        /// Updates the status of the authorization. Useful for setting an authorization to inactive or active.
        /// </summary>
        /// <param name="authorization">the authorization with updated status</param>
        /// <returns>the updated authorization</returns>
        public async Task<Authorization> UpdateAuthorizationAsync(Authorization authorization)
        {
            Arguments.CheckNotNull(authorization, nameof(authorization));

            var request = new AuthorizationUpdateRequest(authorization.Status, authorization.Description);

            return await _service.PatchAuthorizationsIDAsync(authorization.Id, request);
        }

        /// <summary>
        /// Delete an authorization.
        /// </summary>
        /// <param name="authorization">authorization to delete</param>
        /// <returns>authorization deleted</returns>
        public async Task DeleteAuthorizationAsync(Authorization authorization)
        {
            Arguments.CheckNotNull(authorization, nameof(authorization));

            await DeleteAuthorizationAsync(authorization.Id);
        }

        /// <summary>
        /// Delete an authorization.
        /// </summary>
        /// <param name="authorizationId">ID of authorization to delete</param>
        /// <returns>authorization deleted</returns>
        public async Task DeleteAuthorizationAsync(string authorizationId)
        {
            Arguments.CheckNonEmptyString(authorizationId, nameof(authorizationId));

            await _service.DeleteAuthorizationsIDAsync(authorizationId);
        }

        /// <summary>
        /// Clone an authorization.
        /// </summary>
        /// <param name="authorizationId">ID of authorization to clone</param>
        /// <returns>cloned authorization</returns>
        public async Task<Authorization> CloneAuthorizationAsync(string authorizationId)
        {
            Arguments.CheckNonEmptyString(authorizationId, nameof(authorizationId));

            return await FindAuthorizationByIdAsync(authorizationId).ContinueWith(t => CloneAuthorizationAsync(t.Result))
                .Unwrap();
        }

        /// <summary>
        /// Clone an authorization.
        /// </summary>
        /// <param name="authorization">authorization to clone</param>
        /// <returns>cloned authorization</returns>
        public async Task<Authorization> CloneAuthorizationAsync(Authorization authorization)
        {
            Arguments.CheckNotNull(authorization, nameof(authorization));

            var cloned = new Authorization(authorization.OrgID, authorization.Permissions, authorization.Links,
                authorization.Status, authorization.Description);

            return await CreateAuthorizationAsync(cloned);
        }

        /// <summary>
        /// Retrieve an authorization.
        /// </summary>
        /// <param name="authorizationId">ID of authorization to get</param>
        /// <returns>authorization details</returns>
        public async Task<Authorization> FindAuthorizationByIdAsync(string authorizationId)
        {
            Arguments.CheckNonEmptyString(authorizationId, nameof(authorizationId));

            return await _service.GetAuthorizationsIDAsync(authorizationId);
        }

        /// <summary>
        /// List all authorizations.
        /// </summary>
        /// <returns>List all authorizations.</returns>
        public async Task<List<Authorization>> FindAuthorizationsAsync()
        {
            return await FindAuthorizationsByAsync(null, null);
        }

        /// <summary>
        /// List all authorizations belonging to specified user.
        /// </summary>
        /// <param name="user">user</param>
        /// <returns>A list of authorizations</returns>
        public async Task<List<Authorization>> FindAuthorizationsByUserAsync(User user)
        {
            Arguments.CheckNotNull(user, "user");

            return await FindAuthorizationsByUserIdAsync(user.Id);
        }

        /// <summary>
        /// List all authorizations belonging to specified user.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <returns>A list of authorizations</returns>
        public async Task<List<Authorization>> FindAuthorizationsByUserIdAsync(string userId)
        {
            Arguments.CheckNonEmptyString(userId, "User ID");

            return await FindAuthorizationsByAsync(userId, null);
        }

        /// <summary>
        /// List all authorizations belonging to specified user.
        /// </summary>
        /// <param name="userName">Name of User</param>
        /// <returns>A list of authorizations</returns>
        public async Task<List<Authorization>> FindAuthorizationsByUserNameAsync(string userName)
        {
            Arguments.CheckNonEmptyString(userName, "User name");

            return await FindAuthorizationsByAsync(null, userName);
        }

        private async Task<List<Authorization>> FindAuthorizationsByAsync(string userId, string userName)
        {
            return await _service.GetAuthorizationsAsync(null, userId, userName)
                .ContinueWith(t => t.Result._Authorizations);
        }
    }
}