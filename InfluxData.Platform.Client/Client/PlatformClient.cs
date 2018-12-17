using System;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using InfluxData.Platform.Client.Option;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;

namespace InfluxData.Platform.Client.Client
{
    public class PlatformClient : AbstractClient, IDisposable
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
            try
            {
                var request = await Get("/health");

                return Call<Health>(request);
            }
            catch (Exception e)
            {
                return new Health {Status = "error", Message = e.Message};
            }
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