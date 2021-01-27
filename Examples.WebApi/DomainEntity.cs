using System;
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
            };

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

            return point;
        }

        public MemberType ResolveMemberType(MemberInfo memberInfo)
        {
            return memberInfo.Name switch
            {
                "Timestamp" => MemberType.Timestamp,
                "SeriesId" => MemberType.Tag,
                _ => MemberType.Field
            };
        }

        public string GetColumnName(MemberInfo memberInfo)
        {
            return memberInfo.Name switch
            {
                "SeriesId" => "series_id",
                "Value" => "data",
                _ => memberInfo.Name
            };
        }

        public string GetNamedFieldName(MemberInfo memberInfo, object value)
        {
            throw new NotImplementedException();
        }
    }
}