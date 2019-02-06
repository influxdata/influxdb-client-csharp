using InfluxDB.Client.Flux;
using NUnit.Framework;

namespace Client.Legacy.Test
{
    [TestFixture]
    public class FluxClientFactoryTest
    {
        [SetUp]
        public void SetUp()
        {
        }
        
        [Test]
        public void Connect()
        {
            var fluxClient = FluxClientFactory.Create("http://localhost:8093");

            Assert.IsNotNull((fluxClient));
        }
    }
}