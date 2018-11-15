using NUnit.Framework;

namespace Flux.Client.Tests
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
            FluxClient fluxClient = FluxClientFactory.Create("http://localhost:8093");

            Assert.IsNotNull((fluxClient));
        }
    }
}