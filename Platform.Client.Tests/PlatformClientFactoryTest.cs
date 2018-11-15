using NUnit.Framework;
using Platform.Client.Impl;

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
            PlatformClient client = PlatformClientFactory.Create("http://localhost:9999");

            Assert.IsNotNull(client);
        }

        [Test]
        public void CreateInstanceUsername() {

            PlatformClient client = PlatformClientFactory.Create("http://localhost:9999", "user", "secret".ToCharArray());

            Assert.IsNotNull(client);
        }

        [Test]
        public void CreateInstanceToken() {

            PlatformClient client = PlatformClientFactory.Create("http://localhost:9999", "xyz".ToCharArray());

            Assert.IsNotNull(client);
        }
    }
}