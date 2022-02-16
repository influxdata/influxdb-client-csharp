using System;
using System.Net;
using InfluxDB.Client.Api.Client;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Internal;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ApiClientTest
    {
        private ApiClient _apiClient;

        [SetUp]
        public void SetUp()
        {
            var options = new InfluxDBClientOptions.Builder()
                .Url("http://localhost:8086")
                .AuthenticateToken("my-token".ToCharArray())
                .Build();

            _apiClient = new ApiClient(options, new LoggingHandler(LogLevel.Body), new GzipHandler());
        }

        [Test]
        public void SerializeDateTime()
        {
            var serialized = _apiClient.Serialize(new DateTime(2022, 1, 1));

            Assert.AreEqual("\"2022-01-01T00:00:00Z\"", serialized);
        }

        [Test]
        public void SerializeUtcDateTime()
        {
            var dateTime = DateTime.Parse("2020-03-05T00:00:00Z");
            var serialized = _apiClient.Serialize(dateTime);

            Assert.AreEqual("\"2020-03-05T00:00:00Z\"", serialized);
        }

        [Test]
        public void ProxyDefault()
        {
            Assert.AreEqual(null, _apiClient.RestClientOptions.Proxy);
        }

        [Test]
        public void ProxyDefaultConfigured()
        {
            var webProxy = new WebProxy("my-proxy", 8088);

            var options = new InfluxDBClientOptions.Builder()
                .Url("http://localhost:8086")
                .AuthenticateToken("my-token".ToCharArray())
                .Proxy(webProxy)
                .Build();

            _apiClient = new ApiClient(options, new LoggingHandler(LogLevel.Body), new GzipHandler());

            Assert.AreEqual(webProxy, _apiClient.RestClientOptions.Proxy);
        }
    }
}