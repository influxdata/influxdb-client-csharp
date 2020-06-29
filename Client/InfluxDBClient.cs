using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Internal;

namespace InfluxDB.Client
{
    public class InfluxDBClient : IDisposable
    {
        private readonly ApiClient _apiClient;
        private readonly ExceptionFactory _exceptionFactory;
        private readonly HealthService _healthService;
        private readonly LoggingHandler _loggingHandler;
        private readonly GzipHandler _gzipHandler;
        private readonly ReadyService _readyService;

        private readonly SetupService _setupService;
        private readonly InfluxDBClientOptions _options;

        private readonly Subject<Unit> _disposeNotification = new Subject<Unit>();

        protected internal InfluxDBClient(InfluxDBClientOptions options)
        {
            Arguments.CheckNotNull(options, nameof(options));

            _options = options;
            _loggingHandler = new LoggingHandler(options.LogLevel);
            _gzipHandler = new GzipHandler();

            var version = AssemblyHelper.GetVersion(typeof(InfluxDBClient));
            
            _apiClient = new ApiClient(options, _loggingHandler, _gzipHandler);
            _apiClient.RestClient.UserAgent = $"influxdb-client-csharp/{version}";

            _exceptionFactory = (methodName, response) =>
                !response.IsSuccessful ? HttpException.Create(response, response.Content) : null;

            _setupService = new SetupService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };
            _healthService = new HealthService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };
            _readyService = new ReadyService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };
        }

        public void Dispose()
        {
            //
            // Dispose child APIs
            //
            _disposeNotification.OnNext(Unit.Default);

            //
            // signout
            //
            try
            {
                _apiClient.Signout();
            }
            catch (Exception e)
            {
                Trace.WriteLine("The signout exception");
                Trace.WriteLine(e);
            }
        }

        /// <summary>
        /// Get the Query client.
        /// </summary>
        /// <returns>the new client instance for the Query API</returns>
        public QueryApi GetQueryApi()
        {
            var service = new QueryService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new QueryApi(_options, service);
        }

        /// <summary>
        /// Get the Write client.
        /// </summary>
        /// <returns>the new client instance for the Write API</returns>
        public WriteApi GetWriteApi()
        {
            return GetWriteApi(WriteOptions.CreateNew().Build());
        }
        
        /// <summary>
        /// Get the Write async client.
        /// </summary>
        /// <returns>the new client instance for the Write API Async without batching</returns>
        public WriteApiAsync GetWriteApiAsync()
        {
            var service = new WriteService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };
            
            return new WriteApiAsync(_options, service, this);
        }

        /// <summary>
        /// Get the Write client.
        /// </summary>
        /// <param name="writeOptions">the configuration for a write client</param>
        /// <returns>the new client instance for the Write API</returns>
        public WriteApi GetWriteApi(WriteOptions writeOptions)
        {
            var service = new WriteService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            var writeApi = new WriteApi(_options, service, writeOptions, this, _disposeNotification);

            return writeApi;
        }

        /// <summary>
        /// Get the <see cref="Organization" /> client.
        /// </summary>
        /// <returns>the new client instance for Organization API</returns>
        public OrganizationsApi GetOrganizationsApi()
        {
            var service = new OrganizationsService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new OrganizationsApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.User" /> client.
        /// </summary>
        /// <returns>the new client instance for User API</returns>
        public UsersApi GetUsersApi()
        {
            var service = new UsersService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new UsersApi(service);
        }

        /// <summary>
        /// Get the <see cref="Bucket" /> client.
        /// </summary>
        /// <returns>the new client instance for Bucket API</returns>
        public BucketsApi GetBucketsApi()
        {
            var service = new BucketsService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new BucketsApi(service);
        }

        /// <summary>
        /// Get the <see cref="Source" /> client.
        /// </summary>
        /// <returns>the new client instance for Source API</returns>
        public SourcesApi GetSourcesApi()
        {
            var service = new SourcesService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new SourcesApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Authorization" /> client.
        /// </summary>
        /// <returns>the new client instance for Authorization API</returns>
        public AuthorizationsApi GetAuthorizationsApi()
        {
            var service = new AuthorizationsService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new AuthorizationsApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Task" /> client.
        /// </summary>
        /// <returns>the new client instance for Task API</returns>
        public TasksApi GetTasksApi()
        {
            var service = new TasksService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new TasksApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.ScraperTargetResponse" /> client.
        /// </summary>
        /// <returns>the new client instance for Scraper API</returns>
        public ScraperTargetsApi GetScraperTargetsApi()
        {
            var service = new ScraperTargetsService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new ScraperTargetsApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Telegraf" /> client.
        /// </summary>
        /// <returns>the new client instance for Telegrafs API</returns>
        public TelegrafsApi GetTelegrafsApi()
        {
            var service = new TelegrafsService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new TelegrafsApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Label" /> client.
        /// </summary>
        /// <returns>the new client instance for Label API</returns>
        public LabelsApi GetLabelsApi()
        {
            var service = new LabelsService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new LabelsApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.NotificationEndpoint" /> client.
        /// </summary>
        /// <returns>the new client instance for NotificationEndpoint API</returns>
        public NotificationEndpointsApi GetNotificationEndpointsApi()
        {
            var service = new NotificationEndpointsService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new NotificationEndpointsApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.NotificationRules" /> client.
        /// </summary>
        /// <returns>the new client instance for NotificationRules API</returns>
        public NotificationRulesApi GetNotificationRulesApi()
        {
            var service = new NotificationRulesService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new NotificationRulesApi(service);
        }

        /// <summary>
        /// Get the <see cref="InfluxDB.Client.Api.Domain.Check" /> client.
        /// </summary>
        /// <returns>the new client instance for Checks API</returns>
        public ChecksApi GetChecksApi()
        {
            var service = new ChecksService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new ChecksApi(service);
        }
        
        /// <summary>
        /// Get the Delete client.
        /// </summary>
        /// <returns>the new client instance for Delete API</returns>
        public DeleteApi GetDeleteApi()
        {
            var service = new DefaultService((Configuration) _apiClient.Configuration)
            {
                ExceptionFactory = _exceptionFactory
            };

            return new DeleteApi(service);
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
        /// Set the <see cref="LogLevel" /> that is used for logging requests and responses.
        /// </summary>
        /// <returns>Log Level</returns>
        public LogLevel GetLogLevel()
        {
            return _loggingHandler.Level;
        }

        /// <summary>
        /// Enable Gzip compress for http requests.
        ///
        /// <para>Currently only the "Write" and "Query" endpoints supports the Gzip compression.</para>
        /// </summary>
        /// <returns></returns>
        public InfluxDBClient EnableGzip()
        {
            _gzipHandler.EnableGzip();

            return this;
        }

        /// <summary>
        /// Disable Gzip compress for http request body.
        /// </summary>
        /// <returns>this</returns>
        public InfluxDBClient DisableGzip()
        {
            _gzipHandler.DisableGzip();

            return this;
        }

        /// <summary>
        /// Returns whether Gzip compress for http request body is enabled.
        /// </summary>
        /// <returns>true if gzip is enabled.</returns>
        public bool IsGzipEnabled()
        {
            return _gzipHandler.IsEnabledGzip();
        }

        /// <summary>
        /// Get the health of an instance.
        /// </summary>
        /// <returns>health of an instance</returns>
        public async Task<HealthCheck> HealthAsync()
        {
            return await GetHealthAsync(_healthService.GetHealthAsync());
        }

        /// <summary>
        /// The readiness of the InfluxDB 2.0.
        /// </summary>
        /// <returns>return null if the InfluxDB is not ready</returns>
        public async Task<Ready> ReadyAsync()
        {
            try
            {
                return await _readyService.GetReadyAsync();
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
        public async Task<OnboardingResponse> OnboardingAsync(OnboardingRequest onboarding)
        {
            Arguments.CheckNotNull(onboarding, nameof(onboarding));

            return await _setupService.PostSetupAsync(onboarding);
        }

        /// <summary>
        /// Check if database has default user, org, bucket created, returns true if not.
        /// </summary>
        /// <returns>True if onboarding has already been completed otherwise false</returns>
        public async Task<bool> IsOnboardingAllowedAsync()
        {
            var isOnboardingAllowed = _setupService.GetSetupAsync().ContinueWith(t => t.Result.Allowed == true);

            return await isOnboardingAllowed;
        }

        internal static string AuthorizationHeader(string username, string password)
        {
            return "Basic " + Convert.ToBase64String(Encoding.Default.GetBytes(username + ":" + password));
        }

        internal static async Task<HealthCheck> GetHealthAsync(Task<HealthCheck> task)
        {
            Arguments.CheckNotNull(task, nameof(task));

            return await task
                .ContinueWith(t =>
                {
                    if (t.Exception != null)
                    {
                        return new HealthCheck("influxdb", t.Exception?.Message, default(List<HealthCheck>),
                            HealthCheck.StatusEnum.Fail);
                    }

                    return t.Result;
                }, CancellationToken.None);
        }
    }
}