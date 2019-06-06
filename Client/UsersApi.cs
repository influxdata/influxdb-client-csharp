using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using InfluxDB.Client.Domain;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client
{
    public class UsersApi
    {
        private readonly UsersService _service;

        protected internal UsersApi(UsersService service)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
        }

        /// <summary>
        /// Creates a new user and sets <see cref="User.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the user</param>
        /// <returns>Created user</returns>
        public async Task<User> CreateUser(string name)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));

            var user = new User(name);

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

            return await _service.PostUsersAsync(user);
        }

        /// <summary>
        /// Update an user.
        /// </summary>
        /// <param name="user">user update to apply</param>
        /// <returns>user updated</returns>
        public async Task<User> UpdateUser(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return await _service.PatchUsersIDAsync(user.Id, user);
        }

        /// <summary>
        /// Update password to an user.
        /// </summary>
        /// <param name="user">user to update password</param>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>user updated</returns>
        public async Task UpdateUserPassword(User user, string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(user, nameof(user));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            await UpdateUserPassword(user.Id, user.Name, oldPassword, newPassword);
        }

        /// <summary>
        /// Update password to an user.
        /// </summary>
        /// <param name="userId">ID of user to update password</param>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>user updated</returns>
        public async Task UpdateUserPassword(string userId, string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(userId, nameof(userId));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            await FindUserById(userId).ContinueWith(t => UpdateUserPassword(t.Result, oldPassword, newPassword));
        }

        /// <summary>
        /// Delete an user.
        /// </summary>
        /// <param name="userId">ID of user to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteUser(string userId)
        {
            Arguments.CheckNotNull(userId, nameof(userId));

            await _service.DeleteUsersIDAsync(userId);
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

            return await FindUserById(userId).ContinueWith(t => CloneUser(clonedName, t.Result)).Unwrap();
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

            var cloned = new User(clonedName);

            return await CreateUser(cloned);
        }

        /// <summary>
        /// Returns currently authenticated user.
        /// </summary>
        /// <returns>currently authenticated user</returns>
        public async Task<User> Me()
        {
            return await _service.GetMeAsync();
        }

        /// <summary>
        /// Update the password to a currently authenticated user.
        /// </summary>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>currently authenticated user</returns>
        public async Task MeUpdatePassword(string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            await Me().ContinueWith(async t =>
            {
                if (t.Result == null)
                {
                    Trace.WriteLine("User is not authenticated.");

                    return;
                }

                var header = InfluxDBClient.AuthorizationHeader(t.Result.Name, oldPassword);

                await _service.PutMePasswordAsync(new PasswordResetBody(newPassword), null, header);
            });
        }

        /// <summary>
        /// Retrieve an user.
        /// </summary>
        /// <param name="userId">ID of user to get</param>
        /// <returns>User Details</returns>
        public async Task<User> FindUserById(string userId)
        {
            Arguments.CheckNonEmptyString(userId, nameof(userId));

            return await _service.GetUsersIDAsync(userId);
        }

        /// <summary>
        /// List all users.
        /// </summary>
        /// <returns>List all users</returns>
        public async Task<List<User>> FindUsers()
        {
            return await _service.GetUsersAsync().ContinueWith(t => t.Result._Users);
        }

        /// <summary>
        /// Retrieve an user's logs
        /// </summary>
        /// <param name="user">for retrieve logs</param>
        /// <returns>logs</returns>
        public async Task<List<OperationLog>> FindUserLogs(User user)
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
        public async Task<OperationLogs> FindUserLogs(User user, FindOptions findOptions)
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
        public async Task<List<OperationLog>> FindUserLogs(string userId)
        {
            Arguments.CheckNonEmptyString(userId, nameof(userId));

            return await FindUserLogs(userId, new FindOptions()).ContinueWith(t => t.Result.Logs);
        }

        /// <summary>
        /// Retrieve an user's logs
        /// </summary>
        /// <param name="userId">the ID of an user</param>
        /// <param name="findOptions">the find options</param>
        /// <returns>logs</returns>
        public async Task<OperationLogs> FindUserLogs(string userId, FindOptions findOptions)
        {
            Arguments.CheckNonEmptyString(userId, nameof(userId));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return await _service.GetUsersIDLogsAsync(userId, null, findOptions.Offset, findOptions.Limit);
        }

        private async Task UpdateUserPassword(string userId, string userName, string oldPassword,
            string newPassword)
        {
            Arguments.CheckNotNull(userId, nameof(userId));
            Arguments.CheckNotNull(userName, nameof(userName));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            var header = InfluxDBClient.AuthorizationHeader(userName, oldPassword);

            await _service.PutUsersIDPasswordAsync(userId, new PasswordResetBody(newPassword), null, header);
        }
    }
}