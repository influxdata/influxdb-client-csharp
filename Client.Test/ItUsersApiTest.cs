using System;
using System.Linq;
using System.Threading.Tasks;
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
        public async Task CreateUser()
        {
            var userName = GenerateName("John Ryzen");

            var user = await _usersApi.CreateUserAsync(userName);

            Assert.IsNotNull(user);
            Assert.IsNotEmpty(user.Id);
            Assert.AreEqual(user.Name, userName);

            var links = user.Links;

            Assert.IsNotNull(links);
            Assert.AreEqual(links.Self, $"/api/v2/users/{user.Id}");
            Assert.AreEqual(links.Logs, $"/api/v2/users/{user.Id}/logs");
        }

        [Test]
        public async Task DeleteUser()
        {
            var createdUser = await _usersApi.CreateUserAsync(GenerateName("John Ryzen"));
            Assert.IsNotNull(createdUser);

            var foundUser = await _usersApi.FindUserByIdAsync(createdUser.Id);
            Assert.IsNotNull(foundUser);

            // delete user
            await _usersApi.DeleteUserAsync(createdUser);

            var ioe = Assert.ThrowsAsync<HttpException>(async () => await _usersApi.FindUserByIdAsync(createdUser.Id));
            Assert.IsNotNull(ioe);
            Assert.AreEqual("user not found", ioe.Message);
        }

        [Test]
        public async Task FindUserById()
        {
            var userName = GenerateName("John Ryzen");

            var user = await _usersApi.CreateUserAsync(userName);

            var userById = await _usersApi.FindUserByIdAsync(user.Id);

            Assert.IsNotNull(userById);
            Assert.AreEqual(userById.Id, user.Id);
            Assert.AreEqual(userById.Name, user.Name);
        }

        [Test]
        public void FindUserByIdNull()
        {
            var ioe = Assert.ThrowsAsync<HttpException>(async () => await _usersApi.FindUserByIdAsync("020f755c3c082000"));

            Assert.IsNotNull(ioe);
            Assert.AreEqual("user not found", ioe.Message);
        }

        [Test]
        public async Task FindUserLogs()
        {
            var now = new DateTime();

            var user = await _usersApi.MeAsync();
            Assert.IsNotNull(user);

            await _usersApi.UpdateUserAsync(user);

            var userLogs = await _usersApi.FindUserLogsAsync(user);

            Assert.IsTrue(userLogs.Any());
            Assert.AreEqual(userLogs[userLogs.Count - 1].Description, "User Updated");
            Assert.AreEqual(userLogs[userLogs.Count - 1].UserID, user.Id);
            Assert.IsTrue(userLogs[userLogs.Count - 1].Time > now);
        }

        [Test]
        public async Task FindUserLogsFindOptionsNotFound()
        {
            var entries = await _usersApi.FindUserLogsAsync("020f755c3c082000", new FindOptions());

            Assert.IsNotNull(entries);
            Assert.AreEqual(0, entries.Logs.Count);
        }

        [Test]
        public async Task FindUserLogsNotFound()
        {
            var logs = await _usersApi.FindUserLogsAsync("020f755c3c082000");

            Assert.AreEqual(0, logs.Count);
        }

        [Test]
        public async Task FindUserLogsPaging()
        {
            var user = await _usersApi.CreateUserAsync(GenerateName("John Ryzen"));

            foreach (var i in Enumerable.Range(0, 19))
            {
                user.Name = $"{i}_{user.Name}";

                await _usersApi.UpdateUserAsync(user);
            }

            var logs = await _usersApi.FindUserLogsAsync(user);

            Assert.AreEqual(20, logs.Count);
            Assert.AreEqual("User Created", logs[0].Description);
            Assert.AreEqual("User Updated", logs[19].Description);

            var findOptions = new FindOptions {Limit = 5, Offset = 0};

            var entries = await _usersApi.FindUserLogsAsync(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Created", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;

            entries = await _usersApi.FindUserLogsAsync(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Updated", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;

            entries = await _usersApi.FindUserLogsAsync(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Updated", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;

            entries = await _usersApi.FindUserLogsAsync(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Updated", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;

            entries = await _usersApi.FindUserLogsAsync(user, findOptions);
            Assert.AreEqual(0, entries.Logs.Count);

            //
            // Order
            //
            findOptions = new FindOptions {Descending = false};
            entries = await _usersApi.FindUserLogsAsync(user, findOptions);
            Assert.AreEqual(20, entries.Logs.Count);

            Assert.AreEqual("User Updated", entries.Logs[19].Description);
            Assert.AreEqual("User Created", entries.Logs[0].Description);
        }

        [Test]
        public async Task FindUsers()
        {
            var size = (await _usersApi.FindUsersAsync()).Count;

            await _usersApi.CreateUserAsync(GenerateName("John Ryzen"));

            var users = await _usersApi.FindUsersAsync();

            Assert.AreEqual(users.Count, size + 1);
        }

        [Test]
        public async Task MeAuthenticated()
        {
            var me = await _usersApi.MeAsync();

            Assert.IsNotNull(me);
            Assert.AreEqual(me.Name, "my-user");
        }

        [Test]
        public void MeNotAuthenticated()
        {
            Client.Dispose();

            var ioe = Assert.ThrowsAsync<HttpException>(async () => await _usersApi.MeAsync());

            Assert.IsNotNull(ioe);
            Assert.AreEqual("unauthorized access", ioe.Message);
        }

        [Test]
        [Property("basic_auth", "true")]
        [Ignore("TODO not implemented set password https://github.com/influxdata/influxdb/pull/15981")]
        public async Task UpdateMePassword()
        {
            await _usersApi.MeUpdatePasswordAsync("my-password", "my-password");
        }

        [Test]
        public void UpdateMePasswordWrongPassword()
        {
            Client.Dispose();

            var ioe = Assert.ThrowsAsync<AggregateException>(async () =>
                await _usersApi.MeUpdatePasswordAsync("my-password-wrong", "my-password"));

            Assert.IsNotNull(ioe);
            Assert.AreEqual("unauthorized access", ioe.InnerException.Message);
            Assert.AreEqual(typeof(HttpException), ioe.InnerException.GetType());
        }

        [Test]
        [Property("basic_auth", "true")]
        [Ignore("TODO not implemented set password https://github.com/influxdata/influxdb/pull/15981")]
        public async Task UpdatePassword()
        {
            var user = await _usersApi.MeAsync();
            Assert.IsNotNull(user);

            await _usersApi.UpdateUserPasswordAsync(user, "my-password", "my-password");
        }

        [Test]
        [Property("basic_auth", "true")]
        public async Task UpdatePasswordById()
        {
            var user = await _usersApi.MeAsync();
            Assert.IsNotNull(user);

            await _usersApi.UpdateUserPasswordAsync(user.Id, "my-password", "my-password");
        }

        [Test]
        public void UpdatePasswordNotFound()
        {
            var ioe = Assert.ThrowsAsync<AggregateException>(async () =>
                await _usersApi.UpdateUserPasswordAsync("020f755c3c082000", "", "new-password"));

            Assert.IsNotNull(ioe);
            Assert.AreEqual("user not found", ioe.InnerException.Message);
            Assert.AreEqual(typeof(HttpException), ioe.InnerException.GetType());
        }

        [Test]
        public async Task UpdateUser()
        {
            var createdUser = await _usersApi.CreateUserAsync(GenerateName("John Ryzen"));
            createdUser.Name = "Tom Push";

            var updatedUser = await _usersApi.UpdateUserAsync(createdUser);

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Id, createdUser.Id);
            Assert.AreEqual(updatedUser.Name, "Tom Push");
        }

        [Test]
        public async Task CloneUser()
        {
            var source = await _usersApi.CreateUserAsync(GenerateName("John Ryzen"));

            var name = GenerateName("cloned");

            var cloned = await _usersApi.CloneUserAsync(name, source);

            Assert.AreEqual(name, cloned.Name);
        }

        [Test]
        public void CloneUserNotFound()
        {
            var ioe = Assert.ThrowsAsync<AggregateException>(async () =>
                await _usersApi.CloneUserAsync(GenerateName("bucket"), "020f755c3c082000"));

            Assert.AreEqual("user not found", ioe.InnerException.Message);
            Assert.AreEqual(typeof(HttpException), ioe.InnerException.GetType());
        }
    }
}