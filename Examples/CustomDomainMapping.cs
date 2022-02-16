using System;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Writes;

namespace Examples
{
    public static class CustomDomainMapping
    {
        /// <summary>
        /// Define Domain Object
        /// </summary>
        private class Sensor
        {
            /// <summary>
            /// Type of sensor.
            /// </summary>
            public string Type { get; set; }

            /// <summary>
            /// Version of sensor.
            /// </summary>
            public string Version { get; set; }

            /// <summary>
            /// Measured value.
            /// </summary>
            public double Value { get; set; }

            public DateTimeOffset Timestamp { get; set; }

            public override string ToString()
            {
                return $"{Timestamp:MM/dd/yyyy hh:mm:ss.fff tt} {Type}, {Version} value: {Value}";
            }
        }

        /// <summary>
        /// Define Custom Domain Object Converter
        /// </summary>
        private class DomainEntityConverter : IDomainObjectMapper
        {
            /// <summary>
            /// Convert to DomainObject.
            /// </summary>
            public T ConvertToEntity<T>(FluxRecord fluxRecord)
            {
                return (T)ConvertToEntity(fluxRecord, typeof(T));
            }

            public object ConvertToEntity(FluxRecord fluxRecord, Type type)
            {
                if (type != typeof(Sensor))
                {
                    throw new NotSupportedException($"This converter doesn't supports: {type}");
                }

                var customEntity = new Sensor
                {
                    Type = Convert.ToString(fluxRecord.GetValueByKey("type")),
                    Version = Convert.ToString(fluxRecord.GetValueByKey("version")),
                    Value = Convert.ToDouble(fluxRecord.GetValueByKey("data")),
                    Timestamp = fluxRecord.GetTime().GetValueOrDefault().ToDateTimeUtc()
                };
                return Convert.ChangeType(customEntity, type);
            }

            /// <summary>
            /// Convert to Point
            /// </summary>
            public PointData ConvertToPointData<T>(T entity, WritePrecision precision)
            {
                if (!(entity is Sensor sensor))
                {
                    throw new NotSupportedException($"This converter doesn't supports: {entity}");
                }

                var point = PointData
                    .Measurement("sensor")
                    .Tag("type", sensor.Type)
                    .Tag("version", sensor.Version)
                    .Field("data", sensor.Value)
                    .Timestamp(sensor.Timestamp, precision);

                return point;
            }
        }

        public static async Task Main(string[] args)
        {
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

            var converter = new DomainEntityConverter();
            var client = InfluxDBClientFactory.Create(options);

            //
            // Prepare data to write
            //
            var time = new DateTimeOffset(2020, 11, 15, 8, 20, 15,
                new TimeSpan(3, 0, 0));

            var entity1 = new Sensor
            {
                Timestamp = time,
                Type = "temperature",
                Version = "v0.0.2",
                Value = 15
            };
            var entity2 = new Sensor
            {
                Timestamp = time.AddHours(1),
                Type = "temperature",
                Version = "v0.0.2",
                Value = 15
            };
            var entity3 = new Sensor
            {
                Timestamp = time.AddHours(2),
                Type = "humidity",
                Version = "v0.13",
                Value = 74
            };
            var entity4 = new Sensor
            {
                Timestamp = time.AddHours(3),
                Type = "humidity",
                Version = "v0.13",
                Value = 82
            };

            //
            // Write data
            //
            await client.GetWriteApiAsync(converter)
                .WriteMeasurementsAsync(new[] { entity1, entity2, entity3, entity4 }, WritePrecision.S);

            //
            // Query Data to Domain object
            //
            var queryApi = client.GetQueryApiSync(converter);

            //
            // Select ALL
            //
            var query = $"from(bucket:\"{bucket}\") " +
                        "|> range(start: 0) " +
                        "|> filter(fn: (r) => r[\"_measurement\"] == \"sensor\")" +
                        "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";

            var sensors = queryApi.QuerySync<Sensor>(query);
            //
            // Print result
            //
            sensors.ForEach(it => Console.WriteLine(it.ToString()));

            client.Dispose();
        }
    }
}