using System;
using System.Collections.Generic;
using System.Configuration;
using System.Net;
using System.Net.Http;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Web;
using InfluxDB.Client.Configurations;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Writes;

namespace InfluxDB.Client
{
    /// <summary>
    /// InfluxDBClientOptions are used to configure the InfluxDB 2.x connections.
    /// </summary>
    public class InfluxDBClientOptions
    {
        private static readonly Regex DurationRegex = new Regex(@"^(?<Amount>\d+)(?<Unit>[a-zA-Z]{0,2})$",
            RegexOptions.ExplicitCapture |
            RegexOptions.Compiled |
            RegexOptions.CultureInvariant |
            RegexOptions.RightToLeft);

        private string _token;
        private string _url;
        private TimeSpan _timeout;
        private LogLevel _logLevel;
        private string _username;
        private string _password;
        private IWebProxy _webProxy;
        private bool _allowHttpRedirects;
        private bool _verifySsl;
        private X509CertificateCollection _clientCertificates;
        private HttpClient _httpClient;

        /// <summary>
        /// Set the url to connect the InfluxDB.
        /// </summary>
        public string Url
        {
            get => _url;
            private set
            {
                Arguments.CheckNonEmptyString(value, "Url");
                _url = value;
            }
        }

        /// <summary>
        /// Set the timespan to wait before the HTTP request times out.
        /// </summary>
        public TimeSpan Timeout
        {
            get => _timeout;
            set
            {
                Arguments.CheckNotNull(value, "Timeout");
                _timeout = value;
            }
        }

        /// <summary>
        /// Set the log level for the request and response information.
        /// <list type="bullet">
        /// <item>Basic - Logs request and response lines.</item>
        /// <item>Body - Logs request and response lines including headers and body (if present). Note that applying the `Body` LogLevel will disable chunking while streaming and will load the whole response into memory.</item>
        /// <item>Headers - Logs request and response lines including headers.</item>
        /// <item>None - Disable logging.</item>
        /// </list>
        /// </summary>
        public LogLevel LogLevel
        {
            get => _logLevel;
            set
            {
                Arguments.CheckNotNull(value, "LogLevel");
                _logLevel = value;
            }
        }

        /// <summary>
        /// The scheme uses to Authentication.
        /// </summary>
        public AuthenticationScheme AuthScheme { get; private set; }

        /// <summary>
        /// Setup authorization by <see cref="AuthenticationScheme.Token"/>.
        /// </summary>
        public string Token
        {
            get => _token;
            set
            {
                _token = value;
                Arguments.CheckNonEmptyString(_token, "token");

                AuthScheme = AuthenticationScheme.Token;
            }
        }

        /// <summary>
        /// Setup authorization by <see cref="AuthenticationScheme.Session"/>.
        /// </summary>
        public string Username
        {
            get => _username;
            set
            {
                Arguments.CheckNonEmptyString(value, "Username");
                _username = value;

                if (!string.IsNullOrEmpty(_username) && !string.IsNullOrEmpty(Password))
                {
                    AuthScheme = AuthenticationScheme.Session;
                }
            }
        }

        /// <summary>
        /// Setup authorization by <see cref="AuthenticationScheme.Session"/>.
        /// </summary>
        public string Password
        {
            get => _password;
            set
            {
                Arguments.CheckNotNull(value, "Password");
                _password = value;

                if (!string.IsNullOrEmpty(Username) && !string.IsNullOrEmpty(_password))
                {
                    AuthScheme = AuthenticationScheme.Session;
                }
            }
        }

        /// <summary>
        /// Specify the default destination organization for writes and queries.
        /// </summary>
        public string Org { get; set; }

        /// <summary>
        /// Specify the default destination bucket for writes.
        /// </summary>
        public string Bucket { get; set; }

        /// <summary>
        /// Specify the WebProxy instance to use by the WebRequest to connect to external InfluxDB.
        /// </summary>
        public IWebProxy WebProxy
        {
            get => _webProxy;
            set
            {
                Arguments.CheckNotNull(value, "WebProxy");
                _webProxy = value;
            }
        }

        /// <summary>
        /// Configure automatically following HTTP 3xx redirects.
        /// </summary>
        public bool AllowHttpRedirects
        {
            get => _allowHttpRedirects;
            set
            {
                Arguments.CheckNotNull(value, "AllowHttpRedirects");
                _allowHttpRedirects = value;
            }
        }

        /// <summary>
        /// Ignore Certificate Validation Errors when `false`.
        /// </summary>
        public bool VerifySsl
        {
            get => _verifySsl;
            set
            {
                Arguments.CheckNotNull(value, "VerifySsl");
                _verifySsl = value;
            }
        }

