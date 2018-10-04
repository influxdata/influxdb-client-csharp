using Flux.Flux;
using NUnit.Framework;

namespace Flux.Tests.Flux
{
    [TestFixture]
    public class FluxClientFactoryTest
    {
        [OneTimeSetUp]
        public void SetUp()
        {
        }
        
        [Test]
        public void Connect()
        {
            FluxClient fluxClient = FluxClientFactory.Connect("http://localhost:8093");

            Assert.IsNotNull((fluxClient));
        }
    }
}