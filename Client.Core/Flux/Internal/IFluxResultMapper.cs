using System;
using InfluxDB.Client.Core.Flux.Domain;

namespace InfluxDB.Client.Core.Flux.Internal
{
    /// <summary>
    /// Mapper that is used to map FluxRecord into DomainObject.
    /// </summary>
    public interface IFluxResultMapper
    {
        /// <summary>
        /// Converts FluxRecord to DomainObject specified by Type.
        /// </summary>
        /// <param name="fluxRecord">Flux record</param>
        /// <typeparam name="T">Type of DomainObject</typeparam>
        /// <returns>Converted DomainObject</returns>
        T ConvertToEntity<T>(FluxRecord fluxRecord);

        object ConvertToEntity(FluxRecord fluxRecord, Type type);
    }
}