using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Test;
using InfluxDB.Client.Writes;
using NUnit.Framework;
using WireMock.RequestBuilders;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class WriteApiAsyncTest : AbstractMockServerTest
    {
        private InfluxDBClient _influxDbClient;

        [SetUp]
        public new void SetUp()
        {
            _influxDbClient = InfluxDBClientFactory.Create(MockServerUrl, "token");
        }

        [TearDown]
        public new void ResetServer()
        {
            _influxDbClient.Dispose();
        }

        [Test]
        public async Task GroupPointsByPrecisionSame()
        {
            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .RespondWith(CreateResponse("{}"));

            var writeApi = _influxDbClient.GetWriteApiAsync();
            await writeApi.WritePointsAsync("my-bucket", "my-org", new List<PointData>
            {
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 9.0D)
                    .Timestamp(9L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(10L, WritePrecision.S)
            });

            Assert.AreEqual(1, MockServer.LogEntries.Count());

            var request = MockServer.LogEntries.Last();
            Assert.AreEqual(MockServerUrl + "/api/v2/write?org=my-org&bucket=my-bucket&precision=s",
                request.RequestMessage.AbsoluteUrl);
            Assert.AreEqual("h2o,location=coyote_creek water_level=9 9\nh2o,location=coyote_creek water_level=10 10",
                request.RequestMessage.Body);
        }

        [Test]
        public async Task GroupPointsByPrecisionDifferent()
        {
            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .RespondWith(CreateResponse("{}"));

            var writeApi = _influxDbClient.GetWriteApiAsync();
            await writeApi.WritePointsAsync("my-bucket", "my-org", new List<PointData>
            {
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 9.0D)
                    .Timestamp(9L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(10L, WritePrecision.Ms)
            });

            Assert.AreEqual(2, MockServer.LogEntries.Count());

            var request = MockServer.LogEntries.ToList()[0];
            Assert.AreEqual(MockServerUrl + "/api/v2/write?org=my-org&bucket=my-bucket&precision=s",
                request.RequestMessage.AbsoluteUrl);
            Assert.AreEqual("h2o,location=coyote_creek water_level=9 9",
                request.RequestMessage.Body);

            request = MockServer.LogEntries.ToList()[1];
            Assert.AreEqual(MockServerUrl + "/api/v2/write?org=my-org&bucket=my-bucket&precision=ms",
                request.RequestMessage.AbsoluteUrl);
            Assert.AreEqual("h2o,location=coyote_creek water_level=10 10",
                request.RequestMessage.Body);
        }

        [Test]
        public async Task SplitPointList()
        {
            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .RespondWith(CreateResponse("{}"));
            
            var writeApi = _influxDbClient.GetWriteApiAsync();
            
            var points = new List<PointData>
            {
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(1L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(2L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(3L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(4L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(5L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(6L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(7L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(8L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(9L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(10L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(11L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(12L, WritePrecision.S)
            };

            var batches = points
                .Select((x, i) => new { Index = i, Value = x })
                .GroupBy(x => x.Index / 5)
                .Select(x => x.Select(v => v.Value).ToList())
                .ToList();

            foreach (var batch in batches)
            {
                await writeApi.WritePointsAsync("my-bucket", "my-org", batch);
            }
           
            Assert.AreEqual(3, MockServer.LogEntries.Count());
        }
    }
}