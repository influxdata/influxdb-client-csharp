using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;

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
        public async Task<User> CreateUserAsync(string name)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));

            var user = new User(name: name);

            return await CreateUserAsync(user);
        }

        /// <summary>
        /// Creates a new user and sets <see cref="User.Id" /> with the new identifier.
        /// </summary>
        /// <param name="user">name of the user</param>
        /// <returns>Created user</returns>
        public async Task<User> CreateUserAsync(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return await _service.PostUsersAsync(user);
        }

        /// <summary>
        /// Update an user.
        /// </summary>
        /// <param name="user">user update to apply</param>
        /// <returns>user updated</returns>
        public async Task<User> UpdateUserAsync(User user)
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
        public async Task UpdateUserPasswordAsync(User user, string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(user, nameof(user));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            await UpdateUserPasswordAsync(user.Id, user.Name, oldPassword, newPassword);
        }

        /// <summary>
        /// Update password to an user.
        /// </summary>
        /// <param name="userId">ID of user to update password</param>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>user updated</returns>
        public async Task UpdateUserPasswordAsync(string userId, string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(userId, nameof(userId));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            await FindUserByIdAsync(userId).ContinueWith(t => UpdateUserPasswordAsync(t.Result, oldPassword, newPassword));
        }

        /// <summary>
        /// Delete an user.
        /// </summary>
        /// <param name="userId">ID of user to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteUserAsync(string userId)
        {
            Arguments.CheckNotNull(userId, nameof(userId));

            await _service.DeleteUsersIDAsync(userId);
        }

        /// <summary>
        /// Delete an user.
        /// </summary>
        /// <param name="user">user to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteUserAsync(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            await DeleteUserAsync(user.Id);
        }

        /// <summary>
        /// Clone an user.
        /// </summary>
        /// <param name="clonedName">name of cloned user</param>
        /// <param name="userId">ID of user to clone</param>
        /// <returns>cloned user</returns>
        public async Task<User> CloneUserAsync(string clonedName, string userId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(userId, nameof(userId));

            return await FindUserByIdAsync(userId).ContinueWith(t => CloneUserAsync(clonedName, t.Result)).Unwrap();
        }

        /// <summary>
        /// Clone an user.
        /// </summary>
        /// <param name="clonedName">name of cloned user</param>
        /// <param name="user">user to clone</param>
        /// <returns>cloned user</returns>
        public async Task<User> CloneUserAsync(string clonedName, User user)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(user, nameof(user));

            var cloned = new User(name: clonedName);

            return await CreateUserAsync(cloned);
        }

        /// <summary>
        /// Returns currently authenticated user.
        /// </summary>
        /// <returns>currently authenticated user</returns>
        public async Task<User> MeAsync()
        {
            return await _service.GetMeAsync();
        }

        /// <summary>
        /// Update the password to a currently authenticated user.
        /// </summary>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>currently authenticated user</returns>
        public async Task MeUpdatePasswordAsync(string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            await MeAsync().ContinueWith(async t =>
            {
                if (t.Result == null)
                {
                    Trace.WriteLine("User is not authenticated.");

                    return;
                }

                var header = InfluxDBClient.AuthorizationHeader(t.Result.Name, oldPassword);

                await _service.PutMePasswordAsync(new PasswordResetBody(newPassword), null, header);
            }).Unwrap();
        }

        /// <summary>
        /// Retrieve an user.
        /// </summary>
        /// <param name="userId">ID of user to get</param>
        /// <returns>User Details</returns>
        public async Task<User> FindUserByIdAsync(string userId)
        {
            Arguments.CheckNonEmptyString(userId, nameof(userId));

            return await _service.GetUsersIDAsync(userId);
        }

        /// <summary>
        /// List all users.
        /// </summary>
        /// <returns>List all users</returns>
        public async Task<List<User>> FindUsersAsync()
        {
            return await _service.GetUsersAsync().ContinueWith(t => t.Result._Users);
        }

        private async Task UpdateUserPasswordAsync(string userId, string userName, string oldPassword,
            string newPassword)
        {
            Arguments.CheckNotNull(userId, nameof(userId));
            Arguments.CheckNotNull(userName, nameof(userName));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            var header = InfluxDBClient.AuthorizationHeader(userName, oldPassword);

            await _service.PostUsersIDPasswordAsync(userId, new PasswordResetBody(newPassword), null, header);
        }
    }
}