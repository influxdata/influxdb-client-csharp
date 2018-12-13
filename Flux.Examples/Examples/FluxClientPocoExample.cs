using System;
using Flux.Client;
using Flux.Client.Options;

namespace Flux.Examples.Examples
{
    public static class FluxClientPocoExample
    {
        public static void Run()
        {
            var options = new FluxConnectionOptions("http://127.0.0.1:8086");

            var fluxClient = FluxClientFactory.Create(options);

            var fluxQuery = "from(bucket: \"telegraf\")\n"
                               + " |> filter(fn: (r) => (r[\"_measurement\"] == \"cpu\" AND r[\"_field\"] == \"usage_system\"))"
                               + " |> range(start: -1d)"
                               + " |> sample(n: 5, pos: 1)";

            ////Example of additional result stream processing on client side
            fluxClient.Query<Cpu>(fluxQuery,
                            (cancellable, cpu) =>
                            {
                                // process the flux query records
                                Console.WriteLine(cpu.ToString());
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