using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using Newtonsoft.Json.Linq;
using Platform.Common;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;
using Task = System.Threading.Tasks.Task;

namespace InfluxData.Platform.Client.Client
{
    public class UserClient : AbstractClient
    {
        protected internal UserClient(DefaultClientIo client) : base(client)
        {
        }

        /// <summary>
        /// Creates a new user and sets <see cref="User.Id"/> with the new identifier.
        /// </summary>
        /// <param name="name">name of the user</param>
        /// <returns>Created user</returns>
        public async Task<User> CreateUser(string name)
        {
            Arguments.CheckNonEmptyString(name, "User name");

            User user = new User {Name = name};

            return await CreateUser(user);
        }

        /// <summary>
        /// Creates a new user and sets <see cref="User.Id"/> with the new identifier.
        /// </summary>
        /// <param name="user">name of the user</param>
        /// <returns>Created user</returns>
        public async Task<User> CreateUser(User user)
        {
            Arguments.CheckNotNull(user, "User");

            var request = await Post(user, "/api/v2/users");

            return Call<User>(request);
        }

        /// <summary>
        /// Update a user.
        /// </summary>
        /// <param name="user">user update to apply</param>
        /// <returns>user updated</returns>
        public async Task<User> UpdateUser(User user)
        {
            Arguments.CheckNotNull(user, "User");

            var request = await Patch(user, $"/api/v2/users/{user.Id}");

            return Call<User>(request);
        }

        /// <summary>
        /// Update password to a user.
        /// </summary>
        /// <param name="user">user to update password</param>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>user updated</returns>
        public async Task<User> UpdateUserPassword(User user, string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(user, "User");
            Arguments.CheckNotNull(oldPassword, "old password");
            Arguments.CheckNotNull(newPassword, "new password");

            return await UpdateUserPassword(user.Id, user.Name, oldPassword, newPassword);
        }

        /// <summary>
        /// Update password to a user.
        /// </summary>
        /// <param name="userId">ID of user to update password</param>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>user updated</returns>
        public async Task<User> UpdateUserPassword(string userId, string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(userId, "User ID");
            Arguments.CheckNotNull(oldPassword, "old password");
            Arguments.CheckNotNull(newPassword, "new password");

            var user = await FindUserById(userId);
            if (user == null)
            {
                return default(User);
            }

            return await UpdateUserPassword(user, oldPassword, newPassword);
        }

        /// <summary>
        /// Delete a user.
        /// </summary>
        /// <param name="userId">ID of user to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteUser(string userId)
        {
            Arguments.CheckNotNull(userId, "User ID");

            var request = await Delete($"/api/v2/users/{userId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        /// Delete a user.
        /// </summary>
        /// <param name="user">user to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteUser(User user)
        {
            Arguments.CheckNotNull(user, "User");

            await DeleteUser(user.Id);
        }

        /// <summary>
        /// Returns currently authenticated user.
        /// </summary>
        /// <returns>currently authenticated user</returns>
        public async Task<User> Me()
        {
            var request = await Get("/api/v2/me");

            return Call<User>(request, "token required");
        }

        /// <summary>
        /// Update the password to a currently authenticated user.
        /// </summary>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>currently authenticated user</returns>
        public async Task<User> MeUpdatePassword(string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(oldPassword, "old password");
            Arguments.CheckNotNull(newPassword, "new password");

            User user = await Me();
            if (user == null)
            {
                Console.WriteLine("User is not authenticated.");

                return null;
            }

            return await UpdatePassword("/api/v2/me/password", user.Name, oldPassword, newPassword);
        }

        /// <summary>
        /// Retrieve a user.
        /// </summary>
        /// <param name="userId">ID of user to get</param>
        /// <returns>User Details</returns>
        public async Task<User> FindUserById(string userId)
        {
            Arguments.CheckNonEmptyString(userId, "User ID");

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
        /// Retrieve a user's logs.
        /// </summary>
        /// <param name="user">for retrieve logs</param>
        /// <returns>logs</returns>
        public async Task<List<OperationLogEntry>> FindUserLogs(User user)
        {
            Arguments.CheckNotNull(user, "User");

            return await FindUserLogs(user.Id);
        }

        /// <summary>
        /// Retrieve a user's logs.
        /// </summary>
        /// <param name="userId">id of a user</param>
        /// <returns>logs</returns>
        public async Task<List<OperationLogEntry>> FindUserLogs(string userId)
        {
            Arguments.CheckNonEmptyString(userId, "User ID");

            var request = await Get($"/api/v2/users/{userId}/log");

            var logResponse = Call<OperationLogResponse>(request, "oplog not found");
            if (logResponse == null)
            {
                return new List<OperationLogEntry>();
            }

            return logResponse.Log;
        }

        private async Task<User> UpdateUserPassword(string userId, string userName, string oldPassword,
            string newPassword)
        {
            Arguments.CheckNotNull(userId, "User ID");
            Arguments.CheckNotNull(userName, "User Name");
            Arguments.CheckNotNull(oldPassword, "old password");
            Arguments.CheckNotNull(newPassword, "new password");

            return await UpdatePassword($"/api/v2/users/{userId}/password", userName, oldPassword, newPassword);
        }

        private async Task<User> UpdatePassword(string path, string userName, string oldPassword,
            string newPassword)
        {
            Arguments.CheckNotNull(path, "path");
            Arguments.CheckNotNull(userName, "User Name");
            Arguments.CheckNotNull(oldPassword, "old password");
            Arguments.CheckNotNull(newPassword, "new password");

            var header = AuthenticateDelegatingHandler.AuthorizationHeader(userName, oldPassword);

            var json = new JObject {{"password", newPassword}};

            var request = new HttpRequestMessage(new HttpMethod(HttpMethodKind.Put.Name()), path)
            {
                Content = new StringContent(json.ToString())
            };
            request.Headers.Add("Authorization", header);

            var configuredTaskAwaitable = await Client.DoRequest(request).ConfigureAwait(false);

            return Call<User>(configuredTaskAwaitable, "token required");
        }
    }
}