using System;
using System.Linq;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Domain;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItUsersApiTest : AbstractItClientTest
    {
        [SetUp]
        public new void SetUp()
        {
            _usersApi = Client.GetUsersApi();
        }

        private UsersApi _usersApi;

        [Test]
        public void CreateUser()
        {
            var userName = GenerateName("John Ryzen");

            var user = _usersApi.CreateUser(userName);

            Assert.IsNotNull(user);
            Assert.IsNotEmpty(user.Id);
            Assert.AreEqual(user.Name, userName);

            var links = user.Links;

            Assert.IsNotNull(links);
            Assert.AreEqual(links.Self, $"/api/v2/users/{user.Id}");
            Assert.AreEqual(links.Logs, $"/api/v2/users/{user.Id}/logs");
        }

        [Test]
        public void DeleteUser()
        {
            var createdUser = _usersApi.CreateUser(GenerateName("John Ryzen"));
            Assert.IsNotNull(createdUser);

            var foundUser = _usersApi.FindUserById(createdUser.Id);
            Assert.IsNotNull(foundUser);

            // delete user
            _usersApi.DeleteUser(createdUser);

            var ioe = Assert.Throws<HttpException>( () => _usersApi.FindUserById(createdUser.Id));
            Assert.IsNotNull(ioe);
            Assert.AreEqual("user not found", ioe.Message);
        }

        [Test]
        public void FindUserById()
        {
            var userName = GenerateName("John Ryzen");

            var user = _usersApi.CreateUser(userName);

            var userById = _usersApi.FindUserById(user.Id);

            Assert.IsNotNull(userById);
            Assert.AreEqual(userById.Id, user.Id);
            Assert.AreEqual(userById.Name, user.Name);
        }

        [Test]
        public void FindUserByIdNull()
        {
            var ioe = Assert.Throws<HttpException>( () => _usersApi.FindUserById("020f755c3c082000"));

            Assert.IsNotNull(ioe);
            Assert.AreEqual("user not found", ioe.Message);
        }

        [Test]
        public void FindUserLogs()
        {
            var now = new DateTime();

            var user = _usersApi.Me();
            Assert.IsNotNull(user);

            _usersApi.UpdateUser(user);

            var userLogs = _usersApi.FindUserLogs(user);

            Assert.IsTrue(userLogs.Any());
            Assert.AreEqual(userLogs[userLogs.Count - 1].Description, "User Updated");
            //TODO https://github.com/influxdata/influxdb/issues/12544
            // Assert.AreEqual(userLogs[userLogs.Count - 1].UserId, user.Id);
            Assert.IsTrue(userLogs[userLogs.Count - 1].Time > now);
        }

        [Test]
        public void FindUserLogsFindOptionsNotFound()
        {
            var entries = _usersApi.FindUserLogs("020f755c3c082000", new FindOptions());

            Assert.IsNotNull(entries);
            Assert.AreEqual(0, entries.Logs.Count);
        }

        [Test]
        public void FindUserLogsNotFound()
        {
            var logs = _usersApi.FindUserLogs("020f755c3c082000");

            Assert.AreEqual(0, logs.Count);
        }

        [Test]
        public void FindUserLogsPaging()
        {
            var user = _usersApi.CreateUser(GenerateName("John Ryzen"));

            foreach (var i in Enumerable.Range(0, 19))
            {
                user.Name = $"{i}_{user.Name}";

                _usersApi.UpdateUser(user);
            }

            var logs = _usersApi.FindUserLogs(user);

            Assert.AreEqual(20, logs.Count);
            Assert.AreEqual("User Created", logs[0].Description);
            Assert.AreEqual("User Updated", logs[19].Description);

            var findOptions = new FindOptions {Limit = 5, Offset = 0};

            var entries = _usersApi.FindUserLogs(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Created", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;

            entries = _usersApi.FindUserLogs(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Updated", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;

            entries =  _usersApi.FindUserLogs(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Updated", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;

            entries =  _usersApi.FindUserLogs(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Updated", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;

            entries = _usersApi.FindUserLogs(user, findOptions);
            Assert.AreEqual(0, entries.Logs.Count);

            //
            // Order
            //
            findOptions = new FindOptions {Descending = false};
            entries =  _usersApi.FindUserLogs(user, findOptions);
            Assert.AreEqual(20, entries.Logs.Count);

            Assert.AreEqual("User Updated", entries.Logs[19].Description);
            Assert.AreEqual("User Created", entries.Logs[0].Description);
        }

        [Test]
        public void FindUsers()
        {
            var size = (_usersApi.FindUsers()).Count;

            _usersApi.CreateUser(GenerateName("John Ryzen"));

            var users = _usersApi.FindUsers();

            Assert.AreEqual(users.Count, size + 1);
        }

        [Test]
        public void MeAuthenticated()
        {
            var me = _usersApi.Me();

            Assert.IsNotNull(me);
            Assert.AreEqual(me.Name, "my-user");
        }

        [Test]
        public void MeNotAuthenticated()
        {
            Client.Dispose();

            var ioe = Assert.Throws<HttpException>( () => _usersApi.Me());

            Assert.IsNotNull(ioe);
            Assert.AreEqual("unauthorized access", ioe.Message); 
        }

        [Test]
        [Property("basic_auth", "true")]
        public void UpdateMePassword()
        {
            _usersApi.MeUpdatePassword("my-password", "my-password");
        }

        [Test]
        public void UpdateMePasswordWrongPassword()
        {
            Client.Dispose();
            
            var ioe = Assert.Throws<HttpException>( () => _usersApi.MeUpdatePassword("my-password-wrong", "my-password"));

            Assert.IsNotNull(ioe);
            Assert.AreEqual("unauthorized access", ioe.Message);            
        }

        [Test]
        [Property("basic_auth", "true")]
        public void UpdatePassword()
        {
            var user = _usersApi.Me();
            Assert.IsNotNull(user);

            _usersApi.UpdateUserPassword(user, "my-password", "my-password");
        }

        [Test]
        [Property("basic_auth", "true")]
        public void UpdatePasswordById()
        {
            var user = _usersApi.Me();
            Assert.IsNotNull(user);

            _usersApi.UpdateUserPassword(user.Id, "my-password", "my-password");
        }

        [Test]
        public void UpdatePasswordNotFound()
        {
            var ioe = Assert.Throws<HttpException>( () => _usersApi.UpdateUserPassword("020f755c3c082000", "", "new-password"));

            Assert.IsNotNull(ioe);
            Assert.AreEqual("user not found", ioe.Message);
        }

        [Test]
        public void UpdateUser()
        {
            var createdUser = _usersApi.CreateUser(GenerateName("John Ryzen"));
            createdUser.Name = "Tom Push";

            var updatedUser = _usersApi.UpdateUser(createdUser);

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Id, createdUser.Id);
            Assert.AreEqual(updatedUser.Name, "Tom Push");
        }
        
        [Test]
        public void CloneUser()
        {
            var source = _usersApi.CreateUser(GenerateName("John Ryzen"));

            var name = GenerateName("cloned");
            
            var cloned = _usersApi.CloneUser(name, source);
            
            Assert.AreEqual(name, cloned.Name);
        }

        [Test]
        public void CloneUserNotFound()
        {
            var ioe = Assert.Throws<HttpException>( () => _usersApi.CloneUser(GenerateName("bucket"),"020f755c3c082000"));
            
            Assert.AreEqual("user not found", ioe.Message);
        }
    }
}