using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Flux.Client.Options;
using NUnit.Framework;

namespace Flux.Tests
{
    public class AbstractTest
    {
        private static readonly int DefaultWait = 10;
        private static readonly int DefaultInfluxDBSleep = 100;

        protected CountdownEvent CountdownEvent;

        [SetUp]
        public void SetUp()
        {
            CountdownEvent = new CountdownEvent(1);
        }

        protected void WaitToCallback()
        {
            WaitToCallback(DefaultWait);
        }

        protected void WaitToCallback(int seconds)
        {
            WaitToCallback(CountdownEvent, seconds);
        }

        protected void WaitToCallback(CountdownEvent countdownEvent, int seconds) 
        {
            try
            {
                Assert.IsTrue(countdownEvent.Wait(seconds * 1000));
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception: " + e);
            }
        }
        
        protected string GetInfluxDbUrl()
        {
            string influxDbIp = GetOrDefaultEnvironmentVariable("INFLUXDB_IP", "127.0.0.1");
            string influxDbPort = GetOrDefaultEnvironmentVariable("INFLUXDB_PORT_API", "8086");

            return "http://" + influxDbIp + ":" + influxDbPort;
        }

        private string GetOrDefaultEnvironmentVariable(string variable, string def)
        {
            string value = Environment.GetEnvironmentVariable(variable);

            if (string.IsNullOrEmpty(value))
            {
                return def;
            }

            return value;
        }

        protected async Task InfluxDbWrite(string lineProtocol, string databaseName)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(HttpMethodKind.Post.Name()),
                            "/write?db=" + databaseName);
                            
            request.Headers.Add("accept", "application/json");
            request.Content = new StringContent(lineProtocol, Encoding.UTF8, "text/plain");

            await InfluxDbRequest(request);
        }

        protected async Task InfluxDbQuery(string query, string databaseName) 
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(HttpMethodKind.Get.Name()),
                            "/query?db=" + databaseName + ";q=" + query);
                            
            request.Headers.Add("accept", "application/json");

            await InfluxDbRequest(request);
        }

        private async Task InfluxDbRequest(HttpRequestMessage request)
        {
            Assert.IsNotNull(request);

            HttpClient httpClient = new HttpClient();
            
            httpClient.BaseAddress = new Uri(GetInfluxDbUrl());

            try
            {
                var response = await httpClient.SendAsync(request);
                Assert.IsTrue(response.IsSuccessStatusCode);
                
                Thread.Sleep(DefaultInfluxDBSleep);
            }
            catch (Exception)
            {
                Assert.Fail("Unexpected exception");
            }
        }
    }
}