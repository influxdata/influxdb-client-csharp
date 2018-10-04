using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flux.Flux;
using Flux.Flux.Client;
using Flux.Flux.Options;
using NUnit.Framework;
using RestEase;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;

namespace Flux.Tests.Flux
{
    public class AbstractFluxClientTest : AbstractTest
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
            FluxClient = FluxClientFactory.Connect(MockServer.Urls[0]);
        }

        [TearDown]
        public void ShutdownServer()
        {
            MockServer.Stop();
        }
    }
}