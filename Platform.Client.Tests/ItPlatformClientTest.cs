using System;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Client;
using NUnit.Framework;

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
            Assert.AreEqual("howdy y'all", health.Message);
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
        public void QueryClient()
        {
            var queryClient = PlatformClient.CreateQueryClient();
            
            Assert.IsNotNull(queryClient);
        }
    }
}