using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Test;
using InfluxDB.Client.Writes;
using NUnit.Framework;
using WireMock.RequestBuilders;
using WireMock.ResponseBuilders;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class WriteApiTest : AbstractMockServerTest
    {
        [SetUp]
        public new void SetUp()
        {
            _influxDbClient = InfluxDBClientFactory.Create(MockServerUrl, "token");
            _writeApi = _influxDbClient.GetWriteApi(WriteOptions.CreateNew().RetryInterval(1_000).Build());
        }

        [TearDown]
        public new void ResetServer()
        {
            _influxDbClient.Dispose();
            _writeApi.Dispose();
        }

        private WriteApi _writeApi;
        private InfluxDBClient _influxDbClient;

        private IResponseBuilder CreateResponse(string error, int status)
        {
            return CreateResponse($"{{\"error\":\"{error}\"}}",
                    "application/json")
                .WithHeader("X-Influx-Error", error)
                .WithStatusCode(status);
        }

        internal class EventListener
        {
            private readonly AutoResetEvent _autoResetEvent = new AutoResetEvent(false);

            private readonly List<EventArgs> _events = new List<EventArgs>();

            internal EventListener(WriteApi writeApi)
            {
                writeApi.EventHandler += (sender, args) =>
                {
                    _events.Add(args);
                    _autoResetEvent.Set();
                };
            }

            internal T Get<T>() where T : EventArgs
            {
                if (_events.Count == 0)
                {
                    _autoResetEvent.Reset();

                    var timeout = TimeSpan.FromSeconds(10);

                    var waitOne = _autoResetEvent.WaitOne(timeout);
                    if (!waitOne)
                    {
                        Assert.Fail($"The event {typeof(T)} didn't arrive in {timeout}.");
                    }
                }

                var args = _events[0];
                _events.RemoveAt(0);
                Trace.WriteLine(args);

                if (args is T result)
                {
                    return result;
                }

                throw new InvalidCastException($"{args.GetType().FullName} cannot be cast to {typeof(T).FullName}");
            }

            internal int EventCount()
            {
                return _events.Count;
            }

            internal EventListener WaitToSuccess()
            {
                Get<WriteSuccessEvent>();

                return this;
            }
        }

        [Test]
        public void DisposeCallFromInfluxDbClientToWriteApi()
        {
            var writeApi = _influxDbClient.GetWriteApi();

            Assert.False(writeApi.Disposed);
            _influxDbClient.Dispose();
            Assert.True(writeApi.Disposed);
        }

        [Test]
        public void DisposedClientRemovedFromApis()
        {
            var writeApi = _influxDbClient.GetWriteApi();

            Assert.False(writeApi.Disposed);
            writeApi.Dispose();
            Assert.True(writeApi.Disposed);

            _influxDbClient.Dispose();
            // nothing bad happens
        }

        [Test]
        public void Retry()
        {
            var listener = new EventListener(_writeApi);

            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .InScenario("Retry")
                .WillSetStateTo("Retry Started")
                .RespondWith(CreateResponse("token is temporarily over quota", 429));

            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .InScenario("Retry")
                .WhenStateIs("Retry Started")
                .WillSetStateTo("Retry Finished")
                .RespondWith(CreateResponse("{}"));

            _writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1",
                WritePrecision.Ns, "b1", "org1");

            //
            // First request got Retry exception
            //
            var retriableErrorEvent = listener.Get<WriteRetriableErrorEvent>();
            Assert.AreEqual("token is temporarily over quota", retriableErrorEvent.Exception.Message);
            Assert.AreEqual(429, ((HttpException)retriableErrorEvent.Exception).Status);
            Assert.GreaterOrEqual(retriableErrorEvent.RetryInterval, 1000);
            Assert.LessOrEqual(retriableErrorEvent.RetryInterval, 2000);

            //
            // Second request success
            //
            var writeSuccessEvent = listener.Get<WriteSuccessEvent>();
            Assert.AreEqual("h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1",
                writeSuccessEvent.LineProtocol);

            Assert.AreEqual(2, MockServer.LogEntries.Count());

            Assert.AreEqual($"{MockServer.Urls[0]}/api/v2/write?org=org1&bucket=b1&precision=ns",
                MockServer.LogEntries.ToList()[0].RequestMessage.Url);
            Assert.AreEqual("h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1",
                MockServer.LogEntries.ToList()[0].RequestMessage.Body);

            Assert.AreEqual($"{MockServer.Urls[0]}/api/v2/write?org=org1&bucket=b1&precision=ns",
                MockServer.LogEntries.ToList()[1].RequestMessage.Url);
            Assert.AreEqual("h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1",
                MockServer.LogEntries.ToList()[1].RequestMessage.Body);
        }

        [Test]
        public void RetryNotApplied()
        {
            var listener = new EventListener(_writeApi);

            //
            // 400
            //
            MockServer.Reset();
            MockServer
                .Given(Request.Create().UsingPost())
                .RespondWith(CreateResponse("line protocol poorly formed and no points were written", 400));

            _writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1",
                WritePrecision.Ns, "b1", "org1");
            _writeApi.Flush();

            var error = listener.Get<WriteErrorEvent>();

            Assert.IsNotNull(error);
            Assert.AreEqual(typeof(BadRequestException), error.Exception.GetType());
            Assert.AreEqual("line protocol poorly formed and no points were written", error.Exception.Message);
            Assert.AreEqual(400, ((HttpException)error.Exception).Status);

            //
            // 401
            //
            MockServer.Reset();
            MockServer
                .Given(Request.Create().UsingPost())
                .RespondWith(CreateResponse(
                    "token does not have sufficient permissions to write to this organization and bucket or the organization and bucket do not exist",
                    401));

            _writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1",
                WritePrecision.Ns, "b1", "org1");
            _writeApi.Flush();

            error = listener.Get<WriteErrorEvent>();

            Assert.IsNotNull(error);
            Assert.AreEqual(typeof(UnauthorizedException), error.Exception.GetType());
            Assert.AreEqual(
                "token does not have sufficient permissions to write to this organization and bucket or the organization and bucket do not exist",
                error.Exception.Message);
            Assert.AreEqual(401, ((UnauthorizedException)error.Exception).Status);

            //
            // 403
            //
            MockServer.Reset();
            MockServer
                .Given(Request.Create().UsingPost())
                .RespondWith(CreateResponse("no token was sent and they are required", 403));

            _writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1",
                WritePrecision.Ns, "b1", "org1");
            _writeApi.Flush();

            error = listener.Get<WriteErrorEvent>();

            Assert.IsNotNull(error);
            Assert.AreEqual(typeof(ForbiddenException), error.Exception.GetType());
            Assert.AreEqual("no token was sent and they are required", error.Exception.Message);
            Assert.AreEqual(403, ((ForbiddenException)error.Exception).Status);

            //
            // 413
            //
            MockServer.Reset();
            MockServer
                .Given(Request.Create().UsingPost())
                .RespondWith(CreateResponse(
                    "write has been rejected because the payload is too large. Error message returns max size supported. All data in body was rejected and not written",
                    413));

            _writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1",
                WritePrecision.Ns, "b1", "org1");
            _writeApi.Flush();

            error = listener.Get<WriteErrorEvent>();

            Assert.IsNotNull(error);
            Assert.AreEqual(typeof(RequestEntityTooLargeException), error.Exception.GetType());
            Assert.AreEqual(
                "write has been rejected because the payload is too large. Error message returns max size supported. All data in body was rejected and not written",
                error.Exception.Message);
            Assert.AreEqual(413, ((RequestEntityTooLargeException)error.Exception).Status);
        }

        [Test]
        public void RetryWithRetryAfter()
        {
            var listener = new EventListener(_writeApi);

            MockServer.Reset();

            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .InScenario("RetryWithRetryAfter")
                .WillSetStateTo("RetryWithRetryAfter Started")
                .RespondWith(CreateResponse("token is temporarily over quota", 429).WithHeader("Retry-After", "5"));

            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .InScenario("RetryWithRetryAfter")
                .WhenStateIs("RetryWithRetryAfter Started")
                .WillSetStateTo("RetryWithRetryAfter Finished")
                .RespondWith(CreateResponse("{}"));

            _writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1",
                WritePrecision.Ns, "b1", "org1");

            // Retry response
            listener.Get<WriteRetriableErrorEvent>();
            // Success response
            listener.Get<WriteSuccessEvent>();

            var requests = MockServer.LogEntries.ToList();
            Assert.AreEqual(2, requests.Count);

            var request1 = requests[0].RequestMessage.DateTime;
            var request2 = requests[1].RequestMessage.DateTime;

            var timeSpan = request2 - request1;

            Assert.That(timeSpan, Is.GreaterThanOrEqualTo(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void RetryOnNetworkError()
        {
            MockServer.Stop();
            _writeApi.Dispose();

            var options = WriteOptions.CreateNew()
                .BatchSize(1)
                .MaxRetryDelay(2_000)
                .MaxRetries(3)
                .Build();
            _writeApi = _influxDbClient.GetWriteApi(options);

            var listener = new EventListener(_writeApi);

            _writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1",
                WritePrecision.Ns, "b1", "org1");

            // Three attempts
            listener.Get<WriteRetriableErrorEvent>();
            listener.Get<WriteRetriableErrorEvent>();
            listener.Get<WriteRetriableErrorEvent>();
        }


        [Test]
        public void RetryContainsMessage()
        {
            MockServer.Reset();
            _writeApi.Dispose();

            const string json = "{\"code\":\"too many requests\"," +
                                "\"message\":\"org 04014de4ed590000 has exceeded limited_write plan limit\"}";
            var response = CreateResponse(
                    json,
                    "application/json")
                .WithHeader("Retry-After", "5")
                .WithStatusCode(429);

            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .InScenario("RetryWithRetryAfter")
                .WillSetStateTo("RetryWithRetryAfter Started")
                .RespondWith(response);

            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .InScenario("RetryWithRetryAfter")
                .WhenStateIs("RetryWithRetryAfter Started")
                .WillSetStateTo("RetryWithRetryAfter Finished")
                .RespondWith(CreateResponse("{}"));

            var options = WriteOptions.CreateNew()
                .BatchSize(1)
                .RetryInterval(100)
                .MaxRetries(1)
                .Build();
            _writeApi = _influxDbClient.GetWriteApi(options);

            var listener = new EventListener(_writeApi);

            var writer = new StringWriter();
            Trace.Listeners.Add(new TextWriterTraceListener(writer));

            _writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1",
                WritePrecision.Ns, "b1", "org1");

            // One attempt
            listener.Get<WriteRetriableErrorEvent>();

            const string message = "The retriable error occurred during writing of data. " +
                                   "Reason: 'org 04014de4ed590000 has exceeded limited_write plan limit'. " +
                                   "Retry in: 5s.";
            StringAssert.Contains(message, writer.ToString());
        }

        [Test]
        public void TwiceDispose()
        {
            _writeApi.Dispose();
            _writeApi.Dispose();

            _influxDbClient.Dispose();
            _influxDbClient.Dispose();
        }

        [Test]
        public void WaitToCondition()
        {
            var writer = new StringWriter();
            Trace.Listeners.Add(new TextWriterTraceListener(writer));

            WriteApi.WaitToCondition(() => true, 30000);
            WriteApi.WaitToCondition(() => false, 1);
            WriteApi.WaitToCondition(() => false, 1000);

            StringAssert.Contains("The WriteApi can't be gracefully dispose! - 1ms", writer.ToString());
            StringAssert.DoesNotContain("The WriteApi can't be gracefully dispose! - 30000ms", writer.ToString());
            StringAssert.Contains("The WriteApi can't be gracefully dispose! - 1000ms", writer.ToString());
        }

        [Test]
        public void UserAgentHeader()
        {
            var listener = new EventListener(_writeApi);

            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .RespondWith(CreateResponse("{}"));

            _writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1",
                WritePrecision.Ns, "b1", "org1");

            listener.Get<WriteSuccessEvent>();

            var request = MockServer.LogEntries.Last();
            StringAssert.StartsWith("influxdb-client-csharp/4.", request.RequestMessage.Headers["User-Agent"].First());
            StringAssert.EndsWith(".0.0", request.RequestMessage.Headers["User-Agent"].First());
        }

        [Test]
        public void WriteOptionsDefaults()
        {
            var options = WriteOptions.CreateNew().Build();

            Assert.AreEqual(5_000, options.RetryInterval);
            Assert.AreEqual(5, options.MaxRetries);
            Assert.AreEqual(125_000, options.MaxRetryDelay);
            Assert.AreEqual(2, options.ExponentialBase);
        }

        [Test]
        public void WriteOptionsCustom()
        {
            var options = WriteOptions.CreateNew()
                .RetryInterval(1_250)
                .MaxRetries(25)
                .MaxRetryDelay(1_800_000)
                .ExponentialBase(2)
                .Build();

            Assert.AreEqual(1_250, options.RetryInterval);
            Assert.AreEqual(25, options.MaxRetries);
            Assert.AreEqual(1_800_000, options.MaxRetryDelay);
            Assert.AreEqual(2, options.ExponentialBase);
        }

        [Test]
        public void WriteRuntimeException()
        {
            var listener = new EventListener(_writeApi);

            MockServer
                .Given(Request.Create().WithPath("/api/v2/write").UsingPost())
                .RespondWith(CreateResponse("{}"));

            var measurement = new SimpleModel
            {
                Time = new DateTime(2020, 11, 15, 8, 20, 15),
                Device = "id-1",
                Value = 15
            };
            _writeApi.WriteMeasurement(measurement, WritePrecision.S, "b1", "org1");

            var error = listener.Get<WriteRuntimeExceptionEvent>();

            Assert.IsNotNull(error);
            StringAssert.StartsWith("Timestamps must be specified as UTC", error.Exception.Message);

            Assert.AreEqual(0, MockServer.LogEntries.Count());
        }

        [Test]
        public void RequiredOrgBucketWriteApi()
        {
            _influxDbClient.Dispose();

            var options = InfluxDBClientOptions.Builder
                .CreateNew()
                .Url(MockServerUrl)
                .AuthenticateToken("token")
                .Build();

            _influxDbClient = InfluxDBClientFactory.Create(options);
            _writeApi = _influxDbClient.GetWriteApi(WriteOptions.CreateNew().RetryInterval(1_000).Build());

            var ae = Assert.Throws<ArgumentException>(() =>
                _writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1",
                    bucket: "b1"));
            Assert.NotNull(ae);
            Assert.AreEqual(
                "Expecting a non-empty string for 'org' parameter. Please specify the organization as a method parameter or use default configuration at 'InfluxDBClientOptions.Org'.",
                ae.Message);

            ae = Assert.Throws<ArgumentException>(() =>
                _writeApi.WriteRecord("h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1",
                    org: "org1"));
            Assert.NotNull(ae);
            Assert.AreEqual(
                "Expecting a non-empty string for 'bucket' parameter. Please specify the bucket as a method parameter or use default configuration at 'InfluxDBClientOptions.Bucket'.",
                ae.Message);
        }
    }

    [Measurement("m")]
    public class SimpleModel
    {
        [Column(IsTimestamp = true)] public DateTime Time { get; set; }

        [Column("device", IsTag = true)] public string Device { get; set; }

        [Column("value")] public int Value { get; set; }
    }
}