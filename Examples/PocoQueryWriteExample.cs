using System;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;

namespace Examples
{
    public static class PocoQueryWriteExample
    {
        /// <summary>
        /// Define Domain Object
        /// </summary>
        [Measurement("iot_sensor")]
        private class Sensor
        {
            [Column("type", IsTag = true)] public string Type { get; set; }
            [Column("device", IsTag = true)] public string Device { get; set; }

            [Column("humidity")] public double Humidity { get; set; }
            [Column("pressure")] public double Pressure { get; set; }
            [Column("temperature")] public double Temperature { get; set; }

            [Column(IsTimestamp = true)] public DateTime Time { get; set; }

            public override string ToString()
            {
                return
                    $"{Time:MM/dd/yyyy hh:mm:ss.fff tt} {Device}-{Type} humidity: {Humidity}, pressure: {Pressure}, temperature: {Temperature}";
            }
        }

        public static async Task Main(string[] args)
        {
            //
            // Initialize Client
            //
            const string host = "http://localhost:9999";
            const string token = "my-token";
            const string bucket = "my-bucket";
            const string organization = "my-org";
            var options = new InfluxDBClientOptions.Builder()
                .Url(host)
                .AuthenticateToken(token.ToCharArray())
                .Org(organization)
                .Bucket(bucket)
                .Build();
            var client = InfluxDBClientFactory.Create(options);

            //
            // Prepare data to write
            //
            var time = new DateTime(2020, 11, 15, 8, 20, 15, DateTimeKind.Utc);
            var sensorTime1 = new Sensor
            {
                Type = "bme280",
                Device = "raspberrypi",
                Humidity = 54.05,
                Pressure = 991.62,
                Temperature = 15.22,
                Time = time.Subtract(TimeSpan.FromHours(1))
            };

            var sensorTime2 = new Sensor
            {
                Type = "bme280",
                Device = "raspberrypi",
                Humidity = 54.01,
                Pressure = 991.56,
                Temperature = 17.82,
                Time = time
            };

            //
            // Write data
            //
            await client.GetWriteApiAsync()
                .WriteMeasurementsAsync(new[] { sensorTime1, sensorTime2 }, WritePrecision.S);

            //
            // Query Data to Domain object
            //
            var query = $"from(bucket:\"{bucket}\") " +
                        "|> range(start: 0) " +
                        "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";

            var list = await client.GetQueryApi().QueryAsync<Sensor>(query);
            //or as an alternative:
            // var list = await client.GetQueryApi().QueryAsync(query, typeof(Sensor));

            //
            // Print result
            //
            list.ForEach(it => Console.WriteLine(it.ToString()));

            client.Dispose();
        }
    }
}