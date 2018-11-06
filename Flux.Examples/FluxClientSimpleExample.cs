using System;
using System.Collections.Generic;
using Flux.Client;
using Flux.Client.Options;
using Platform.Common.Flux.Domain;

namespace Flux.Examples
{
    public static class FluxClientSimpleExample
    {
        public static void Run()
        {
            Console.WriteLine("Start");

            var options = new FluxConnectionOptions("http://127.0.0.1:8086", TimeSpan.FromSeconds(20));
            var client = FluxClientFactory.Create(options);

            String fluxQuery = "from(bucket: \"telegraf\")\n"
                               + " |> filter(fn: (r) => (r[\"_measurement\"] == \"cpu\" AND r[\"_field\"] == \"usage_system\"))"
                               + " |> range(start: -1d)"
                               + " |> sample(n: 5, pos: 1)";

            List<FluxTable> tables = client.Query(fluxQuery).GetAwaiter().GetResult();
            
            if (tables != null)
            {
                foreach (FluxTable fluxTable in tables)
                {
                    foreach (FluxRecord fluxRecord in fluxTable.Records)
                    {
                        Console.WriteLine(fluxRecord.GetTime() + ": " + fluxRecord.GetValueByKey("_value"));
                    }
                }
            }
        }
    }
}