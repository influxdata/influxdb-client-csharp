using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;
using Tomlyn;
using Tomlyn.Model;

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
                { "percpu", true },
                { "totalcpu", true },
                { "collect_cpu_time", false },
                { "report_active", false },
                { "avoid_null", null }
            };
            return new TelegrafPlugin(TelegrafPlugin.TypeEnum.Input, "cpu", config: config);
        }

        private static TelegrafPlugin NewOutputPlugin()
        {
            var config = new Dictionary<string, object>
            {
                { "organization", "my-org" },
                { "bucket", "my-bucket" },
                { "token", "$INFLUX_TOKEN" },
                { "urls", new List<string> { "http://localhost:9999" } }
            };

            return new TelegrafPlugin(TelegrafPlugin.TypeEnum.Output, "influxdb_v2", "my instance",
                config);
        }

        [Test]
        public async Task CloneTelegraf()
        {
            var source = await _telegrafsApi.CreateTelegrafAsync(GenerateName("Telegraf"), "test-config", _organization,
                new List<TelegrafPlugin> { NewOutputPlugin(), NewCpuPlugin() });

            var properties = new Dictionary<string, string> { { "color", "green" }, { "location", "west" } };

            var label = await Client.GetLabelsApi()
                .CreateLabelAsync(GenerateName("Cool Resource"), properties, _organization.Id);
            await _telegrafsApi.AddLabelAsync(label, source);

            var name = GenerateName("cloned");

            var cloned = await _telegrafsApi.CloneTelegrafAsync(name, source);

            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual(_organization.Id, cloned.OrgID);
            Assert.AreEqual("test-config", cloned.Description);
            Assert.AreEqual(source.Config, cloned.Config);
            Assert.AreEqual(source.Metadata.Buckets.Count, cloned.Metadata.Buckets.Count);
            Assert.AreEqual(source.Metadata.Buckets[0], cloned.Metadata.Buckets[0]);
        }

        [Test]
        public void CloneTelegrafNotFound()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _telegrafsApi.CloneTelegrafAsync(GenerateName("tc"), "020f755c3c082000"));

            Assert.AreEqual(typeof(NotFoundException), ioe.GetType());
            Assert.AreEqual("telegraf configuration not found", ioe.Message);
        }

        [Test]
        public async Task CreateTelegraf()
        {
            var name = GenerateName("Telegraf");

            var output = NewOutputPlugin();
            var cpu = NewCpuPlugin();

            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafAsync(name, "test-config", _organization,
                    new List<TelegrafPlugin> { output, cpu });

            Assert.IsNotNull(telegrafConfig);
            Assert.AreEqual(name, telegrafConfig.Name);
            Assert.AreEqual("test-config", telegrafConfig.Description);
            Assert.AreEqual(_organization.Id, telegrafConfig.OrgID);

            Assert.IsNotNull(telegrafConfig.Metadata);
            Assert.IsNotNull(telegrafConfig.Metadata.Buckets);
            Assert.AreEqual(1, telegrafConfig.Metadata.Buckets.Count);

            var toml = Toml.Parse(telegrafConfig.Config).ToModel();
            var agent = (TomlTable)toml["agent"];
            Assert.IsNotNull(agent);
            Assert.AreEqual("10s", agent["interval"]);
            Assert.IsTrue((bool)agent["round_interval"]);
            Assert.AreEqual(1000, agent["metric_batch_size"]);
            Assert.AreEqual(10000, agent["metric_buffer_limit"]);
            Assert.AreEqual("0s", agent["collection_jitter"]);
            Assert.AreEqual("0s", agent["flush_jitter"]);
            Assert.AreEqual("", agent["precision"]);
            Assert.IsFalse((bool)agent["omit_hostname"]);

            var tomlInflux = (TomlTableArray)((TomlTable)toml["outputs"])["influxdb_v2"];
            Assert.AreEqual(1, tomlInflux.Count);
            Assert.AreEqual("my-bucket", tomlInflux[0]["bucket"]);
            Assert.AreEqual("my-org", tomlInflux[0]["organization"]);
            Assert.AreEqual("$INFLUX_TOKEN", tomlInflux[0]["token"]);
            Assert.AreEqual(1, ((TomlArray)tomlInflux[0]["urls"]).Count);
            Assert.AreEqual("http://localhost:9999", ((TomlArray)tomlInflux[0]["urls"])[0]);

            var tomlCpu = (TomlTableArray)((TomlTable)toml["inputs"])["cpu"];
            Assert.AreEqual(1, tomlCpu.Count);
            Assert.IsTrue((bool)tomlCpu[0]["totalcpu"]);
            Assert.IsFalse((bool)tomlCpu[0]["collect_cpu_time"]);
            Assert.IsFalse((bool)tomlCpu[0]["report_active"]);
            Assert.IsTrue((bool)tomlCpu[0]["percpu"]);
        }

        [Test]
        public async Task DeleteTelegraf()
        {
            var createdConfig = await _telegrafsApi
                .CreateTelegrafAsync(GenerateName("Telegraf"), "test-config", _organization,
                    new List<TelegrafPlugin> { NewOutputPlugin(), NewCpuPlugin() });
            Assert.IsNotNull(createdConfig);

            var foundTelegraf = await _telegrafsApi.FindTelegrafByIdAsync(createdConfig.Id);
            Assert.IsNotNull(foundTelegraf);

            // delete source
            await _telegrafsApi.DeleteTelegrafAsync(createdConfig);

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _telegrafsApi.FindTelegrafByIdAsync(createdConfig.Id));

            Assert.AreEqual("telegraf configuration not found", ioe.Message);
            Assert.AreEqual(typeof(NotFoundException), ioe.GetType());
        }

        [Test]
        public void DeleteTelegrafNotFound()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _telegrafsApi.DeleteTelegrafAsync("020f755c3d082000"));

            Assert.AreEqual("telegraf configuration not found", ioe.Message);
            Assert.AreEqual(404, ioe.Status);
        }

        [Test]
        public async Task FindTelegrafById()
        {
            var name = GenerateName("Telegraf");
            var telegrafConfig = await _telegrafsApi
                .CreateTelegrafAsync(name, "test-config", _organization,
                    new List<TelegrafPlugin> { NewOutputPlugin(), NewCpuPlugin() });

            var telegrafConfigById = await _telegrafsApi.FindTelegrafByIdAsync(telegrafConfig.Id);

            Assert.IsNotNull(telegrafConfigById);
            Assert.AreEqual(name, telegrafConfigById.Name);
            Assert.AreEqual("test-config", telegrafConfigById.Description);
            Assert.AreEqual(_organization.Id, telegrafConfigById.OrgID);

            Assert.IsNotNull(telegrafConfigById.Metadata);
            Assert.IsNotNull(telegrafConfigById.Metadata.Buckets);
            Assert.AreEqual(1, telegrafConfigById.Metadata.Buckets.Count);
            Assert.AreEqual(telegrafConfig.Config, telegrafConfigById.Config);
        }

        [Test]
        public void FindTelegrafByIdNull()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _telegrafsApi.FindTelegrafByIdAsync("020f755c3d082000"));

            Assert.AreEqual("telegraf configuration not found", ioe.Message);
            Assert.AreEqual(typeof(NotFoundException), ioe.GetType());
        }

        [Test]
        public async Task FindTelegrafByOrg()
        {
            var orgName = GenerateName("Constant Pro");

            var organization = await Client.GetOrganizationsApi().CreateOrganizationAsync(orgName);
            var telegrafConfigs = await _telegrafsApi.FindTelegrafsByOrgAsync(organization);

            Assert.AreEqual(0, telegrafConfigs.Count);

            await _telegrafsApi
                .CreateTelegrafAsync(GenerateName("Telegraf"), "test-config", organization,
                    new List<TelegrafPlugin> { NewOutputPlugin(), NewCpuPlugin() });

            telegrafConfigs = await _telegrafsApi.FindTelegrafsByOrgAsync(organization);

            Assert.AreEqual(1, telegrafConfigs.Count);

            await _telegrafsApi.DeleteTelegrafAsync(telegrafConfigs[0]);
        }

        [Test]
        public async Task FindTelegrafs()
        {
            var size = (await _telegrafsApi.FindTelegrafsAsync()).Count;

            await _telegrafsApi.CreateTelegrafAsync(GenerateName("Telegraf"), "test-config", _organization,
                new List<TelegrafPlugin> { NewOutputPlugin(), NewCpuPlugin() });

            var telegrafConfigs = await _telegrafsApi.FindTelegrafsAsync();
            Assert.AreEqual(size + 1, telegrafConfigs.Count);
        }

        [Test]
        public async Task GetToml()
        {
            var telegrafConfig = await _telegrafsApi.CreateTelegrafAsync(GenerateName("Telegraf"), "test-config",
                _organization, new List<TelegrafPlugin> { NewOutputPlugin(), NewCpuPlugin() });

            var toml = await _telegrafsApi.GetTOMLAsync(telegrafConfig);

            Assert.True(toml.Contains("[[inputs.cpu]]"));
            Assert.True(toml.Contains("[[outputs.influxdb_v2]]"));
            Assert.True(toml.Contains("organization = \"my-org\""));
            Assert.True(toml.Contains("bucket = \"my-bucket\""));
        }

        [Test]
        public void GetTomlNotFound()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () =>
                await _telegrafsApi.GetTOMLAsync("020f755c3d082000"));

            Assert.AreEqual("telegraf configuration not found", ioe.Message);
            Assert.AreEqual(404, ioe.Status);
        }

        [Test]
        public async Task Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var telegrafConfig = await _telegrafsApi.CreateTelegrafAsync(GenerateName("Telegraf"), "test-config",
                _organization, new List<TelegrafPlugin> { NewOutputPlugin(), NewCpuPlugin() });

            var properties = new Dictionary<string, string> { { "color", "green" }, { "location", "west" } };

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
            var telegrafConfig = await _telegrafsApi.CreateTelegrafAsync(GenerateName("Telegraf"), "test-config",
                _organization, new List<TelegrafPlugin> { NewOutputPlugin(), NewCpuPlugin() });

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
        [Ignore("https://github.com/influxdata/influxdb/issues/20005")]
        public async Task Owner()
        {
            var telegrafConfig = await _telegrafsApi.CreateTelegrafAsync(GenerateName("Telegraf"), "test-config",
                _organization, new List<TelegrafPlugin> { NewOutputPlugin(), NewCpuPlugin() });

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
            var telegrafConfig = await _telegrafsApi.CreateTelegrafAsync(GenerateName("Telegraf"), "test-config",
                _organization, new List<TelegrafPlugin> { NewOutputPlugin(), NewCpuPlugin() });

            telegrafConfig.Description = "updated";
            telegrafConfig.Config = "my-updated-config";

            telegrafConfig = await _telegrafsApi.UpdateTelegrafAsync(telegrafConfig);

            Assert.AreEqual("updated", telegrafConfig.Description);
            Assert.AreEqual("my-updated-config", telegrafConfig.Config);
        }
    }
}