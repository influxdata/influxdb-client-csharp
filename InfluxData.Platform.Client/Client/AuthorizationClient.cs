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
    }
}