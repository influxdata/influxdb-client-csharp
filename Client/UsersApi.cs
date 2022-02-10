using System;
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
        public Task<User> CreateUserAsync(string name)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));

            var user = new User(name: name);

            return CreateUserAsync(user);
        }

        /// <summary>
        /// Creates a new user and sets <see cref="User.Id" /> with the new identifier.
        /// </summary>
        /// <param name="user">name of the user</param>
        /// <returns>Created user</returns>
        public Task<User> CreateUserAsync(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return _service.PostUsersAsync(ToPostUser(user));
        }

        /// <summary>
        /// Update an user.
        /// </summary>
        /// <param name="user">user update to apply</param>
        /// <returns>user updated</returns>
        public Task<User> UpdateUserAsync(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return _service.PatchUsersIDAsync(user.Id, ToPostUser(user));
        }

        /// <summary>
        /// Update password to an user.
        /// </summary>
        /// <param name="user">user to update password</param>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>user updated</returns>
        public Task UpdateUserPasswordAsync(User user, string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(user, nameof(user));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            return UpdateUserPasswordAsync(user.Id, user.Name, oldPassword, newPassword);
        }

        /// <summary>
        /// Update password to an user.
        /// </summary>
        /// <param name="userId">ID of user to update password</param>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>user updated</returns>
        public Task UpdateUserPasswordAsync(string userId, string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(userId, nameof(userId));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            return FindUserByIdAsync(userId)
                .ContinueWith(t => UpdateUserPasswordAsync(t.Result, oldPassword, newPassword));
        }

        /// <summary>
        /// Delete an user.
        /// </summary>
        /// <param name="userId">ID of user to delete</param>
        /// <returns>async task</returns>
        public Task DeleteUserAsync(string userId)
        {
            Arguments.CheckNotNull(userId, nameof(userId));

            return _service.DeleteUsersIDAsync(userId);
        }

        /// <summary>
        /// Delete an user.
        /// </summary>
        /// <param name="user">user to delete</param>
        /// <returns>async task</returns>
        public Task DeleteUserAsync(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return DeleteUserAsync(user.Id);
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

            var user = await FindUserByIdAsync(userId).ConfigureAwait(false);

            return await CloneUserAsync(clonedName, user).ConfigureAwait(false);
        }

        /// <summary>
        /// Clone an user.
        /// </summary>
        /// <param name="clonedName">name of cloned user</param>
        /// <param name="user">user to clone</param>
        /// <returns>cloned user</returns>
        public Task<User> CloneUserAsync(string clonedName, User user)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(user, nameof(user));

            var cloned = new User(name: clonedName);

            return CreateUserAsync(cloned);
        }

        /// <summary>
        /// Returns currently authenticated user.
        /// </summary>
        /// <returns>currently authenticated user</returns>
        public Task<User> MeAsync()
        {
            return _service.GetMeAsync();
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

            var me = await MeAsync().ConfigureAwait(false);
            if (me == null)
            {
                Trace.WriteLine("User is not authenticated.");
                return;
            }

            var header = InfluxDBClient.AuthorizationHeader(me.Name, oldPassword);

            await _service.PutMePasswordAsync(new PasswordResetBody(newPassword), null, header).ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve an user.
        /// </summary>
        /// <param name="userId">ID of user to get</param>
        /// <returns>User Details</returns>
        public Task<User> FindUserByIdAsync(string userId)
        {
            Arguments.CheckNonEmptyString(userId, nameof(userId));

            return _service.GetUsersIDAsync(userId);
        }

        /// <summary>
        /// List all users.
        /// </summary>
        /// <param name="offset"> (optional)</param>
        /// <param name="limit"> (optional, default to 20)</param>
        /// <param name="after">The last resource ID from which to seek from (but not including). This is to be used instead of &#x60;offset&#x60;. (optional)</param>
        /// <param name="name"> (optional)</param>
        /// <param name="id"> (optional)</param>
        /// <returns>List all users</returns>
        public async Task<List<User>> FindUsersAsync(int? offset = null, int? limit = null, string after = null,
            string name = null, string id = null)
        {
            var response = await _service.GetUsersAsync(offset: offset, limit: limit, after: after, name: name, id: id)
                .ConfigureAwait(false);
            return response._Users;
        }

        private Task UpdateUserPasswordAsync(string userId, string userName, string oldPassword,
            string newPassword)
        {
            Arguments.CheckNotNull(userId, nameof(userId));
            Arguments.CheckNotNull(userName, nameof(userName));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            var header = InfluxDBClient.AuthorizationHeader(userName, oldPassword);

            return _service.PostUsersIDPasswordAsync(userId, new PasswordResetBody(newPassword), null, header);
        }

        private PostUser ToPostUser(User user)
        {
            Enum.TryParse(user.Status.ToString(), true, out PostUser.StatusEnum status);
            var postUser = new PostUser(user.OauthID, user.Name, status);
            return postUser;
        }
    }
}