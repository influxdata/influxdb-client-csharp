using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using InfluxData.Platform.Client.Option;
using Platform.Common.Flux.Error;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;

namespace InfluxData.Platform.Client.Client
{
    public class PlatformClient : AbstractPlatformClient, IDisposable
    {
        private readonly AuthenticateDelegatingHandler _authenticateDelegatingHandler;
        private readonly LoggingHandler _loggingHandler;

        protected internal PlatformClient(PlatformOptions options)
        {
            Arguments.CheckNotNull(options, "PlatformOptions");

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
        public QueryClient CreateQueryClient()
        {
            return new QueryClient(Client);
        }

        /// <summary>
        /// Get the Write client.
        /// </summary>
        /// <returns>the new client instance for the Write API</returns>
        public WriteClient CreateWriteClient()
        {
            return new WriteClient(Client, WriteOptions.CreateNew().Build());
        }

        /// <summary>
        /// Get the Write client.
        /// </summary>
        /// <param name="writeOptions">the configuration for a write client</param>
        /// <returns>the new client instance for the Write API</returns>
        public WriteClient CreateWriteClient(WriteOptions writeOptions)
        {
            return new WriteClient(Client, writeOptions);
        }

        /// <summary>
        /// Get the <see cref="Domain.Organization"/> client.
        /// </summary>
        /// <returns>the new client instance for Organization API</returns>
        public OrganizationClient CreateOrganizationClient()
        {
            return new OrganizationClient(Client);
        }

        /// <summary>
        /// Get the <see cref="Domain.User"/> client.
        /// </summary>
        /// <returns>the new client instance for User API</returns>
        public UserClient CreateUserClient()
        {
            return new UserClient(Client);
        }

        /// <summary>
        /// Get the <see cref="Domain.Bucket"/> client.
        /// </summary>
        /// <returns>the new client instance for Bucket API</returns>
        public BucketClient CreateBucketClient()
        {
            return new BucketClient(Client);
        }

        /// <summary>
        /// Get the <see cref="Domain.Source"/> client.
        /// </summary>
        /// <returns>the new client instance for Source API</returns>
        public SourceClient CreateSourceClient()
        {
            return new SourceClient(Client);
        }

        /// <summary>
        /// Get the <see cref="Domain.Authorization"/> client.
        /// </summary>
        /// <returns>the new client instance for Authorization API</returns>
        public AuthorizationClient CreateAuthorizationClient()
        {
            return new AuthorizationClient(Client);
        }

        /// <summary>
        /// Get the <see cref="Domain.Task"/> client.
        /// </summary>
        /// <returns>the new client instance for Task API</returns>
        public TaskClient CreateTaskClient()
        {
            return new TaskClient(Client);
        }
        
        /// <summary>
        /// Get the <see cref="Domain.ScraperTarget"/> client.
        /// </summary>
        /// <returns>the new client instance for Scraper API</returns>
        public ScraperClient CreateScraperClient()
        {
            return new ScraperClient(Client);
        }

        /// <summary>
        /// Get the <see cref="Domain.Label"/> client.
        /// </summary>
        /// <returns>the new client instance for Label API</returns>
        public LabelClient CreateLabelClient()
        {
            return new LabelClient(Client);
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
        /// The readiness of the InfluxData Platform.
        /// </summary>
        /// <returns>return null if the platform is not ready</returns>
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