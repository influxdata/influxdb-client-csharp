using System;
using System.Net;
using System.Net.Http;
using System.Reflection;
using InfluxDB.Client.Flux;
using NUnit.Framework;
using RestSharp;

namespace Client.Legacy.Test
{
    [TestFixture]
    public class FluxClientTest
    {
        private FluxClient _fluxClient;

        [SetUp]
        public void SetUp()
        {
            _fluxClient = FluxClientFactory.Create("http://localhost:8093");
        }

        [Test]
        public void Connect()
        {
            Assert.IsNotNull(_fluxClient);
        }

        [Test]
        public void ProxyDefault()
        {
            var restClient = GetRestClientOptions(_fluxClient);

            Assert.AreEqual(null, restClient?.Proxy);
        }

        [Test]
        public void ProxyDefaultConfigured()
        {
            var webProxy = new WebProxy("my-proxy", 8088);

            var options = new FluxConnectionOptions("http://127.0.0.1:8086",
                TimeSpan.FromSeconds(60),
                webProxy: webProxy);

            var fluxClient = FluxClientFactory.Create(options);

            Assert.AreEqual(webProxy, GetRestClientOptions(fluxClient).Proxy);
        }

        [Test]
        public void HttpClientIsDisposed()
        {
            _fluxClient.Dispose();
            var restClient = GetRestClient(_fluxClient);

            var httpClientInfo =
                restClient.GetType().GetProperty("HttpClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var httpClient = (HttpClient)httpClientInfo!.GetValue(restClient);
            var disposedInfo =
                httpClient!.GetType().GetField("_disposed", BindingFlags.NonPublic | BindingFlags.Instance);
            var disposed = (bool)disposedInfo!.GetValue(httpClient)!;

            Assert.AreEqual(true, disposed);
        }

        private static RestClient GetRestClient(FluxClient fluxClient)
        {
            var restClientInfo =
                fluxClient.GetType().BaseType!.GetField("RestClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var restClient = (RestClient)restClientInfo!.GetValue(fluxClient);
            return restClient;
        }

        private RestClientOptions GetRestClientOptions(FluxClient fluxClient)
        {
            var restClient = GetRestClient(fluxClient);
            var restClientOptionsInfo = restClient!.GetType()
                .GetProperty("Options", BindingFlags.NonPublic | BindingFlags.Instance);
            return (RestClientOptions)restClientOptionsInfo!.GetValue(restClient);
        }
    }
}