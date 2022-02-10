using System;
using System.Threading.Tasks;
using InfluxDB.Client.Flux;
using NUnit.Framework;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Client.Legacy.Test
{
    public class MockServerFluxClientPingTest : AbstractFluxClientTest
    {
        [Test]
        public async Task Healthy()
        {
            MockServer.Given(Request.Create().WithPath("/ping").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(204));

            Assert.IsTrue(await FluxClient.PingAsync());
        }

        [Test]
        public async Task ServerError()
        {
            MockServer.Given(Request.Create().WithPath("/ping").UsingGet())
                .RespondWith(CreateErrorResponse(""));

            Assert.IsFalse(await FluxClient.PingAsync());
        }

        [Test]
        public async Task NotRunningServer()
        {
            MockServer.Stop();

            Assert.IsFalse(await FluxClient.PingAsync());
        }

        [Test]
        public async Task WithAuthentication()
        {
            FluxClient =
                FluxClientFactory.Create(new FluxConnectionOptions(MockServerUrl, "my-user",
                    "my-password".ToCharArray()));

            MockServer.Given(Request.Create()
                    .WithPath("/ping")
                    .WithParam("u", new ExactMatcher("my-user"))
                    .WithParam("p", new ExactMatcher("my-password"))
                    .UsingGet())
                .RespondWith(Response.Create().WithStatusCode(204));

            Assert.IsTrue(await FluxClient.PingAsync());
        }

        [Test]
        public async Task WithBasicAuthentication()
        {
            FluxClient = FluxClientFactory.Create(new FluxConnectionOptions(MockServerUrl, "my-user",
                "my-password".ToCharArray(), FluxConnectionOptions.AuthenticationType.BasicAuthentication));

            var auth = System.Text.Encoding.UTF8.GetBytes("my-user:my-password");

            MockServer.Given(Request.Create()
                    .WithPath("/ping")
                    .WithHeader("Authorization",
                        new ExactMatcher("Basic " + Convert.ToBase64String(auth)))
                    .UsingGet())
                .RespondWith(Response.Create().WithStatusCode(204));

            Assert.IsTrue(await FluxClient.PingAsync());
        }
    }
}