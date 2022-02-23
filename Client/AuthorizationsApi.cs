using System;
using System.Collections.Generic;
using System.Threading;
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the created authorization</returns>
        public Task<Authorization> CreateAuthorizationAsync(Organization organization, List<Permission> permissions,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(organization, "organization");
            Arguments.CheckNotNull(permissions, "permissions");

            return CreateAuthorizationAsync(organization.Id, permissions, cancellationToken);
        }

        /// <summary>
        /// Create an authorization with defined permissions.
        /// </summary>
        /// <param name="orgId">the owner id of the authorization</param>
        /// <param name="permissions">the permissions for the authorization</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the created authorization</returns>
        public Task<Authorization> CreateAuthorizationAsync(string orgId, List<Permission> permissions,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(orgId, "orgId");
            Arguments.CheckNotNull(permissions, "permissions");

            var authorization =
                new AuthorizationPostRequest(orgId, null, permissions, AuthorizationUpdateRequest.StatusEnum.Active);

            return CreateAuthorizationAsync(authorization, cancellationToken);
        }

        /// <summary>
        /// Create an authorization with defined permissions.
        /// </summary>
        /// <param name="authorization">authorization to create</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the created authorization</returns>
        [Obsolete("This method is obsolete. Call 'CreateAuthorizationAsync(AuthorizationPostRequest)' instead.", false)]
        public Task<Authorization> CreateAuthorizationAsync(Authorization authorization,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(authorization, nameof(authorization));

            var request =
                new AuthorizationPostRequest(authorization.OrgID,
                    authorization.UserID,
                    authorization.Permissions, description: authorization.Description);

            return CreateAuthorizationAsync(request, cancellationToken);
        }

        /// <summary>
        /// Create an authorization with defined permissions.
        /// </summary>
        /// <param name="authorization">authorization to create</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the created authorization</returns>
        public Task<Authorization> CreateAuthorizationAsync(AuthorizationPostRequest authorization,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(authorization, nameof(authorization));

            return _service.PostAuthorizationsAsync(authorization, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Updates the status of the authorization. Useful for setting an authorization to inactive or active.
        /// </summary>
        /// <param name="authorization">the authorization with updated status</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the updated authorization</returns>
        public Task<Authorization> UpdateAuthorizationAsync(Authorization authorization,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(authorization, nameof(authorization));

            var request = new AuthorizationUpdateRequest(authorization.Status, authorization.Description);

            return _service.PatchAuthorizationsIDAsync(authorization.Id, request, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Delete an authorization.
        /// </summary>
        /// <param name="authorization">authorization to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>authorization deleted</returns>
        public Task DeleteAuthorizationAsync(Authorization authorization, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(authorization, nameof(authorization));

            return DeleteAuthorizationAsync(authorization.Id, cancellationToken);
        }

        /// <summary>
        /// Delete an authorization.
        /// </summary>
        /// <param name="authorizationId">ID of authorization to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>authorization deleted</returns>
        public Task DeleteAuthorizationAsync(string authorizationId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(authorizationId, nameof(authorizationId));

            return _service.DeleteAuthorizationsIDAsync(authorizationId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Clone an authorization.
        /// </summary>
        /// <param name="authorizationId">ID of authorization to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned authorization</returns>
        public async Task<Authorization> CloneAuthorizationAsync(string authorizationId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(authorizationId, nameof(authorizationId));

            var authorization =
                await FindAuthorizationByIdAsync(authorizationId, cancellationToken).ConfigureAwait(false);
            return await CloneAuthorizationAsync(authorization, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Clone an authorization.
        /// </summary>
        /// <param name="authorization">authorization to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned authorization</returns>
        public Task<Authorization> CloneAuthorizationAsync(Authorization authorization,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(authorization, nameof(authorization));

            var cloned = new AuthorizationPostRequest(authorization.OrgID, authorization.UserID,
                authorization.Permissions,
                authorization.Status, authorization.Description);

            return CreateAuthorizationAsync(cloned, cancellationToken);
        }

        /// <summary>
        /// Retrieve an authorization.
        /// </summary>
        /// <param name="authorizationId">ID of authorization to get</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>authorization details</returns>
        public Task<Authorization> FindAuthorizationByIdAsync(string authorizationId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(authorizationId, nameof(authorizationId));

            return _service.GetAuthorizationsIDAsync(authorizationId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// List all authorizations.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List all authorizations.</returns>
        public Task<List<Authorization>> FindAuthorizationsAsync(CancellationToken cancellationToken = default)
        {
            return FindAuthorizationsByAsync(null, null, cancellationToken);
        }

        /// <summary>
        /// List all authorizations belonging to specified user.
        /// </summary>
        /// <param name="user">user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of authorizations</returns>
        public Task<List<Authorization>> FindAuthorizationsByUserAsync(User user,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(user, "user");

            return FindAuthorizationsByUserIdAsync(user.Id, cancellationToken);
        }

        /// <summary>
        /// List all authorizations belonging to specified user.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of authorizations</returns>
        public Task<List<Authorization>> FindAuthorizationsByUserIdAsync(string userId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(userId, "User ID");

            return FindAuthorizationsByAsync(userId, null, cancellationToken);
        }

        /// <summary>
        /// List all authorizations belonging to specified user.
        /// </summary>
        /// <param name="userName">Name of User</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of authorizations</returns>
        public Task<List<Authorization>> FindAuthorizationsByUserNameAsync(string userName,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(userName, "User name");

            return FindAuthorizationsByAsync(null, userName, cancellationToken);
        }

        private async Task<List<Authorization>> FindAuthorizationsByAsync(string userId, string userName,
            CancellationToken cancellationToken = default)
        {
            var response = await _service
                .GetAuthorizationsAsync(null, userId, userName, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response._Authorizations;
        }
    }
}