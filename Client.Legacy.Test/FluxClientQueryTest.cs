using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Exceptions;
using NUnit.Framework;
using WireMock.RequestBuilders;

namespace Client.Legacy.Test
{
    public class FluxClientQueryTest : AbstractFluxClientTest
    {
        [Test]
        public void Query()
        {

            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                            .RespondWith(CreateResponse());

            var result =  FluxClient.Query("from(bucket:\"telegraf\")");

            AssertSuccessResult(result);
        }

        [Test]
        public void QueryToPoco()
        {
            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                            .RespondWith(CreateResponse());

            var result = FluxClient.Query<Free>("from(bucket:\"telegraf\")");

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
        public void QueryError()
        {
            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                            .RespondWith(CreateErrorResponse("Flux query is not valid"));
            
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
        public void QueryErrorSuccessResponse()
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
                FluxClient.Query("from(bucket:\"telegraf\")");

                Assert.Fail();
            }
            catch (FluxQueryException e)
            {
                Assert.That(e.Message.Equals("failed to create physical plan: invalid time bounds from procedure from: bounds contain zero time"));
                Assert.AreEqual(e.Reference, 897);
            }
        }

        [Test]
        public void QueryErrorSuccessResponseWithoutReference()
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
                FluxClient.Query("from(bucket:\"telegraf\")");

                Assert.Fail();
            }
            catch (InfluxException e)
            {
                Assert.That(e.Message.Equals("failed to create physical plan: invalid time bounds from procedure from: bounds contain zero time"));
            }
        }

        [Test]
        public void QueryCallback()
        {
            CountdownEvent = new CountdownEvent(4);

            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                            .RespondWith(CreateResponse());

            var records = new List<FluxRecord>();

            FluxClient.Query("from(bucket:\"telegraf\")",
                            (cancellable, result) =>
                            {
                                records.Add(result);

                                CountdownEvent.Signal();
                            });

            WaitToCallback();

            AssertRecords(records);
        }

        [Test]
        public void QueryCallbackOnComplete()
        {
            CountdownEvent = new CountdownEvent(5);

            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                            .RespondWith(CreateResponse());

            var records = new List<FluxRecord>();

            FluxClient.Query("from(bucket:\"telegraf\")",
                            (cancellable, result) =>
                            {
                                records.Add(result);

                                CountdownEvent.Signal();
                            }, error => Assert.Fail("Unreachable"), 
                            () => CountdownEvent.Signal());

            WaitToCallback();

            AssertRecords(records);
        }

        [Test]
        public void QueryCallbackError()
        {
            MockServer.Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                            .RespondWith(CreateErrorResponse("Flux query is not valid"));

            FluxClient.Query("from(bucket:\"telegraf\")",
                            (cancellable, result) => Assert.Fail("Unreachable"), 
                            error => CountdownEvent.Signal());

            WaitToCallback();
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
            [Column("host")] 
            public string Host { get; set; }

            [Column("region")]
            public string Region { get; set; }

            [Column("_value")]
            public long Mem { get; set; }
        }
    }
}