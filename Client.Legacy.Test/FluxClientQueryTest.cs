using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Exceptions;
using InfluxDB.Client.Flux;
using NUnit.Framework;
using WireMock.Matchers;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace Client.Legacy.Test
{
    public class FluxClientQueryTest : AbstractFluxClientTest
    {
        [Test]
        public async Task Query()
        {
            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateResponse());

            var result = await FluxClient.QueryAsync("from(bucket:\"telegraf\")");

            AssertSuccessResult(result);
        }

        [Test]
        public async Task QueryToPoco()
        {
            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateResponse());

            var result = await FluxClient.QueryAsync<Free>("from(bucket:\"telegraf\")");

            Assert.That(result.Count == 4);

            // 1
            Assert.AreEqual("A", result[0].Host);
            Assert.AreEqual("west", result[0].Region);
            Assert.AreEqual(10L, result[0].Mem);

            // 2
            Assert.AreEqual("B", result[1].Host);
            Assert.AreEqual("west", result[1].Region);
            Assert.AreEqual(20L, result[1].Mem);

            // 3
            Assert.AreEqual("A", result[2].Host);
            Assert.AreEqual("west", result[2].Region);
            Assert.AreEqual(11L, result[2].Mem);

            // 4
            Assert.AreEqual("B", result[3].Host);
            Assert.AreEqual("west", result[3].Region);
            Assert.AreEqual(22L, result[3].Mem);
        }

        [Test]
        public async Task QueryError()
        {
            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateErrorResponse("Flux query is not valid"));

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
        public async Task ErrorAsStream()
        {
            var response = Response.Create()
                .WithStatusCode(403)
                .WithBody(
                    "Flux query service disabled. Verify flux-enabled=true in the [http] section of the InfluxDB config.");

            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(response);

            try
            {
                await FluxClient.QueryAsync("from(bucket:\"telegraf\")");

                Assert.Fail();
            }
            catch (InfluxException e)
            {
                Assert.AreEqual(e.Status, 403);
                Assert.AreEqual(e.Message,
                    "Flux query service disabled. Verify flux-enabled=true in the [http] section of the InfluxDB config.");
            }
        }

        [Test]
        public async Task QueryErrorSuccessResponse()
        {
            var error = "#datatype,string,string\n"
                        + "#group,true,true\n"
                        + "#default,,\n"
                        + ",error,reference\n"
                        + ",failed to create physical plan: invalid time bounds from procedure from: bounds contain zero time,897";

            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateResponse(error));

            try
            {
                await FluxClient.QueryAsync("from(bucket:\"telegraf\")");

                Assert.Fail();
            }
            catch (FluxQueryException e)
            {
                Assert.That(e.Message.Equals(
                    "failed to create physical plan: invalid time bounds from procedure from: bounds contain zero time"));
                Assert.AreEqual(e.Reference, 897);
            }
        }

        [Test]
        public async Task QueryErrorSuccessResponseWithoutReference()
        {
            var error = "#datatype,string,string\n"
                        + "#group,true,true\n"
                        + "#default,,\n"
                        + ",error,reference\n"
                        + ",failed to create physical plan: invalid time bounds from procedure from: bounds contain zero time,";

            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateResponse(error));

            try
            {
                await FluxClient.QueryAsync("from(bucket:\"telegraf\")");

                Assert.Fail();
            }
            catch (InfluxException e)
            {
                Assert.That(e.Message.Equals(
                    "failed to create physical plan: invalid time bounds from procedure from: bounds contain zero time"));
            }
        }

        [Test]
        public async Task QueryCallback()
        {
            CountdownEvent = new CountdownEvent(4);

            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateResponse());

            var records = new List<FluxRecord>();

            await FluxClient.QueryAsync("from(bucket:\"telegraf\")",
                result =>
                {
                    records.Add(result);

                    CountdownEvent.Signal();
                });

            WaitToCallback();

            AssertRecords(records);
        }

        [Test]
        public async Task QueryCallbackOnComplete()
        {
            CountdownEvent = new CountdownEvent(5);

            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateResponse());

            var records = new List<FluxRecord>();

            await FluxClient.QueryAsync("from(bucket:\"telegraf\")",
                result =>
                {
                    records.Add(result);

                    CountdownEvent.Signal();
                }, error => Assert.Fail("Unreachable"),
                () => CountdownEvent.Signal());

            WaitToCallback();

            AssertRecords(records);
        }

        [Test]
        public async Task QueryCallbackError()
        {
            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateErrorResponse("Flux query is not valid"));

            await FluxClient.QueryAsync("from(bucket:\"telegraf\")",
                result => Assert.Fail("Unreachable"),
                error => CountdownEvent.Signal());

            WaitToCallback();
        }

        [Test]
        public async Task UserAgentHeader()
        {
            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateResponse());

            await FluxClient.QueryAsync("from(bucket:\"telegraf\")");

            var request = MockServer.LogEntries.Last();
            StringAssert.StartsWith("influxdb-client-csharp/4.", request.RequestMessage.Headers["User-Agent"].First());
            StringAssert.EndsWith(".0.0", request.RequestMessage.Headers["User-Agent"].First());
        }

        [Test]
        public async Task WithAuthentication()
        {
            FluxClient =
                FluxClientFactory.Create(new FluxConnectionOptions(MockServerUrl, "my-user",
                    "my-password".ToCharArray()));

            MockServer.Given(Request.Create()
                    .WithPath("/api/v2/query")
                    .WithParam("u", new ExactMatcher("my-user"))
                    .WithParam("p", new ExactMatcher("my-password"))
                    .UsingPost())
                .RespondWith(CreateResponse());

            var result = await FluxClient.QueryAsync("from(bucket:\"telegraf\")");

            AssertSuccessResult(result);
        }

        [Test]
        public async Task WithBasicAuthentication()
        {
            FluxClient = FluxClientFactory.Create(new FluxConnectionOptions(MockServerUrl, "my-user",
                "my-password".ToCharArray(), FluxConnectionOptions.AuthenticationType.BasicAuthentication));

            var auth = System.Text.Encoding.UTF8.GetBytes("my-user:my-password");

            MockServer.Given(Request.Create()
                    .WithPath("/api/v2/query")
                    .WithHeader("Authorization",
                        new ExactMatcher("Basic " + Convert.ToBase64String(auth)))
                    .UsingPost())
                .RespondWith(CreateResponse());

            var result = await FluxClient.QueryAsync("from(bucket:\"telegraf\")");

            AssertSuccessResult(result);
        }

        private void AssertSuccessResult(List<FluxTable> tables)
        {
            Assert.IsNotNull(tables);
            Assert.That(tables.Count == 1);
            var records = tables[0].Records;
            AssertRecords(records);
        }

        private void AssertRecords(List<FluxRecord> records)
        {
            Assert.That(records.Count == 4);
        }

        private class Free
        {
            [Column("host")] public string Host { get; set; }

            [Column("region")] public string Region { get; set; }

            [Column("_value")] public long Mem { get; set; }
        }
    }
}