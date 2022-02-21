using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
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
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created user</returns>
        public Task<User> CreateUserAsync(string name, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));

            var user = new User(name: name);

            return CreateUserAsync(user, cancellationToken);
        }

        /// <summary>
        /// Creates a new user and sets <see cref="User.Id" /> with the new identifier.
        /// </summary>
        /// <param name="user">name of the user</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Created user</returns>
        public Task<User> CreateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return _service.PostUsersAsync(ToPostUser(user), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Update an user.
        /// </summary>
        /// <param name="user">user update to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>user updated</returns>
        public Task<User> UpdateUserAsync(User user, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return _service.PatchUsersIDAsync(user.Id, ToPostUser(user), cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Update password to an user.
        /// </summary>
        /// <param name="user">user to update password</param>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>user updated</returns>
        public Task UpdateUserPasswordAsync(User user, string oldPassword, string newPassword,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(user, nameof(user));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            return UpdateUserPasswordAsync(user.Id, user.Name, oldPassword, newPassword, cancellationToken);
        }

        /// <summary>
        /// Update password to an user.
        /// </summary>
        /// <param name="userId">ID of user to update password</param>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>user updated</returns>
        public Task UpdateUserPasswordAsync(string userId, string oldPassword, string newPassword,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(userId, nameof(userId));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            return FindUserByIdAsync(userId, cancellationToken)
                .ContinueWith(t => UpdateUserPasswordAsync(t.Result, oldPassword, newPassword), cancellationToken);
        }

        /// <summary>
        /// Delete an user.
        /// </summary>
        /// <param name="userId">ID of user to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>async task</returns>
        public Task DeleteUserAsync(string userId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(userId, nameof(userId));

            return _service.DeleteUsersIDAsync(userId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Delete an user.
        /// </summary>
        /// <param name="user">user to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>async task</returns>
        public Task DeleteUserAsync(User user, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return DeleteUserAsync(user.Id, cancellationToken);
        }

        /// <summary>
        /// Clone an user.
        /// </summary>
        /// <param name="clonedName">name of cloned user</param>
        /// <param name="userId">ID of user to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned user</returns>
        public async Task<User> CloneUserAsync(string clonedName, string userId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(userId, nameof(userId));

            var user = await FindUserByIdAsync(userId, cancellationToken).ConfigureAwait(false);

            return await CloneUserAsync(clonedName, user, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Clone an user.
        /// </summary>
        /// <param name="clonedName">name of cloned user</param>
        /// <param name="user">user to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned user</returns>
        public Task<User> CloneUserAsync(string clonedName, User user, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(user, nameof(user));

            var cloned = new User(name: clonedName);

            return CreateUserAsync(cloned, cancellationToken);
        }

        /// <summary>
        /// Returns currently authenticated user.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>currently authenticated user</returns>
        public Task<User> MeAsync(CancellationToken cancellationToken = default)
        {
            return _service.GetMeAsync(cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Update the password to a currently authenticated user.
        /// </summary>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>currently authenticated user</returns>
        public async Task MeUpdatePasswordAsync(string oldPassword, string newPassword,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            var me = await MeAsync(cancellationToken).ConfigureAwait(false);
            if (me == null)
            {
                Trace.WriteLine("User is not authenticated.");
                return;
            }

            var header = InfluxDBClient.AuthorizationHeader(me.Name, oldPassword);

            await _service.PutMePasswordAsync(new PasswordResetBody(newPassword), null, header, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Retrieve an user.
        /// </summary>
        /// <param name="userId">ID of user to get</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>User Details</returns>
        public Task<User> FindUserByIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(userId, nameof(userId));

            return _service.GetUsersIDAsync(userId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// List all users.
        /// </summary>
        /// <param name="offset"> (optional)</param>
        /// <param name="limit"> (optional, default to 20)</param>
        /// <param name="after">The last resource ID from which to seek from (but not including). This is to be used instead of &#x60;offset&#x60;. (optional)</param>
        /// <param name="name"> (optional)</param>
        /// <param name="id"> (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List all users</returns>
        public async Task<List<User>> FindUsersAsync(int? offset = null, int? limit = null, string after = null,
            string name = null, string id = null, CancellationToken cancellationToken = default)
        {
            var response = await _service.GetUsersAsync(offset: offset, limit: limit, after: after, name: name, id: id,
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response._Users;
        }

        private Task UpdateUserPasswordAsync(string userId, string userName, string oldPassword,
            string newPassword, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(userId, nameof(userId));
            Arguments.CheckNotNull(userName, nameof(userName));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            var header = InfluxDBClient.AuthorizationHeader(userName, oldPassword);

            return _service.PostUsersIDPasswordAsync(userId, new PasswordResetBody(newPassword), null, header,
                cancellationToken);
        }

        private PostUser ToPostUser(User user)
        {
            Enum.TryParse(user.Status.ToString(), true, out PostUser.StatusEnum status);
            var postUser = new PostUser(user.OauthID, user.Name, status);
            return postUser;
        }
    }
}