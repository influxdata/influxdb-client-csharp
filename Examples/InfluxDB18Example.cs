using System;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Writes;

namespace Examples
{
    public class InfluxDB18Example
    {
        public static async Task Main(string[] args)
        {
            const string database = "telegraf";
            const string retentionPolicy = "autogen";

            var client = InfluxDBClientFactory.CreateV1("http://localhost:8086",
                "username",
                "password".ToCharArray(),
                database,
                retentionPolicy);

            Console.WriteLine("*** Write Points ***");

            using (var writeApi = client.GetWriteApi())
            {
                var point = PointData.Measurement("mem")
                    .Tag("host", "host1")
                    .Field("used_percent", 28.43234543);

                writeApi.WritePoint(point);
            }

            Console.WriteLine("*** Query Points ***");

            var query = $"from(bucket: \"{database}/{retentionPolicy}\") |> range(start: -1h)";
            var fluxTables = await client.GetQueryApi().QueryAsync(query);
            var fluxRecords = fluxTables[0].Records;
            fluxRecords.ForEach(record =>
            {
                Console.WriteLine(
                    $"{record.GetTime()} {record.GetMeasurement()}: {record.GetField()} {record.GetValue()}");
            });

            client.Dispose();
        }
    }
}