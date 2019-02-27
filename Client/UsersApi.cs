using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Internal;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Internal;
using Newtonsoft.Json.Linq;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client
{
    public class UsersApi : AbstractInfluxDBClient
    {
        protected internal UsersApi(DefaultClientIo client) : base(client)
        {
        }

        /// <summary>
        /// Creates a new user and sets <see cref="User.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the user</param>
        /// <returns>Created user</returns>
        public async Task<User> CreateUser(string name)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));

            var user = new User {Name = name};

            return await CreateUser(user);
        }

        /// <summary>
        /// Creates a new user and sets <see cref="User.Id" /> with the new identifier.
        /// </summary>
        /// <param name="user">name of the user</param>
        /// <returns>Created user</returns>
        public async Task<User> CreateUser(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            var request = await Post(user, "/api/v2/users");

            return Call<User>(request);
        }

        /// <summary>
        /// Update an user.
        /// </summary>
        /// <param name="user">user update to apply</param>
        /// <returns>user updated</returns>
        public async Task<User> UpdateUser(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            var request = await Patch(user, $"/api/v2/users/{user.Id}");

            return Call<User>(request);
        }

        /// <summary>
        /// Update password to an user.
        /// </summary>
        /// <param name="user">user to update password</param>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>user updated</returns>
        public async Task<User> UpdateUserPassword(User user, string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(user, nameof(user));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            return await UpdateUserPassword(user.Id, user.Name, oldPassword, newPassword);
        }

        /// <summary>
        /// Update password to an user.
        /// </summary>
        /// <param name="userId">ID of user to update password</param>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>user updated</returns>
        public async Task<User> UpdateUserPassword(string userId, string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(userId, nameof(userId));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            var user = await FindUserById(userId);
            if (user == null) return default(User);

            return await UpdateUserPassword(user, oldPassword, newPassword);
        }

        /// <summary>
        /// Delete an user.
        /// </summary>
        /// <param name="userId">ID of user to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteUser(string userId)
        {
            Arguments.CheckNotNull(userId, nameof(userId));

            var request = await Delete($"/api/v2/users/{userId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        /// Delete an user.
        /// </summary>
        /// <param name="user">user to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteUser(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            await DeleteUser(user.Id);
        }
        
        /// <summary>
        /// Clone an user.
        /// </summary>
        /// <param name="clonedName">name of cloned user</param>
        /// <param name="userId">ID of user to clone</param>
        /// <returns>cloned user</returns>
        public async Task<User> CloneUser(string clonedName, string userId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(userId, nameof(userId));

            var user = await FindUserById(userId);
            if (user == null)
            {
                throw new InvalidOperationException($"NotFound User with ID: {userId}");
            }

            return await CloneUser(clonedName, user);
        }

        /// <summary>
        /// Clone an user.
        /// </summary>
        /// <param name="clonedName">name of cloned user</param>
        /// <param name="user">user to clone</param>
        /// <returns>cloned user</returns>
        public async Task<User> CloneUser(string clonedName, User user)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(user, nameof(user));

            var cloned = new User
            {
                Name = clonedName
            };

            return await CreateUser(cloned);
        }

        /// <summary>
        /// Returns currently authenticated user.
        /// </summary>
        /// <returns>currently authenticated user</returns>
        public async Task<User> Me()
        {
            var request = await Get("/api/v2/me");

            return Call<User>(request, 401);
        }

        /// <summary>
        /// Update the password to a currently authenticated user.
        /// </summary>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>currently authenticated user</returns>
        public async Task<User> MeUpdatePassword(string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            var user = await Me();
            if (user == null)
            {
                Trace.WriteLine("User is not authenticated.");

                return null;
            }

            return await UpdatePassword("/api/v2/me/password", user.Name, oldPassword, newPassword);
        }

        /// <summary>
        /// Retrieve an user.
        /// </summary>
        /// <param name="userId">ID of user to get</param>
        /// <returns>User Details</returns>
        public async Task<User> FindUserById(string userId)
        {
            Arguments.CheckNonEmptyString(userId, nameof(userId));

            var request = await Get($"/api/v2/users/{userId}");

            return Call<User>(request, "user not found");
        }

        /// <summary>
        /// List all users.
        /// </summary>
        /// <returns>List all users</returns>
        public async Task<List<User>> FindUsers()
        {
            var request = await Get("/api/v2/users");

            var users = Call<Users>(request);

            return users?.UserList;
        }

        /// <summary>
        /// Retrieve an user's logs
        /// </summary>
        /// <param name="user">for retrieve logs</param>
        /// <returns>logs</returns>
        public async Task<List<OperationLogEntry>> FindUserLogs(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return await FindUserLogs(user.Id);
        }

        /// <summary>
        /// Retrieve an user's logs
        /// </summary>
        /// <param name="user">for retrieve logs</param>
        /// <param name="findOptions">the find options</param>
        /// <returns>logs</returns>
        public async Task<OperationLogEntries> FindUserLogs(User user, FindOptions findOptions)
        {
            Arguments.CheckNotNull(user, nameof(user));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return await FindUserLogs(user.Id, findOptions);
        }

        /// <summary>
        /// Retrieve an user's logs
        /// </summary>
        /// <param name="userId">the ID of an user</param>
        /// <returns>logs</returns>
        public async Task<List<OperationLogEntry>> FindUserLogs(string userId)
        {
            Arguments.CheckNonEmptyString(userId, nameof(userId));

            return (await FindUserLogs(userId, new FindOptions())).Logs;
        }

        /// <summary>
        /// Retrieve an user's logs
        /// </summary>
        /// <param name="userId">the ID of an user</param>
        /// <param name="findOptions">the find options</param>
        /// <returns>logs</returns>
        public async Task<OperationLogEntries> FindUserLogs(string userId, FindOptions findOptions)
        {
            Arguments.CheckNonEmptyString(userId, nameof(userId));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            var request = await Get($"/api/v2/users/{userId}/log?" + CreateQueryString(findOptions));

            return GetOperationLogEntries(request);
        }

        private async Task<User> UpdateUserPassword(string userId, string userName, string oldPassword,
            string newPassword)
        {
            Arguments.CheckNotNull(userId, nameof(userId));
            Arguments.CheckNotNull(userName, nameof(userName));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            return await UpdatePassword($"/api/v2/users/{userId}/password", userName, oldPassword, newPassword);
        }

        private async Task<User> UpdatePassword(string path, string userName, string oldPassword,
            string newPassword)
        {
            Arguments.CheckNotNull(path, nameof(path));
            Arguments.CheckNotNull(userName, nameof(userName));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            var header = AuthenticateDelegatingHandler.AuthorizationHeader(userName, oldPassword);

            var json = new JObject {{"password", newPassword}};

            var request = new HttpRequestMessage(new HttpMethod(HttpMethodKind.Put.Name()), path)
            {
                Content = new StringContent(json.ToString())
            };
            request.Headers.Add("Authorization", header);

            var configuredTaskAwaitable = await Client.DoRequest(request).ConfigureAwait(false);

            return Call<User>(configuredTaskAwaitable, 401);
        }
    }
}