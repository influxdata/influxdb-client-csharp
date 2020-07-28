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
            _influxDbClient = InfluxDBClientFactory.Create(MockServerUrl, "token".ToCharArray());
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
            await writeApi.WritePointsAsync("my-org", "my-bucket", new List<PointData>
            {
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 9.0D)
                    .Timestamp(9L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(10L, WritePrecision.S)
            });

            Assert.AreEqual(1, MockServer.LogEntries.Count());

            var request = MockServer.LogEntries.Last();
            Assert.AreEqual(MockServerUrl + "/api/v2/write?org=my-bucket&bucket=my-org&precision=s",
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
            await writeApi.WritePointsAsync("my-org", "my-bucket", new List<PointData>
            {
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 9.0D)
                    .Timestamp(9L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(10L, WritePrecision.Ms)
            });

            Assert.AreEqual(2, MockServer.LogEntries.Count());

            var request = MockServer.LogEntries.ToList()[0];
            Assert.AreEqual(MockServerUrl + "/api/v2/write?org=my-bucket&bucket=my-org&precision=s",
                request.RequestMessage.AbsoluteUrl);
            Assert.AreEqual("h2o,location=coyote_creek water_level=9 9",
                request.RequestMessage.Body);

            request = MockServer.LogEntries.ToList()[1];
            Assert.AreEqual(MockServerUrl + "/api/v2/write?org=my-bucket&bucket=my-org&precision=ms",
                request.RequestMessage.AbsoluteUrl);
            Assert.AreEqual("h2o,location=coyote_creek water_level=10 10",
                request.RequestMessage.Body);
        }
    }
}