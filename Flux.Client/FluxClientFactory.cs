using Flux.Client.Options;

namespace Flux.Client
{
/**
 * The Factory that create a instance of a Flux client.
 */
    public class FluxClientFactory
    {
        /**
         * Create a instance of the Flux client.
         *
         * @param orgID the organization id required by Flux
         * @param url   the url to connect to Flux.
         * @return client
         * @see FluxConnectionOptions.Builder#orgID(String)
         * @see FluxConnectionOptions.Builder#url(String)
         */
        public static FluxClient Create(string url)
        {
            FluxConnectionOptions options = new FluxConnectionOptions(url);

            return Create(options);
        }

        /**
         * Create a instance of the Flux client.
         *
         * @param options the connection configuration
         * @return client
         */
        public static FluxClient Create(FluxConnectionOptions options)
        {
            return new FluxClient(options);
        }
    }
}