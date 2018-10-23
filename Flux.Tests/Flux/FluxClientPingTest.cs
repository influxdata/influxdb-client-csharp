using System.Threading.Tasks;
using NUnit.Framework;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Flux.Tests.Flux
{
    public class FluxClientPingTest : AbstractFluxClientTest
    {
        [Test]
        public async Task Healthy()
        {
            MockServer.Given(Request.Create().WithPath("/ping").UsingGet())
                            .RespondWith(Response.Create().WithStatusCode(204));
            
            Assert.IsTrue(await FluxClient.Ping());
        }
        
        [Test]
        public async Task ServerError()
        {
            MockServer.Given(Request.Create().WithPath("/ping").UsingGet())
                            .RespondWith(CreateErrorResponse(""));

            Assert.IsFalse(await FluxClient.Ping());
        }
        
        [Test]
        public async Task NotRunningServer()
        {
            MockServer.Stop();

            Assert.IsFalse(await FluxClient.Ping());
        }
    }
}