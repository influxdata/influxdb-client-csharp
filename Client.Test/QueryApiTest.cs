using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Test;
using NUnit.Framework;
using WireMock.RequestBuilders;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class QueryApiTest : AbstractMockServerTest
    {
        private InfluxDBClient _influxDbClient;
        private QueryApi _queryApi;

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
            _influxDbClient.SetLogLevel(LogLevel.Body);
            _queryApi = _influxDbClient.GetQueryApi();
        }

        [Test]
        public async Task ParallelRequest()
        {
            MockServer
                .Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateResponse(Data));

            var stopWatch = new Stopwatch();
            stopWatch.Start();
            
            var tasks = new List<Task<List<FluxTable>>>();
            foreach (var _ in Enumerable.Range(0, 100))
            {
                tasks.Add(_queryApi.QueryAsync("from(bucket:\"my-bucket\") |> range(start: 0)", "my-org"));
            }
            await Task.WhenAll(tasks);
            
            var ts = stopWatch.Elapsed;
            Assert.LessOrEqual(ts.TotalSeconds, 10, $"Elapsed time: {ts}");
        }

        [Test]
        public async Task GenericAndTypeofCalls()
        {
            MockServer
                .Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateResponse(Data));
            
            var measurements = await _queryApi.QueryAsync<SyncPoco>("from(...");
            var measurementsTypeof = await _queryApi.QueryAsync("from(...",typeof(SyncPoco));

            Assert.AreEqual(2, measurements.Count);
            Assert.AreEqual(2, measurementsTypeof.Count);
            Assert.AreEqual(12.25, measurements[0].Value);
            Assert.AreEqual(13.00, measurements[1].Value);
            Assert.IsAssignableFrom<SyncPoco>(measurementsTypeof[0]);
            var cast = measurementsTypeof.Cast<SyncPoco>().ToList();
            Assert.AreEqual(measurements[0].Timestamp, cast[0].Timestamp);
            Assert.AreEqual(12.25, cast[0].Value);
            Assert.AreEqual(13.00, cast[1].Value);
        }

        [Test]
        public async Task QueryAsyncEnumerable()
        {
            MockServer
                .Given(Request.Create().WithPath("/api/v2/query").UsingPost())
                .RespondWith(CreateResponse(Data));

            var measurements = _queryApi.QueryAsyncEnumerable<SyncPoco>(
                new Query(null, "from(...)"),
                "my-org", new CancellationToken());
            
            var list = new List<SyncPoco>();
            await foreach (var item in measurements.ConfigureAwait(false))
            {
                list.Add(item);
            }

            Assert.AreEqual(2, list.Count);
        }
        
        private class SyncPoco
        {
            [Column("id", IsTag = true)] public string Tag { get; set; }

            [Column("_value")] public double Value { get; set; }

            [Column(IsTimestamp = true)] public Object Timestamp { get; set; }
        }
    }
}