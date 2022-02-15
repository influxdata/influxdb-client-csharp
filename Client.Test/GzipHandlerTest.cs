using System.Linq;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Test;
using NUnit.Framework;
using WireMock.RequestBuilders;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class GzipHandlerTest : AbstractMockServerTest
    {
        private InfluxDBClient _client;

        [SetUp]
        public new void SetUp()
        {
            _client = InfluxDBClientFactory.Create(MockServerUrl);
        }

        [Test]
        public async Task GzipDisabledNotSupportEndPoint()
        {
            _client.DisableGzip();

            MockServer
                .Given(Request.Create().UsingGet())
                .RespondWith(CreateResponse("{\"name\":\"Tom\"}", "application/json"));

            var user = await _client.GetUsersApi().MeAsync();
            Assert.AreEqual("Tom", user.Name);

            var requestEntry = MockServer.LogEntries.Last();
            Assert.AreEqual($"{MockServerUrl}/api/v2/me", requestEntry.RequestMessage.Url);
            Assert.IsFalse(requestEntry.RequestMessage.Headers.ContainsKey("Content-Encoding"));
            Assert.AreEqual("identity", requestEntry.RequestMessage.Headers["Accept-Encoding"].First());
        }

        [Test]
        public async Task GzipDisabledQuery()
        {
            _client.DisableGzip();

            MockServer
                .Given(Request.Create().UsingPost())
                .RespondWith(CreateResponse(""));

            var response = await _client.GetQueryApi().QueryAsync("from", "my-org");
            Assert.AreEqual(0, response.Count);

            var requestEntry = MockServer.LogEntries.Last();
            Assert.AreEqual($"{MockServerUrl}/api/v2/query?org=my-org", requestEntry.RequestMessage.Url);
            Assert.IsFalse(requestEntry.RequestMessage.Headers.ContainsKey("Content-Encoding"));
            Assert.AreEqual("identity", requestEntry.RequestMessage.Headers["Accept-Encoding"].First());
        }

        [Test]
        public void GzipDisabledWrite()
        {
            _client.DisableGzip();

            MockServer
                .Given(Request.Create().UsingPost())
                .RespondWith(CreateResponse(""));

            var writeApi = _client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(writeApi);
            writeApi.WriteRecord("h2o_feet,location=coyote_creek water_level=1.0 1", WritePrecision.Ns, "my-bucket",
                "my-org");
            writeApi.Flush();
            listener.WaitToSuccess();

            var requestEntry = MockServer.LogEntries.Last();
            Assert.AreEqual($"{MockServerUrl}/api/v2/write?org=my-org&bucket=my-bucket&precision=ns",
                requestEntry.RequestMessage.Url);
            Assert.AreEqual("identity", requestEntry.RequestMessage.Headers["Content-Encoding"].First());
            Assert.AreEqual("identity", requestEntry.RequestMessage.Headers["Accept-Encoding"].First());
        }

        [Test]
        public async Task GzipEnabledNotSupportEndPoint()
        {
            _client.EnableGzip();

            MockServer
                .Given(Request.Create().UsingGet())
                .RespondWith(CreateResponse("{\"name\":\"Tom\"}", "application/json"));

            var user = await _client.GetUsersApi().MeAsync();
            Assert.AreEqual("Tom", user.Name);

            var requestEntry = MockServer.LogEntries.Last();
            Assert.AreEqual($"{MockServerUrl}/api/v2/me",
                requestEntry.RequestMessage.Url);
            Assert.IsFalse(requestEntry.RequestMessage.Headers.ContainsKey("Content-Encoding"));
            Assert.AreEqual("identity",
                requestEntry.RequestMessage.Headers["Accept-Encoding"].First());
        }

        [Test]
        public async Task GzipEnabledQuery()
        {
            _client.EnableGzip();

            MockServer
                .Given(Request.Create().UsingPost())
                .RespondWith(CreateResponse("", "text/csv"));

            var response = await _client.GetQueryApi().QueryAsync("from", "my-org");
            Assert.AreEqual(0, response.Count);

            var requestEntry = MockServer.LogEntries.Last();
            Assert.AreEqual($"{MockServerUrl}/api/v2/query?org=my-org",
                requestEntry.RequestMessage.Url);
            Assert.IsFalse(requestEntry.RequestMessage.Headers.ContainsKey("Content-Encoding"));
            Assert.AreEqual("gzip", requestEntry.RequestMessage.Headers["Accept-Encoding"].First());
        }

        [Test]
        public void GzipEnabledWrite()
        {
            _client.EnableGzip();

            MockServer
                .Given(Request.Create().UsingPost())
                .RespondWith(CreateResponse("", "text/csv"));

            var writeApi = _client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(writeApi);
            writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ water_level=1.0 1", WritePrecision.Ns,
                "my-bucket", "my-org");
            writeApi.Flush();
            listener.WaitToSuccess();

            var requestEntry = MockServer.LogEntries.Last();
            Assert.AreEqual($"{MockServerUrl}/api/v2/write?org=my-org&bucket=my-bucket&precision=ns",
                requestEntry.RequestMessage.Url);
            Assert.AreEqual("gzip", requestEntry.RequestMessage.Headers["Content-Encoding"].First());
            Assert.AreEqual("identity",
                requestEntry.RequestMessage.Headers["Accept-Encoding"].First());
            Assert.AreEqual("gzip", requestEntry.RequestMessage.DetectedCompression);
        }
    }
}