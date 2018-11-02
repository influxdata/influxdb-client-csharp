using System.Threading.Tasks;
using Flux.Client;
using NUnit.Framework;
using WireMock.Server;

namespace Flux.Tests.Flux
{
    public class AbstractMockServerFluxClientTest : AbstractMockServerTest
    {
        protected FluxClient FluxClient;
        protected FluentMockServer MockServer;

        [OneTimeSetUp]
        public void SetUp()
        {
            MockServer = FluentMockServer.Start();
            
            SetUpAsync().Wait();            
        }
        
        async Task SetUpAsync()
        {
            FluxClient = FluxClientFactory.Create(MockServer.Urls[0]);
        }

        [TearDown]
        public void ShutdownServer()
        {
            MockServer.Stop();
        }
    }
}