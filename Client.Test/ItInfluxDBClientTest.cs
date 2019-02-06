using System;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Domain;
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
            Assert.IsTrue(health.IsHealthy());
            Assert.AreEqual("ready for queries and writes", health.Message);
        }

        [Test]
        public async Task HealthNotRunningInstance()
        {
            var clientNotRunning = InfluxDBClientFactory.Create("http://localhost:8099");
            var health = await clientNotRunning.Health();

            Assert.IsNotNull(health);
            Assert.IsFalse(health.IsHealthy());
            Assert.AreEqual("Connection refused", health.Message);

            clientNotRunning.Dispose();
        }


        [Test]
        public async Task Ready()
        {
            var ready = await Client.Ready();

            Assert.IsNotNull(ready);
            Assert.AreEqual("ready", ready.Status);
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
        public void QueryClient()
        {
            var queryClient = Client.GetQueryApi();
            
            Assert.IsNotNull(queryClient);
        }

        [Test]
        public async Task IsOnBoardingNotAllowed()
        {
            var onboardingAllowed = await Client.IsOnboardingAllowed();
            
            Assert.IsFalse(onboardingAllowed);
        }

        [Test]
        public void OnBoardingAlreadyDone()
        {
            var onboarding = new Onboarding{Username = "admin", Password = "111111", Org = "Testing", Bucket = "test"};
            
            var ex = Assert.ThrowsAsync<HttpException>(async () => await Client.Onboarding(onboarding));
            
            Assert.AreEqual("onboarding has already been completed", ex.Message);
            Assert.AreEqual(422, ex.Status);
        }

        [Test]
        public async Task Onboarding()
        {
            var url = "http://" + GetInfluxDb2Ip() + ":9990";

            using (var client = InfluxDBClientFactory.Create(url))
            {
                Assert.IsTrue(await client.IsOnboardingAllowed());
            }

            var onboarding = await InfluxDBClientFactory.Onboarding(url, "admin", "111111", "Testing", "test");
            
            Assert.IsNotNull(onboarding.User);
            Assert.IsNotEmpty(onboarding.User.Id);
            Assert.AreEqual("admin", onboarding.User.Name);
            
            Assert.IsNotNull(onboarding.Bucket);
            Assert.IsNotEmpty(onboarding.Bucket.Id);
            Assert.AreEqual("test", onboarding.Bucket.Name);
            
            Assert.IsNotNull(onboarding.Organization);
            Assert.IsNotEmpty(onboarding.Organization.Id);
            Assert.AreEqual("Testing", onboarding.Organization.Name);
            
            Assert.IsNotNull(onboarding.Authorization);
            Assert.IsNotEmpty(onboarding.Authorization.Id);
            Assert.IsNotEmpty(onboarding.Authorization.Token);

            using (var client = InfluxDBClientFactory.Create(url, onboarding.Authorization.Token.ToCharArray()))
            {
                var user = await client.GetUsersApi().Me();
                
                Assert.IsNotNull(user);
                Assert.AreEqual("admin", user.Name);
            }
        }
    }
}