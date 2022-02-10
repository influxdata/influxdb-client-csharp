using System;
using System.Threading.Tasks;
using InfluxDB.Client.Flux;

namespace Examples
{
    public static class FluxExample
    {
        public static async Task Main(string[] args)
        {
            using var fluxClient = FluxClientFactory.Create("http://localhost:8086/");

            var fluxQuery = "from(bucket: \"telegraf\")\n"
                            + " |> filter(fn: (r) => (r[\"_measurement\"] == \"cpu\" AND r[\"_field\"] == \"usage_system\"))"
                            + " |> range(start: -1d)"
                            + " |> sample(n: 5, pos: 1)";

            await fluxClient.QueryAsync(fluxQuery, record =>
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