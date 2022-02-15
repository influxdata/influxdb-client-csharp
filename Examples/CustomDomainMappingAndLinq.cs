using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Linq;
using InfluxDB.Client.Writes;

namespace Examples
{
    public static class CustomDomainMappingAndLinq
    {
        /// <summary>
        /// Define Domain Object
        /// </summary>
        private class DomainEntity
        {
            public Guid SeriesId { get; set; }

            public double Value { get; set; }

            public DateTimeOffset Timestamp { get; set; }

            public ICollection<DomainEntityAttribute> Properties { get; set; }

            public override string ToString()
            {
                return $"{Timestamp:MM/dd/yyyy hh:mm:ss.fff tt} {SeriesId} value: {Value}, " +
                       $"properties: {string.Join(", ", Properties)}.";
            }
        }

        /// <summary>
        /// Attributes of DomainObject
        /// </summary>
        private class DomainEntityAttribute
        {
            public string Name { get; set; }
            public int Value { get; set; }

            public override string ToString()
            {
                return $"{Name}={Value}";
            }
        }


        /// <summary>
        /// Define Custom Domain Object Converter
        /// </summary>
        private class DomainEntityConverter : IDomainObjectMapper, IMemberNameResolver
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
                if (type != typeof(DomainEntity))
                {
                    throw new NotSupportedException($"This converter doesn't supports: {typeof(DomainEntity)}");
                }

                var customEntity = new DomainEntity
                {
                    SeriesId = Guid.Parse(Convert.ToString(fluxRecord.GetValueByKey("series_id"))!),
                    Value = Convert.ToDouble(fluxRecord.GetValueByKey("data")),
                    Timestamp = fluxRecord.GetTime().GetValueOrDefault().ToDateTimeUtc(),
                    Properties = new List<DomainEntityAttribute>()
                };

                foreach (var (key, value) in fluxRecord.Values)
                    if (key.StartsWith("property_"))
                    {
                        var attribute = new DomainEntityAttribute
                        {
                            Name = key.Replace("property_", string.Empty), Value = Convert.ToInt32(value)
                        };

                        customEntity.Properties.Add(attribute);
                    }

                return Convert.ChangeType(customEntity, type);
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

                foreach (var attribute in ce.Properties ?? new List<DomainEntityAttribute>())
                    point = point.Field($"property_{attribute.Name}", attribute.Value);

                Console.WriteLine($"LP: '{point.ToLineProtocol()}'");

                return point;
            }

            /// <summary>
            /// How the Domain Object property is mapped into InfluxDB schema. Is it Timestamp, Tag, ...?
            /// </summary>
            public MemberType ResolveMemberType(MemberInfo memberInfo)
            {
                switch (memberInfo.Name)
                {
                    case "Timestamp":
                        return MemberType.Timestamp;
                    case "Name":
                        return MemberType.NamedField;
                    case "Value":
                        return MemberType.NamedFieldValue;
                    case "SeriesId":
                        return MemberType.Tag;
                    default:
                        return MemberType.Field;
                }
            }

            /// <summary>
            /// How your property is named in InfluxDB.
            /// </summary>
            public string GetColumnName(MemberInfo memberInfo)
            {
                switch (memberInfo.Name)
                {
                    case "SeriesId":
                        return "series_id";
                    case "Value":
                        return "data";
                    default:
                        return memberInfo.Name;
                }
            }

            /// <summary>
            /// Return name for flattened properties.
            /// </summary>
            public string GetNamedFieldName(MemberInfo memberInfo, object value)
            {
                return $"property_{Convert.ToString(value)}";
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

            var entity1 = new DomainEntity
            {
                Timestamp = time,
                SeriesId = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"),
                Value = 15,
                Properties = new List<DomainEntityAttribute>
                {
                    new DomainEntityAttribute
                        { Name = "height", Value = 4 },
                    new DomainEntityAttribute
                        { Name = "width", Value = 110 }
                }
            };
            var entity2 = new DomainEntity
            {
                Timestamp = time.AddHours(1),
                SeriesId = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"),
                Value = 15,
                Properties = new List<DomainEntityAttribute>
                {
                    new DomainEntityAttribute
                        { Name = "height", Value = 5 },
                    new DomainEntityAttribute
                        { Name = "width", Value = 160 }
                }
            };
            var entity3 = new DomainEntity
            {
                Timestamp = time.AddHours(2),
                SeriesId = Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"),
                Value = 15,
                Properties = new List<DomainEntityAttribute>
                {
                    new DomainEntityAttribute
                        { Name = "height", Value = 5 },
                    new DomainEntityAttribute
                        { Name = "width", Value = 110 }
                }
            };
            var entity4 = new DomainEntity
            {
                Timestamp = time.AddHours(3),
                SeriesId = Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"),
                Value = 15,
                Properties = new List<DomainEntityAttribute>
                {
                    new DomainEntityAttribute
                        { Name = "height", Value = 6 },
                    new DomainEntityAttribute
                        { Name = "width", Value = 160 }
                }
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
            var query = from s in InfluxDBQueryable<DomainEntity>.Queryable("my-bucket", "my-org", queryApi, converter)
                select s;
            Console.WriteLine("==== Select ALL ====");
            query.ToList().ForEach(it => Console.WriteLine(it.ToString()));

            //
            // Filter By Tag
            //
            query = from s in InfluxDBQueryable<DomainEntity>.Queryable("my-bucket", "my-org", queryApi, converter)
                where s.SeriesId == Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7")
                select s;
            Console.WriteLine("==== Filter by Tag ====");
            query.ToList().ForEach(it => Console.WriteLine(it.ToString()));

            //
            // Use Take + Skip
            //
            query = (from s in InfluxDBQueryable<DomainEntity>.Queryable("my-bucket", "my-org", queryApi, converter)
                    select s)
                .Take(1)
                .Skip(1);
            Console.WriteLine("==== Use Take + Skip ====");
            query.ToList().ForEach(it => Console.WriteLine(it.ToString()));

            //
            // Use Time Range
            //
            query = from s in InfluxDBQueryable<DomainEntity>.Queryable("my-bucket", "my-org", queryApi, converter)
                where s.Timestamp > time.AddMinutes(30) && s.Timestamp < time.AddHours(3)
                select s;
            Console.WriteLine("==== Use Time Range ====");
            query.ToList().ForEach(it => Console.WriteLine(it.ToString()));

            //
            // Use Any
            //
            query = from s in InfluxDBQueryable<DomainEntity>.Queryable("my-bucket", "my-org", queryApi, converter)
                where s.Properties.Any(a => a.Name == "width" && a.Value == 160)
                select s;
            Console.WriteLine("==== Use Any ====");
            query.ToList().ForEach(it => Console.WriteLine(it.ToString()));

            //
            // Debug Query
            //
            Console.WriteLine("==== Debug LINQ Queryable Flux output ====");
            var influxQuery = ((InfluxDBQueryable<DomainEntity>)query).ToDebugQuery();
            foreach (var statement in influxQuery.Extern.Body)
            {
                var os = statement as OptionStatement;
                var va = os?.Assignment as VariableAssignment;
                var name = va?.Id.Name;
                var value = va?.Init.GetType().GetProperty("Value")?.GetValue(va.Init, null);

                Console.WriteLine($"{name}={value}");
            }

            Console.WriteLine();
            Console.WriteLine(influxQuery._Query);

            client.Dispose();
        }
    }
}