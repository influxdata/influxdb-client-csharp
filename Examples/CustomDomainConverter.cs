using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Linq;
using InfluxDB.Client.Writes;

namespace Examples
{
    public static class CustomDomainConverter
    {
        /// <summary>
        /// Define Domain Object
        /// </summary>
        private class DomainEntity
        {
            public Guid SeriesId { get; set; }

            public double Value { get; set; }

            public DateTimeOffset Timestamp { get; set; }

            public Dictionary<string, string> Properties { get; set; }

            public override string ToString()
            {
                return $"{Timestamp:MM/dd/yyyy hh:mm:ss.fff tt} {SeriesId} value: {Value}, " +
                       $"properties: {string.Join(", ", Properties)}.";
            }
        }


        /// <summary>
        /// Define Custom Domain Object Converter
        /// </summary>
        private class DomainEntityConverter : IInfluxDBEntityConverter
        {
            /// <summary>
            /// Convert to DomainObject.
            /// </summary>
            public T ConvertToEntity<T>(FluxRecord fluxRecord)
            {
                if (typeof(T) != typeof(DomainEntity))
                {
                    throw new NotSupportedException($"This converter doesn't supports: {typeof(DomainEntity)}");
                }

                var customEntity = new DomainEntity
                {
                    SeriesId = Guid.Parse(Convert.ToString(fluxRecord.GetValueByKey("series_id"))!),
                    Value = Convert.ToDouble(fluxRecord.GetValueByKey("data")),
                    Timestamp = fluxRecord.GetTime().GetValueOrDefault().ToDateTimeUtc(),
                    Properties = new Dictionary<string, string>()
                };
                
                foreach (var (key, value) in fluxRecord.Values)
                {
                    if (key.StartsWith("property_"))
                    {
                        customEntity.Properties.Add(key.Replace("property_", string.Empty),  
                            Convert.ToString(value));
                    }
                }

                return (T) Convert.ChangeType(customEntity, typeof(T));
            }

            /// <summary>
            /// Convert to Point
            /// </summary>
            public PointData ConvertToPointData<T>(T entity, WritePrecision precision)
            {
                if (!(entity is DomainEntity ce))
                {
                    throw new NotSupportedException($"This converter doesn't supports: {typeof(DomainEntity)}");
                }

                var point = PointData
                    .Measurement("custom_measurement")
                    .Tag("series_id", ce.SeriesId.ToString())
                    .Field("data", ce.Value)
                    .Timestamp(ce.Timestamp, precision);

                foreach (var (key, value) in ce.Properties ?? new Dictionary<string, string>())
                {
                    point = point.Field($"property_{key}", value);
                }

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

            var domainEntity = new DomainEntity
            {
                Timestamp = time,
                SeriesId = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"),
                Value = 15,
                Properties = new Dictionary<string, string> {{"height", "4"}, {"width", "10"}}
            };

            //
            // Write data
            //
            await client.GetWriteApiAsync(converter)
                .WriteMeasurementsAsync(WritePrecision.S, domainEntity);

            //
            // Query Data to Domain object
            //
            var queryApi = client.GetQueryApi(converter);
            var query = from s in InfluxDBQueryable<DomainEntity>.Queryable("my-bucket", "my-org", queryApi)
                select s;
            var list = query.ToList();

            //
            // Print result
            //
            list.ForEach(it => Console.WriteLine(it.ToString()));

            client.Dispose();
        }
    }
}