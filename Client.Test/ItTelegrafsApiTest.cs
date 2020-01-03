using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;
using Tomlyn;

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

            foreach (var telegrafConfig in await _telegrafsApi.FindTelegrafsAsync())
                await _telegrafsApi.DeleteTelegrafAsync(telegrafConfig);
        }

        private TelegrafsApi _telegrafsApi;
        private UsersApi _usersApi;
        private Organization _organization;

        private static TelegrafPlugin NewCpuPlugin()
        {
            var config = new Dictionary<string, object>
            {
                {"percpu", true},
                {"totalcpu", true},
                {"collect_cpu_time", false},
                {"report_active", false},
                {"avoid_null", null},
            };
            return new TelegrafPlugin(TelegrafPlugin.TypeEnum.Inputs, "cpu", config: config);
        }

        private static TelegrafPlugin NewOutputPlugin()
        {
            var config = new Dictionary<string, object>
            {
                {"organization", "my-org"},
                {"bucket", "my-bucket"},
                {"token", "$INFLUX_TOKEN"},
                {"urls", new List<string> {"http://127.0.0.1:9999"}}
            };

            return new TelegrafPlugin(TelegrafPlugin.TypeEnum.Outputs, "influxdb_v2", "my instance",
                config);
        }

        [Test]
        public async Task CloneTelegraf()
        {
            var source = await _telegrafsApi
                .CreateTelegrafAsync(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = await Client.GetLabelsApi()
                .CreateLabelAsync(GenerateName("Cool Resource"), properties, _organization.Id);
            await _telegrafsApi.AddLabelAsync(label, source);

            var name = GenerateName("cloned");

            var cloned = await _telegrafsApi.CloneTelegrafAsync(name, source);

            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual(_organization.Id, cloned.OrgID);
            Assert.AreEqual("test-config", cloned.Description);
            Assert.AreEqual(1_000, cloned.Agent.CollectionInterval);
            Assert.AreEqual(2, cloned.Plugins.Count);
            Assert.AreEqual(TelegrafPluginInputCpu.NameEnum.Cpu, ((TelegrafPluginInputCpu) cloned.Plugins[0]).Name);
            Assert.AreEqual(TelegrafPluginOutputInfluxDBV2.NameEnum.Influxdbv2,
                ((TelegrafPluginOutputInfluxDBV2) cloned.Plugins[1]).Name);

            var labels = await _telegrafsApi.GetLabelsAsync(cloned);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
        }

        [Test]
        public void CloneTelegrafNotFound()
        {
            var ioe = Assert.ThrowsAsync<AggregateException>(async () =>
                await _telegrafsApi.CloneTelegrafAsync(GenerateName("tc"), "020f755c3c082000"));

            Assert.AreEqual(typeof(HttpException), ioe.InnerException.InnerException.GetType());
            Assert.AreEqual("telegraf configuration not found", ioe.InnerException.InnerException.Message);
        }

        [Test]
        public async Task CreateTelegraf()
        {
            var name = GenerateName("Telegraf");

            var output = NewOutputPlugin();
            var cpu = NewCpuPlugin();

            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafAsync(name, "test-config", _organization,
                    new List<TelegrafPlugin> {output, cpu});

            Assert.IsNotNull(telegrafConfig);
            Assert.AreEqual(name, telegrafConfig.Name);
            Assert.AreEqual("test-config", telegrafConfig.Description);
            Assert.AreEqual(_organization.Id, telegrafConfig.OrgID);
            
            Assert.IsNotNull(telegrafConfig.Metadata);
            Assert.IsNotNull(telegrafConfig.Metadata.Buckets);
            Assert.AreEqual(1, telegrafConfig.Metadata.Buckets.Count);

            var toml = Toml.Parse(telegrafConfig.Config).ToModel();
            Toml agent = toml.getTable("agent");
        }

        [Test]
        public async Task DeleteTelegraf()
        {
            var createdConfig = await _telegrafsApi
                .CreateTelegrafAsync(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});
            Assert.IsNotNull(createdConfig);

            var foundTelegraf = await _telegrafsApi.FindTelegrafByIdAsync(createdConfig.Id);
            Assert.IsNotNull(foundTelegraf);

            // delete source
            await _telegrafsApi.DeleteTelegrafAsync(createdConfig);

            var ioe = Assert.ThrowsAsync<AggregateException>(async () =>
                await _telegrafsApi.FindTelegrafByIdAsync(createdConfig.Id));

            Assert.AreEqual("telegraf configuration not found", ioe.InnerException.Message);
            Assert.AreEqual(typeof(HttpException), ioe.InnerException.GetType());
        }

        [Test]
        public void DeleteTelegrafNotFound()
        {
            var ioe = Assert.ThrowsAsync<HttpException>(async () =>
                await _telegrafsApi.DeleteTelegrafAsync("020f755c3d082000"));

            Assert.AreEqual("telegraf configuration not found", ioe.Message);
            Assert.AreEqual(404, ioe.Status);
        }

        [Test]
        public async Task FindTelegrafById()
        {
            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafAsync(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var telegrafConfigById = await _telegrafsApi.FindTelegrafByIdAsync(telegrafConfig.Id);

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
            var ioe = Assert.ThrowsAsync<AggregateException>(async () =>
                await _telegrafsApi.FindTelegrafByIdAsync("020f755c3d082000"));

            Assert.AreEqual("telegraf configuration not found", ioe.InnerException.Message);
            Assert.AreEqual(typeof(HttpException), ioe.InnerException.GetType());
        }

        [Test]
        public async Task FindTelegrafByOrg()
        {
            var orgName = GenerateName("Constant Pro");

            var organization = await Client.GetOrganizationsApi().CreateOrganizationAsync(orgName);
            var telegrafConfigs = await _telegrafsApi.FindTelegrafsByOrgAsync(organization);

            Assert.AreEqual(0, telegrafConfigs.Count);

            await _telegrafsApi
                .CreateTelegrafAsync(GenerateName("tc"), "test-config", organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            telegrafConfigs = await _telegrafsApi.FindTelegrafsByOrgAsync(organization);

            Assert.AreEqual(1, telegrafConfigs.Count);

            await _telegrafsApi.DeleteTelegrafAsync(telegrafConfigs[0]);
        }

        [Test]
        public async Task FindTelegrafs()
        {
            var size = (await _telegrafsApi.FindTelegrafsAsync()).Count;

            await _telegrafsApi
                .CreateTelegrafAsync(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var telegrafConfigs = await _telegrafsApi.FindTelegrafsAsync();
            Assert.AreEqual(size + 1, telegrafConfigs.Count);
        }

        [Test]
        public async Task GetToml()
        {
            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafAsync(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var toml = await _telegrafsApi.GetTOMLAsync(telegrafConfig);

            Assert.True(toml.Contains("[[inputs.cpu]]"));
            Assert.True(toml.Contains("[[outputs.influxdb_v2]]"));
            Assert.True(toml.Contains("organization = \"my-org\""));
            Assert.True(toml.Contains("bucket = \"my-bucket\""));
        }

        [Test]
        public void GetTomlNotFound()
        {
            var ioe = Assert.ThrowsAsync<HttpException>(async () =>
                await _telegrafsApi.GetTOMLAsync("020f755c3d082000"));

            Assert.AreEqual("telegraf configuration not found", ioe.Message);
            Assert.AreEqual(404, ioe.Status);
        }

        [Test]
        public async Task Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafAsync(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var properties = new Dictionary<string, string> {{"color", "green"}, {"location", "west"}};

            var label = await labelClient.CreateLabelAsync(GenerateName("Cool Resource"), properties, _organization.Id);

            var labels = await _telegrafsApi.GetLabelsAsync(telegrafConfig);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = await _telegrafsApi.AddLabelAsync(label, telegrafConfig);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = await _telegrafsApi.GetLabelsAsync(telegrafConfig);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            await _telegrafsApi.DeleteLabelAsync(label, telegrafConfig);

            labels = await _telegrafsApi.GetLabelsAsync(telegrafConfig);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        public async Task Member()
        {
            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafAsync(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var members = await _telegrafsApi.GetMembersAsync(telegrafConfig);
            Assert.AreEqual(0, members.Count);

            var user = await _usersApi.CreateUserAsync(GenerateName("Luke Health"));

            var resourceMember = await _telegrafsApi.AddMemberAsync(user, telegrafConfig);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceMember.RoleEnum.Member);

            members = await _telegrafsApi.GetMembersAsync(telegrafConfig);
            Assert.AreEqual(1, members.Count);
            Assert.AreEqual(members[0].Id, user.Id);
            Assert.AreEqual(members[0].Name, user.Name);
            Assert.AreEqual(members[0].Role, ResourceMember.RoleEnum.Member);

            await _telegrafsApi.DeleteMemberAsync(user, telegrafConfig);

            members = await _telegrafsApi.GetMembersAsync(telegrafConfig);
            Assert.AreEqual(0, members.Count);
        }

        [Test]
        public async Task Owner()
        {
            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafAsync(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            var owners = await _telegrafsApi.GetOwnersAsync(telegrafConfig);
            Assert.AreEqual(1, owners.Count);
            Assert.AreEqual("my-user", owners[0].Name);

            var user = await _usersApi.CreateUserAsync(GenerateName("Luke Health"));

            var resourceMember = await _telegrafsApi.AddOwnerAsync(user, telegrafConfig);
            Assert.IsNotNull(resourceMember);
            Assert.AreEqual(resourceMember.Id, user.Id);
            Assert.AreEqual(resourceMember.Name, user.Name);
            Assert.AreEqual(resourceMember.Role, ResourceOwner.RoleEnum.Owner);

            owners = await _telegrafsApi.GetOwnersAsync(telegrafConfig);
            Assert.AreEqual(2, owners.Count);
            Assert.AreEqual(owners[1].Id, user.Id);
            Assert.AreEqual(owners[1].Name, user.Name);
            Assert.AreEqual(owners[1].Role, ResourceOwner.RoleEnum.Owner);

            await _telegrafsApi.DeleteOwnerAsync(user, telegrafConfig);

            owners = await _telegrafsApi.GetOwnersAsync(telegrafConfig);
            Assert.AreEqual(1, owners.Count);
        }

        [Test]
        public async Task UpdateTelegraf()
        {
            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafAsync(GenerateName("tc"), "test-config", _organization, 1_000,
                    new List<TelegrafRequestPlugin> {NewCpuPlugin(), NewOutputPlugin()});

            telegrafConfig.Description = "updated";
            telegrafConfig.Agent.CollectionInterval = 500;
            telegrafConfig.Plugins.RemoveAt(0);

            telegrafConfig = await _telegrafsApi.UpdateTelegrafAsync(telegrafConfig);

            Assert.AreEqual("updated", telegrafConfig.Description);
            Assert.AreEqual(500, telegrafConfig.Agent.CollectionInterval);
            Assert.AreEqual(1, telegrafConfig.Plugins.Count);
        }
    }
}