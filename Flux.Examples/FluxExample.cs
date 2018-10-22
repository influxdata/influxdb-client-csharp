using System;
using Flux.Flux;
using Flux.Flux.Options;

namespace Flux.Examples
{
    public class FluxExample
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

            client.Query(fluxQuery).GetAwaiter().GetResult();
        }
    }
}