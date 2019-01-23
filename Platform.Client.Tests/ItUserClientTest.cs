using System.Linq;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Client;
using NodaTime;
using NUnit.Framework;

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
        public async Task FindUsers()
        {
            var size = (await _userClient.FindUsers()).Count;

            await _userClient.CreateUser(GenerateName("John Ryzen"));

            var users = await _userClient.FindUsers();

            Assert.AreEqual(users.Count, size + 1);
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
        public async Task UpdateUser()
        {
            var createdUser = await _userClient.CreateUser(GenerateName("John Ryzen"));
            createdUser.Name = "Tom Push";

            var updatedUser = await _userClient.UpdateUser(createdUser);

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Id, createdUser.Id);
            Assert.AreEqual(updatedUser.Name, "Tom Push");
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
        public async Task UpdatePassword() {

            var user = await _userClient.Me();
            Assert.IsNotNull(user);

            var updatedUser = await _userClient.UpdateUserPassword(user, "my-password", "my-password");

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Name, user.Name);
            Assert.AreEqual(updatedUser.Id, user.Id);
        }

        [Test]
        public async Task UpdatePasswordNotFound() {

            var updatedUser =  await _userClient.UpdateUserPassword("020f755c3c082000", "", "new-password");

            Assert.IsNull(updatedUser);
        }

        [Test]
        [Property("basic_auth", "true")]
        public async Task UpdatePasswordById() {

            var user = await _userClient.Me();
            Assert.IsNotNull(user);

            var updatedUser = await _userClient.UpdateUserPassword(user.Id, "my-password", "my-password");

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Name, user.Name);
            Assert.AreEqual(updatedUser.Id, user.Id);
        }
        
        [Test]
        public async Task FindUserLogs() {

            var now = new Instant();

            var user = await _userClient.Me();
            Assert.IsNotNull(user);

            await _userClient.UpdateUser(user);

            var userLogs =  await _userClient.FindUserLogs(user);
            
            Assert.IsTrue(userLogs.Any());
            Assert.AreEqual(userLogs[0].Description, "User Updated");
            Assert.AreEqual(userLogs[0].UserId, user.Id);
            Assert.IsTrue(userLogs[0].Time > now);
        }

        [Test]
        public async Task FindUserLogsNotFound() {
            var userLogs = await _userClient.FindUserLogs("020f755c3c082000");
            
            Assert.IsFalse(userLogs.Any());
        }
    }
}