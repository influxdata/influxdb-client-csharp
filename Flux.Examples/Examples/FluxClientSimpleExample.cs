using System;
using Flux.Client;
using Flux.Client.Options;

namespace Flux.Examples.Examples
{
    public static class FluxClientSimpleExample
    {
        public static void Run()
        {
            Console.WriteLine("Start");

            var options = new FluxConnectionOptions("http://127.0.0.1:8086", TimeSpan.FromSeconds(20));
            var client = FluxClientFactory.Create(options);

            var fluxQuery = "from(bucket: \"telegraf\")\n"
                               + " |> filter(fn: (r) => (r[\"_measurement\"] == \"cpu\" AND r[\"_field\"] == \"usage_system\"))"
                               + " |> range(start: -1d)"
                               + " |> sample(n: 5, pos: 1)";

            var tables = client.Query(fluxQuery).GetAwaiter().GetResult();
            
            if (tables != null)
            {
                foreach (var fluxTable in tables)
                {
                    foreach (var fluxRecord in fluxTable.Records)
                    {
                        Console.WriteLine(fluxRecord.GetTime() + ": " + fluxRecord.GetValueByKey("_value"));
                    }
                }
            }
        }
    }
}