using System;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Generated.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItInfluxDBClientTest : AbstractItClientTest
    {
        [Test]
        public void Health()
        {
            var health = Client.Health();

            Assert.IsNotNull(health);
            Assert.AreEqual("influxdb", health.Name);
            Assert.AreEqual(Check.StatusEnum.Pass, health.Status);
            Assert.AreEqual("ready for queries and writes", health.Message);
        }

        [Test]
        public void HealthNotRunningInstance()
        {
            var clientNotRunning = InfluxDBClientFactory.Create("http://localhost:8099");
            var health = clientNotRunning.Health();

            Assert.IsNotNull(health);
            Assert.AreEqual(Check.StatusEnum.Fail, health.Status);
            Assert.IsTrue(health.Message.StartsWith("Connection refused"));

            clientNotRunning.Dispose();
        }

        [Test]
        public void IsOnBoardingNotAllowed()
        {
            var onboardingAllowed = Client.IsOnboardingAllowed();

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
        public void Onboarding()
        {
            var url = "http://" + GetInfluxDb2Ip() + ":9990";

            using (var client = InfluxDBClientFactory.Create(url))
            {
                Assert.IsTrue(client.IsOnboardingAllowed());
            }

            var onboarding = InfluxDBClientFactory.Onboarding(url, "admin", "11111111", "Testing", "test");

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
                var user = client.GetUsersApi().Me();

                Assert.IsNotNull(user);
                Assert.AreEqual("admin", user.Name);
            }
        }

        [Test]
        public void OnBoardingAlreadyDone()
        {
            var onboarding = new OnboardingRequest("admin", "11111111", "Testing", "test");

            var ex = Assert.Throws<HttpException>(() => Client.Onboarding(onboarding));

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
        public void Ready()
        {
            var ready = Client.Ready();

            Assert.IsNotNull(ready);
            Assert.AreEqual(Generated.Domain.Ready.StatusEnum.Ready, ready.Status);
            Assert.Greater(DateTime.UtcNow, ready.Started);
            Assert.IsNotEmpty(ready.Up);
        }

        [Test]
        public void ReadyNotRunningInstance()
        {
            var clientNotRunning = InfluxDBClientFactory.Create("http://localhost:8099");
            var ready = clientNotRunning.Ready();

            Assert.IsNull(ready);

            clientNotRunning.Dispose();
        }
    }
}