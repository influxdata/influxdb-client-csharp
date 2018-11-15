using System;
using Flux.Client;

namespace Flux.Examples.Examples
{
    public static class FluxExample
    {
        public static void Run()
        {
            var fluxClient = FluxClientFactory.Create("http://localhost:8086/");

            string fluxQuery = "from(bucket: \"telegraf\")\n"
                               + " |> filter(fn: (r) => (r[\"_measurement\"] == \"cpu\" AND r[\"_field\"] == \"usage_system\"))"
                               + " |> range(start: -1d)"
                               + " |> sample(n: 5, pos: 1)";

            fluxClient.Query(fluxQuery, (cancellable, record) =>
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
                            }).GetAwaiter().GetResult();
        }
    }
}