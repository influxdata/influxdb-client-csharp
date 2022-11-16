using System;

namespace InfluxDB.Client.Flux
{
    /// <summary>
    /// The Factory that create a instance of a Flux client.
    /// </summary>
    public class FluxClientFactory
    {
        /// <summary>
        /// Create a instance of the Flux client.
        /// </summary>
        /// <param name="connectionString">the connectionString to connect to InfluxDB</param>
        /// <returns>client</returns>
        /// <remarks>Deprecated - please use use object initializer <see cref="FluxClient(string)"/></remarks>
        [Obsolete("This method is deprecated. Call 'FluxClient' initializer instead.", false)]
        public static FluxClient Create(string connectionString)
        {
            var options = new FluxConnectionOptions(connectionString);

            return Create(options);
        }

        /// <summary>
        /// Create a instance of the Flux client.
        /// </summary>
        /// <param name="options">the connection configuration</param>
        /// <returns></returns>
        /// <remarks>Deprecated - please use use object initializer <see cref="FluxClient(FluxConnectionOptions)"/></remarks>
        [Obsolete("This method is deprecated. Call 'FluxClient' initializer instead.", false)]
        public static FluxClient Create(FluxConnectionOptions options)
        {
            return new FluxClient(options);
        }
    }
}