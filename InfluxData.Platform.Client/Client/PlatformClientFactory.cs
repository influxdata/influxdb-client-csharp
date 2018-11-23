using InfluxData.Platform.Client.Option;
using Platform.Common.Platform;

namespace InfluxData.Platform.Client.Client
{
    public static class PlatformClientFactory
    {
        /// <summary>
        /// Create a instance of the Platform client.
        /// </summary>
        /// <param name="url">the url to connect to the Platform</param>
        /// <returns>client</returns>
        public static PlatformClient Create(string url)
        {
            PlatformOptions options = PlatformOptions.Builder
                .CreateNew()
                .Url(url)
                .Build();

            return Create(options);
        }

        /// <summary>
        /// Create a instance of the Platform client.
        /// </summary>
        /// <param name="url">the url to connect to the Platform</param>
        /// <param name="username">the username to use in the basic auth</param>
        /// <param name="password">the password to use in the basic auth</param>
        /// <returns>client</returns>
        public static PlatformClient Create(string url, string username, char[] password)
        {
            PlatformOptions options = PlatformOptions.Builder
                .CreateNew()
                .Url(url)
                .Authenticate(username, password)
                .Build();

            return Create(options);
        }

        /// <summary>
        /// Create a instance of the Platform client.
        /// </summary>
        /// <param name="url">the url to connect to the Platform</param>
        /// <param name="token">the token to use for the authorization</param>
        /// <returns>client</returns>
        public static PlatformClient Create(string url, char[] token)
        {
            PlatformOptions options = PlatformOptions.Builder
                .CreateNew()
                .Url(url)
                .AuthenticateToken(token)
                .Build();

            return Create(options);
        }

        /// <summary>
        /// Create a instance of the Platform client.
        /// </summary>
        /// <param name="options">the connection configuration</param>
        /// <returns>client</returns>
        public static PlatformClient Create(PlatformOptions options)
        {
            Arguments.CheckNotNull(options, "PlatformOptions");

            return new PlatformClient(options);
        }
    }
}