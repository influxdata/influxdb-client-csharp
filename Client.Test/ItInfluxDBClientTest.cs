using System;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItInfluxDBClientTest : AbstractItClientTest
    {
        [Test]
        public async Task Health()
        {
            var health = await Client.Health();

            Assert.IsNotNull(health);
            Assert.AreEqual("influxdb", health.Name);
            Assert.AreEqual(HealthCheck.StatusEnum.Pass, health.Status);
            Assert.AreEqual("ready for queries and writes", health.Message);
        }

        [Test]
        public async Task HealthNotRunningInstance()
        {
            var clientNotRunning = InfluxDBClientFactory.Create("http://localhost:8099");
            var health = await clientNotRunning.Health();

            Assert.IsNotNull(health);
            Assert.AreEqual(HealthCheck.StatusEnum.Fail, health.Status);
            Assert.IsTrue(health.Message.Contains("Connection refused") || 
                          health.Message.Contains("Cannot assign requested address"), 
                $"The health message: {health.Message}");

            clientNotRunning.Dispose();
        }

        [Test]
        public async Task IsOnBoardingNotAllowed()
        {
            var onboardingAllowed = await Client.IsOnboardingAllowed();

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
            var url = $"http://{GetOrDefaultEnvironmentVariable("INFLUXDB_2_ONBOARDING_IP", "127.0.0.1")}:" +
                      $"{GetOrDefaultEnvironmentVariable("INFLUXDB_2_ONBOARDING_PORT", "9990")}";

            using (var client = InfluxDBClientFactory.Create(url))
            {
                Assert.IsTrue(await client.IsOnboardingAllowed());
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

            using (var client = InfluxDBClientFactory.Create(url, onboarding.Auth.Token.ToCharArray()))
            {
                var user = await client.GetUsersApi().Me();

                Assert.IsNotNull(user);
                Assert.AreEqual("admin", user.Name);
            }
        }

        [Test]
        public void OnBoardingAlreadyDone()
        {
            var onboarding = new OnboardingRequest("admin", "11111111", "Testing", "test");

            var ex = Assert.ThrowsAsync<HttpException>(async () => await Client.Onboarding(onboarding));

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
            var ready = await Client.Ready();

            Assert.IsNotNull(ready);
            Assert.AreEqual(Api.Domain.Ready.StatusEnum.Ready, ready.Status);
            Assert.Greater(DateTime.UtcNow, ready.Started);
            Assert.IsNotEmpty(ready.Up);
        }

        [Test]
        public async Task ReadyNotRunningInstance()
        {
            var clientNotRunning = InfluxDBClientFactory.Create("http://localhost:8099");
            var ready = await clientNotRunning.Ready();

            Assert.IsNull(ready);

            clientNotRunning.Dispose();
        }
    }
}