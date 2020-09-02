using System;
using System.Configuration;
using System.Text.RegularExpressions;
using System.Web;
using InfluxDB.Client.Configurations;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Writes;

namespace InfluxDB.Client
{
    /// <summary>
    /// InfluxDBClientOptions are used to configure the InfluxDB 2.0 connections.
    /// </summary>
    public class InfluxDBClientOptions
    {
        private static readonly Regex DurationRegex = new Regex(@"^(?<Amount>\d+)(?<Unit>[a-zA-Z]{0,2})$",
            RegexOptions.ExplicitCapture |
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.RightToLeft);

        public string Url { get; }
        public LogLevel LogLevel { get; }

        public AuthenticationScheme AuthScheme { get; }
        public char[] Token { get; }
        public string Username { get; }
        public char[] Password { get; }

        public string Org { get; }
        public string Bucket { get; }

        public TimeSpan Timeout { get; }
        public TimeSpan ReadWriteTimeout { get; }

        public PointSettings PointSettings { get; }

        private InfluxDBClientOptions(Builder builder)
        {
            Arguments.CheckNotNull(builder, nameof(builder));

            Url = builder.UrlString;
            LogLevel = builder.LogLevelValue;
            AuthScheme = builder.AuthScheme;
            Token = builder.Token;
            Username = builder.Username;
            Password = builder.Password;

            Org = builder.OrgString;
            Bucket = builder.BucketString;

            Timeout = builder.Timeout;
            ReadWriteTimeout = builder.ReadWriteTimeout;

            PointSettings = builder.PointSettings;
        }

        /// <summary>
        /// The scheme uses to Authentication.
        /// </summary>
        public enum AuthenticationScheme
        {
            /// <summary>
            /// Basic auth.
            /// </summary>
            Session,

            /// <summary>
            /// Authentication token.
            /// </summary>
            Token
        }

        /// <summary>
        /// A builder for <see cref="InfluxDBClientOptions"/>.
        /// </summary>
        public sealed class Builder
        {
            internal string UrlString;
            internal LogLevel LogLevelValue;

            internal AuthenticationScheme AuthScheme;
            internal char[] Token;
            internal string Username;
            internal char[] Password;
            internal TimeSpan Timeout;
            internal TimeSpan ReadWriteTimeout;

            internal string OrgString;
            internal string BucketString;

            internal PointSettings PointSettings = new PointSettings();

            public static Builder CreateNew()
            {
                return new Builder();
            }

            /// <summary>
            /// Set the url to connect the InfluxDB.
            /// </summary>
            /// <param name="url">the url to connect the InfluxDB. It must be defined.</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder Url(string url)
            {
                Arguments.CheckNonEmptyString(url, nameof(url));

                UrlString = url;

                return this;
            }

            /// <summary>
            /// Set the log level for the request and response information.
            /// </summary>
            /// <param name="logLevel">The log level for the request and response information.</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder LogLevel(LogLevel logLevel)
            {
                Arguments.CheckNotNull(logLevel, nameof(logLevel));

                LogLevelValue = logLevel;

                return this;
            }

            /// <summary>
            /// Set the Timeout to connect the InfluxDB.
            /// </summary>
            /// <param name="timeout">the timeout to connect the InfluxDB. It must be defined.</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder TimeOut(TimeSpan timeout)
            {
                Arguments.CheckNotNull(timeout, nameof(timeout));

                Timeout = timeout;

                return this;
            }

            /// <summary>
            /// Set the read and write timeout from the InfluxDB.
            /// </summary>
            /// <param name="readWriteTimeout">the timeout to read and write from the InfluxDB. It must be defined.</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder ReadWriteTimeOut(TimeSpan readWriteTimeout)
            {
                Arguments.CheckNotNull(readWriteTimeout, nameof(readWriteTimeout));

                ReadWriteTimeout = readWriteTimeout;

                return this;
            }

            /// <summary>
            /// Setup authorization by <see cref="AuthenticationScheme.Session"/>.
            /// </summary>
            /// <param name="username">the username to use in the basic auth</param>
            /// <param name="password">the password to use in the basic auth</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder Authenticate(string username,
                char[] password)
            {
                Arguments.CheckNonEmptyString(username, "username");
                Arguments.CheckNotNull(password, "password");

                AuthScheme = AuthenticationScheme.Session;
                Username = username;
                Password = password;

                return this;
            }

            /// <summary>
            /// Setup authorization by <see cref="AuthenticationScheme.Token"/>.
            /// </summary>
            /// <param name="token">the token to use for the authorization</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder AuthenticateToken(char[] token)
            {
                Arguments.CheckNotNull(token, "token");

                AuthScheme = AuthenticationScheme.Token;
                Token = token;

                return this;
            }
            
            /// <summary>
            /// Setup authorization by <see cref="AuthenticationScheme.Token"/>.
            /// </summary>
            /// <param name="token">the token to use for the authorization</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder AuthenticateToken(string token)
            {
                return AuthenticateToken(token.ToCharArray());
            }

            /// <summary>
            /// Specify the default destination organization for writes and queries.
            /// </summary>
            /// <param name="org">the default destination organization for writes and queries</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder Org(string org)
            {
                OrgString = org;

                return this;
            }

