using System;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItUsersApiTest : AbstractItClientTest
    {
        [SetUp]
        public new async Task SetUp()
        {
            _usersApi = Client.GetUsersApi();

            foreach (var user in (await _usersApi.FindUsersAsync()).Where(user => user.Name.EndsWith("-IT")))
                await _usersApi.DeleteUserAsync(user);
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

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _usersApi.FindUserByIdAsync(createdUser.Id));
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
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _usersApi.FindUserByIdAsync("020f755c3c082000"));

            Assert.IsNotNull(ioe);
            Assert.AreEqual("user not found", ioe.Message);
        }

        [Test]
        public async Task FindUsers()
        {
            var size = (await _usersApi.FindUsersAsync()).Count;

            await _usersApi.CreateUserAsync(GenerateName("John Ryzen"));

            var users = await _usersApi.FindUsersAsync();

            Assert.AreEqual(size + 1, users.Count);
        }

        [Test]
        public async Task MeAuthenticated()
        {
            var me = await _usersApi.MeAsync();

            Assert.IsNotNull(me);
            Assert.AreEqual(me.Name, "my-user");
        }

        [Test]
        [Property("basic_auth", "true")]
        [Ignore("TODO not implemented set password https://github.com/influxdata/influxdb/pull/15981")]
        public async Task UpdateMePassword()
        {
            await _usersApi.MeUpdatePasswordAsync("my-password", "my-password");
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
        [Ignore("TODO not implemented set password https://github.com/influxdata/influxdb/pull/15981")]
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
            Assert.AreEqual(typeof(NotFoundException), ioe.InnerException.GetType());
        }

        [Test]
        public async Task UpdateUser()
        {
            var createdUser = await _usersApi.CreateUserAsync(GenerateName("John Ryzen"));
            var name = GenerateName("Tom Push");
            createdUser.Name = name;

            var updatedUser = await _usersApi.UpdateUserAsync(createdUser);

            Assert.IsNotNull(updatedUser);
            Assert.AreEqual(updatedUser.Id, createdUser.Id);
            Assert.AreEqual(updatedUser.Name, name);
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
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _usersApi.CloneUserAsync(GenerateName("bucket"), "020f755c3c082000"));

            Assert.AreEqual("user not found", ioe.Message);
            Assert.AreEqual(typeof(NotFoundException), ioe.GetType());
        }
    }
}