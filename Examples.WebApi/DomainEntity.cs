using System;
using System.Collections.Generic;
using System.Reflection;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Linq;
using InfluxDB.Client.Writes;

namespace Examples.WebApi
{
    public class DomainEntity
    {
        public Guid SeriesId { get; set; }

        public double Value { get; set; }

        public DateTimeOffset Timestamp { get; set; }

        public ICollection<DomainEntityAttribute> Attributes { get; set; }

        public override string ToString()
        {
            return $"{Timestamp:MM/dd/yyyy hh:mm:ss.fff tt} {SeriesId} value: {Value}, " +
                   $"properties: {string.Join(", ", Attributes)}.";
        }
    }
    
    public class DomainEntityAttribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
            
        public override string ToString()
        {
            return $"{Name}={Value}";
        }
    }

    public class DomainEntityConverter : IInfluxDBEntityConverter, IMemberNameResolver
    {
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
                Attributes = new List<DomainEntityAttribute>()
            };
            
            foreach (var (key, value) in fluxRecord.Values)
            {
                if (key.StartsWith("property_"))
                {
                    var attribute = new DomainEntityAttribute
                    {
                        Name = key.Replace("property_", string.Empty), Value = Convert.ToString(value)
                    };
                        
                    customEntity.Attributes.Add(attribute);
                }
            }

            return (T) Convert.ChangeType(customEntity, typeof(T));
        }

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
            
            foreach (var attribute in ce.Attributes ?? new List<DomainEntityAttribute>())
            {
                point = point.Field($"property_{attribute.Name}", attribute.Value);
            }

            return point;
        }

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

        public string GetNamedFieldName(MemberInfo memberInfo, object value)
        {
            return $"property_{Convert.ToString(value)}";
        }
    }
}