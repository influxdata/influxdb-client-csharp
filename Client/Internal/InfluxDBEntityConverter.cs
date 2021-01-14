using System.Reflection;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Internal;
using InfluxDB.Client.Writes;

namespace InfluxDB.Client.Internal
{
    internal class InfluxDBEntityConverter : IInfluxDBEntityConverter
    {
        private static readonly FluxResultMapper ResultMapper = new FluxResultMapper();
        private static readonly MeasurementMapper MeasurementMapper = new MeasurementMapper();

        public T ConvertToEntity<T>(FluxRecord fluxRecord)
        {
            return ResultMapper.ToPoco<T>(fluxRecord);
        }

        public bool IsTimestamp(PropertyInfo propertyInfo)
        {
            return ResultMapper.IsTimestamp(propertyInfo);
        }

        public string GetColumnName(PropertyInfo propertyInfo)
        {
            return ResultMapper.GetColumnName(propertyInfo);
        }

        public PointData ConvertToPointData<T>(T entity, WritePrecision precision)
        {
            return MeasurementMapper.ToPoint(entity, precision);
        }
    }
}