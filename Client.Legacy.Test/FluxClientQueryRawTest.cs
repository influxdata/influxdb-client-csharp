using System.Collections.Generic;
using System.Threading;
using InfluxDB.Client.Core.Exceptions;
using NUnit.Framework;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Client.Legacy.Test
{
    public class FluxClientQueryRawTest : AbstractFluxClientTest
    {
        [Test]
        public void QueryRaw()
        {
            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                            .RespondWith(CreateResponse());

            var result =  FluxClient.QueryRaw("from(bucket:\"telegraf\")");

            AssertSuccessResult(result);
        }

        [Test]
        public void QueryRawError()
        {
            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                            .RespondWith(CreateErrorResponse());

            try
            {
                 FluxClient.QueryRaw("from(bucket:\"telegraf\")");

                Assert.Fail();
            }
            catch (InfluxException e)
            {
                Assert.That(e.Message.Equals("Flux query is not valid"));
            }
        }

        [Test]
        public void QueryRawCallback()
        {
            CountdownEvent = new CountdownEvent(8);

            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                            .RespondWith(CreateResponse());

            var results = new List<string>();

             FluxClient.QueryRaw("from(bucket:\"telegraf\")",
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
        public void QueryRawCallbackOnComplete()
        {
            CountdownEvent = new CountdownEvent(1);

            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                            .RespondWith(CreateResponse());

            var results = new List<string>();

             FluxClient.QueryRaw("from(bucket:\"telegraf\")", null,
                            (cancellable, result) => results.Add(result),
                            error => Assert.Fail("Unreachable"),
                            () => CountdownEvent.Signal());

            WaitToCallback();
            AssertSuccessResult(string.Join("\n", results));
        }

        [Test]
        public void QueryRawCallbackOnError()
        {
            MockServer.Stop();

             FluxClient.QueryRaw("from(bucket:\"telegraf\")",
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