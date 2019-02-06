using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Internal;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Internal;

namespace InfluxDB.Client
{
    public class InfluxDBClient : AbstractInfluxDBClient, IDisposable
    {
        private readonly AuthenticateDelegatingHandler _authenticateDelegatingHandler;
        private readonly LoggingHandler _loggingHandler;

        protected internal InfluxDBClient(InfluxDBClientOptions options)
        {
            Arguments.CheckNotNull(options, nameof(options));

            _loggingHandler = new LoggingHandler(LogLevel.None);
            _authenticateDelegatingHandler = new AuthenticateDelegatingHandler(options)
            {
                InnerHandler = _loggingHandler
            };

            Client.HttpClient = new HttpClient(_authenticateDelegatingHandler);
            Client.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            Client.HttpClient.BaseAddress = new Uri(options.Url);
            Client.HttpClient.Timeout = options.Timeout;
        }

        /// <summary>
        /// Get the Query client.
        /// </summary>
        /// <returns>the new client instance for the Query API</returns>
        public QueryApi GetQueryApi()
        {
            return new QueryApi(Client);
        }

        /// <summary>
        /// Get the Write client.
        /// </summary>
        /// <returns>the new client instance for the Write API</returns>
        public WriteApi GetWriteApi()
        {
            return new WriteApi(Client, WriteOptions.CreateNew().Build());
        }

        /// <summary>
        /// Get the Write client.
        /// </summary>
        /// <param name="writeOptions">the configuration for a write client</param>
        /// <returns>the new client instance for the Write API</returns>
        public WriteApi GetWriteApi(WriteOptions writeOptions)
        {
            return new WriteApi(Client, writeOptions);
        }

        /// <summary>
        /// Get the <see cref="Domain.Organization"/> client.
        /// </summary>
        /// <returns>the new client instance for Organization API</returns>
        public OrganizationsApi GetOrganizationsApi()
        {
            return new OrganizationsApi(Client);
        }

        /// <summary>
        /// Get the <see cref="Domain.User"/> client.
        /// </summary>
        /// <returns>the new client instance for User API</returns>
        public UsersApi GetUsersApi()
        {
            return new UsersApi(Client);
        }

        /// <summary>
        /// Get the <see cref="Domain.Bucket"/> client.
        /// </summary>
        /// <returns>the new client instance for Bucket API</returns>
        public BucketsApi GetBucketsApi()
        {
            return new BucketsApi(Client);
        }

        /// <summary>
        /// Get the <see cref="Domain.Source"/> client.
        /// </summary>
        /// <returns>the new client instance for Source API</returns>
        public SourcesApi GetSourcesApi()
        {
            return new SourcesApi(Client);
        }

        /// <summary>
        /// Get the <see cref="Domain.Authorization"/> client.
        /// </summary>
        /// <returns>the new client instance for Authorization API</returns>
        public AuthorizationsApi GetAuthorizationsApi()
        {
            return new AuthorizationsApi(Client);
        }

        /// <summary>
        /// Get the <see cref="Domain.Task"/> client.
        /// </summary>
        /// <returns>the new client instance for Task API</returns>
        public TasksApi GetTasksApi()
        {
            return new TasksApi(Client);
        }
        
        /// <summary>
        /// Get the <see cref="Domain.ScraperTarget"/> client.
        /// </summary>
        /// <returns>the new client instance for Scraper API</returns>
        public ScraperTargetsApi GetScraperTargetsApi()
        {
            return new ScraperTargetsApi(Client);
        }

        /// <summary>
        /// Get the <see cref="Domain.Label"/> client.
        /// </summary>
        /// <returns>the new client instance for Label API</returns>
        public LabelsApi GetLabelsApi()
        {
            return new LabelsApi(Client);
        }
        
        /// <summary>
        /// Set the log level for the request and response information.
        /// </summary>
        /// <param name="logLevel">the log level to set</param>
        public void SetLogLevel(LogLevel logLevel)
        {
            Arguments.CheckNotNull(logLevel, nameof(logLevel));

            _loggingHandler.Level = logLevel;
        }

        /// <summary>
        /// Set the <see cref="LogLevel"/> that is used for logging requests and responses.
        /// </summary>
        /// <returns>Log Level</returns>
        public LogLevel GetLogLevel()
        {
            return _loggingHandler.Level;
        }

        /// <summary>
        /// Get the health of an instance.
        /// </summary>
        /// <returns>health of an instance</returns>
        public async Task<Health> Health()
        {
            return await GetHealth("/health");
        }

        /// <summary>
        /// The readiness of the InfluxDB 2.0.
        /// </summary>
        /// <returns>return null if the InfluxDB is not ready</returns>
        public async Task<Ready> Ready()
        {
            try
            {
                var request = await Get("/ready");

                return Call<Ready>(request);
            }
            catch (Exception e)
            {
                Trace.TraceError($"The exception: '{e.Message}' occurs during check instance readiness.");

                return null;
            }
        }

        /// <summary>
        /// Post onboarding request, to setup initial user, org and bucket.
        /// </summary>
        /// <param name="onboarding">to setup defaults</param>
        /// <exception cref="HttpException">With status code 422 when an onboarding has already been completed</exception>
        /// <returns>defaults for first run</returns>
        public async Task<OnboardingResponse> Onboarding(Onboarding onboarding) 
        {
            Arguments.CheckNotNull(onboarding, nameof(onboarding));

            var request = await Post(onboarding, "/api/v2/setup");

            return Call<OnboardingResponse>(request);
        }

        
        /// <summary>
        /// Check if database has default user, org, bucket created, returns true if not.
        /// </summary>
        /// <returns>True if onboarding has already been completed otherwise false</returns>
        public async Task<bool> IsOnboardingAllowed()
        {
            var request = await Get("/api/v2/setup");
            
            return Call<IsOnboarding>(request).Allowed;
        }
        
        public void Dispose()
        {
            //
            // signout
            //
            try
            {
                var signout = _authenticateDelegatingHandler.Signout();

                signout.Wait();
            }
            catch (Exception e)
            {
                Trace.WriteLine("The signout exception");
                Trace.WriteLine(e);
            }
        }
    }
}