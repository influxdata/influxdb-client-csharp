using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flux.Client.Tests;
using NUnit.Framework;
using Platform.Common.Flux.Error;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Flux.Client.Tests
{
    public class FluxClientQueryRawTest : AbstractFluxClientTest
    {
        [Test]
        public async Task QueryRaw()
        {
            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                            .RespondWith(CreateResponse());

            string result = await FluxClient.QueryRaw("from(bucket:\"telegraf\")");

            AssertSuccessResult(result);
        }

        [Test]
        public async Task QueryRawError()
        {
            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                            .RespondWith(CreateErrorResponse());

            try
            {
                await FluxClient.QueryRaw("from(bucket:\"telegraf\")");

                Assert.Fail();
            }
            catch (InfluxException e)
            {
                Assert.That(e.Errors.Count == 1);
                Assert.That(e.Errors[0].Contains("Flux query is not valid"));
            }
        }

        [Test]
        public async Task QueryRawCallback()
        {
            CountdownEvent = new CountdownEvent(8);

            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                            .RespondWith(CreateResponse());

            List<string> results = new List<string>();

            await FluxClient.QueryRaw("from(bucket:\"telegraf\")",
                            (cancellable, result) =>
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

            List<string> results = new List<string>();

            await FluxClient.QueryRaw("from(bucket:\"telegraf\")", null,
                            (cancellable, result) => results.Add(result),
                            error => Assert.Fail("Unreachable"),
                            () => CountdownEvent.Signal());

            WaitToCallback();
            AssertSuccessResult(string.Join("\n", results));
        }

        [Test]
        public async Task QueryRawCallbackOnError()
        {
            MockServer.Stop();

            await FluxClient.QueryRaw("from(bucket:\"telegraf\")",
                            (cancellable, result) => Assert.Fail("Unreachable"),
                            error => CountdownEvent.Signal());

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