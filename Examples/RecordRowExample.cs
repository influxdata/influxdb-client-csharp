using System;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;


namespace Examples
{
    public static class RecordRowExample
    {
        public static async Task Main()
        {
            const string url = "http://localhost:9999/";
            const string username = "my-user";
            const string password = "my-password";
            const string bucket = "my-bucket";
            const string org = "my-org";

            using var client = InfluxDBClientFactory.Create(url, username, password.ToCharArray());

            //
            // Get ID of Organization with specified name
            //
            var orgId = (await client.GetOrganizationsApi().FindOrganizationsAsync(org: org)).First().Id;

            //
            // Prepare Data
            //
            var writeApi = client.GetWriteApiAsync();
            for (var i = 1; i <= 5; i++)
                await writeApi.WriteRecordAsync($"point,table=my-table result={i}", WritePrecision.Ns, bucket, org);

            //
            // Query data with pivot
            //
            var queryApi = client.GetQueryApi();
            var fluxQuery = $"from(bucket: \"{bucket}\")\n"
                            + " |> range(start: -1m)"
                            + " |> filter(fn: (r) => (r[\"_measurement\"] == \"point\"))"
                            + " |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";
            var tables = await queryApi.QueryAsync(fluxQuery, orgId);

            //
            // Write data to output
            //
            if (tables != null)
            {
                // using FluxRecord.Values - Dictionary<string,object> - can`t contains duplicity key names
                Console.WriteLine("-------------------------------- FluxRecord.Values -------------------------------");
                foreach (var fluxRecord in tables.SelectMany(fluxTable => fluxTable.Records))
                    Console.WriteLine("{" + string.Join(", ",
                        fluxRecord.Values.Select(kv => kv.Key + ": " + kv.Value).ToArray()) + "}");

                // using FluxRecord.Row - List<KeyValuePair<string, object>> - contains all data
                Console.WriteLine("--------------------------------- FluxRecord.Row ---------------------------------");
                foreach (var fluxRecord in tables.SelectMany(fluxTable => fluxTable.Records))
                {
                    Console.WriteLine("{" + string.Join(", ", fluxRecord.Row) + "}");
                    Console.Write("\n");
                }
            }

            client.Dispose();
        }
    }
}