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
        public static FluxClient Create(FluxConnectionOptions options)
        {
            return new FluxClient(options);
        }
    }
}