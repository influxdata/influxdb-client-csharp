using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Test;
using NUnit.Framework;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;
using WireMock.Server;
using WireMock.Settings;

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
            const string data = "{\"runs\":[" +
                                "{\"id\":\"runId\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T07:33:44.390263749Z\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:41.252492+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:41.334601+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:41.437055+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:41.568936+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:41.64818+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:41.749176+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:41.82996+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:41.908611+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:41.9828+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:42.090233+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:42.193205+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:42.271324+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:42.338836+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:42.446591+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:42.549676+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:42.631707+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:42.714726+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:42.806946+01:00\"}," +
                                "{\"id\":\"Bucket Updated\",\"taskID\":\"taskID\",\"startedAt\":\"2019-02-26T08:15:42.889206+01:00\"}]}";

            MockServer
                .Given(Request.Create().UsingGet())
                .RespondWith(CreateResponse(data, "application/json"));

            var runs = await _client.GetTasksApi().GetRunsAsync("taskId");
            Assert.AreEqual(20, runs.Count);
            foreach (var run in runs) Assert.IsNotNull(run.StartedAt);
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
                writeApi.WriteRecord("h2o_feet,location=coyote_creek water_level=1.0 1", WritePrecision.Ns, "b1",
                    "org1");
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
                writeApi.WriteRecord("h2o_feet,location=coyote_creek water_level=1.0 1", WritePrecision.Ns, "b1",
                    "org1");
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

            var request = MockServer.LogEntries.Last();
            StringAssert.StartsWith("influxdb-client-csharp/4.", request.RequestMessage.Headers["User-Agent"].First());
            StringAssert.EndsWith(".0.0", request.RequestMessage.Headers["User-Agent"].First());
        }

        [Test]
        public void TrailingSlashInUrl()
        {
            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .RespondWith(CreateResponse("{}"));

            using (var writeApi = _client.GetWriteApi())
            {
                writeApi.WriteRecord("h2o_feet,location=coyote_creek water_level=1.0 1", WritePrecision.Ns, "b1",
                    "org1");
            }

            var request = MockServer.LogEntries.Last();
            Assert.AreEqual(MockServerUrl + "/api/v2/write?org=org1&bucket=b1&precision=ns",
                request.RequestMessage.AbsoluteUrl);

            _client.Dispose();
            _client = InfluxDBClientFactory.Create(MockServerUrl + "/");

            using (var writeApi = _client.GetWriteApi())
            {
                writeApi.WriteRecord("h2o_feet,location=coyote_creek water_level=1.0 1", WritePrecision.Ns, "b1",
                    "org1");
            }

            request = MockServer.LogEntries.Last();
            Assert.AreEqual(MockServerUrl + "/api/v2/write?org=org1&bucket=b1&precision=ns",
                request.RequestMessage.AbsoluteUrl);

            _client.Dispose();
            _client = InfluxDBClientFactory.Create(new InfluxDBClientOptions.Builder().Url(MockServerUrl)
                .AuthenticateToken("my-token").Build());

            using (var writeApi = _client.GetWriteApi())
            {
                writeApi.WriteRecord("h2o_feet,location=coyote_creek water_level=1.0 1", WritePrecision.Ns, "b1",
                    "org1");
            }

            request = MockServer.LogEntries.Last();
            Assert.AreEqual(MockServerUrl + "/api/v2/write?org=org1&bucket=b1&precision=ns",
                request.RequestMessage.AbsoluteUrl);

            _client.Dispose();
            _client = InfluxDBClientFactory.Create(new InfluxDBClientOptions.Builder().Url(MockServerUrl + "/")
                .AuthenticateToken("my-token").Build());

            using (var writeApi = _client.GetWriteApi())
            {
                writeApi.WriteRecord("h2o_feet,location=coyote_creek water_level=1.0 1", WritePrecision.Ns, "b1",
                    "org1");
            }

            request = MockServer.LogEntries.Last();
            Assert.AreEqual(MockServerUrl + "/api/v2/write?org=org1&bucket=b1&precision=ns",
                request.RequestMessage.AbsoluteUrl);

            Assert.True(MockServer.LogEntries.Any());
            foreach (var logEntry in MockServer.LogEntries)
                StringAssert.StartsWith(MockServerUrl + "/api/v2/", logEntry.RequestMessage.AbsoluteUrl);
        }

        [Test]
        public void ProduceTypedException()
        {
            MockServer
                .Given(Request.Create().UsingGet())
                .RespondWith(CreateErrorResponse("unauthorized", 401));

            var ioe = Assert.ThrowsAsync<UnauthorizedException>(async () =>
                await _client.GetAuthorizationsApi().FindAuthorizationByIdAsync("id"));

            Assert.AreEqual("unauthorized", ioe.Message);
        }

        [Test]
        public void CreateService()
        {
            var service = _client.CreateService<DBRPsService>(typeof(DBRPsService));

            Assert.IsNotNull(service);
            Assert.IsInstanceOf(typeof(DBRPsService), service);
        }

        [Test]
        public async Task RedirectToken()
        {
            _client.Dispose();
            _client = InfluxDBClientFactory.Create(new InfluxDBClientOptions.Builder()
                .Url(MockServerUrl)
                .AuthenticateToken("my-token")
                .AllowRedirects(true)
                .Build());

            var anotherServer = WireMockServer.Start(new WireMockServerSettings
            {
                UseSSL = false
            });

            // redirect to another server
            MockServer
                .Given(Request.Create().UsingGet())
                .RespondWith(Response.Create().WithStatusCode(301).WithHeader("location", anotherServer.Urls[0]));


            // success response
            anotherServer
                .Given(Request.Create().UsingGet())
                .RespondWith(CreateResponse("{\"status\":\"active\"}", "application/json"));

            var authorization = await _client.GetAuthorizationsApi().FindAuthorizationByIdAsync("id");
            Assert.AreEqual(AuthorizationUpdateRequest.StatusEnum.Active, authorization.Status);

            StringAssert.StartsWith("Token my-token",
                MockServer.LogEntries.Last().RequestMessage.Headers["Authorization"].First());
            Assert.False(anotherServer.LogEntries.Last().RequestMessage.Headers.ContainsKey("Authorization"));

            anotherServer.Stop();
        }

        [Test]
        public async Task RedirectCookie()
        {
            _client.Dispose();
            _client = InfluxDBClientFactory.Create(new InfluxDBClientOptions.Builder()
                .Url(MockServerUrl)
                .Authenticate("my-username", "my-password".ToCharArray())
                .AllowRedirects(true)
                .Build());

            var anotherServer = WireMockServer.Start(new WireMockServerSettings
            {
                UseSSL = false
            });

            // auth cookies
            MockServer
                .Given(Request.Create().UsingPost())
                .RespondWith(Response.Create().WithHeader("Set-Cookie", "session=xyz"));

            // redirect to another server
            MockServer
                .Given(Request.Create().UsingGet())
                .RespondWith(Response.Create().WithStatusCode(301).WithHeader("location", anotherServer.Urls[0]));

            // success response
            anotherServer
                .Given(Request.Create().UsingGet())
                .RespondWith(CreateResponse("{\"status\":\"active\"}", "application/json"));

            var authorization = await _client.GetAuthorizationsApi().FindAuthorizationByIdAsync("id");
            Assert.AreEqual(AuthorizationUpdateRequest.StatusEnum.Active, authorization.Status);

            Assert.AreEqual("xyz", MockServer.LogEntries.Last().RequestMessage.Cookies["session"]);
            Assert.AreEqual("xyz", anotherServer.LogEntries.Last().RequestMessage.Cookies["session"]);

            anotherServer.Stop();
        }

        [Test]
        public async Task Anonymous()
        {
            _client.Dispose();
            _client = InfluxDBClientFactory.Create(new InfluxDBClientOptions.Builder()
                .Url(MockServerUrl)
                .Build());

            MockServer
                .Given(Request.Create().UsingGet())
                .RespondWith(CreateResponse("{\"status\":\"active\"}", "application/json"));

            await _client.GetAuthorizationsApi().FindAuthorizationByIdAsync("id");
            var request = MockServer.LogEntries.Last();

            CollectionAssert.DoesNotContain(request.RequestMessage.Headers.Keys, "Authorization");
        }

        [Test]
        public void HttpClientIsDisposed()
        {
            _client.Dispose();
            var apiClientInfo =
                _client.GetType().GetField("_apiClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var apiClient = (ApiClient)apiClientInfo!.GetValue(_client);

            var httpClientInfo =
                apiClient!.RestClient.GetType()
                    .GetProperty("HttpClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var httpClient = (HttpClient)httpClientInfo!.GetValue(apiClient.RestClient);
            var disposedInfo =
                httpClient!.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance);
            var disposed = (bool)disposedInfo!.GetValue(httpClient)!;

            Assert.AreEqual(true, disposed);
        }

        [Test]
        public async Task VersionIsNotCaseSensitive()
        {
            MockServer.Given(Request.Create().WithPath("/ping").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(204)
                    .WithHeader("x-influxdb-version", "2.0.0"));

            Assert.AreEqual("2.0.0", await _client.VersionAsync());
        }

        [Test]
        public async Task CustomCertificateValidationCallback()
        {
            using var mockServerSsl = WireMockServer.Start(new WireMockServerSettings
            {
                UseSSL = true
            });

            var reached = false;

            _client.Dispose();
            _client = InfluxDBClientFactory.Create(new InfluxDBClientOptions.Builder()
                .Url(mockServerSsl.Urls[0])
                .RemoteCertificateValidationCallback((sender, certificate, chain, errors) => reached = true)
                .Build());

            mockServerSsl.Given(Request.Create().WithPath("/ping").UsingGet())
                .RespondWith(Response.Create().WithStatusCode(204)
                    .WithHeader("x-influxdb-version", "2.0.0"));

            await _client.VersionAsync();

            Assert.IsTrue(reached);
        }
    }
}