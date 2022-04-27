using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;

namespace Examples
{
    /// <summary>
    /// Warning: Invokable Scripts are supported only in InfluxDB Cloud,
    /// currently there is no support in InfluxDB OSS.
    /// </summary>
    public static class InvokableScripts
    {
        public static async Task Main(string[] args)
        {
            const string host = "https://us-west-2-1.aws.cloud2.influxdata.com";
            const string token = "my-token";
            const string bucket = "my-bucket";
            const string organization = "my-org";

            var options = new InfluxDBClientOptions.Builder()
                .Url(host)
                .AuthenticateToken(token.ToCharArray())
                .Org(organization)
                .Bucket(bucket)
                .Build();

            using var client = InfluxDBClientFactory.Create(options);
            client.SetLogLevel(LogLevel.Body);

            //
            // Prepare data
            //
            var point1 = PointData.Measurement("my_measurement")
                .Tag("location", "Prague")
                .Field("temperature", 25.3);
            var point2 = PointData.Measurement("my_measurement")
                .Tag("location", "New York")
                .Field("temperature", 24.3);

            await client.GetWriteApiAsync().WritePointsAsync(new[] { point1, point2 });

            var scriptsApi = client.GetInvokableScriptsApi();

            //
            // Create Invokable Script
            //
            Console.WriteLine("------- Create -------\n");
            const string scriptQuery = "from(bucket: params.bucket_name) |> range(start: -6h) |> limit(n:2)";
            var createRequest = new ScriptCreateRequest(
                $"my_script_{DateTime.Now.Ticks}",
                "my first try",
                scriptQuery,
                ScriptLanguage.Flux);

            var createdScript = await scriptsApi.CreateScriptAsync(createRequest);
            Console.WriteLine(createdScript);

            //
            // Update Invokable Script
            //
            Console.WriteLine("------- Update -------\n");
            var updateRequest = new ScriptUpdateRequest(description: "my updated description");
            createdScript = await scriptsApi.UpdateScriptAsync(createdScript.Id, updateRequest);
            Console.WriteLine(createdScript);

            //
            // Invoke a script
            //
            var bindParams = new Dictionary<string, object>
            {
                { "bucket_name", bucket }
            };
            // FluxTables
            Console.WriteLine("\n------- Invoke to FluxTables -------\n");
            var tables = await scriptsApi.InvokeScriptAsync(createdScript.Id, bindParams);
            foreach (var record in tables.SelectMany(table => table.Records))
                Console.WriteLine(
                    $"{record.GetValueByKey("_time")} {record.GetValueByKey("location")}: {record.GetField()} {record.GetValue()}");

            // Stream of FluxRecords
            Console.WriteLine("\n------- Invoke to Stream of FluxRecords -------\n");
            var records = scriptsApi.InvokeScriptEnumerableAsync(createdScript.Id, bindParams);
            await foreach (var record in records)
                Console.WriteLine(
                    $"{record.GetValueByKey("_time")} {record.GetValueByKey("location")}: {record.GetField()} {record.GetValue()}");

            // RAW
            Console.WriteLine("\n------- Invoke to Raw-------\n");
            var raw = await scriptsApi.InvokeScriptRawAsync(createdScript.Id, bindParams);
            Console.WriteLine($"RAW output:\n {raw}");

            // Measurements
            Console.WriteLine("\n------- Invoke to Measurements -------\n");
            var measurements =
                await scriptsApi.InvokeScriptMeasurementsAsync<InvokableScriptPojo>(createdScript.Id, bindParams);
            foreach (var measurement in measurements) Console.WriteLine($"{measurement}");

            // Invoke to Stream of Measurements
            Console.WriteLine("\n------- Invoke to Stream of Measurements -------\n");
            var measurementsStream =
                scriptsApi.InvokeScriptMeasurementsEnumerableAsync<InvokableScriptPojo>(createdScript.Id, bindParams);
            await foreach (var measurement in measurementsStream) Console.WriteLine($"{measurement}");

            //
            // List scripts
            //
            Console.WriteLine("\n------- List -------\n");
            var scripts = await scriptsApi.FindScriptsAsync();
            foreach (var script in scripts)
                Console.WriteLine($" ---\n ID: {script.Id}\n Name: {script.Name}\n Description: {script.Description}");
            Console.WriteLine("---");

            //
            // Delete previously created Script
            //
            Console.WriteLine("------- Delete -------\n");
            await scriptsApi.DeleteScriptAsync(createdScript.Id);
            Console.WriteLine($"Successfully deleted script: '{createdScript.Name}'");
        }

        private class InvokableScriptPojo
        {
            [Column("location", IsTag = true)] public string Location { get; set; }

            [Column("value")] public string Value { get; set; }

            [Column("_time")] public string Time { get; set; }

            public override string ToString()
            {
                return $"{Time:MM/dd/yyyy hh:mm:ss.fff tt} {Location} value: {Value}";
            }
        }
    }
}