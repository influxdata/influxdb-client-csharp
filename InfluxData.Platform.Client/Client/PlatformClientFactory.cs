using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
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
            var options = PlatformOptions.Builder
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
            var options = PlatformOptions.Builder
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
            var options = PlatformOptions.Builder
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
        
        /// <summary>
        /// Post onboarding request, to setup initial user, org and bucket.
        /// </summary>
        /// <param name="url">the url to connect to the InfluxDB</param>
        /// <param name="username">the name of an user</param>
        /// <param name="password">the password to connect as an user</param>
        /// <param name="org">the name of an organization</param>
        /// <param name="bucket">the name of a bucket</param>
        /// <returns>Created default user, bucket, org.</returns>
        public static Task<OnboardingResponse> Onboarding(string url, string username, string password, string org, 
            string bucket)
        {
            Arguments.CheckNonEmptyString(url, nameof(url));
            Arguments.CheckNonEmptyString(username, nameof(username));
            Arguments.CheckNonEmptyString(password, nameof(password));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));

            var onboarding = new Onboarding {Username = username, Password = password, Org = org, Bucket = bucket};

            return Onboarding(url, onboarding);
        }

        /// <summary>
        /// Post onboarding request, to setup initial user, org and bucket.
        /// </summary>
        /// <param name="url">the url to connect to the InfluxDB</param>
        /// <param name="onboarding">the defaults</param>
        /// <returns>Created default user, bucket, org.</returns>
        public static Task<OnboardingResponse> Onboarding(string url, Onboarding onboarding)
        {
            Arguments.CheckNonEmptyString(url, nameof(url));
            Arguments.CheckNotNull(onboarding, nameof(onboarding));


            using (var platformClient = new PlatformClient(PlatformOptions.Builder.CreateNew().Url(url).Build()))
            {
                return platformClient.Onboarding(onboarding);
            }
        }
    }
}