            /// <summary>
            /// Specify the default destination bucket for writes.
            /// </summary>
            /// <param name="bucket">default destination bucket for writes</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder Bucket(string bucket)
            {
                BucketString = bucket;

                return this;
            }

            /// <summary>
            /// Add default tag that will be use for writes by Point and POJO.
            ///
            /// <para>
            /// The expressions can be:
            /// <list type="bullet">
            /// <item>"California Miner" - static value</item>
            /// <item>"${version}" - application settings</item>
            /// <item>"${env.hostname}" - environment property</item>
            /// </list>
            /// </para>
            /// </summary>
            /// <param name="tagName">the tag name</param>
            /// <param name="expression">the tag value expression</param>
            /// <returns></returns>
            public Builder AddDefaultTag(string tagName, string expression)
            {
                Arguments.CheckNotNull(tagName, nameof(tagName));

                PointSettings.AddDefaultTag(tagName, expression);

                return this;
            }

            /// <summary>
            /// Configure Builder via App.config.
            /// </summary>
            /// <returns><see cref="Builder"/></returns>
            internal Builder LoadConfig()
            {
                var config = (Influx2) ConfigurationManager.GetSection("influx2");

                var url = config?.Url;
                var org = config?.Org;
                var bucket = config?.Bucket;
                var token = config?.Token;
                var logLevel = config?.LogLevel;
                var timeout = config?.Timeout;
                var readWriteTimeout = config?.ReadWriteTimeout;

                var tags = config?.Tags;
                if (tags != null)
                {
                    foreach (Influx2.TagElement o in tags)
                    {
                        AddDefaultTag(o.Name, o.Value);
                    }
                }

                return Configure(url, org, bucket, token, logLevel, timeout, readWriteTimeout);
            }

            /// <summary>
            /// Configure Builder via connection string.
            /// </summary>
            /// <param name="connectionString">connection string with various configurations</param>
            /// <returns><see cref="Builder"/></returns>
            internal Builder ConnectionString(string connectionString)
            {
                Arguments.CheckNonEmptyString(connectionString, nameof(connectionString));

                var uri = new Uri(connectionString);

                var url = uri.GetLeftPart(UriPartial.Path);

                var query = HttpUtility.ParseQueryString(uri.Query);
                var org = query.Get("org");
                var bucket = query.Get("bucket");
                var token = query.Get("token");
                var logLevel = query.Get("logLevel");
                var timeout = query.Get("timeout");
                var readWriteTimeout = query.Get("readWriteTimeout");

                return Configure(url, org, bucket, token, logLevel, timeout, readWriteTimeout);
            }

            private Builder Configure(string url, string org, string bucket, string token, string logLevel,
                string timeout, string readWriteTimeout)
            {
                Url(url);
                Org(org);
                Bucket(bucket);

                if (!string.IsNullOrWhiteSpace(token))
                {
                    AuthenticateToken(token);
                }

                if (!string.IsNullOrWhiteSpace(logLevel))
                {
                    Enum.TryParse(logLevel, true, out LogLevelValue);
                }

                if (!string.IsNullOrWhiteSpace(timeout))
                {
                    TimeOut(ToTimeout(timeout));
                }

                if (!string.IsNullOrWhiteSpace(readWriteTimeout))
                {
                    ReadWriteTimeOut(ToTimeout(readWriteTimeout));
                }

                return this;
            }

            private TimeSpan ToTimeout(string value)
            {
                var matcher = DurationRegex.Match(value);
                if (!matcher.Success)
                {
                    throw new InfluxException($"'{value}' is not a valid duration");
                }

                var amount = matcher.Groups["Amount"].Value;
                var unit = matcher.Groups["Unit"].Value;

                TimeSpan duration;
                switch (string.IsNullOrWhiteSpace(unit) ? "ms" : unit.ToLower())
                {
                    case "ms":
                        duration = TimeSpan.FromMilliseconds(double.Parse(amount));
                        break;

                    case "s":
                        duration = TimeSpan.FromSeconds(double.Parse(amount));
                        break;

                    case "m":
                        duration = TimeSpan.FromMinutes(double.Parse(amount));
                        break;

                    default:
                        throw new InfluxException($"unknown unit for '{value}'");
                }

                return duration;
            }

            /// <summary>
            /// Build an instance of InfluxDBClientOptions.
            /// </summary>
            /// <returns><see cref="InfluxDBClientOptions"/></returns>
            /// <exception cref="InvalidOperationException">If url is not defined.</exception>
            public InfluxDBClientOptions Build()
            {
                if (string.IsNullOrEmpty(UrlString))
                {
                    throw new InvalidOperationException("The url to connect the InfluxDB has to be defined.");
                }

                if (Timeout == TimeSpan.Zero || Timeout == TimeSpan.FromMilliseconds(0))
                {
                    Timeout = TimeSpan.FromSeconds(10);
                }

                if (ReadWriteTimeout == TimeSpan.Zero || ReadWriteTimeout == TimeSpan.FromMilliseconds(0))
                {
                    ReadWriteTimeout = TimeSpan.FromSeconds(10);
                }

                return new InfluxDBClientOptions(this);
            }
        }
    }
}