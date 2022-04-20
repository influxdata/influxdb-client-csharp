using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Test;
using InfluxDB.Client.Writes;
using NUnit.Framework;
using RestSharp;
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
            await writeApi.WritePointsAsync(new List<PointData>
            {
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 9.0D)
                    .Timestamp(9L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(10L, WritePrecision.S)
            }, "my-bucket", "my-org");

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
            await writeApi.WritePointsAsync(new List<PointData>
            {
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 9.0D)
                    .Timestamp(9L, WritePrecision.S),
                PointData.Measurement("h2o").Tag("location", "coyote_creek").Field("water_level", 10.0D)
                    .Timestamp(10L, WritePrecision.Ms)
            }, "my-bucket", "my-org");

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

            foreach (var batch in batches) await writeApi.WritePointsAsync(batch, "my-bucket", "my-org");

            Assert.AreEqual(3, MockServer.LogEntries.Count());
        }

        [Test]
        public async Task WriteRecordsWithIRestResponse()
        {
            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .RespondWith(CreateResponse("{}"));

            var writeApi = _influxDbClient.GetWriteApiAsync();
            var response = await writeApi.WriteRecordsAsyncWithIRestResponse(
                new[] { "h2o,location=coyote_creek water_level=9 1" },
                WritePrecision.Ms, "my-bucket", "my-org");
            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);

            var request = MockServer.LogEntries.ToList()[0];
            StringAssert.EndsWith("/api/v2/write?org=my-org&bucket=my-bucket&precision=ms",
                request.RequestMessage.AbsoluteUrl);
            Assert.AreEqual("h2o,location=coyote_creek water_level=9 1", GetRequestBody(response));
        }

        [Test]
        public async Task WritePointsWithIRestResponse()
        {
            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .RespondWith(CreateResponse("{}"));

            var writeApi = _influxDbClient.GetWriteApiAsync();
            var responses = await writeApi.WritePointsAsyncWithIRestResponse(
                new[]
                {
                    PointData.Measurement("h2o")
                        .Tag("location", "coyote_creek")
                        .Field("water_level", 9.0D)
                        .Timestamp(9L, WritePrecision.S),
                    PointData.Measurement("h2o")
                        .Tag("location", "coyote_creek")
                        .Field("water_level", 10.0D)
                        .Timestamp(10L, WritePrecision.Ms)
                },
                "my-bucket",
                "my-org");

            Assert.AreEqual(2, responses.Length);
            Assert.AreEqual(HttpStatusCode.OK, responses[0].StatusCode);
            Assert.AreEqual("h2o,location=coyote_creek water_level=9 9", GetRequestBody(responses[0]));
            Assert.AreEqual(HttpStatusCode.OK, responses[1].StatusCode);
            Assert.AreEqual("h2o,location=coyote_creek water_level=10 10", GetRequestBody(responses[1]));
        }

        [Test]
        public async Task WriteMeasurementsWithIRestResponse()
        {
            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .RespondWith(CreateResponse("{}"));

            var writeApi = _influxDbClient.GetWriteApiAsync();
            var response = await writeApi.WriteMeasurementsAsyncWithIRestResponse(
                new[]
                {
                    new SimpleModel
                    {
                        Time = new DateTime(2020, 11, 15, 8, 20, 15, DateTimeKind.Utc),
                        Device = "id-1",
                        Value = 16
                    }
                },
                bucket: "my-bucket",
                org: "my-org");

            Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
            Assert.AreEqual("m,device=id-1 value=16i 1605428415000000000", GetRequestBody(response));
        }

        [Test]
        public void RequiredOrgBucketWriteApiAsync()
        {
            _influxDbClient.Dispose();

            var options = InfluxDBClientOptions.Builder
                .CreateNew()
                .Url(MockServerUrl)
                .AuthenticateToken("token")
                .Build();

            _influxDbClient = InfluxDBClientFactory.Create(options);
            var writeApi = _influxDbClient.GetWriteApiAsync();

            var ae = Assert.ThrowsAsync<ArgumentException>(async () =>
                await writeApi.WriteRecordAsync(
                    "h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1", bucket: "b1"));
            Assert.NotNull(ae);
            Assert.AreEqual(
                "Expecting a non-empty string for 'org' parameter. Please specify the organization as a method parameter or use default configuration at 'InfluxDBClientOptions.Org'.",
                ae.Message);

            ae = Assert.ThrowsAsync<ArgumentException>(async () =>
                await writeApi.WriteRecordAsync(
                    "h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1", org: "org1"));
            Assert.NotNull(ae);
            Assert.AreEqual(
                "Expecting a non-empty string for 'bucket' parameter. Please specify the bucket as a method parameter or use default configuration at 'InfluxDBClientOptions.Bucket'.",
                ae.Message);
        }

        [Test]
        public async Task UseDefaultOrganizationAndBucket()
        {
            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .RespondWith(CreateResponse("{}"));

            _influxDbClient.Dispose();

            var options = InfluxDBClientOptions.Builder
                .CreateNew()
                .Url(MockServerUrl)
                .AuthenticateToken("token")
                .Bucket("my-bucket")
                .Org("my-org")
                .Build();

            _influxDbClient = InfluxDBClientFactory.Create(options);

            var writeApi = _influxDbClient.GetWriteApiAsync();
            await writeApi.WriteRecordsAsyncWithIRestResponse(new[] { "mem,location=a level=1.0 1" });
            await writeApi.WritePointsAsyncWithIRestResponse(new[]
                { PointData.Measurement("h2o").Field("water_level", 9.0D) });
        }

        private string GetRequestBody(RestResponse restResponse)
        {
            var bytes = (byte[])restResponse.Request?.Parameters
                            .GetParameters(ParameterType.RequestBody)
                            .TryFind("text/plain")?.Value ??
                        throw new AssertionException("The body is required.");
            return Encoding.Default.GetString(bytes);
        }
    }
}