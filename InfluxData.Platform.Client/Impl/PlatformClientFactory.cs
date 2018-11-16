using InfluxData.Platform.Client.Option;
using Platform.Common.Platform;

namespace InfluxData.Platform.Client.Impl
{
    public class PlatformClientFactory
    {
        /**
         * Create a instance of the Platform client.
         *
         * @param url      the url to connect to the Platform
         * @return client
         * @see PlatformOptions.Builder#url(string)
         */
        public static PlatformClient Create(string url) 
        {
            PlatformOptions options = PlatformOptions.Builder
                            .CreateNew()
                            .Url(url)
                            .Build();

            return Create(options);
        }
        
        /**
         * Create a instance of the Platform client.
         *
         * @param url      the url to connect to the Platform
         * @param username the username to use in the basic auth
         * @param password the password to use in the basic auth
         * @return client
         * @see PlatformOptions.Builder#url(String)
         */
        public static PlatformClient Create(string url,
                        string username,
                        char[] password) 
        {
            PlatformOptions options = PlatformOptions.Builder
                            .CreateNew()
                            .Url(url)
                            .Authenticate(username, password)
                            .Build();

            return Create(options);
        }

        /**
         * Create a instance of the Platform client.
         *
         * @param url      the url to connect to the Platform
         * @param token    the token to use for the authorization
         * @return client
         * @see PlatformOptions.Builder#url(String)
         */
        public static PlatformClient Create(string url,
                        char[] token) 
        {
            PlatformOptions options = PlatformOptions.Builder
                            .CreateNew()
                            .Url(url)
                            .AuthenticateToken(token)
                            .Build();

            return Create(options);
        }

        /**
         * Create a instance of the Platform client.
         *
         * @param options the connection configuration
         * @return client
         */
        public static PlatformClient Create(PlatformOptions options) 
        {
            Arguments.CheckNotNull(options, "PlatformOptions");

            return new PlatformClient(options);
        }
    }
}