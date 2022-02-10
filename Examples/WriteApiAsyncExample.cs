using System;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;

namespace Examples
{
    public static class WriteApiAsyncExample
    {
        [Measurement("temperature")]
        private class Temperature
        {
            [Column("location", IsTag = true)] public string Location { get; set; }

            [Column("value")] public double Value { get; set; }

            [Column(IsTimestamp = true)] public DateTime Time { get; set; }
        }

        public static async Task Main(string[] args)
        {
            var influxDbClient = InfluxDBClientFactory.Create("http://localhost:9999",
                "my-user", "my-password".ToCharArray());

            //
            // Write Data
            //
            var writeApiAsync = influxDbClient.GetWriteApiAsync();

            //
            //
            // Write by LineProtocol
            //
            await writeApiAsync.WriteRecordAsync("my-bucket", "my-org", WritePrecision.Ns,
                "temperature,location=north value=60.0");

            //
            //
            // Write by Data Point
            //               
            var point = PointData.Measurement("temperature")
                .Tag("location", "west")
                .Field("value", 55D)
                .Timestamp(DateTime.UtcNow.AddSeconds(-10), WritePrecision.Ns);

            await writeApiAsync.WritePointAsync("my-bucket", "my-org", point);

            //
            // Write by POCO
            //
            var temperature = new Temperature { Location = "south", Value = 62D, Time = DateTime.UtcNow };

            await writeApiAsync.WriteMeasurementAsync("my-bucket", "my-org", WritePrecision.Ns, temperature);

            //
            // Check written data
            //
            var tables = await influxDbClient.GetQueryApi()
                .QueryAsync("from(bucket:\"my-bucket\") |> range(start: 0)", "my-org");

            tables.ForEach(table =>
            {
                var fluxRecords = table.Records;
                fluxRecords.ForEach(record => { Console.WriteLine($"{record.GetTime()}: {record.GetValue()}"); });
            });

            influxDbClient.Dispose();
        }
    }
}