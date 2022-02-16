using System.Threading.Tasks;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Client.Legacy.Test
{
    public class FluxClientVersionTest : AbstractFluxClientTest
    {
        [Test]
        public async Task Version()
        {
            MockServer.Given(Request.Create().WithPath("/ping").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(204)
                    .WithHeader("X-Influxdb-Version", "1.7.0"));

            Assert.AreEqual("1.7.0", await FluxClient.VersionAsync());
        }

        [Test]
        public async Task VersionUnknown()
        {
            MockServer.Given(Request.Create().WithPath("/ping").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(204));

            Assert.AreEqual("unknown", await FluxClient.VersionAsync());
        }

        [Test]
        public async Task Error()
        {
            MockServer.Stop();

            try
            {
                await FluxClient.VersionAsync();

                Assert.Fail();
            }
            catch (InfluxException e)
            {
                Assert.IsNotEmpty(e.Message);
            }
        }
    }
}