using System;
using System.Collections.Generic;
using Flux.Client;
using Flux.Flux.Options;
using Platform.Common.Flux.Domain;

namespace Flux.Examples
{
    public class FluxClientSimpleExample
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Start");

            FluxConnectionOptions options = new FluxConnectionOptions("http://127.0.0.1:8086", TimeSpan.FromSeconds(20));
            FluxClient client = FluxClientFactory.Connect(options);

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