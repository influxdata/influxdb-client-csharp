using NUnit.Framework;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Client.Legacy.Test
{
    public class MockServerFluxClientPingTest : AbstractFluxClientTest
    {
        [Test]
        public void Healthy()
        {
            MockServer.Given(Request.Create().WithPath("/ping").UsingGet())
                            .RespondWith(Response.Create().WithStatusCode(204));
            
            Assert.IsTrue( FluxClient.Ping());
        }
        
        [Test]
        public void ServerError()
        {
            MockServer.Given(Request.Create().WithPath("/ping").UsingGet())
                            .RespondWith(CreateErrorResponse(""));

            Assert.IsFalse( FluxClient.Ping());
        }
        
        [Test]
        public void NotRunningServer()
        {
            MockServer.Stop();

            Assert.IsFalse( FluxClient.Ping());
        }
    }
}