using System.Linq;
using InfluxDB.Client.Domain;
using NodaTime;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

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

            var user = await _usersApi.CreateUser(userName);

            Assert.IsNotNull(user);
            Assert.IsNotEmpty(user.Id);
            Assert.AreEqual(user.Name, userName);

            var links = user.Links;

            Assert.That(links.Count == 2);
            Assert.AreEqual(links["self"], $"/api/v2/users/{user.Id}");
            Assert.AreEqual(links["log"], $"/api/v2/users/{user.Id}/log");
        }

        [Test]
        public async Task DeleteUser()
        {
            var createdUser = await _usersApi.CreateUser(GenerateName("John Ryzen"));
            Assert.IsNotNull(createdUser);

            var foundUser = await _usersApi.FindUserById(createdUser.Id);
            Assert.IsNotNull(foundUser);

            // delete user
            await _usersApi.DeleteUser(createdUser);

            foundUser = await _usersApi.FindUserById(createdUser.Id);
            Assert.IsNull(foundUser);
        }

        [Test]
        public async Task FindUserById()
        {
            var userName = GenerateName("John Ryzen");

            var user = await _usersApi.CreateUser(userName);

            var userById = await _usersApi.FindUserById(user.Id);

            Assert.IsNotNull(userById);
            Assert.AreEqual(userById.Id, user.Id);
            Assert.AreEqual(userById.Name, user.Name);
        }

        [Test]
        public async Task FindUserByIdNull()
        {
            var user = await _usersApi.FindUserById("020f755c3c082000");

            Assert.IsNull(user);
        }

        [Test]
        public async Task FindUserLogs()
        {
            var now = new Instant();

            var user = await _usersApi.Me();
            Assert.IsNotNull(user);

            await _usersApi.UpdateUser(user);

            var userLogs = await _usersApi.FindUserLogs(user);

            Assert.IsTrue(userLogs.Any());
            Assert.AreEqual(userLogs[userLogs.Count - 1].Description, "User Updated");
            Assert.AreEqual(userLogs[userLogs.Count - 1].UserId, user.Id);
            Assert.IsTrue(userLogs[userLogs.Count - 1].Time.ToInstant() > now);
        }

        [Test]
        public async Task FindUserLogsFindOptionsNotFound()
        {
            var entries = await _usersApi.FindUserLogs("020f755c3c082000", new FindOptions());

            Assert.IsNotNull(entries);
            Assert.AreEqual(0, entries.Logs.Count);
        }

        [Test]
        public async Task FindUserLogsNotFound()
        {
            var logs = await _usersApi.FindUserLogs("020f755c3c082000");

            Assert.AreEqual(0, logs.Count);
        }

        [Test]
        public async Task FindUserLogsPaging()
        {
            var user = await _usersApi.CreateUser(GenerateName("John Ryzen"));

            foreach (var i in Enumerable.Range(0, 19))
            {
                user.Name = $"{i}_{user.Name}";

                await _usersApi.UpdateUser(user);
            }

            var logs = await _usersApi.FindUserLogs(user);

            Assert.AreEqual(20, logs.Count);
            Assert.AreEqual("User Created", logs[0].Description);
            Assert.AreEqual("User Updated", logs[19].Description);

            var findOptions = new FindOptions {Limit = 5, Offset = 0};

            var entries = await _usersApi.FindUserLogs(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Created", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            //TODO isNotNull FindOptions also in Log API? 
            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _usersApi.FindUserLogs(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Updated", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _usersApi.FindUserLogs(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Updated", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _usersApi.FindUserLogs(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Updated", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _usersApi.FindUserLogs(user, findOptions);
            Assert.AreEqual(0, entries.Logs.Count);
            Assert.IsNull(entries.GetNextPage());

            //
            // Order
            //
            findOptions = new FindOptions {Descending = false};
            entries = await _usersApi.FindUserLogs(user, findOptions);
            Assert.AreEqual(20, entries.Logs.Count);

            Assert.AreEqual("User Updated", entries.Logs[19].Description);
            Assert.AreEqual("User Created", entries.Logs[0].Description);
        }

        [Test]
        public async Task FindUsers()
        {
            var size = (await _usersApi.FindUsers()).Count;

            await _usersApi.CreateUser(GenerateName("John Ryzen"));

            var users = await _usersApi.FindUsers();

            Assert.AreEqual(users.Count, size + 1);
        }

        [Test]
        public async Task MeAuthenticated()
        {
            var me = await _usersApi.Me();

            Assert.IsNotNull(me);
            Assert.AreEqual(me.Name, "my-user");
        }

        [Test]
        public async Task MeNotAuthenticated()
        {
            Client.Dispose();

            var me = await _usersApi.Me();

            Assert.IsNull(me);
        }

        [Test]
        [Property("basic_auth", "true")]
        public async Task UpdateMePassword()
        {
            var me = await _usersApi.MeUpdatePassword("my-password", "my-password");

            Assert.IsNotNull(me);
            Assert.AreEqual(me.Name, "my-user");
        }

        [Test]
        public async Task UpdateMePasswordNotAuthenticate()
        {
            Client.Dispose();

            var me = await _usersApi.MeUpdatePassword("my-password", "my-password");

            Assert.IsNull(me);
        }

        [Test]
        [Property("basic_auth", "true")]
        public async Task UpdatePassword()
        {
            var user = await _usersApi.Me();
            Assert.IsNotNull(user);

            var updatedUser = await _usersApi.UpdateUserPassword(user, "my-password", "my-password");

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Name, user.Name);
            Assert.AreEqual(updatedUser.Id, user.Id);
        }

        [Test]
        [Property("basic_auth", "true")]
        public async Task UpdatePasswordById()
        {
            var user = await _usersApi.Me();
            Assert.IsNotNull(user);

            var updatedUser = await _usersApi.UpdateUserPassword(user.Id, "my-password", "my-password");

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Name, user.Name);
            Assert.AreEqual(updatedUser.Id, user.Id);
        }

        [Test]
        public async Task UpdatePasswordNotFound()
        {
            var updatedUser = await _usersApi.UpdateUserPassword("020f755c3c082000", "", "new-password");

            Assert.IsNull(updatedUser);
        }

        [Test]
        public async Task UpdateUser()
        {
            var createdUser = await _usersApi.CreateUser(GenerateName("John Ryzen"));
            createdUser.Name = "Tom Push";

            var updatedUser = await _usersApi.UpdateUser(createdUser);

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Id, createdUser.Id);
            Assert.AreEqual(updatedUser.Name, "Tom Push");
        }
    }
}