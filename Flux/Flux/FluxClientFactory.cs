using Flux.Flux.Options;

namespace Flux.Flux
{
/**
 * The Factory that create a instance of a Flux client.
 */
    public class FluxClientFactory
    {
        /**
         * Create a instance of the Flux client.
         *
         * @param options the connection configuration
         * @return client
         */
        public static FluxClient Connect(FluxConnectionOptions options)
        {
            return new FluxClient(options);
        }
    }
}