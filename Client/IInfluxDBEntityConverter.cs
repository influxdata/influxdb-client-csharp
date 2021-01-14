using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Flux.Internal;
using InfluxDB.Client.Writes;

namespace InfluxDB.Client
{
    /// <summary>
    /// An implementation of this class is used to convert POCO entity into <see cref="InfluxDB.Client.Writes.PointData"/>
    /// and <see cref="InfluxDB.Client.Core.Flux.Domain.FluxRecord"/> back to Poco. 
    /// </summary>
    public interface IInfluxDBEntityConverter: IFluxResultMapper
    {
        /// <summary>
        /// Converts entity to corresponding PointData.
        /// </summary>
        /// <param name="entity">Entity to convert</param>
        /// <param name="precision">Required timestamp precision</param>
        /// <typeparam name="T">Type of entity</typeparam>
        /// <returns>Converted DataPoint.</returns>
        PointData ConvertToPointData<T>(T entity, WritePrecision precision);
    }
}