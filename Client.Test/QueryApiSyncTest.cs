using System;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Test;
using NUnit.Framework;
using WireMock.RequestBuilders;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class QueryApiSyncTest : AbstractMockServerTest
    {
        private InfluxDBClient _influxDbClient;
        private QueryApiSync _queryApiSync;

        private const string Data =
            "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,string,string,string,string,double\n"
            + "#group,false,false,false,false,false,false,false,false,false,false\n"
            + "#default,_result,,,,,,,,,\n"
            + ",result,table,_start,_stop,_time,id,_field,_measurement,host,_value\n"
            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,ab,free,mem,A,12.25\n"
            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,cd,free,mem,A,13.00\n";

        [SetUp]
        public new void SetUp()
        {
            var options = InfluxDBClientOptions.Builder
                .CreateNew()
                .Url(MockServerUrl)
                .AuthenticateToken("token")
                .Org("my-org")
                .Build();

            _influxDbClient = InfluxDBClientFactory.Create(options);
            _queryApiSync = _influxDbClient.GetQueryApiSync();
        }

        [TearDown]
        public void TearDown()
        {
            _influxDbClient?.Dispose();
        }

        [Test]
        public void Measurement()
        {
            MockServer
                .Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateResponse(Data));

            var measurements = _queryApiSync.QuerySync<SyncPoco>("from(...");

            Assert.AreEqual(2, measurements.Count);
            Assert.AreEqual(12.25, measurements[0].Value);
            Assert.AreEqual(13.00, measurements[1].Value);
        }

        [Test]
        public void FluxTable()
        {
            MockServer
                .Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateResponse(Data));

            var tables = _queryApiSync.QuerySync("from(...");

            Assert.AreEqual(1, tables.Count);
            Assert.AreEqual(2, tables[0].Records.Count);
            Assert.AreEqual(12.25, tables[0].Records[0].GetValue());
            Assert.AreEqual(13.00, tables[0].Records[1].GetValue());
        }

        [Test]
        public void RequiredOrgQuerySync()
        {
            _influxDbClient.Dispose();

            var options = InfluxDBClientOptions.Builder
                .CreateNew()
                .Url(MockServerUrl)
                .AuthenticateToken("token")
                .Build();

            _influxDbClient = InfluxDBClientFactory.Create(options);
            _queryApiSync = _influxDbClient.GetQueryApiSync();

            var ae = Assert.Throws<ArgumentException>(() => _queryApiSync.QuerySync("from(..."));

            Assert.NotNull(ae);
            Assert.AreEqual(
                "Expecting a non-empty string for 'org' parameter. Please specify the organization as a method parameter or use default configuration at 'InfluxDBClientOptions.Org'.",
                ae.Message);
        }

        private class SyncPoco
        {
            [Column("id", IsTag = true)] public string Tag { get; set; }

            [Column("_value")] public double Value { get; set; }

            [Column(IsTimestamp = true)] public object Timestamp { get; set; }
        }
    }
}