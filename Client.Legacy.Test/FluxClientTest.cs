using System;
using System.Net;
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
            var restClient = GetRestClient(_fluxClient);

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
            
            Assert.AreEqual(webProxy, GetRestClient(fluxClient).Proxy);
        }

        private RestClient GetRestClient(FluxClient fluxClient)
        {
            var restClientInfo = fluxClient.GetType().GetField("RestClient", BindingFlags.NonPublic | BindingFlags.Instance);
            var restClient = (RestClient) restClientInfo?.GetValue(fluxClient);
            return restClient;
        }
    }
}