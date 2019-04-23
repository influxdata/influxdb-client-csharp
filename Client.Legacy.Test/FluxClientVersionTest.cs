using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Client.Legacy.Test
{
    public class FluxClientVersionTest : AbstractFluxClientTest
    {
        [Test]
        public void Version() 
        {
            MockServer.Given(Request.Create().WithPath("/ping").UsingGet())
                            .RespondWith(Response.Create().WithStatusCode(204)
                                            .WithHeader("X-Influxdb-Version", "1.7.0"));

            Assert.AreEqual("1.7.0", FluxClient.Version());
        }

        [Test]
        public void  VersionUnknown() 
        {
            MockServer.Given(Request.Create().WithPath("/ping").UsingGet())
                            .RespondWith(Response.Create().WithStatusCode(204));

            Assert.AreEqual("unknown", FluxClient.Version());
        }

        [Test]
        public void Error()
        {
            MockServer.Stop();

            try
            {
                FluxClient.Version();

                Assert.Fail();
            }
            catch (InfluxException e)
            {
                Assert.IsNotEmpty(e.Message);
            }

        }
    }
}