        /// <summary>
        /// Callback function for handling the remote SSL Certificate Validation.
        /// The callback takes precedence over `VerifySsl`. 
        /// </summary>
        public RemoteCertificateValidationCallback VerifySslCallback { get; set; }

        /// <summary>
        /// Set X509CertificateCollection to be sent with HTTP requests
        /// </summary>
        public X509CertificateCollection ClientCertificates
        {
            get => _clientCertificates;
            set
            {
                Arguments.CheckNotNull(value, "ClientCertificates");
                _clientCertificates = value;
            }
        }

        /// <summary>
        /// The setting for store data point: default values, threshold, ...
        /// </summary>
        public PointSettings PointSettings { get; }

        /// <summary>
        /// Default tags that will be use for writes by Point and POJO.
        /// </summary>
        public Dictionary<string, string> DefaultTags
        {
            get => PointSettings.DefaultTags;
            set => PointSettings.DefaultTags = value;
        }

        /// <summary>
        /// Add default tag that will be use for writes by Point and POJO.
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
        public void AddDefaultTag(string tagName, string expression)
        {
            Arguments.CheckNotNull(tagName, nameof(tagName));
            PointSettings.AddDefaultTag(tagName, expression);
        }

        /// <summary>
        /// Add default tags that will be use for writes by Point and POJO.
        /// <see cref="AddDefaultTag"/>
        /// </summary>
        /// <param name="tags">tags dictionary</param>
        public void AddDefaultTags(Dictionary<string, string> tags)
        {
            foreach (var tag in tags)
            {
                Arguments.CheckNotNull(tag.Key, "TagName");
                PointSettings.AddDefaultTag(tag.Key, tag.Value);
            }
        }

        /// <summary>
        /// Add a HttpClient as a part of InfluxDBClientOptions
        /// </summary>
        public HttpClient HttpClient
        {
            get => _httpClient;
            set => _httpClient = value;
        }

        /// <summary>
        /// Create an instance of InfluxDBClientOptions. The url could be a connection string with various configurations.
        ///<para>
        /// e.g.: "http://localhost:8086?timeout=5000&amp;logLevel=BASIC
        /// The following options are supported:
        /// <list type="bullet">
        /// <item>Timeout - timespan to wait before the HTTP request times out</item>
        /// <item>LogLevel - log level for the request and response information</item>
        /// <item>Token - setup authorization by <see cref="AuthenticationScheme.Token"/></item>
        /// <item>Username - with Password property setup authorization by <see cref="AuthenticationScheme.Session"/></item>
        /// <item>Password - with Username property setup authorization by <see cref="AuthenticationScheme.Session"/></item>
        /// <item>Org - specify the default destination organization for writes and queries</item>
        /// <item>Bucket - specify the default destination bucket for writes</item>
        /// <item>WebProxy - specify the WebProxy instance to use by the WebRequest to connect to external InfluxDB.</item>
        /// <item>AllowHttpRedirects - configure automatically following HTTP 3xx redirects</item>
        /// <item>VerifySsl - ignore Certificate Validation Errors when `false`</item>
        /// <item>VerifySslCallback - callback function for handling the remote SSL Certificate Validation. The callback takes precedence over `VerifySsl`</item>
        /// <item>ClientCertificates - set X509CertificateCollection to be sent with HTTP requests</item>
        /// <item>DefaultTags - tags that will be use for writes by Point and POJO</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="url">url to connect the InfluxDB</param>
        public InfluxDBClientOptions(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                throw new ArgumentException("The url to connect the InfluxDB has to be defined.");
            }

            var uri = new Uri(url);

            Url = uri.GetLeftPart(UriPartial.Path);
            if (string.IsNullOrEmpty(Url))
            {
                throw new ArgumentException("The url to connect the InfluxDB has to be defined.");
            }

            var query = HttpUtility.ParseQueryString(uri.Query);
            Org = query.Get("org");
            Bucket = query.Get("bucket");
            AllowHttpRedirects = Convert.ToBoolean(query.Get("allowHttpRedirects"));

            var verifySslValue = query.Get("verifySsl");
            var token = query.Get("token");
            var logLevel = query.Get("logLevel");
            var timeout = query.Get("timeout");

            VerifySsl = Convert.ToBoolean(string.IsNullOrEmpty(verifySslValue) ? "true" : verifySslValue);

            if (!string.IsNullOrWhiteSpace(token))
            {
                Token = token;
            }

            if (!string.IsNullOrWhiteSpace(logLevel))
            {
                Enum.TryParse(logLevel, true, out LogLevel logLevelValue);
                LogLevel = logLevelValue;
            }

            if (!string.IsNullOrWhiteSpace(timeout))
            {
                Timeout = ToTimeout(timeout);
            }

            if (Timeout == TimeSpan.Zero || Timeout == TimeSpan.FromMilliseconds(0))
            {
                Timeout = TimeSpan.FromSeconds(10);
            }

