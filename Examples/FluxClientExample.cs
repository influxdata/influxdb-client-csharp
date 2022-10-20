using System;
using System.Threading.Tasks;
using InfluxDB.Client.Flux;

namespace Examples
{
    public static class FluxClientExample
    {
        public static async Task Main()
        {
            var options = new FluxConnectionOptions("http://127.0.0.1:8086");

            using var client = new FluxClient(options);

            var fluxQuery = "from(bucket: \"telegraf\")\n"
                            + " |> filter(fn: (r) => (r[\"_measurement\"] == \"cpu\" AND r[\"_field\"] == \"usage_system\"))"
                            + " |> range(start: -1d)"
                            + " |> sample(n: 5, pos: 1)";

            await client.QueryAsync(fluxQuery, record =>
                {
                    // process the flux query records
                    Console.WriteLine(record.GetTime() + ": " + record.GetValue());
                },
                (error) =>
                {
                    // error handling while processing result
                    Console.WriteLine(error.ToString());
                }, () =>
                {
                    // on complete
                    Console.WriteLine("Query completed");
                });
        }
    }
}