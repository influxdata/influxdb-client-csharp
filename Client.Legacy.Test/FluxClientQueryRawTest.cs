using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Client.Legacy.Test
{
    public class FluxClientQueryRawTest : AbstractFluxClientTest
    {
        [Test]
        public async Task QueryRaw()
        {
            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateResponse());

            var result = await FluxClient.QueryRawAsync("from(bucket:\"telegraf\")");

            AssertSuccessResult(result);
        }

        [Test]
        public async Task QueryRawError()
        {
            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateErrorResponse());

            try
            {
                await FluxClient.QueryRawAsync("from(bucket:\"telegraf\")");

                Assert.Fail();
            }
            catch (InfluxException e)
            {
                Assert.That(e.Message.Equals("Flux query is not valid"));
            }
        }

        [Test]
        public async Task QueryRawCallback()
        {
            CountdownEvent = new CountdownEvent(8);

            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateResponse());

            var results = new List<string>();

            await FluxClient.QueryRawAsync("from(bucket:\"telegraf\")", result =>
            {
                results.Add(result);
                CountdownEvent.Signal();
            });

            WaitToCallback();

            Assert.That(results.Count == 8);
            AssertSuccessResult(string.Join("\n", results));
        }

        [Test]
        public async Task QueryRawCallbackOnComplete()
        {
            CountdownEvent = new CountdownEvent(1);

            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateResponse());

            var results = new List<string>();

            await FluxClient.QueryRawAsync("from(bucket:\"telegraf\")",
                result => results.Add(result),
                onError: error => Assert.Fail("Unreachable"), onComplete: () => CountdownEvent.Signal());

            WaitToCallback();
            AssertSuccessResult(string.Join("\n", results));
        }

        [Test]
        public async Task QueryRawCallbackOnError()
        {
            MockServer.Stop();

            await FluxClient.QueryRawAsync("from(bucket:\"telegraf\")",
                result => Assert.Fail("Unreachable"),
                onError: error => CountdownEvent.Signal());

            WaitToCallback();
        }

        private void AssertSuccessResult(string result)
        {
            Assert.NotNull(result);
            Assert.AreEqual(SuccessData, result);
        }

        private IResponseBuilder CreateErrorResponse()
        {
            return CreateErrorResponse("Flux query is not valid");
        }
    }
}