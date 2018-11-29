using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;
using Task = System.Threading.Tasks.Task;

namespace InfluxData.Platform.Client.Client
{
    public class AuthorizationClient: AbstractClient
    {
        protected internal AuthorizationClient(DefaultClientIo client) : base(client)
        {
        }

        /// <summary>
        /// Create an authorization with defined permissions.
        /// </summary>
        /// <param name="user">the owner of the authorization</param>
        /// <param name="permissions">the permissions for the authorization</param>
        /// <returns>the created authorization</returns>
        public async Task<Authorization> CreateAuthorization(User user, List<Permission> permissions)
        {
            Arguments.CheckNotNull(user, "user");
            Arguments.CheckNotNull(permissions, "permissions");
            
            return await CreateAuthorization(user.Id, permissions);
        }

        /// <summary>
        /// Create an authorization with defined permissions.
        /// </summary>
        /// <param name="userId">the owner id of the authorization</param>
        /// <param name="permissions">the permissions for the authorization</param>
        /// <returns>the created authorization</returns>
        public async Task<Authorization> CreateAuthorization(string userId, List<Permission> permissions)
        {
            Arguments.CheckNonEmptyString(userId, "userId");
            Arguments.CheckNotNull(permissions, "permissions");

            var authorization = new Authorization {UserId = userId, Permissions = permissions};

            return await CreateAuthorization(authorization);
        }

        /// <summary>
        /// Create an authorization with defined permissions.
        /// </summary>
        /// <param name="authorization">authorization to create</param>
        /// <returns>the created authorization</returns>
        public async Task<Authorization> CreateAuthorization(Authorization authorization)
        {
            Arguments.CheckNotNull(authorization, "authorization");
            
            var response = await Post(authorization, "/api/v2/authorizations");

            return Call<Authorization>(response);
        }

        /// <summary>
        /// Updates the status of the authorization. Useful for setting an authorization to inactive or active.
        /// </summary>
        /// <param name="authorization">the authorization with updated status</param>
        /// <returns>the updated authorization</returns>
        public async Task<Authorization> UpdateAuthorization(Authorization authorization)
        {
            Arguments.CheckNotNull(authorization, "authorization");
            
            var response = await Patch(authorization, $"/api/v2/authorizations/{authorization.Id}");

            return Call<Authorization>(response);
        }

        /// <summary>
        /// Delete a authorization.
        /// </summary>
        /// <param name="authorization">authorization to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteAuthorization(Authorization authorization)
        {
            Arguments.CheckNotNull(authorization, "authorization");

            await DeleteAuthorization(authorization.Id);
        }
        
        /// <summary>
        /// Delete a authorization.
        /// </summary>
        /// <param name="authorizationId">ID of authorization to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteAuthorization(string authorizationId)
        {
            Arguments.CheckNonEmptyString(authorizationId, "Authorization ID");

            
            var request = await Delete($"/api/v2/authorizations/{authorizationId}");

            RaiseForInfluxError(request);
        }
        
        /// <summary>
        /// Retrieve a authorization.
        /// </summary>
        /// <param name="authorizationId">ID of authorization to get</param>
        /// <returns>authorization details</returns>
        public async Task<Authorization> FindAuthorizationById(string authorizationId)
        {
            Arguments.CheckNonEmptyString(authorizationId, "Authorization ID");

            var request = await Get($"/api/v2/authorizations/{authorizationId}");

            return Call<Authorization>(request, "authorization not found");
        }

        /// <summary>
        /// List all authorizations belonging to specified user.
        /// </summary>
        /// <param name="user">user</param>
        /// <returns>A list of authorizations</returns>
        public async Task<List<Authorization>> FindAuthorizationsByUser(User user)
        {
            Arguments.CheckNotNull(user, "user");
            
            return await FindAuthorizationsByUserId(user.Id);
        }
        
        /// <summary>
        /// List all authorizations belonging to specified user.
        /// </summary>
        /// <param name="userId">ID of user</param>
        /// <returns>A list of authorizations</returns>
        public async Task<List<Authorization>> FindAuthorizationsByUserId(string userId)
        {
            Arguments.CheckNonEmptyString(userId, "User ID");
            
            return await FindAuthorizationsBy(userId, null);
        }
        
        /// <summary>
        /// List all authorizations belonging to specified user.
        /// </summary>
        /// <param name="userName">Name of User</param>
        /// <returns>A list of authorizations</returns>
        public async Task<List<Authorization>> FindAuthorizationsByUserName(string userName)
        {
            Arguments.CheckNonEmptyString(userName, "User name");
            
            return await FindAuthorizationsBy(null, userName);
        }

        private async Task<List<Authorization>> FindAuthorizationsBy(string userId, string userName)
        {
            var request = await Get($"/api/v2/authorizations?userID={userId}&user={userName}");
            
            var authorizations = Call<Authorizations>(request);

            return authorizations.Auths;
        }
    }
}