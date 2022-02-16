using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Flux.Internal;
using InfluxDB.Client.Writes;

namespace InfluxDB.Client
{
    /// <summary>
    /// An implementation of this class is used to convert DomainObject entity into <see cref="InfluxDB.Client.Writes.PointData"/>
    /// and <see cref="InfluxDB.Client.Core.Flux.Domain.FluxRecord"/> back to DomainObject. 
    /// </summary>
    public interface IDomainObjectMapper : IFluxResultMapper
    {
        /// <summary>
        /// Converts DomainObject to corresponding PointData.
        /// </summary>
        /// <param name="entity">DomainObject to convert</param>
        /// <param name="precision">Required timestamp precision</param>
        /// <typeparam name="T">Type of DomainObject</typeparam>
        /// <returns>Converted DataPoint</returns>
        PointData ConvertToPointData<T>(T entity, WritePrecision precision);
    }
}