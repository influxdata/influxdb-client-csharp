using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Test;
using NUnit.Framework;
using WireMock.RequestBuilders;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class InfluxDbClientTest : AbstractMockServerTest
    {
        private InfluxDBClient _client;

        [SetUp]
        public new void SetUp()
        {
            _client = InfluxDBClientFactory.Create(MockServerUrl);
        }

        [Test]
        public async Task ParseKnownEnum()
        {
            MockServer
                .Given(Request.Create().UsingGet())
                .RespondWith(CreateResponse("{\"status\":\"active\"}", "application/json"));

            var authorization = await _client.GetAuthorizationsApi().FindAuthorizationByIdAsync("id");

            Assert.AreEqual(AuthorizationUpdateRequest.StatusEnum.Active, authorization.Status);
        }

        [Test]
        public void ParseUnknownEnumAsNull()
        {
            MockServer
                .Given(Request.Create().UsingGet())
                .RespondWith(CreateResponse("{\"status\":\"unknown\"}", "application/json"));

            var ioe = Assert.ThrowsAsync<ApiException>(async () =>
                await _client.GetAuthorizationsApi().FindAuthorizationByIdAsync("id"));

            Assert.IsTrue(ioe.Message.StartsWith("Error converting value \"unknown\" to typ"));
        }

        [Test]
        public async Task ParseDate()
        {
            const string data = "{\"links\":{\"self\":\"/api/v2/buckets/0376298868765000/log\"},\"logs\":[" +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Created\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T07:33:44.390263749Z\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.252492+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.334601+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.437055+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.568936+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.64818+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.749176+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.82996+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.908611+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.9828+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.090233+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.193205+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.271324+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.338836+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.446591+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.549676+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.631707+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.714726+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.806946+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.889206+01:00\"}]}";

            MockServer
                .Given(Request.Create().UsingGet())
                .RespondWith(CreateResponse(data, "application/json"));

            var logs = await _client.GetBucketsApi().FindBucketLogsAsync("id");

            Assert.AreEqual(20, logs.Count);
        }

        [Test]
        public void Gzip()
        {
            Assert.IsFalse(_client.IsGzipEnabled());

            // Enable GZIP
            _client.EnableGzip();
            Assert.IsTrue(_client.IsGzipEnabled());

            // Disable GZIP
            _client.DisableGzip();
            Assert.IsFalse(_client.IsGzipEnabled());
        }

        [Test]
        public void LogLevelWithQueryString()
        {
            var writer = new StringWriter();
            Trace.Listeners.Add(new TextWriterTraceListener(writer));
            
            _client.SetLogLevel(LogLevel.Headers);
            
            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .RespondWith(CreateResponse("{}"));

            using (var writeApi = _client.GetWriteApi())
            {
                writeApi.WriteRecord("b1", "org1", WritePrecision.Ns,
                    "h2o_feet,location=coyote_creek water_level=1.0 1");
            }
            
            StringAssert.Contains("org=org1", writer.ToString());
            StringAssert.Contains("bucket=b1", writer.ToString());
            StringAssert.Contains("precision=ns", writer.ToString());
        }
        
        [Test]
        public void LogLevelWithoutQueryString()
        {
            var writer = new StringWriter();
            Trace.Listeners.Add(new TextWriterTraceListener(writer));
            
            _client.SetLogLevel(LogLevel.Basic);
            
            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .RespondWith(CreateResponse("{}"));

            using (var writeApi = _client.GetWriteApi())
            {
                writeApi.WriteRecord("b1", "org1", WritePrecision.Ns,
                    "h2o_feet,location=coyote_creek water_level=1.0 1");
            }
            
            StringAssert.DoesNotContain("org=org1", writer.ToString());
            StringAssert.DoesNotContain("bucket=b1", writer.ToString());
            StringAssert.DoesNotContain("precision=ns", writer.ToString());
        }
        
        [Test]
        public async Task UserAgentHeader()
        {
            MockServer
                .Given(Request.Create().UsingGet())
                .RespondWith(CreateResponse("{\"status\":\"active\"}", "application/json"));

            await _client.GetAuthorizationsApi().FindAuthorizationByIdAsync("id");

            var request= MockServer.LogEntries.Last();
            StringAssert.StartsWith("influxdb-client-csharp/1.", request.RequestMessage.Headers["User-Agent"].First());
            StringAssert.EndsWith(".0.0", request.RequestMessage.Headers["User-Agent"].First());
        }
    }
}