            PointSettings = new PointSettings();
        }

        /// <summary>
        /// Configure InfluxDBClientOptions via App.config.
        /// </summary>
        /// <param name="sectionName">Name of configuration section. Useful for tests.</param>
        /// <returns><see cref="InfluxDBClientOptions"/></returns>
        public static InfluxDBClientOptions LoadConfig(string sectionName = "influx2")
        {
            var config = (Influx2)ConfigurationManager.GetSection(sectionName);
            if (config == null)
            {
                const string message = "The configuration doesn't contains a 'influx2' section. " +
                                       "The minimal configuration should contains an url of InfluxDB. " +
                                       "For more details see: " +
                                       "https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/README.md#client-configuration-file";

                throw new ConfigurationErrorsException(message);
            }

            var url = config.Url;
            var org = config.Org;
            var bucket = config.Bucket;
            var token = config.Token;
            var logLevel = config.LogLevel;
            var timeout = config.Timeout;
            var allowHttpRedirects = config.AllowHttpRedirects;
            var verifySsl = config.VerifySsl;

            var influxDbClientOptions = new InfluxDBClientOptions(url)
            {
                Org = org,
                Bucket = bucket,
                AllowHttpRedirects = allowHttpRedirects,
                VerifySsl = verifySsl
            };

            if (!string.IsNullOrWhiteSpace(token))
            {
                influxDbClientOptions.Token = token;
            }

            if (!string.IsNullOrWhiteSpace(logLevel))
            {
                Enum.TryParse(logLevel, true, out LogLevel logLevelValue);
                influxDbClientOptions.LogLevel = logLevelValue;
            }

            if (!string.IsNullOrWhiteSpace(timeout))
            {
                influxDbClientOptions.Timeout = ToTimeout(timeout);
            }

            if (influxDbClientOptions.Timeout == TimeSpan.Zero ||
                influxDbClientOptions.Timeout == TimeSpan.FromMilliseconds(0))
            {
                influxDbClientOptions.Timeout = TimeSpan.FromSeconds(10);
            }

            var tags = config.Tags;
            if (tags != null)
            {
                foreach (Influx2.TagElement o in tags)
                    influxDbClientOptions.PointSettings.AddDefaultTag(o.Name, o.Value);
            }

            return influxDbClientOptions;
        }

        private InfluxDBClientOptions(Builder builder)
        {
            Arguments.CheckNotNull(builder, nameof(builder));

            Url = builder.UrlString;
            LogLevel = builder.LogLevelValue;
            AuthScheme = builder.AuthScheme;

            switch (builder.AuthScheme)
            {
                case AuthenticationScheme.Token:
                    Token = builder.Token;
                    break;
                case AuthenticationScheme.Session:
                    Username = builder.Username;
                    Password = builder.Password;
                    break;
            }

            Org = builder.OrgString;
            Bucket = builder.BucketString;
            Timeout = builder.Timeout;
            AllowHttpRedirects = builder.AllowHttpRedirects;
            PointSettings = builder.PointSettings;
            VerifySsl = builder.VerifySslCertificates;
            VerifySslCallback = builder.VerifySslCallback;

            if (builder.WebProxy != null)
            {
                WebProxy = builder.WebProxy;
            }

            if (builder.CertificateCollection != null)
            {
                ClientCertificates = builder.CertificateCollection;
            }

            if (builder.HttpClient != null)
            {
                HttpClient = builder.HttpClient;
            }
        }

        private static TimeSpan ToTimeout(string value)
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
        /// The scheme uses to Authentication.
        /// </summary>
        public enum AuthenticationScheme
        {
            /// <summary>
            /// Anonymous.
            /// </summary>
            Anonymous,

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
            internal string Token;
            internal string Username;
            internal string Password;
            internal TimeSpan Timeout;

            internal string OrgString;
            internal string BucketString;

            internal IWebProxy WebProxy;
            internal bool AllowHttpRedirects;
            internal bool VerifySslCertificates = true;
            internal RemoteCertificateValidationCallback VerifySslCallback;
            internal X509CertificateCollection CertificateCollection;
            internal HttpClient HttpClient;

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
            /// Set the timespan to wait before the HTTP request times out.
            /// </summary>
            /// <param name="timeout">The timespan to wait before the HTTP request times out. It must be defined.</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder TimeOut(TimeSpan timeout)
            {
                Arguments.CheckNotNull(timeout, nameof(timeout));

                Timeout = timeout;

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
                Password = new string(password);

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
                Token = new string(token);

                return this;
            }

