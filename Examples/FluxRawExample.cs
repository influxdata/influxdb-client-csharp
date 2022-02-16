using System;
using System.Threading.Tasks;
using InfluxDB.Client.Flux;

namespace Examples
{
    public static class FluxRawExample
    {
        public static async Task Main(string[] args)
        {
            using var fluxClient = FluxClientFactory.Create("http://localhost:8086/");

            var fluxQuery = "from(bucket: \"telegraf\")\n"
                            + " |> filter(fn: (r) => (r[\"_measurement\"] == \"cpu\" AND r[\"_field\"] == \"usage_system\"))"
                            + " |> range(start: -1d)"
                            + " |> sample(n: 5, pos: 1)";

            await fluxClient.QueryRawAsync(fluxQuery, line =>
                {
                    // process the flux query result record
                    Console.WriteLine(line);
                },
                onError: error =>
                {
                    // error handling while processing result
                    Console.WriteLine(error.ToString());
                },
                onComplete: () =>
                {
                    // on complete
                    Console.WriteLine("Query completed");
                });
        }
    }
}