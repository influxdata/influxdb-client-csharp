using InfluxData.Platform.Client.Client;
using NUnit.Framework;

namespace Platform.Client.Tests
{
    [TestFixture]
    public class PlatformClientFactoryTest
    {
        [SetUp]
        public void SetUp()
        {
        }
        
        [Test]
        public void CreateInstance() 
        {
            var client = PlatformClientFactory.Create("http://localhost:9999");

            Assert.IsNotNull(client);
        }

        [Test]
        public void CreateInstanceUsername() {

            var client = PlatformClientFactory.Create("http://localhost:9999", "user", "secret".ToCharArray());

            Assert.IsNotNull(client);
        }

        [Test]
        public void CreateInstanceToken() {

            var client = PlatformClientFactory.Create("http://localhost:9999", "xyz".ToCharArray());

            Assert.IsNotNull(client);
        }
    }
}