            /// <summary>
            /// Setup authorization by <see cref="AuthenticationScheme.Token"/>.
            /// </summary>
            /// <param name="token">the token to use for the authorization</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder AuthenticateToken(string token)
            {
                Arguments.CheckNonEmptyString(token, "token");
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
            /// <returns><see cref="Builder"/></returns>
            public Builder AddDefaultTag(string tagName, string expression)
            {
                Arguments.CheckNotNull(tagName, nameof(tagName));

                PointSettings.AddDefaultTag(tagName, expression);

                return this;
            }

            /// <summary>
            /// Specify the WebProxy instance to use by the WebRequest to connect to external InfluxDB.
            /// </summary>
            /// <param name="webProxy">The WebProxy to use to access the InfluxDB.</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder Proxy(IWebProxy webProxy)
            {
                Arguments.CheckNotNull(webProxy, nameof(webProxy));

                WebProxy = webProxy;

                return this;
            }

            /// <summary>
            /// Configure automatically following HTTP 3xx redirects.
            /// </summary>
            /// <param name="allowHttpRedirects">configure HTTP redirects</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder AllowRedirects(bool allowHttpRedirects)
            {
                Arguments.CheckNotNull(allowHttpRedirects, nameof(allowHttpRedirects));

                AllowHttpRedirects = allowHttpRedirects;

                return this;
            }

            /// <summary>
            /// Ignore Certificate Validation Errors when `false`.
            /// </summary>
            /// <param name="verifySsl">validates Certificates</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder VerifySsl(bool verifySsl)
            {
                Arguments.CheckNotNull(verifySsl, nameof(verifySsl));

                VerifySslCertificates = verifySsl;

                return this;
            }

            /// <summary>
            /// Callback function for handling the remote SSL Certificate Validation.
            /// The callback takes precedence over `VerifySsl`. 
            /// </summary>
            /// <param name="callback"></param>
            /// <returns></returns>
            public Builder RemoteCertificateValidationCallback(RemoteCertificateValidationCallback callback)
            {
                VerifySslCallback = callback;

                return this;
            }

            /// <summary>
            /// Set X509CertificateCollection to be sent with HTTP requests
            /// </summary>
            /// <param name="clientCertificates">certificate collections</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder ClientCertificates(X509CertificateCollection clientCertificates)
            {
                Arguments.CheckNotNull(clientCertificates, nameof(clientCertificates));

                CertificateCollection = clientCertificates;

                return this;
            }

            /// <summary>
            /// Configure Builder via App.config.
            /// </summary>
            /// <param name="sectionName">Name of configuration section. Useful for tests.</param>
            /// <returns><see cref="Builder"/></returns>
            internal Builder LoadConfig(string sectionName = "influx2")
            {
                var config = (Influx2)ConfigurationManager.GetSection(sectionName);
                if (config == null)
                {
                    const string message = "The configuration doesn't contains a 'influx2' section. " +
                                           "The minimal configuration should contains an url of InfluxDB. " +
                                           "For more details see: " +
                                           "https://github.com/influxdata/influxdb-client-csharp/blob/master/Client/README.md#client-configuration-file";

                    throw new ConfigurationErrorsException(message);
                }

                var url = config.Url;
                var org = config.Org;
                var bucket = config.Bucket;
                var token = config.Token;
                var logLevel = config.LogLevel;
                var timeout = config.Timeout;
                var allowHttpRedirects = config.AllowHttpRedirects;
                var verifySsl = config.VerifySsl;

                var tags = config.Tags;
                if (tags != null)
                {
                    foreach (Influx2.TagElement o in tags) AddDefaultTag(o.Name, o.Value);
                }

                return Configure(url, org, bucket, token, logLevel, timeout, allowHttpRedirects, verifySsl);
            }

            /// <summary>
            /// Configure Builder via connection string.
            /// </summary>
            /// <param name="connectionString">connection string with various configurations</param>
            /// <returns><see cref="Builder"/></returns>
            public Builder ConnectionString(string connectionString)
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
                var allowHttpRedirects = Convert.ToBoolean(query.Get("allowHttpRedirects"));
                var verifySslValue = query.Get("verifySsl");
                var verifySsl = Convert.ToBoolean(string.IsNullOrEmpty(verifySslValue) ? "true" : verifySslValue);

                return Configure(url, org, bucket, token, logLevel, timeout, allowHttpRedirects, verifySsl);
            }

            private Builder Configure(string url, string org, string bucket, string token, string logLevel,
                string timeout, bool allowHttpRedirects = false, bool verifySsl = true)
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

                AllowRedirects(allowHttpRedirects);

                VerifySsl(verifySsl);

                return this;
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

                return new InfluxDBClientOptions(this);
            }

            /// <summary>
            /// Configure HttpClient
            /// </summary>
            /// <param name="httpClient"></param>
            /// <returns></returns>
            public Builder SetHttpClient(HttpClient httpClient)
            {
                Arguments.CheckNotNull(httpClient, nameof(httpClient));

                HttpClient = httpClient;

                return this;
            }
        }
    }
}