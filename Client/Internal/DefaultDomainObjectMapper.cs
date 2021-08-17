using System;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Internal;
using InfluxDB.Client.Writes;

namespace InfluxDB.Client.Internal
{
    /// <summary>
    /// Default implementation of DomainObject mapper.
    /// </summary>
    internal class DefaultDomainObjectMapper : IDomainObjectMapper
    {
        private readonly FluxResultMapper _resultMapper = new FluxResultMapper();
        private readonly MeasurementMapper _measurementMapper = new MeasurementMapper();

        public T ConvertToEntity<T>(FluxRecord fluxRecord)
        {
            return _resultMapper.ToPoco<T>(fluxRecord);
        }

        public object ConvertToEntity(FluxRecord fluxRecord, Type type)
        {
            return _resultMapper.ToPoco(fluxRecord, type);
        }

        public PointData ConvertToPointData<T>(T entity, WritePrecision precision)
        {
            return _measurementMapper.ToPoint(entity, precision);
        }
    }
}