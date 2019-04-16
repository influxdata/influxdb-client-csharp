using System;
using System.Collections.Generic;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItTelegrafsApiTest : AbstractItClientTest
    {
        [SetUp]
        public new async Task SetUp()
        {
            _telegrafsApi = Client.GetTelegrafsApi();
            _usersApi = Client.GetUsersApi();

            _organization = await FindMyOrg();

            foreach (var telegrafConfig in await _telegrafsApi.FindTelegrafConfigs())
                await _telegrafsApi.DeleteTelegrafConfig(telegrafConfig);

            Client.SetLogLevel(LogLevel.Body);
        }

        private TelegrafsApi _telegrafsApi;
        private UsersApi _usersApi;
        private Organization _organization;

        private static TelegrafPlugin NewCpuPlugin()
        {
            return new TelegrafPlugin {Name = "cpu", Type = TelegrafPluginType.Input};
        }

        private static TelegrafPlugin NewOutputPlugin()
        {
            var output = new TelegrafPlugin
            {
                Name = "influxdb_v2", Type = TelegrafPluginType.Output, Comment = "Output to Influx 2.0"
            };
            output.Config.Add("organization", "my-org");
            output.Config.Add("bucket", "my-bucket");
            output.Config.Add("urls", new[] {"http://127.0.0.1:9999"});
            output.Config.Add("token", "$INFLUX_TOKEN");

            return output;
        }

        [Test]
        public async Task CloneTelegrafConfig()
        {
            var source = await _telegrafsApi
                .CreateTelegrafConfig(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = Client.GetLabelsApi().CreateLabel(GenerateName("Cool Resource"), properties, _organization.Id);
            await _telegrafsApi.AddLabel(label, source);

            var name = GenerateName("cloned");

            var cloned = await _telegrafsApi.CloneTelegrafConfig(name, source);

            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual(_organization.Id, cloned.OrgId);
            Assert.AreEqual("test-config", cloned.Description);
            Assert.AreEqual(1_000, cloned.Agent.CollectionInterval);
            Assert.AreEqual(2, cloned.Plugins.Count);
            Assert.AreEqual("cpu", cloned.Plugins[0].Name);
            Assert.AreEqual("influxdb_v2", cloned.Plugins[1].Name);

            var labels = await _telegrafsApi.GetLabels(cloned);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
        }

        [Test]
        public void CloneTelegrafConfigNotFound()
        {
            var ioe = Assert.ThrowsAsync<InvalidOperationException>(async () =>
                await _telegrafsApi.CloneTelegrafConfig(GenerateName("tc"), "020f755c3c082000"));

            Assert.AreEqual("NotFound TelegrafConfig with ID: 020f755c3c082000", ioe.Message);
        }

        [Test]
        public async Task CreateTelegrafConfig()
        {
            var name = GenerateName("TelegrafConfig");

            var output = NewOutputPlugin();
            var cpu = NewCpuPlugin();

            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafConfig(name, "test-config", _organization, 1_000,
                    new List<TelegrafPlugin> {output, cpu});

            Assert.IsNotNull(telegrafConfig);
            Assert.AreEqual(name, telegrafConfig.Name);
            Assert.AreEqual("test-config", telegrafConfig.Description);
            Assert.AreEqual(_organization.Id, telegrafConfig.OrgId);
            Assert.IsNotNull(telegrafConfig.Agent);
            Assert.AreEqual(1_000, telegrafConfig.Agent.CollectionInterval);
            Assert.AreEqual(2, telegrafConfig.Plugins.Count);
            Assert.AreEqual("influxdb_v2", telegrafConfig.Plugins[0].Name);
            Assert.AreEqual("Output to Influx 2.0", telegrafConfig.Plugins[0].Comment);
            Assert.AreEqual(TelegrafPluginType.Output, telegrafConfig.Plugins[0].Type);
            Assert.AreEqual("cpu", telegrafConfig.Plugins[1].Name);
            Assert.AreEqual(TelegrafPluginType.Input, telegrafConfig.Plugins[1].Type);
        }

        [Test]
        public async Task DeleteTelegrafConfig()
        {
            var createdConfig = await _telegrafsApi
                .CreateTelegrafConfig(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafPlugin> {NewCpuPlugin(), NewOutputPlugin()});
            Assert.IsNotNull(createdConfig);

            var foundTelegrafConfig = await _telegrafsApi.FindTelegrafConfigById(createdConfig.Id);
            Assert.IsNotNull(foundTelegrafConfig);

            // delete source
            await _telegrafsApi.DeleteTelegrafConfig(createdConfig);

            foundTelegrafConfig = await _telegrafsApi.FindTelegrafConfigById(createdConfig.Id);
            Assert.IsNull(foundTelegrafConfig);
        }

        [Test]
        public void DeleteTelegrafConfigNotFound()
        {
            var ioe = Assert.ThrowsAsync<HttpException>(async () =>
                await _telegrafsApi.DeleteTelegrafConfig("020f755c3d082000"));

            Assert.AreEqual("telegraf configuration not found", ioe.Message);
            Assert.AreEqual(404, ioe.Status);
        }

        [Test]
        public async Task FindTelegrafConfigById()
        {
            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafConfig(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var telegrafConfigById = await _telegrafsApi.FindTelegrafConfigById(telegrafConfig.Id);

            Assert.IsNotNull(telegrafConfigById);
            Assert.AreEqual(telegrafConfig.Id, telegrafConfigById.Id);
            Assert.AreEqual(telegrafConfig.Name, telegrafConfigById.Name);
            Assert.AreEqual(telegrafConfig.OrgId, telegrafConfigById.OrgId);
            Assert.AreEqual(telegrafConfig.Description, telegrafConfigById.Description);
            Assert.AreEqual(telegrafConfig.Agent.CollectionInterval, telegrafConfigById.Agent.CollectionInterval);
            Assert.AreEqual(2, telegrafConfigById.Plugins.Count);
        }

        [Test]
        public async Task FindTelegrafConfigByIdNull()
        {
            var telegrafConfig = await _telegrafsApi.FindTelegrafConfigById("020f755c3d082000");

            Assert.IsNull(telegrafConfig);
        }

        [Test]
        public async Task FindTelegrafConfigByOrg()
        {
            var orgName = GenerateName("Constant Pro");

            var organization = await Client.GetOrganizationsApi().CreateOrganization(orgName);
            var telegrafConfigs = await _telegrafsApi.FindTelegrafConfigsByOrg(organization);

            Assert.AreEqual(0, telegrafConfigs.Count);

            await _telegrafsApi
                .CreateTelegrafConfig(GenerateName("tc"), "test-config", organization, 1_000,
                    new List<TelegrafPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            telegrafConfigs = await _telegrafsApi.FindTelegrafConfigsByOrg(organization);

            Assert.AreEqual(1, telegrafConfigs.Count);

            await _telegrafsApi.DeleteTelegrafConfig(telegrafConfigs[0]);
        }

        [Test]
        public async Task FindTelegrafConfigs()
        {
            var size = (await _telegrafsApi.FindTelegrafConfigs()).Count;

            await _telegrafsApi
                .CreateTelegrafConfig(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var telegrafConfigs = await _telegrafsApi.FindTelegrafConfigs();
            Assert.AreEqual(size + 1, telegrafConfigs.Count);
        }

        [Test]
        public async Task GetToml()
        {
            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafConfig(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var toml = await _telegrafsApi.GetTOML(telegrafConfig);

            Assert.True(toml.Contains("[[inputs.cpu]]"));
            Assert.True(toml.Contains("[[outputs.influxdb_v2]]"));
            Assert.True(toml.Contains("organization = \"my-org\""));
            Assert.True(toml.Contains("bucket = \"my-bucket\""));
        }

        [Test]
        public void GetTomlNotFound()
        {
            var ioe = Assert.ThrowsAsync<HttpException>(async () =>
                await _telegrafsApi.GetTOML("020f755c3d082000"));

            Assert.AreEqual("telegraf configuration not found", ioe.Message);
            Assert.AreEqual(404, ioe.Status);
        }

        [Test]
        public async Task Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafConfig(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = labelClient.CreateLabel(GenerateName("Cool Resource"), properties, _organization.Id);

            var labels = await _telegrafsApi.GetLabels(telegrafConfig);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = await _telegrafsApi.AddLabel(label, telegrafConfig);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = await _telegrafsApi.GetLabels(telegrafConfig);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            await _telegrafsApi.DeleteLabel(label, telegrafConfig);

            labels = await _telegrafsApi.GetLabels(telegrafConfig);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        public async Task Member()
        {
            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafConfig(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var members = await _telegrafsApi.GetMembers(telegrafConfig);
            Assert.AreEqual(0, members.Count);

            var user = _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _telegrafsApi.AddMember(user, telegrafConfig);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.UserId, user.Id);
            Assert.AreEqual(resourceMember.UserName, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.UserType.Member);

            members = await _telegrafsApi.GetMembers(telegrafConfig);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].UserId, user.Id);
            Assert.AreEqual(members[0].UserName, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.UserType.Member);

            await _telegrafsApi.DeleteMember(user, telegrafConfig);

            members = await _telegrafsApi.GetMembers(telegrafConfig);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        public async Task Owner()
        {
            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafConfig(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var owners = await _telegrafsApi.GetOwners(telegrafConfig);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual("my-user", owners[0].UserName);

            var user = _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = await _telegrafsApi.AddOwner(user, telegrafConfig);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.UserId, user.Id);
            Assert.AreEqual(resourceMember.UserName, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.UserType.Owner);

            owners = await _telegrafsApi.GetOwners(telegrafConfig);
            Assert.AreEqual(2, owners.Count);
            Assert.AreEqual(owners[1].UserId, user.Id);
            Assert.AreEqual(owners[1].UserName, user.Name);
            Assert.AreEqual(owners[1].Role, ResourceMember.UserType.Owner);

            await _telegrafsApi.DeleteOwner(user, telegrafConfig);

            owners = await _telegrafsApi.GetOwners(telegrafConfig);
            Assert.AreEqual(1, owners.Count);
        }

        [Test]
        public async Task UpdateTelegrafConfig()
        {
            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafConfig(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            telegrafConfig.Description = "updated";
            telegrafConfig.Agent.CollectionInterval = 500;
            telegrafConfig.Plugins.RemoveAt(0);

            telegrafConfig = await _telegrafsApi.UpdateTelegrafConfig(telegrafConfig);

            Assert.AreEqual("updated", telegrafConfig.Description);
            Assert.AreEqual(500, telegrafConfig.Agent.CollectionInterval);
            Assert.AreEqual(1, telegrafConfig.Plugins.Count);
        }
    }
}