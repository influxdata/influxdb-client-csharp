using System;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NUnit.Framework;
using Platform.Common.Flux.Error;
using Platform.Common.Platform.Rest;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    [TestFixture]
    public class ItPlatformClientTest : AbstractItClientTest
    {
        [Test]
        public async Task Health()
        {
            var health = await PlatformClient.Health();

            Assert.IsNotNull(health);
            Assert.IsTrue(health.IsHealthy());
            Assert.AreEqual("ready for queries and writes", health.Message);
        }

        [Test]
        public async Task HealthNotRunningInstance()
        {
            var clientNotRunning = PlatformClientFactory.Create("http://localhost:8099");
            var health = await clientNotRunning.Health();

            Assert.IsNotNull(health);
            Assert.IsFalse(health.IsHealthy());
            Assert.AreEqual("Connection refused", health.Message);

            clientNotRunning.Dispose();
        }


        [Test]
        public async Task Ready()
        {
            var ready = await PlatformClient.Ready();

            Assert.IsNotNull(ready);
            Assert.AreEqual("ready", ready.Status);
            Assert.Greater(DateTime.UtcNow, ready.Started);
            Assert.IsNotEmpty(ready.Up);
        }

        [Test]
        public async Task ReadyNotRunningInstance()
        {
            var clientNotRunning = PlatformClientFactory.Create("http://localhost:8099");
            var ready = await clientNotRunning.Ready();

            Assert.IsNull(ready);
            
            clientNotRunning.Dispose();
        }

        [Test]
        public void Logging()
        {
            // Default None
            Assert.AreEqual(LogLevel.None, PlatformClient.GetLogLevel());
            
            // Headers
            PlatformClient.SetLogLevel(LogLevel.Headers);
            Assert.AreEqual(LogLevel.Headers, PlatformClient.GetLogLevel());
        }

        [Test]
        public void QueryClient()
        {
            var queryClient = PlatformClient.CreateQueryClient();
            
            Assert.IsNotNull(queryClient);
        }

        [Test]
        public async Task IsOnBoardingNotAllowed()
        {
            var onboardingAllowed = await PlatformClient.IsOnboardingAllowed();
            
            Assert.IsFalse(onboardingAllowed);
        }

        [Test]
        public void OnBoardingAlreadyDone()
        {
            var onboarding = new Onboarding{Username = "admin", Password = "111111", Org = "Testing", Bucket = "test"};
            
            var ex = Assert.ThrowsAsync<HttpException>(async () => await PlatformClient.Onboarding(onboarding));
            
            Assert.AreEqual("onboarding has already been completed", ex.Message);
            Assert.AreEqual(422, ex.Status);
        }

        [Test]
        public async Task Onboarding()
        {
            var url = "http://" + GetPlatformIp() + ":9990";

            using (var platformClient = PlatformClientFactory.Create(url))
            {
                Assert.IsTrue(await platformClient.IsOnboardingAllowed());
            }

            var onboarding = await PlatformClientFactory.Onboarding(url, "admin", "111111", "Testing", "test");
            
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

            using (var platformClient = PlatformClientFactory.Create(url, onboarding.Authorization.Token.ToCharArray()))
            {
                var user = await platformClient.CreateUserClient().Me();
                
                Assert.IsNotNull(user);
                Assert.AreEqual("admin", user.Name);
            }
        }
    }
}