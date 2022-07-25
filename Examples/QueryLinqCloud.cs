using System;
using System.Collections.Generic;
using System.Diagnostics;
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
    public static class QueryLinqCloud
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
            public string Value { get; set; }

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
                            Name = key.Replace("property_", string.Empty), Value = Convert.ToString(value)
                        };

                        customEntity.Properties.Add(attribute);
                    }

                return Convert.ChangeType(customEntity, type);
            }

            public PointData ConvertToPointData<T>(T entity, WritePrecision precision)
            {
                throw new NotImplementedException();
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

        public static void Main(string[] args)
        {
            const string host = "https://us-west-2-1.aws.cloud2.influxdata.com";
            const string token = "...";
            const string bucket = "linq_bucket";
            const string organization = "jakub_bednar";
            var options = new InfluxDBClientOptions.Builder()
                .Url(host)
                .AuthenticateToken(token.ToCharArray())
                .Org(organization)
                .Bucket(bucket)
                .Build();

            var converter = new DomainEntityConverter();
            var client = InfluxDBClientFactory.Create(options)
                .EnableGzip();

            //
            // Query Data to Domain object
            //
            var queryApi = client.GetQueryApiSync(converter);

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            //
            // Select Cloud
            //
            var query = from s in InfluxDBQueryable<DomainEntity>.Queryable(bucket, organization, queryApi, converter)
                select s;

            var entities = query.ToList();

            Console.WriteLine("==== Cloud Query Results ====");
            Console.WriteLine($"> count: {entities.Count}");
            Console.WriteLine($"> first: {entities.First()}");
            Console.WriteLine($"> elapsed seconds: {stopWatch.Elapsed.TotalSeconds}");
            Console.WriteLine();

            //
            // Debug Query
            //
            Console.WriteLine("==== Debug LINQ Queryable Flux output ====");
            var influxQuery = ((InfluxDBQueryable<DomainEntity>)query).ToDebugQuery();
            Console.WriteLine("> variables:");
            foreach (var statement in influxQuery.Extern.Body)
            {
                var os = statement as OptionStatement;
                var va = os?.Assignment as VariableAssignment;
                var name = va?.Id.Name;
                var value = va?.Init.GetType().GetProperty("Value")?.GetValue(va.Init, null);

                Console.WriteLine($"{name}={value}");
            }

            Console.WriteLine();
            Console.WriteLine("> query:");
            Console.WriteLine(influxQuery._Query);

            client.Dispose();
        }
    }
}