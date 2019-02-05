using System.Linq;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NodaTime;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    [TestFixture]
    public class ItUserClientTest : AbstractItClientTest
    {
        [SetUp]
        public new void SetUp()
        {
            _userClient = PlatformClient.CreateUserClient();
        }

        private UserClient _userClient;

        [Test]
        public async Task CreateUser()
        {
            var userName = GenerateName("John Ryzen");

            var user = await _userClient.CreateUser(userName);

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
            var createdUser = await _userClient.CreateUser(GenerateName("John Ryzen"));
            Assert.IsNotNull(createdUser);

            var foundUser = await _userClient.FindUserById(createdUser.Id);
            Assert.IsNotNull(foundUser);

            // delete user
            await _userClient.DeleteUser(createdUser);

            foundUser = await _userClient.FindUserById(createdUser.Id);
            Assert.IsNull(foundUser);
        }

        [Test]
        public async Task FindUserById()
        {
            var userName = GenerateName("John Ryzen");

            var user = await _userClient.CreateUser(userName);

            var userById = await _userClient.FindUserById(user.Id);

            Assert.IsNotNull(userById);
            Assert.AreEqual(userById.Id, user.Id);
            Assert.AreEqual(userById.Name, user.Name);
        }

        [Test]
        public async Task FindUserByIdNull()
        {
            var user = await _userClient.FindUserById("020f755c3c082000");

            Assert.IsNull(user);
        }

        [Test]
        public async Task FindUserLogs()
        {
            var now = new Instant();

            var user = await _userClient.Me();
            Assert.IsNotNull(user);

            await _userClient.UpdateUser(user);

            var userLogs = await _userClient.FindUserLogs(user);

            Assert.IsTrue(userLogs.Any());
            Assert.AreEqual(userLogs[userLogs.Count - 1].Description, "User Updated");
            Assert.AreEqual(userLogs[userLogs.Count - 1].UserId, user.Id);
            Assert.IsTrue(userLogs[userLogs.Count - 1].Time > now);
        }

        [Test]
        public async Task FindUserLogsFindOptionsNotFound()
        {
            var entries = await _userClient.FindUserLogs("020f755c3c082000", new FindOptions());

            Assert.IsNotNull(entries);
            Assert.AreEqual(0, entries.Logs.Count);
        }

        [Test]
        public async Task FindUserLogsNotFound()
        {
            var logs = await _userClient.FindUserLogs("020f755c3c082000");

            Assert.AreEqual(0, logs.Count);
        }

        [Test]
        public async Task FindUserLogsPaging()
        {
            var user = await _userClient.CreateUser(GenerateName("John Ryzen"));

            foreach (var i in Enumerable.Range(0, 19))
            {
                user.Name = $"{i}_{user.Name}";

                await _userClient.UpdateUser(user);
            }

            var logs = await _userClient.FindUserLogs(user);

            Assert.AreEqual(20, logs.Count);
            Assert.AreEqual("User Created", logs[0].Description);
            Assert.AreEqual("User Updated", logs[19].Description);

            var findOptions = new FindOptions {Limit = 5, Offset = 0};

            var entries = await _userClient.FindUserLogs(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Created", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            //TODO isNotNull FindOptions also in Log API? 
            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _userClient.FindUserLogs(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Updated", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _userClient.FindUserLogs(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Updated", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _userClient.FindUserLogs(user, findOptions);
            Assert.AreEqual(5, entries.Logs.Count);
            Assert.AreEqual("User Updated", entries.Logs[0].Description);
            Assert.AreEqual("User Updated", entries.Logs[1].Description);
            Assert.AreEqual("User Updated", entries.Logs[2].Description);
            Assert.AreEqual("User Updated", entries.Logs[3].Description);
            Assert.AreEqual("User Updated", entries.Logs[4].Description);

            findOptions.Offset += 5;
            Assert.IsNull(entries.GetNextPage());

            entries = await _userClient.FindUserLogs(user, findOptions);
            Assert.AreEqual(0, entries.Logs.Count);
            Assert.IsNull(entries.GetNextPage());

            //
            // Order
            //
            findOptions = new FindOptions {Descending = false};
            entries = await _userClient.FindUserLogs(user, findOptions);
            Assert.AreEqual(20, entries.Logs.Count);

            Assert.AreEqual("User Updated", entries.Logs[19].Description);
            Assert.AreEqual("User Created", entries.Logs[0].Description);
        }

        [Test]
        public async Task FindUsers()
        {
            var size = (await _userClient.FindUsers()).Count;

            await _userClient.CreateUser(GenerateName("John Ryzen"));

            var users = await _userClient.FindUsers();

            Assert.AreEqual(users.Count, size + 1);
        }

        [Test]
        public async Task MeAuthenticated()
        {
            var me = await _userClient.Me();

            Assert.IsNotNull(me);
            Assert.AreEqual(me.Name, "my-user");
        }

        [Test]
        public async Task MeNotAuthenticated()
        {
            PlatformClient.Dispose();

            var me = await _userClient.Me();

            Assert.IsNull(me);
        }

        [Test]
        [Property("basic_auth", "true")]
        public async Task UpdateMePassword()
        {
            var me = await _userClient.MeUpdatePassword("my-password", "my-password");

            Assert.IsNotNull(me);
            Assert.AreEqual(me.Name, "my-user");
        }

        [Test]
        public async Task UpdateMePasswordNotAuthenticate()
        {
            PlatformClient.Dispose();

            var me = await _userClient.MeUpdatePassword("my-password", "my-password");

            Assert.IsNull(me);
        }

        [Test]
        [Property("basic_auth", "true")]
        public async Task UpdatePassword()
        {
            var user = await _userClient.Me();
            Assert.IsNotNull(user);

            var updatedUser = await _userClient.UpdateUserPassword(user, "my-password", "my-password");

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Name, user.Name);
            Assert.AreEqual(updatedUser.Id, user.Id);
        }

        [Test]
        [Property("basic_auth", "true")]
        public async Task UpdatePasswordById()
        {
            var user = await _userClient.Me();
            Assert.IsNotNull(user);

            var updatedUser = await _userClient.UpdateUserPassword(user.Id, "my-password", "my-password");

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Name, user.Name);
            Assert.AreEqual(updatedUser.Id, user.Id);
        }

        [Test]
        public async Task UpdatePasswordNotFound()
        {
            var updatedUser = await _userClient.UpdateUserPassword("020f755c3c082000", "", "new-password");

            Assert.IsNull(updatedUser);
        }

        [Test]
        public async Task UpdateUser()
        {
            var createdUser = await _userClient.CreateUser(GenerateName("John Ryzen"));
            createdUser.Name = "Tom Push";

            var updatedUser = await _userClient.UpdateUser(createdUser);

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Id, createdUser.Id);
            Assert.AreEqual(updatedUser.Name, "Tom Push");
        }
    }
}