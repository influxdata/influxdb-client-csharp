using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Writes;
using RestSharp;

namespace Examples
{
    public class HttpErrorHandling
    {
        private static InfluxDBClient _client;
        private static List<string> _lpRecords;

        private static void Setup()
        {
            _client = new InfluxDBClient("http://localhost:9999",
                "my-user", "my-password");
            var nowMillis = DateTimeOffset.Now.ToUnixTimeMilliseconds();

            _lpRecords = new List<string>()
            {
                $"temperature,location=north value=42 {nowMillis}",
                $"temperature,location=north value=17 {nowMillis - 10000}",
                $"temperature,location=north value= {nowMillis - 20000}", // one flaky record 
                $"temperature,location=north value=5 {nowMillis - 30000}"
            };
        }

        private static void TearDown()
        {
            _client.Dispose();
        }

        private static Dictionary<string, string> Headers2Dictionary(IEnumerable<HeaderParameter> headers)
        {
            var result = new Dictionary<string, string>();
            foreach (var hp in headers)
                result.Add(hp.Name, hp.Value);
            return result;
        }

        private static async Task WriteRecordsAsync()
        {
            Console.WriteLine("Write records async with one invalid record.");

            //
            // Write Data
            //
            var writeApiAsync = _client.GetWriteApiAsync();

            try
            {
                await writeApiAsync.WriteRecordsAsync(_lpRecords, WritePrecision.Ms,
                    "my-bucket", "my-org");
            }
            catch (HttpException he)
            {
                Console.WriteLine("   WARNING write failed");
                var headersDix = Headers2Dictionary(he.Headers);
                Console.WriteLine("   Caught Exception({0}) {1} with the following headers:",
                    he.GetType(),
                    he.Message);
                foreach (var key in headersDix.Keys)
                    Console.WriteLine($"      {key}: {headersDix[key]}");
            }
            finally
            {
                Console.WriteLine("   ====================");
            }
        }

        private static void WriteRecordsWithErrorEvent()
        {
            Console.WriteLine("Write records with error event.");

            var caughtErrorCount = 0;
            using (var writeApi = _client.GetWriteApi())
            {
                writeApi.EventHandler += (sender, eventArgs) =>
                {
                    switch (eventArgs)
                    {
                        case WriteErrorEvent wee:
                            Console.WriteLine("   WARNING write failed");
                            Console.WriteLine("   Received event WriteErrorEvent with:");
                            Console.WriteLine("      Status: {0}", ((HttpException)wee.Exception).Status);
                            Console.WriteLine("      Exception: {0}", wee.Exception.GetType());
                            Console.WriteLine("      Message: {0}", wee.Exception.Message);
                            Console.WriteLine("      Headers:");
                            var headersDix = Headers2Dictionary(wee.GetHeaders());
                            foreach (var key in headersDix.Keys)
                                Console.WriteLine($"         {key}: {headersDix[key]}");
                            caughtErrorCount++;
                            break;
                        default:
                            throw new Exception("Should only receive WriteErrorEvent");
                    }
                };
                Console.WriteLine("Trying the records list");
                writeApi.WriteRecords(_lpRecords, WritePrecision.Ms, "my-bucket", "my-org");
                var slept = 0;
                while (caughtErrorCount == 0 && slept < 3001) slept += 1000;
                Thread.Sleep(1000);
                if (slept > 3000)
                {
                    Console.WriteLine("WARN, did not encounter expected error");
                }


                // manually retry the bad record
                Console.WriteLine("Manually retrying the bad record.");
                writeApi.WriteRecord(_lpRecords[2], WritePrecision.Ms, "my-bucket", "my-org");
                slept = 0;
                while (caughtErrorCount == 1 && slept < 3001) slept += 1000;
                Thread.Sleep(1000);
                if (slept > 3000)
                {
                    Console.WriteLine("WARN, did not encounter expected error");
                }
            }
        }

        public static async Task Main()
        {
            Console.WriteLine("Main()");
            Setup();
            await WriteRecordsAsync();
            WriteRecordsWithErrorEvent();
            TearDown();
        }
    }
}