using System;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItInfluxDBClientTest : AbstractItClientTest
    {
        [Test]
        public async Task Health()
        {
            var health = await Client.HealthAsync();

            Assert.IsNotNull(health);
            Assert.AreEqual("influxdb", health.Name);
            Assert.AreEqual(HealthCheck.StatusEnum.Pass, health.Status);
            Assert.AreEqual("ready for queries and writes", health.Message);
        }

        [Test]
        public async Task HealthNotRunningInstance()
        {
            var clientNotRunning = InfluxDBClientFactory.Create("http://localhost:8099");
            var health = await clientNotRunning.HealthAsync();

            Assert.IsNotNull(health);
            Assert.AreEqual(HealthCheck.StatusEnum.Fail, health.Status);
            Assert.IsTrue(health.Message.Contains("Connection refused") ||
                          health.Message.Contains("Cannot assign requested address"),
                $"The health message: {health.Message}");

            clientNotRunning.Dispose();
        }

        [Test]
        public async Task Ping()
        {
            Assert.IsTrue(await Client.PingAsync());
        }

        [Test]
        public async Task PingNotRunningInstance()
        {
            var clientNotRunning = InfluxDBClientFactory.Create("http://localhost:8099");

            Assert.IsFalse(await clientNotRunning.PingAsync());

            clientNotRunning.Dispose();
        }

        [Test]
        public async Task Version()
        {
            Assert.IsNotEmpty(await Client.VersionAsync());
        }

        [Test]
        public void VersionNotRunningInstance()
        {
            var clientNotRunning = InfluxDBClientFactory.Create("http://localhost:8099");

            var ex = Assert.ThrowsAsync<InfluxException>(async () => await clientNotRunning.VersionAsync());

            Assert.NotNull(ex);
            Assert.IsNotEmpty(ex.Message);
            Assert.AreEqual(0, ex.Status);

            clientNotRunning.Dispose();
        }

        [Test]
        public async Task IsOnBoardingNotAllowed()
        {
            var onboardingAllowed = await Client.IsOnboardingAllowedAsync();

            Assert.IsFalse(onboardingAllowed);
        }

        [Test]
        public void Logging()
        {
            // Default None
            Assert.AreEqual(LogLevel.None, Client.GetLogLevel());

            // Headers
            Client.SetLogLevel(LogLevel.Headers);
            Assert.AreEqual(LogLevel.Headers, Client.GetLogLevel());
        }

        [Test]
        public async Task Onboarding()
        {
            var url = $"http://{GetOrDefaultEnvironmentVariable("INFLUXDB_2_ONBOARDING_IP", "localhost")}:" +
                      $"{GetOrDefaultEnvironmentVariable("INFLUXDB_2_ONBOARDING_PORT", "9990")}";

            using (var client = InfluxDBClientFactory.Create(url))
            {
                Assert.IsTrue(await client.IsOnboardingAllowedAsync());
            }

            var onboarding = await InfluxDBClientFactory.Onboarding(url, "admin", "11111111", "Testing", "test");

            Assert.IsNotNull(onboarding.User);
            Assert.IsNotEmpty(onboarding.User.Id);
            Assert.AreEqual("admin", onboarding.User.Name);

            Assert.IsNotNull(onboarding.Bucket);
            Assert.IsNotEmpty(onboarding.Bucket.Id);
            Assert.AreEqual("test", onboarding.Bucket.Name);

            Assert.IsNotNull(onboarding.Org);
            Assert.IsNotEmpty(onboarding.Org.Id);
            Assert.AreEqual("Testing", onboarding.Org.Name);

            Assert.IsNotNull(onboarding.Auth);
            Assert.IsNotEmpty(onboarding.Auth.Id);
            Assert.IsNotEmpty(onboarding.Auth.Token);

            using (var client = InfluxDBClientFactory.Create(url, onboarding.Auth.Token))
            {
                var user = await client.GetUsersApi().MeAsync();

                Assert.IsNotNull(user);
                Assert.AreEqual("admin", user.Name);
            }
        }

        [Test]
        public void OnBoardingAlreadyDone()
        {
            var onboarding = new OnboardingRequest("admin", "11111111", "Testing", "test");

            var ex = Assert.ThrowsAsync<UnprocessableEntityException>(async () =>
                await Client.OnboardingAsync(onboarding));

            Assert.AreEqual("onboarding has already been completed", ex.Message);
            Assert.AreEqual(422, ex.Status);
        }

        [Test]
        public void QueryClient()
        {
            var queryClient = Client.GetQueryApi();

            Assert.IsNotNull(queryClient);
        }


        [Test]
        public async Task Ready()
        {
            var ready = await Client.ReadyAsync();

            Assert.IsNotNull(ready);
            Assert.AreEqual(Api.Domain.Ready.StatusEnum.Ready, ready.Status);
            Assert.Greater(DateTime.UtcNow, ready.Started);
            Assert.IsNotEmpty(ready.Up);
        }

        [Test]
        public async Task ReadyNotRunningInstance()
        {
            var clientNotRunning = InfluxDBClientFactory.Create("http://localhost:8099");
            var ready = await clientNotRunning.ReadyAsync();

            Assert.IsNull(ready);

            clientNotRunning.Dispose();
        }

        [Test]
        public async Task UseUsernamePassword()
        {
            Client.Dispose();

            Client = InfluxDBClientFactory.Create(InfluxDbUrl, "my-user", "my-password".ToCharArray());

            var measurement = $"mem_{DateTimeOffset.Now.ToUnixTimeSeconds()}";
            await Client
                .GetWriteApiAsync()
                .WriteRecordAsync($"{measurement},tag=a value=10i", WritePrecision.Ns, "my-bucket", "my-org");

            var query = $@"from(bucket: ""my-bucket"")
                |> range(start: 0)
                |> filter(fn: (r) => r[""_measurement""] == ""{measurement}"")";
            var tables = await Client.GetQueryApi().QueryAsync(query, "my-org");
            Assert.AreEqual(1, tables.Count);
            Assert.AreEqual(1, tables[0].Records.Count);
            Assert.AreEqual(10, tables[0].Records[0].GetValue());

            // delete data
            await Client.GetDeleteApi()
                .Delete(DateTime.UtcNow.AddHours(-1), DateTime.UtcNow, "", "my-bucket", "my-org");

            Client.Dispose();

            // test already disposed
            var ioe = Assert.ThrowsAsync<ObjectDisposedException>(async () =>
                await Client.GetQueryApi().QueryAsync(query, "my-org"));

            Assert.IsNotNull(ioe);
        }
    }
}