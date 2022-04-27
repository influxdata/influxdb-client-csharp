using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;

namespace InfluxDB.Client
{
    public static class InfluxDBClientFactory
    {
        /// <summary>
        /// Create a instance of the InfluxDB 2.x client that is configured via <code>App.config</code>.
        /// </summary>
        /// <returns>client</returns>
        public static InfluxDBClient Create()
        {
            var options = InfluxDBClientOptions.Builder
                .CreateNew()
                .LoadConfig()
                .Build();

            return Create(options);
        }

        /// <summary>
        /// Create a instance of the InfluxDB 2.x client. The url could be a connection string with various configurations.
        /// <para>
        /// e.g.: "http://localhost:8086?timeout=5000&amp;logLevel=BASIC
        /// </para>
        /// </summary>
        /// <param name="connectionString">connection string with various configurations</param>
        /// <returns>client</returns>
        public static InfluxDBClient Create(string connectionString)
        {
            var options = InfluxDBClientOptions.Builder
                .CreateNew()
                .ConnectionString(connectionString)
                .Build();

            return Create(options);
        }

        /// <summary>
        /// Create a instance of the InfluxDB 2.x client.
        /// </summary>
        /// <param name="url">the url to connect to the InfluxDB 2.x</param>
        /// <param name="username">the username to use in the basic auth</param>
        /// <param name="password">the password to use in the basic auth</param>
        /// <returns>client</returns>
        public static InfluxDBClient Create(string url, string username, char[] password)
        {
            var options = InfluxDBClientOptions.Builder
                .CreateNew()
                .Url(url)
                .Authenticate(username, password)
                .Build();

            return Create(options);
        }

        /// <summary>
        /// Create a instance of the InfluxDB 2.x client.
        /// </summary>
        /// <param name="url">the url to connect to the InfluxDB 2.x</param>
        /// <param name="token">the token to use for the authorization</param>
        /// <returns>client</returns>
        public static InfluxDBClient Create(string url, char[] token)
        {
            var options = InfluxDBClientOptions.Builder
                .CreateNew()
                .Url(url)
                .AuthenticateToken(token)
                .Build();

            return Create(options);
        }

        /// <summary>
        /// Create a instance of the InfluxDB 2.x client.
        /// </summary>
        /// <param name="url">the url to connect to the InfluxDB 2.x</param>
        /// <param name="token">the token to use for the authorization</param>
        /// <returns>client</returns>
        public static InfluxDBClient Create(string url, string token)
        {
            var options = InfluxDBClientOptions.Builder
                .CreateNew()
                .Url(url)
                .AuthenticateToken(token)
                .Build();

            return Create(options);
        }

        /// <summary>
        /// Create a instance of the InfluxDB 2.x client to connect into InfluxDB 1.8.
        /// </summary>
        /// <param name="url">the url to connect to the InfluxDB 1.8</param>
        /// <param name="username">authorization username</param>
        /// <param name="password">authorization password</param>
        /// <param name="database">database name</param>
        /// <param name="retentionPolicy">retention policy</param>
        /// <returns>client</returns>
        public static InfluxDBClient CreateV1(string url, string username, char[] password, string database,
            string retentionPolicy)
        {
            Arguments.CheckNonEmptyString(database, nameof(database));

            var options = InfluxDBClientOptions.Builder
                .CreateNew()
                .Url(url)
                .Org("-")
                .AuthenticateToken($"{username}:{new string(password)}")
                .Bucket($"{database}/{retentionPolicy}")
                .Build();

            return Create(options);
        }

        /// <summary>
        /// Create a instance of the InfluxDB 2.x client.
        /// </summary>
        /// <param name="options">the connection configuration</param>
        /// <returns>client</returns>
        public static InfluxDBClient Create(InfluxDBClientOptions options)
        {
            Arguments.CheckNotNull(options, nameof(options));

            return new InfluxDBClient(options);
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
        public static Task<OnboardingResponse> Onboarding(string url, string username, string password,
            string org,
            string bucket)
        {
            Arguments.CheckNonEmptyString(url, nameof(url));
            Arguments.CheckNonEmptyString(username, nameof(username));
            Arguments.CheckNonEmptyString(password, nameof(password));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));

            var onboarding = new OnboardingRequest(username, password, org, bucket);

            return Onboarding(url, onboarding);
        }

        /// <summary>
        /// Post onboarding request, to setup initial user, org and bucket.
        /// </summary>
        /// <param name="url">the url to connect to the InfluxDB</param>
        /// <param name="onboarding">the defaults</param>
        /// <returns>Created default user, bucket, org.</returns>
        public static async Task<OnboardingResponse> Onboarding(string url, OnboardingRequest onboarding)
        {
            Arguments.CheckNonEmptyString(url, nameof(url));
            Arguments.CheckNotNull(onboarding, nameof(onboarding));


            using (var client = new InfluxDBClient(InfluxDBClientOptions.Builder.CreateNew().Url(url).Build()))
            {
                return await client.OnboardingAsync(onboarding).ConfigureAwait(false);
            }
        }
    }
}