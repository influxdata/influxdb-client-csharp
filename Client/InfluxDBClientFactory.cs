using InfluxDB.Client.Core;
using InfluxDB.Client.Generated.Domain;

namespace InfluxDB.Client
{
    public static class InfluxDBClientFactory
    {
        /// <summary>
        ///     Create a instance of the InfluxDB 2.0 client.
        /// </summary>
        /// <param name="url">the url to connect to the InfluxDB 2.0</param>
        /// <returns>client</returns>
        public static InfluxDBClient Create(string url)
        {
            var options = InfluxDBClientOptions.Builder
                .CreateNew()
                .Url(url)
                .Build();

            return Create(options);
        }

        /// <summary>
        ///     Create a instance of the InfluxDB 2.0 client.
        /// </summary>
        /// <param name="url">the url to connect to the InfluxDB 2.0</param>
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
        ///     Create a instance of the InfluxDB 2.0 client.
        /// </summary>
        /// <param name="url">the url to connect to the InfluxDB 2.0</param>
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
        ///     Create a instance of the InfluxDB 2.0 client.
        /// </summary>
        /// <param name="options">the connection configuration</param>
        /// <returns>client</returns>
        public static InfluxDBClient Create(InfluxDBClientOptions options)
        {
            Arguments.CheckNotNull(options, nameof(options));

            return new InfluxDBClient(options);
        }

        /// <summary>
        ///     Post onboarding request, to setup initial user, org and bucket.
        /// </summary>
        /// <param name="url">the url to connect to the InfluxDB</param>
        /// <param name="username">the name of an user</param>
        /// <param name="password">the password to connect as an user</param>
        /// <param name="org">the name of an organization</param>
        /// <param name="bucket">the name of a bucket</param>
        /// <returns>Created default user, bucket, org.</returns>
        public static OnboardingResponse Onboarding(string url, string username, string password, string org,
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
        ///     Post onboarding request, to setup initial user, org and bucket.
        /// </summary>
        /// <param name="url">the url to connect to the InfluxDB</param>
        /// <param name="onboarding">the defaults</param>
        /// <returns>Created default user, bucket, org.</returns>
        public static OnboardingResponse Onboarding(string url, OnboardingRequest onboarding)
        {
            Arguments.CheckNonEmptyString(url, nameof(url));
            Arguments.CheckNotNull(onboarding, nameof(onboarding));


            using (var client = new InfluxDBClient(InfluxDBClientOptions.Builder.CreateNew().Url(url).Build()))
            {
                return client.Onboarding(onboarding);
            }
        }
    }
}