using System.Collections.Generic;
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
        private UserClient _userClient;

        [SetUp]
        public new void SetUp()
        {
            _userClient = PlatformClient.CreateUserClient();
        }

        [Test]
        public async Task CreateUser()
        {
            string userName = GenerateName("John Ryzen");

            User user = await _userClient.CreateUser(userName);

            Assert.IsNotNull(user);
            Assert.IsNotEmpty(user.Id);
            Assert.AreEqual(user.Name, userName);

            var links = user.Links;

            Assert.That(links.Count == 2);
            Assert.AreEqual(links["self"], $"/api/v2/users/{user.Id}");
            Assert.AreEqual(links["log"], $"/api/v2/users/{user.Id}/log");
        }

        [Test]
        public async Task FindUserById()
        {
            string userName = GenerateName("John Ryzen");

            User user = await _userClient.CreateUser(userName);

            User userById = await _userClient.FindUserById(user.Id);

            Assert.IsNotNull(userById);
            Assert.AreEqual(userById.Id, user.Id);
            Assert.AreEqual(userById.Name, user.Name);
        }

        [Test]
        public async Task FindUserByIdNull()
        {
            User user = await _userClient.FindUserById("020f755c3c082000");

            Assert.IsNull(user);
        }

        [Test]
        public async Task FindUsers()
        {
            int size = (await _userClient.FindUsers()).Count;

            await _userClient.CreateUser(GenerateName("John Ryzen"));

            List<User> users = await _userClient.FindUsers();

            Assert.AreEqual(users.Count, size + 1);
        }

        [Test]
        public async Task DeleteUser()
        {
            User createdUser = await _userClient.CreateUser(GenerateName("John Ryzen"));
            Assert.IsNotNull(createdUser);

            User foundUser = await _userClient.FindUserById(createdUser.Id);
            Assert.IsNotNull(foundUser);

            // delete user
            await _userClient.DeleteUser(createdUser);

            foundUser = await _userClient.FindUserById(createdUser.Id);
            Assert.IsNull(foundUser);
        }

        [Test]
        public async Task UpdateUser()
        {
            User createdUser = await _userClient.CreateUser(GenerateName("John Ryzen"));
            createdUser.Name = "Tom Push";

            User updatedUser = await _userClient.UpdateUser(createdUser);

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Id, createdUser.Id);
            Assert.AreEqual(updatedUser.Name, "Tom Push");
        }

        [Test]
        public async Task MeAuthenticated()
        {
            User me = await _userClient.Me();

            Assert.IsNotNull(me);
            Assert.AreEqual(me.Name, "my-user");
        }

        [Test]
        public async Task MeNotAuthenticated()
        {
            PlatformClient.Dispose();

            User me = await _userClient.Me();

            Assert.IsNull(me);
        }

        [Test]
        public async Task UpdateMePassword()
        {
            User me = await _userClient.MeUpdatePassword("my-password", "my-password");

            Assert.IsNotNull(me);
            Assert.AreEqual(me.Name, "my-user");
        }

        [Test]
        public async Task UpdateMePasswordNotAuthenticate()
        {
            PlatformClient.Dispose();

            User me = await _userClient.MeUpdatePassword("my-password", "my-password");

            Assert.IsNull(me);
        }
        
        [Test]
        public async Task UpdatePassword() {

            User user = await _userClient.Me();
            Assert.IsNotNull(user);

            User updatedUser = await _userClient.UpdateUserPassword(user, "my-password", "my-password");

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Name, user.Name);
            Assert.AreEqual(updatedUser.Id, user.Id);
        }

        [Test]
        public async Task UpdatePasswordNotFound() {

            User updatedUser =  await _userClient.UpdateUserPassword("020f755c3c082000", "", "new-password");

            Assert.IsNull(updatedUser);
        }

        [Test]
        public async Task UpdatePasswordById() {

            User user = await _userClient.Me();
            Assert.IsNotNull(user);

            User updatedUser = await _userClient.UpdateUserPassword(user.Id, "my-password", "my-password");

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Name, user.Name);
            Assert.AreEqual(updatedUser.Id, user.Id);
        }
        
        [Test]
        public async Task FindUserLogs() {

            Instant now = new Instant();

            User user = await _userClient.Me();
            Assert.IsNotNull(user);

            await _userClient.UpdateUser(user);

            List<OperationLogEntry> userLogs =  await _userClient.FindUserLogs(user);
            
            Assert.IsTrue(userLogs.Any());
            Assert.AreEqual(userLogs[0].Description, "User Updated");
            Assert.AreEqual(userLogs[0].UserId, user.Id);
            Assert.IsTrue(userLogs[0].Time > now);
        }

        [Test]
        public async Task FindUserLogsNotFound() {
            List<OperationLogEntry> userLogs = await _userClient.FindUserLogs("020f755c3c082000");
            
            Assert.IsFalse(userLogs.Any());
        }
    }
}