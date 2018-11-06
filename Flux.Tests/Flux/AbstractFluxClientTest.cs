using Flux.Client;
using NUnit.Framework;
using WireMock.Server;

namespace Flux.Tests.Flux
{
    public class AbstractMockServerFluxClientTest : AbstractMockServerTest
    {
        protected FluxClient FluxClient;
        protected FluentMockServer MockServer;

        [SetUp]
        public void SetUp()
        {
            MockServer = FluentMockServer.Start();
            
            FluxClient = FluxClientFactory.Create(MockServer.Urls[0]);           
        }

        [TearDown]
        public void ShutdownServer()
        {
            MockServer.Stop();
        }
    }
}