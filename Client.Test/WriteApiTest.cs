using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using InfluxDB.Client.Api.Domain;
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
        private WriteApi _writeApi;
        private InfluxDBClient _influxDbClient;

        [SetUp]
        public new void SetUp()
        {
             _influxDbClient = InfluxDBClientFactory.Create(MockServerUrl, "token".ToCharArray());
            _writeApi = _influxDbClient.GetWriteApi();
        }

        [TearDown]
        public new void ResetServer()
        {
            _influxDbClient.Dispose();
            _writeApi.Dispose();
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

            _writeApi.WriteRecord("b1", "org1", WritePrecision.Ns,
                "h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1");

            //
            // First request got Retry exception
            //
            var retriableErrorEvent = listener.Get<WriteRetryableErrorEvent>();
            Assert.AreEqual("token is temporarily over quota", retriableErrorEvent.Exception.Message);
            Assert.AreEqual(429, ((HttpException) retriableErrorEvent.Exception).Status);
            Assert.AreEqual(1000, retriableErrorEvent.RetryInterval);

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

            _writeApi.WriteRecord("b1", "org1", WritePrecision.Ns,
                "h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1");

            // Retry response
            listener.Get<WriteRetryableErrorEvent>();
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

            _writeApi.WriteRecord("b1", "org1", WritePrecision.Ns,
                "h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1");
            _writeApi.Flush();

            var error = listener.Get<WriteErrorEvent>();

            Assert.IsNotNull(error);
            Assert.AreEqual(typeof(HttpException), error.Exception.GetType());
            Assert.AreEqual("line protocol poorly formed and no points were written", error.Exception.Message);
            Assert.AreEqual(400, ((HttpException) error.Exception).Status);

            //
            // 401
            //
            MockServer.Reset();
            MockServer
                .Given(Request.Create().UsingPost())
                .RespondWith(CreateResponse(
                    "token does not have sufficient permissions to write to this organization and bucket or the organization and bucket do not exist",
                    401));

            _writeApi.WriteRecord("b1", "org1", WritePrecision.Ns,
                "h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1");
            _writeApi.Flush();

            error = listener.Get<WriteErrorEvent>();

            Assert.IsNotNull(error);
            Assert.AreEqual(typeof(HttpException), error.Exception.GetType());
            Assert.AreEqual(
                "token does not have sufficient permissions to write to this organization and bucket or the organization and bucket do not exist",
                error.Exception.Message);
            Assert.AreEqual(401, ((HttpException) error.Exception).Status);

            //
            // 403
            //
            MockServer.Reset();
            MockServer
                .Given(Request.Create().UsingPost())
                .RespondWith(CreateResponse("no token was sent and they are required", 403));

            _writeApi.WriteRecord("b1", "org1", WritePrecision.Ns,
                "h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1");
            _writeApi.Flush();

            error = listener.Get<WriteErrorEvent>();

            Assert.IsNotNull(error);
            Assert.AreEqual(typeof(HttpException), error.Exception.GetType());
            Assert.AreEqual("no token was sent and they are required", error.Exception.Message);
            Assert.AreEqual(403, ((HttpException) error.Exception).Status);

            //
            // 413
            //
            MockServer.Reset();
            MockServer
                .Given(Request.Create().UsingPost())
                .RespondWith(CreateResponse(
                    "write has been rejected because the payload is too large. Error message returns max size supported. All data in body was rejected and not written",
                    413));

            _writeApi.WriteRecord("b1", "org1", WritePrecision.Ns,
                "h2o_feet,location=coyote_creek level\\ description=\"feet 1\",water_level=1.0 1");
            _writeApi.Flush();

            error = listener.Get<WriteErrorEvent>();

            Assert.IsNotNull(error);
            Assert.AreEqual(typeof(HttpException), error.Exception.GetType());
            Assert.AreEqual(
                "write has been rejected because the payload is too large. Error message returns max size supported. All data in body was rejected and not written",
                error.Exception.Message);
            Assert.AreEqual(413, ((HttpException) error.Exception).Status);
        }

        [Test]
        public void TwiceDispose()
        {
            _writeApi.Dispose();
            _writeApi.Dispose();
            
            _influxDbClient.Dispose();
            _influxDbClient.Dispose();
        }
        
        private IResponseBuilder CreateResponse(string error, int status)
        {
            return CreateResponse($"{{\"error\":\"{error}\"}}",
                    "application/json")
                .WithHeader("X-Influx-Error", error)
                .WithStatusCode(status);
        }

        private class EventListener 
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

            internal T Get<T>()
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

                return (T) Convert.ChangeType(args, typeof(T));
            }
        }
    }
}