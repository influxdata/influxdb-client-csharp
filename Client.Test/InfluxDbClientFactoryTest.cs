using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class InfluxDbClientFactoryTest
    {
        [SetUp]
        public void SetUp()
        {
        }
        
        [Test]
        public void CreateInstance() 
        {
            var client = InfluxDBClientFactory.Create("http://localhost:9999");

            Assert.IsNotNull(client);
        }

        [Test]
        public void CreateInstanceUsername() {

            var client = InfluxDBClientFactory.Create("http://localhost:9999", "user", "secret".ToCharArray());

            Assert.IsNotNull(client);
        }

        [Test]
        public void CreateInstanceToken() {

            var client = InfluxDBClientFactory.Create("http://localhost:9999", "xyz".ToCharArray());

            Assert.IsNotNull(client);
        }
    }
}