using System;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

namespace InfluxDB.Client.Core.Test
{
    public class AbstractTest
    {
        private static readonly TraceListener ConsoleOutListener = new TextWriterTraceListener(Console.Out);
        private static readonly int DefaultWait = 10;
        private static readonly int DefaultInfluxDBSleep = 100;

        protected CountdownEvent CountdownEvent;

        [SetUp]
        public void SetUp()
        {
            CountdownEvent = new CountdownEvent(1);

            if (!Trace.Listeners.Contains(ConsoleOutListener))
            {
                Trace.Listeners.Add(ConsoleOutListener);
            }
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
            var influxDbIp = GetOrDefaultEnvironmentVariable("INFLUXDB_IP", "localhost");
            var influxDbPort = GetOrDefaultEnvironmentVariable("INFLUXDB_PORT_API", "8086");

            return "http://" + influxDbIp + ":" + influxDbPort;
        }

        protected string GetInfluxDb2Url()
        {
            var influxDbIp = GetInfluxDb2Ip();
            var influxDbPort = GetOrDefaultEnvironmentVariable("INFLUXDB_2_PORT", "9999");

            return "http://" + influxDbIp + ":" + influxDbPort;
        }

        protected string GetInfluxDb2Ip()
        {
            return GetOrDefaultEnvironmentVariable("INFLUXDB_2_IP", "localhost");
        }

        protected string GetOrDefaultEnvironmentVariable(string variable, string def)
        {
            var value = Environment.GetEnvironmentVariable(variable);

            if (string.IsNullOrEmpty(value))
            {
                return def;
            }

            return value;
        }

        protected async Task InfluxDbWrite(string lineProtocol, string databaseName)
        {
            var request = new HttpRequestMessage(new HttpMethod("POST"),
                "/write?db=" + databaseName);

            request.Headers.Add("accept", "application/json");
            request.Content = new StringContent(lineProtocol, Encoding.UTF8, "text/plain");

            await InfluxDbRequest(request);
        }

        protected async Task InfluxDbQuery(string query, string databaseName)
        {
            var request = new HttpRequestMessage(new HttpMethod("GET"),
                "/query?db=" + databaseName + ";q=" + query);

            request.Headers.Add("accept", "application/json");

            await InfluxDbRequest(request);
        }

        private async Task InfluxDbRequest(HttpRequestMessage request)
        {
            Assert.IsNotNull(request);

            var httpClient = new HttpClient();

            httpClient.BaseAddress = new Uri(GetInfluxDbUrl());

            try
            {
                var response = await httpClient.SendAsync(request);
                Assert.IsTrue(response.IsSuccessStatusCode, $"Failed to make HTTP request: {response.ReasonPhrase}");

                Thread.Sleep(DefaultInfluxDBSleep);
            }
            catch (Exception e)
            {
                Assert.Fail("Unexpected exception: " + e);
            }
        }
    }
}