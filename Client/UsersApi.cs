using System.Collections.Generic;
using System.Diagnostics;
using InfluxDB.Client.Core;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Generated.Domain;
using InfluxDB.Client.Generated.Service;
using InfluxDB.Client.Internal;

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
        public User CreateUser(string name)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));

            var user = new User(name);

            return CreateUser(user);
        }

        /// <summary>
        /// Creates a new user and sets <see cref="User.Id" /> with the new identifier.
        /// </summary>
        /// <param name="user">name of the user</param>
        /// <returns>Created user</returns>
        public User CreateUser(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return _service.UsersPost(user);
        }

        /// <summary>
        /// Update an user.
        /// </summary>
        /// <param name="user">user update to apply</param>
        /// <returns>user updated</returns>
        public User UpdateUser(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return _service.UsersUserIDPatch(user.Id, user);
        }

        /// <summary>
        /// Update password to an user.
        /// </summary>
        /// <param name="user">user to update password</param>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>user updated</returns>
        public void UpdateUserPassword(User user, string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(user, nameof(user));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            UpdateUserPassword(user.Id, user.Name, oldPassword, newPassword);
        }

        /// <summary>
        /// Update password to an user.
        /// </summary>
        /// <param name="userId">ID of user to update password</param>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>user updated</returns>
        public void UpdateUserPassword(string userId, string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(userId, nameof(userId));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            var user = FindUserById(userId);

            UpdateUserPassword(user, oldPassword, newPassword);
        }

        /// <summary>
        /// Delete an user.
        /// </summary>
        /// <param name="userId">ID of user to delete</param>
        /// <returns>async task</returns>
        public void DeleteUser(string userId)
        {
            Arguments.CheckNotNull(userId, nameof(userId));

            _service.UsersUserIDDelete(userId);
        }

        /// <summary>
        /// Delete an user.
        /// </summary>
        /// <param name="user">user to delete</param>
        /// <returns>async task</returns>
        public void DeleteUser(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            DeleteUser(user.Id);
        }

        /// <summary>
        /// Clone an user.
        /// </summary>
        /// <param name="clonedName">name of cloned user</param>
        /// <param name="userId">ID of user to clone</param>
        /// <returns>cloned user</returns>
        public User CloneUser(string clonedName, string userId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(userId, nameof(userId));

            var user = FindUserById(userId);

            return CloneUser(clonedName, user);
        }

        /// <summary>
        /// Clone an user.
        /// </summary>
        /// <param name="clonedName">name of cloned user</param>
        /// <param name="user">user to clone</param>
        /// <returns>cloned user</returns>
        public User CloneUser(string clonedName, User user)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(user, nameof(user));

            var cloned = new User(clonedName);

            return CreateUser(cloned);
        }

        /// <summary>
        /// Returns currently authenticated user.
        /// </summary>
        /// <returns>currently authenticated user</returns>
        public User Me()
        {
            return _service.MeGet();
        }

        /// <summary>
        /// Update the password to a currently authenticated user.
        /// </summary>
        /// <param name="oldPassword">old password</param>
        /// <param name="newPassword">new password</param>
        /// <returns>currently authenticated user</returns>
        public void MeUpdatePassword(string oldPassword, string newPassword)
        {
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            var user = Me();
            if (user == null)
            {
                Trace.WriteLine("User is not authenticated.");

                return;
            }

            var header = AuthenticateDelegatingHandler.AuthorizationHeader(user.Name, oldPassword);
            
            _service.MePasswordPut(new PasswordResetBody(newPassword), null, header);
        }

        /// <summary>
        /// Retrieve an user.
        /// </summary>
        /// <param name="userId">ID of user to get</param>
        /// <returns>User Details</returns>
        public User FindUserById(string userId)
        {
            Arguments.CheckNonEmptyString(userId, nameof(userId));

            return _service.UsersUserIDGet(userId);
        }

        /// <summary>
        /// List all users.
        /// </summary>
        /// <returns>List all users</returns>
        public List<User> FindUsers()
        {
            return _service.UsersGet()._Users;
        }

        /// <summary>
        /// Retrieve an user's logs
        /// </summary>
        /// <param name="user">for retrieve logs</param>
        /// <returns>logs</returns>
        public List<OperationLog> FindUserLogs(User user)
        {
            Arguments.CheckNotNull(user, nameof(user));

            return FindUserLogs(user.Id);
        }

        /// <summary>
        /// Retrieve an user's logs
        /// </summary>
        /// <param name="user">for retrieve logs</param>
        /// <param name="findOptions">the find options</param>
        /// <returns>logs</returns>
        public OperationLogs FindUserLogs(User user, FindOptions findOptions)
        {
            Arguments.CheckNotNull(user, nameof(user));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return FindUserLogs(user.Id, findOptions);
        }

        /// <summary>
        /// Retrieve an user's logs
        /// </summary>
        /// <param name="userId">the ID of an user</param>
        /// <returns>logs</returns>
        public List<OperationLog> FindUserLogs(string userId)
        {
            Arguments.CheckNonEmptyString(userId, nameof(userId));

            return FindUserLogs(userId, new FindOptions()).Logs;
        }

        /// <summary>
        /// Retrieve an user's logs
        /// </summary>
        /// <param name="userId">the ID of an user</param>
        /// <param name="findOptions">the find options</param>
        /// <returns>logs</returns>
        public OperationLogs FindUserLogs(string userId, FindOptions findOptions)
        {
            Arguments.CheckNonEmptyString(userId, nameof(userId));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return _service.UsersUserIDLogsGet(userId, null, findOptions.Offset, findOptions.Limit);
        }

        private void UpdateUserPassword(string userId, string userName, string oldPassword,
            string newPassword)
        {
            Arguments.CheckNotNull(userId, nameof(userId));
            Arguments.CheckNotNull(userName, nameof(userName));
            Arguments.CheckNotNull(oldPassword, nameof(oldPassword));
            Arguments.CheckNotNull(newPassword, nameof(newPassword));

            var header = AuthenticateDelegatingHandler.AuthorizationHeader(userName, oldPassword);

            _service.UsersUserIDPasswordPut(userId, new PasswordResetBody(newPassword), null, header);
        }
    }
}