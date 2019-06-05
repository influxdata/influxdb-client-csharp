using System.Collections.Generic;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItTelegrafsApiTest : AbstractItClientTest
    {
        [SetUp]
        public new void SetUp()
        {
            _telegrafsApi = Client.GetTelegrafsApi();
            _usersApi = Client.GetUsersApi();

            _organization = FindMyOrg();

            foreach (var telegrafConfig in _telegrafsApi.FindTelegrafs()) _telegrafsApi.DeleteTelegraf(telegrafConfig);

            Client.SetLogLevel(LogLevel.Body);
        }

        private TelegrafsApi _telegrafsApi;
        private UsersApi _usersApi;
        private Organization _organization;

        private static TelegrafRequestPlugin NewCpuPlugin()
        {
            return new TelegrafPluginInputCpu();
        }

        private static TelegrafRequestPlugin NewOutputPlugin()
        {
            var config = new TelegrafPluginOutputInfluxDBV2Config(new List<string> {"http://127.0.0.1:9999"},
                "$INFLUX_TOKEN", "my-org", "my-bucket");

            var output = new TelegrafPluginOutputInfluxDBV2(TelegrafPluginOutputInfluxDBV2.NameEnum.Influxdbv2,
                TelegrafRequestPlugin.TypeEnum.Output, "Output to Influx 2.0", config);

            return output;
        }

        [Test]
        public void CloneTelegraf()
        {
            var source = _telegrafsApi
                .CreateTelegraf(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = Client.GetLabelsApi().CreateLabel(GenerateName("Cool Resource"), properties, _organization.Id);
            _telegrafsApi.AddLabel(label, source);

            var name = GenerateName("cloned");

            var cloned = _telegrafsApi.CloneTelegraf(name, source);

            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual(_organization.Id, cloned.OrgID);
            Assert.AreEqual("test-config", cloned.Description);
            Assert.AreEqual(1_000, cloned.Agent.CollectionInterval);
            Assert.AreEqual(2, cloned.Plugins.Count);
            Assert.AreEqual(TelegrafPluginInputCpu.NameEnum.Cpu, ((TelegrafPluginInputCpu) cloned.Plugins[0]).Name);
            Assert.AreEqual(TelegrafPluginOutputInfluxDBV2.NameEnum.Influxdbv2,
                ((TelegrafPluginOutputInfluxDBV2) cloned.Plugins[1]).Name);

            var labels = _telegrafsApi.GetLabels(cloned);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
        }

        [Test]
        public void CloneTelegrafNotFound()
        {
            var ioe = Assert.Throws<HttpException>(() =>
                _telegrafsApi.CloneTelegraf(GenerateName("tc"), "020f755c3c082000"));

            Assert.AreEqual("telegraf configuration not found", ioe.Message);
        }

        [Test]
        public void CreateTelegraf()
        {
            var name = GenerateName("Telegraf");

            var output = NewOutputPlugin();
            var cpu = NewCpuPlugin();

            var telegrafConfig = _telegrafsApi
                .CreateTelegraf(name, "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {output, cpu});

            Assert.IsNotNull(telegrafConfig);
            Assert.AreEqual(name, telegrafConfig.Name);
            Assert.AreEqual("test-config", telegrafConfig.Description);
            Assert.AreEqual(_organization.Id, telegrafConfig.OrgID);
            Assert.IsNotNull(telegrafConfig.Agent);
            Assert.AreEqual(1_000, telegrafConfig.Agent.CollectionInterval);
            Assert.AreEqual(2, telegrafConfig.Plugins.Count);
            Assert.AreEqual(TelegrafPluginOutputInfluxDBV2.NameEnum.Influxdbv2,
                ((TelegrafPluginOutputInfluxDBV2) telegrafConfig.Plugins[0]).Name);
            Assert.AreEqual("Output to Influx 2.0",
                ((TelegrafPluginOutputInfluxDBV2) telegrafConfig.Plugins[0]).Comment);
            Assert.AreEqual(TelegrafRequestPlugin.TypeEnum.Output, telegrafConfig.Plugins[0].type);
            Assert.AreEqual(TelegrafPluginInputCpu.NameEnum.Cpu,
                ((TelegrafPluginInputCpu) telegrafConfig.Plugins[1]).Name);
            Assert.AreEqual(TelegrafRequestPlugin.TypeEnum.Input, telegrafConfig.Plugins[1].type);
        }

        [Test]
        public void DeleteTelegraf()
        {
            var createdConfig = _telegrafsApi
                .CreateTelegraf(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});
            Assert.IsNotNull(createdConfig);

            var foundTelegraf = _telegrafsApi.FindTelegrafById(createdConfig.Id);
            Assert.IsNotNull(foundTelegraf);

            // delete source
            _telegrafsApi.DeleteTelegraf(createdConfig);

            var ioe = Assert.Throws<HttpException>(() => _telegrafsApi.FindTelegrafById(createdConfig.Id));

            Assert.AreEqual("telegraf configuration not found", ioe.Message);
        }

        [Test]
        public void DeleteTelegrafNotFound()
        {
            var ioe = Assert.Throws<HttpException>(() =>
                _telegrafsApi.DeleteTelegraf("020f755c3d082000"));

            Assert.AreEqual("telegraf configuration not found", ioe.Message);
            Assert.AreEqual(404, ioe.Status);
        }

        [Test]
        public void FindTelegrafById()
        {
            var telegrafConfig = _telegrafsApi
                .CreateTelegraf(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var telegrafConfigById = _telegrafsApi.FindTelegrafById(telegrafConfig.Id);

            Assert.IsNotNull(telegrafConfigById);
            Assert.AreEqual(telegrafConfig.Id, telegrafConfigById.Id);
            Assert.AreEqual(telegrafConfig.Name, telegrafConfigById.Name);
            Assert.AreEqual(telegrafConfig.OrgID, telegrafConfigById.OrgID);
            Assert.AreEqual(telegrafConfig.Description, telegrafConfigById.Description);
            Assert.AreEqual(telegrafConfig.Agent.CollectionInterval, telegrafConfigById.Agent.CollectionInterval);
            Assert.AreEqual(2, telegrafConfigById.Plugins.Count);
        }

        [Test]
        public void FindTelegrafByIdNull()
        {
            var ioe = Assert.Throws<HttpException>(() =>  _telegrafsApi.FindTelegrafById("020f755c3d082000"));

            Assert.AreEqual("telegraf configuration not found", ioe.Message);
        }

        [Test]
        public void FindTelegrafByOrg()
        {
            var orgName = GenerateName("Constant Pro");

            var organization = Client.GetOrganizationsApi().CreateOrganization(orgName);
            var telegrafConfigs = _telegrafsApi.FindTelegrafsByOrg(organization);

            Assert.AreEqual(0, telegrafConfigs.Count);

            _telegrafsApi
                .CreateTelegraf(GenerateName("tc"), "test-config", organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            telegrafConfigs = _telegrafsApi.FindTelegrafsByOrg(organization);

            Assert.AreEqual(1, telegrafConfigs.Count);

            _telegrafsApi.DeleteTelegraf(telegrafConfigs[0]);
        }

        [Test]
        public void FindTelegrafs()
        {
            var size = (_telegrafsApi.FindTelegrafs()).Count;

            _telegrafsApi
                .CreateTelegraf(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var telegrafConfigs = _telegrafsApi.FindTelegrafs();
            Assert.AreEqual(size + 1, telegrafConfigs.Count);
        }

        [Test]
        public void GetToml()
        {
            var telegrafConfig = _telegrafsApi
                .CreateTelegraf(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var toml = _telegrafsApi.GetTOML(telegrafConfig);

            Assert.True(toml.Contains("[[inputs.cpu]]"));
            Assert.True(toml.Contains("[[outputs.influxdb_v2]]"));
            Assert.True(toml.Contains("organization = \"my-org\""));
            Assert.True(toml.Contains("bucket = \"my-bucket\""));
        }

        [Test]
        public void GetTomlNotFound()
        {
            var ioe = Assert.Throws<HttpException>(() =>
                _telegrafsApi.GetTOML("020f755c3d082000"));

            Assert.AreEqual("telegraf configuration not found", ioe.Message);
            Assert.AreEqual(404, ioe.Status);
        }

        [Test]
        public void Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var telegrafConfig = _telegrafsApi
                .CreateTelegraf(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = labelClient.CreateLabel(GenerateName("Cool Resource"), properties, _organization.Id);

            var labels = _telegrafsApi.GetLabels(telegrafConfig);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = _telegrafsApi.AddLabel(label, telegrafConfig);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = _telegrafsApi.GetLabels(telegrafConfig);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            _telegrafsApi.DeleteLabel(label, telegrafConfig);

            labels = _telegrafsApi.GetLabels(telegrafConfig);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        public void Member()
        {
            var telegrafConfig = _telegrafsApi
                .CreateTelegraf(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var members = _telegrafsApi.GetMembers(telegrafConfig);
            Assert.AreEqual(0, members.Count);

            var user = _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = _telegrafsApi.AddMember(user, telegrafConfig);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.RoleEnum.Member);

            members = _telegrafsApi.GetMembers(telegrafConfig);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].Id, user.Id);
            Assert.AreEqual(members[0].Name, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.RoleEnum.Member);

            _telegrafsApi.DeleteMember(user, telegrafConfig);

            members = _telegrafsApi.GetMembers(telegrafConfig);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        public void Owner()
        {
            var telegrafConfig = _telegrafsApi
                .CreateTelegraf(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var owners = _telegrafsApi.GetOwners(telegrafConfig);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual("my-user", owners[0].Name);

            var user = _usersApi.CreateUser(GenerateName("Luke Health"));

            var resourceMember = _telegrafsApi.AddOwner(user, telegrafConfig);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceOwner.RoleEnum.Owner);

            owners = _telegrafsApi.GetOwners(telegrafConfig);
            Assert.AreEqual(2, owners.Count);
            Assert.AreEqual(owners[1].Id, user.Id);
            Assert.AreEqual(owners[1].Name, user.Name);
            Assert.AreEqual(owners[1].Role, ResourceOwner.RoleEnum.Owner);

            _telegrafsApi.DeleteOwner(user, telegrafConfig);

            owners = _telegrafsApi.GetOwners(telegrafConfig);
            Assert.AreEqual(1, owners.Count);
        }

        [Test]
        public void UpdateTelegraf()
        {
            var telegrafConfig = _telegrafsApi
                .CreateTelegraf(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            telegrafConfig.Description = "updated";
            telegrafConfig.Agent.CollectionInterval = 500;
            telegrafConfig.Plugins.RemoveAt(0);

            telegrafConfig = _telegrafsApi.UpdateTelegraf(telegrafConfig);

            Assert.AreEqual("updated", telegrafConfig.Description);
            Assert.AreEqual(500, telegrafConfig.Agent.CollectionInterval);
            Assert.AreEqual(1, telegrafConfig.Plugins.Count);
        }
    }
}