using System.Threading.Tasks;
using NUnit.Framework;
using Platform.Common.Flux.Error;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Flux.Tests.Flux
{
    public class FluxClientVersionTest : AbstractFluxClientTest
    {
        [Test]
        public async Task Version() 
        {
            MockServer.Given(Request.Create().WithPath("/ping").UsingGet())
                            .RespondWith(Response.Create().WithStatusCode(204)
                                            .WithHeader("X-Influxdb-Version", "1.7.0"));

            Assert.AreEqual("1.7.0", await FluxClient.Version());
        }

        [Test]
        public async Task  VersionUnknown() 
        {
            MockServer.Given(Request.Create().WithPath("/ping").UsingGet())
                            .RespondWith(Response.Create().WithStatusCode(204));

            Assert.AreEqual("unknown", await FluxClient.Version());
        }

        [Test]
        public async Task Error()
        {
            MockServer.Stop();

            try
            {
                await FluxClient.Version();

                Assert.Fail();
            }
            catch (InfluxException e)
            {
                Assert.That(e.Errors.Count == 1);
            }

        }
    }
}