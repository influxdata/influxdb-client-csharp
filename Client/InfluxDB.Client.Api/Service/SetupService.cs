/*
 * InfluxDB OSS API Service
 *
 * The InfluxDB v2 API provides a programmatic interface for all interactions with InfluxDB. Access the InfluxDB API using the `/api/v2/` endpoint. 
 *
 * The version of the OpenAPI document: 2.0.0
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using InfluxDB.Client.Core.Api;
using InfluxDB.Client.Api.Domain;

namespace InfluxDB.Client.Api.Service
{

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ISetupServiceSync : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Check if database has default user, org, bucket
        /// </summary>
        /// <remarks>
        /// Returns &#x60;true&#x60; if no default user, organization, or bucket has been created.
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>IsOnboarding</returns>
        IsOnboarding GetSetup(string zapTraceSpan = default(string));

        /// <summary>
        /// Check if database has default user, org, bucket
        /// </summary>
		/// <remarks>
        /// Returns &#x60;true&#x60; if no default user, organization, or bucket has been created.
        /// </remarks>
		/// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
		/// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
		/// <returns>RequestOptions</returns>
		RequestOptions GetSetupWithRequestOptions(string zapTraceSpan = default(string));

		/// <summary>
        /// Check if database has default user, org, bucket
        /// </summary>
        /// <remarks>
        /// Returns &#x60;true&#x60; if no default user, organization, or bucket has been created.
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of IsOnboarding</returns>
        ApiResponse<IsOnboarding> GetSetupWithHttpInfo(string zapTraceSpan = default(string));
        /// <summary>
        /// Set up initial user, org and bucket
        /// </summary>
        /// <remarks>
        /// Post an onboarding request to set up initial user, org and bucket.
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">Source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>OnboardingResponse</returns>
        OnboardingResponse PostSetup(OnboardingRequest onboardingRequest, string zapTraceSpan = default(string));

        /// <summary>
        /// Set up initial user, org and bucket
        /// </summary>
		/// <remarks>
        /// Post an onboarding request to set up initial user, org and bucket.
        /// </remarks>
		/// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
		/// <param name="onboardingRequest">Source to create</param>
		/// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
		/// <returns>RequestOptions</returns>
		RequestOptions PostSetupWithRequestOptions(OnboardingRequest onboardingRequest, string zapTraceSpan = default(string));

		/// <summary>
        /// Set up initial user, org and bucket
        /// </summary>
        /// <remarks>
        /// Post an onboarding request to set up initial user, org and bucket.
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">Source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of OnboardingResponse</returns>
        ApiResponse<OnboardingResponse> PostSetupWithHttpInfo(OnboardingRequest onboardingRequest, string zapTraceSpan = default(string));
        #endregion Synchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ISetupServiceAsync : IApiAccessor
    {
        #region Asynchronous Operations
        /// <summary>
        /// Check if database has default user, org, bucket
        /// </summary>
        /// <remarks>
        /// Returns &#x60;true&#x60; if no default user, organization, or bucket has been created.
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of IsOnboarding</returns>
        System.Threading.Tasks.Task<IsOnboarding> GetSetupAsync(string zapTraceSpan = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Check if database has default user, org, bucket
        /// </summary>
        /// <remarks>
        /// Returns &#x60;true&#x60; if no default user, organization, or bucket has been created.
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (IsOnboarding)</returns>
        System.Threading.Tasks.Task<ApiResponse<IsOnboarding>> GetSetupWithHttpInfoAsync(string zapTraceSpan = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        /// <summary>
        /// Set up initial user, org and bucket
        /// </summary>
        /// <remarks>
        /// Post an onboarding request to set up initial user, org and bucket.
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">Source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of OnboardingResponse</returns>
        System.Threading.Tasks.Task<OnboardingResponse> PostSetupAsync(OnboardingRequest onboardingRequest, string zapTraceSpan = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));

        /// <summary>
        /// Set up initial user, org and bucket
        /// </summary>
        /// <remarks>
        /// Post an onboarding request to set up initial user, org and bucket.
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">Source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (OnboardingResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<OnboardingResponse>> PostSetupWithHttpInfoAsync(OnboardingRequest onboardingRequest, string zapTraceSpan = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken));
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ISetupService : ISetupServiceSync, ISetupServiceAsync
    {

    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class SetupService : IDisposable, ISetupService
    {
        private InfluxDB.Client.Core.Api.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupService"/> class.
        /// **IMPORTANT** This will also create an istance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHander</see>.
        /// </summary>
        /// <returns></returns>
        public SetupService() : this((string)null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupService"/> class.
        /// **IMPORTANT** This will also create an istance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHander</see>.
        /// </summary>
        /// <param name="basePath">The target service's base path in URL format.</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        public SetupService(string basePath)
        {
            this.Configuration = InfluxDB.Client.Core.Api.Configuration.MergeConfigurations(
                InfluxDB.Client.Core.Api.GlobalConfiguration.Instance,
                new InfluxDB.Client.Core.Api.Configuration { BasePath = basePath }
            );
            this.ApiClient = new InfluxDB.Client.Core.Api.ApiClient(this.Configuration.BasePath);
            this.Client =  this.ApiClient;
            this.AsynchronousClient = this.ApiClient;
            this.ExceptionFactory = InfluxDB.Client.Core.Api.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupService"/> class using Configuration object.
        /// **IMPORTANT** This will also create an istance of HttpClient, which is less than ideal.
        /// It's better to reuse the <see href="https://docs.microsoft.com/en-us/dotnet/architecture/microservices/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests#issues-with-the-original-httpclient-class-available-in-net">HttpClient and HttpClientHander</see>.
        /// </summary>
        /// <param name="configuration">An instance of Configuration.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        public SetupService(InfluxDB.Client.Core.Api.Configuration configuration)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");

            this.Configuration = InfluxDB.Client.Core.Api.Configuration.MergeConfigurations(
                InfluxDB.Client.Core.Api.GlobalConfiguration.Instance,
                configuration
            );
            this.ApiClient = new InfluxDB.Client.Core.Api.ApiClient(this.Configuration.BasePath);
            this.Client = this.ApiClient;
            this.AsynchronousClient = this.ApiClient;
            ExceptionFactory = InfluxDB.Client.Core.Api.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupService"/> class.
        /// </summary>
        /// <param name="client">An instance of HttpClient.</param>
        /// <param name="handler">An optional instance of HttpClientHandler that is used by HttpClient.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        /// <remarks>
        /// Some configuration settings will not be applied without passing an HttpClientHandler.
        /// The features affected are: Setting and Retrieving Cookies, Client Certificates, Proxy settings.
        /// </remarks>
        public SetupService(HttpClient client, HttpClientHandler handler = null) : this(client, (string)null, handler)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupService"/> class.
        /// </summary>
        /// <param name="client">An instance of HttpClient.</param>
        /// <param name="basePath">The target service's base path in URL format.</param>
        /// <param name="handler">An optional instance of HttpClientHandler that is used by HttpClient.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        /// <returns></returns>
        /// <remarks>
        /// Some configuration settings will not be applied without passing an HttpClientHandler.
        /// The features affected are: Setting and Retrieving Cookies, Client Certificates, Proxy settings.
        /// </remarks>
        public SetupService(HttpClient client, string basePath, HttpClientHandler handler = null)
        {
            if (client == null) throw new ArgumentNullException("client");

            this.Configuration = InfluxDB.Client.Core.Api.Configuration.MergeConfigurations(
                InfluxDB.Client.Core.Api.GlobalConfiguration.Instance,
                new InfluxDB.Client.Core.Api.Configuration { BasePath = basePath }
            );
            this.ApiClient = new InfluxDB.Client.Core.Api.ApiClient(client, this.Configuration.BasePath, handler);
            this.Client =  this.ApiClient;
            this.AsynchronousClient = this.ApiClient;
            this.ExceptionFactory = InfluxDB.Client.Core.Api.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupService"/> class using Configuration object.
        /// </summary>
        /// <param name="client">An instance of HttpClient.</param>
        /// <param name="configuration">An instance of Configuration.</param>
        /// <param name="handler">An optional instance of HttpClientHandler that is used by HttpClient.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <returns></returns>
        /// <remarks>
        /// Some configuration settings will not be applied without passing an HttpClientHandler.
        /// The features affected are: Setting and Retrieving Cookies, Client Certificates, Proxy settings.
        /// </remarks>
        public SetupService(HttpClient client, InfluxDB.Client.Core.Api.Configuration configuration, HttpClientHandler handler = null)
        {
            if (configuration == null) throw new ArgumentNullException("configuration");
            if (client == null) throw new ArgumentNullException("client");

            this.Configuration = InfluxDB.Client.Core.Api.Configuration.MergeConfigurations(
                InfluxDB.Client.Core.Api.GlobalConfiguration.Instance,
                configuration
            );
            this.ApiClient = new InfluxDB.Client.Core.Api.ApiClient(client, this.Configuration.BasePath, handler);
            this.Client = this.ApiClient;
            this.AsynchronousClient = this.ApiClient;
            ExceptionFactory = InfluxDB.Client.Core.Api.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupService"/> class
        /// using a Configuration object and client instance.
        /// </summary>
        /// <param name="client">The client interface for synchronous API access.</param>
        /// <param name="asyncClient">The client interface for asynchronous API access.</param>
        /// <param name="configuration">The configuration object.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public SetupService(InfluxDB.Client.Core.Api.ISynchronousClient client, InfluxDB.Client.Core.Api.IAsynchronousClient asyncClient, InfluxDB.Client.Core.Api.IReadableConfiguration configuration)
        {
            if (client == null) throw new ArgumentNullException("client");
            if (asyncClient == null) throw new ArgumentNullException("asyncClient");
            if (configuration == null) throw new ArgumentNullException("configuration");

            this.Client = client;
            this.AsynchronousClient = asyncClient;
            this.Configuration = configuration;
            this.ExceptionFactory = InfluxDB.Client.Core.Api.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Disposes resources if they were created by us
        /// </summary>
        public void Dispose()
        {
            this.ApiClient?.Dispose();
        }

        /// <summary>
        /// Holds the ApiClient if created
        /// </summary>
        public InfluxDB.Client.Core.Api.ApiClient ApiClient { get; set; } = null;

        /// <summary>
        /// The client for accessing this underlying API asynchronously.
        /// </summary>
        public InfluxDB.Client.Core.Api.IAsynchronousClient AsynchronousClient { get; set; }

        /// <summary>
        /// The client for accessing this underlying API synchronously.
        /// </summary>
        public InfluxDB.Client.Core.Api.ISynchronousClient Client { get; set; }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public string GetBasePath()
        {
            return this.Configuration.BasePath;
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public InfluxDB.Client.Core.Api.IReadableConfiguration Configuration { get; set; }

        /// <summary>
        /// Provides a factory method hook for the creation of exceptions.
        /// </summary>
        public InfluxDB.Client.Core.Api.ExceptionFactory ExceptionFactory
        {
            get
            {
                if (_exceptionFactory != null && _exceptionFactory.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("Multicast delegate for ExceptionFactory is unsupported.");
                }
                return _exceptionFactory;
            }
            set { _exceptionFactory = value; }
        }

        /// <summary>
        /// Check if database has default user, org, bucket Returns &#x60;true&#x60; if no default user, organization, or bucket has been created.
        /// </summary>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>IsOnboarding</returns>
        public IsOnboarding GetSetup(string zapTraceSpan = default(string))
        {
            InfluxDB.Client.Core.Api.ApiResponse<IsOnboarding> localVarResponse = GetSetupWithHttpInfo(zapTraceSpan);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Check if database has default user, org, bucket Returns &#x60;true&#x60; if no default user, organization, or bucket has been created.
        /// </summary>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of IsOnboarding</returns>
        public InfluxDB.Client.Core.Api.ApiResponse<IsOnboarding> GetSetupWithHttpInfo(string zapTraceSpan = default(string))
        {
			var localVarRequestOptions = GetSetupWithRequestOptions(zapTraceSpan);

            // make the HTTP request
            var localVarResponse = this.Client.Get<IsOnboarding>("/api/v2/setup", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("GetSetup", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Check if database has default user, org, bucket Returns &#x60;true&#x60; if no default user, organization, or bucket has been created.
        /// </summary>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>RequestOptions</returns>
        public RequestOptions GetSetupWithRequestOptions(string zapTraceSpan = default(string))
        {
            InfluxDB.Client.Core.Api.RequestOptions localVarRequestOptions = new InfluxDB.Client.Core.Api.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = InfluxDB.Client.Core.Api.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = InfluxDB.Client.Core.Api.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            if (zapTraceSpan != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Zap-Trace-Span", InfluxDB.Client.Core.Api.ClientUtils.ParameterToString(zapTraceSpan)); // header parameter
            }

            // authentication (TokenAuthentication) required
            if (!string.IsNullOrEmpty(this.Configuration.GetApiKeyWithPrefix("Authorization")))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", this.Configuration.GetApiKeyWithPrefix("Authorization"));
            }

            return localVarRequestOptions;
        }

        /// <summary>
        /// Check if database has default user, org, bucket Returns &#x60;true&#x60; if no default user, organization, or bucket has been created.
        /// </summary>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of IsOnboarding</returns>
        public async System.Threading.Tasks.Task<IsOnboarding> GetSetupAsync(string zapTraceSpan = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            InfluxDB.Client.Core.Api.ApiResponse<IsOnboarding> localVarResponse = await GetSetupWithHttpInfoAsync(zapTraceSpan, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Check if database has default user, org, bucket Returns &#x60;true&#x60; if no default user, organization, or bucket has been created.
        /// </summary>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (IsOnboarding)</returns>
        public async System.Threading.Tasks.Task<InfluxDB.Client.Core.Api.ApiResponse<IsOnboarding>> GetSetupWithHttpInfoAsync(string zapTraceSpan = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {

            InfluxDB.Client.Core.Api.RequestOptions localVarRequestOptions = new InfluxDB.Client.Core.Api.RequestOptions();

            string[] _contentTypes = new string[] {
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };


            var localVarContentType = InfluxDB.Client.Core.Api.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = InfluxDB.Client.Core.Api.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            if (zapTraceSpan != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Zap-Trace-Span", InfluxDB.Client.Core.Api.ClientUtils.ParameterToString(zapTraceSpan)); // header parameter
            }

            // authentication (TokenAuthentication) required
            if (!string.IsNullOrEmpty(this.Configuration.GetApiKeyWithPrefix("Authorization")))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", this.Configuration.GetApiKeyWithPrefix("Authorization"));
            }

            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.GetAsync<IsOnboarding>("/api/v2/setup", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("GetSetup", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Set up initial user, org and bucket Post an onboarding request to set up initial user, org and bucket.
        /// </summary>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">Source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>OnboardingResponse</returns>
        public OnboardingResponse PostSetup(OnboardingRequest onboardingRequest, string zapTraceSpan = default(string))
        {
            InfluxDB.Client.Core.Api.ApiResponse<OnboardingResponse> localVarResponse = PostSetupWithHttpInfo(onboardingRequest, zapTraceSpan);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Set up initial user, org and bucket Post an onboarding request to set up initial user, org and bucket.
        /// </summary>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">Source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of OnboardingResponse</returns>
        public InfluxDB.Client.Core.Api.ApiResponse<OnboardingResponse> PostSetupWithHttpInfo(OnboardingRequest onboardingRequest, string zapTraceSpan = default(string))
        {
			var localVarRequestOptions = PostSetupWithRequestOptions(onboardingRequest, zapTraceSpan);

            // make the HTTP request
            var localVarResponse = this.Client.Post<OnboardingResponse>("/api/v2/setup", localVarRequestOptions, this.Configuration);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("PostSetup", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

        /// <summary>
        /// Set up initial user, org and bucket Post an onboarding request to set up initial user, org and bucket.
        /// </summary>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">Source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>RequestOptions</returns>
        public RequestOptions PostSetupWithRequestOptions(OnboardingRequest onboardingRequest, string zapTraceSpan = default(string))
        {
            // verify the required parameter 'onboardingRequest' is set
            if (onboardingRequest == null)
                throw new InfluxDB.Client.Core.Api.ApiException(400, "Missing required parameter 'onboardingRequest' when calling SetupService->PostSetup");

            InfluxDB.Client.Core.Api.RequestOptions localVarRequestOptions = new InfluxDB.Client.Core.Api.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };

            var localVarContentType = InfluxDB.Client.Core.Api.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = InfluxDB.Client.Core.Api.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            if (zapTraceSpan != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Zap-Trace-Span", InfluxDB.Client.Core.Api.ClientUtils.ParameterToString(zapTraceSpan)); // header parameter
            }
            localVarRequestOptions.Data = onboardingRequest;

            // authentication (TokenAuthentication) required
            if (!string.IsNullOrEmpty(this.Configuration.GetApiKeyWithPrefix("Authorization")))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", this.Configuration.GetApiKeyWithPrefix("Authorization"));
            }

            return localVarRequestOptions;
        }

        /// <summary>
        /// Set up initial user, org and bucket Post an onboarding request to set up initial user, org and bucket.
        /// </summary>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">Source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of OnboardingResponse</returns>
        public async System.Threading.Tasks.Task<OnboardingResponse> PostSetupAsync(OnboardingRequest onboardingRequest, string zapTraceSpan = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            InfluxDB.Client.Core.Api.ApiResponse<OnboardingResponse> localVarResponse = await PostSetupWithHttpInfoAsync(onboardingRequest, zapTraceSpan, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Set up initial user, org and bucket Post an onboarding request to set up initial user, org and bucket.
        /// </summary>
        /// <exception cref="InfluxDB.Client.Core.Api.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">Source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="cancellationToken">Cancellation Token to cancel the request.</param>
        /// <returns>Task of ApiResponse (OnboardingResponse)</returns>
        public async System.Threading.Tasks.Task<InfluxDB.Client.Core.Api.ApiResponse<OnboardingResponse>> PostSetupWithHttpInfoAsync(OnboardingRequest onboardingRequest, string zapTraceSpan = default(string), System.Threading.CancellationToken cancellationToken = default(System.Threading.CancellationToken))
        {
            // verify the required parameter 'onboardingRequest' is set
            if (onboardingRequest == null)
                throw new InfluxDB.Client.Core.Api.ApiException(400, "Missing required parameter 'onboardingRequest' when calling SetupService->PostSetup");


            InfluxDB.Client.Core.Api.RequestOptions localVarRequestOptions = new InfluxDB.Client.Core.Api.RequestOptions();

            string[] _contentTypes = new string[] {
                "application/json"
            };

            // to determine the Accept header
            string[] _accepts = new string[] {
                "application/json"
            };


            var localVarContentType = InfluxDB.Client.Core.Api.ClientUtils.SelectHeaderContentType(_contentTypes);
            if (localVarContentType != null) localVarRequestOptions.HeaderParameters.Add("Content-Type", localVarContentType);

            var localVarAccept = InfluxDB.Client.Core.Api.ClientUtils.SelectHeaderAccept(_accepts);
            if (localVarAccept != null) localVarRequestOptions.HeaderParameters.Add("Accept", localVarAccept);

            if (zapTraceSpan != null)
            {
                localVarRequestOptions.HeaderParameters.Add("Zap-Trace-Span", InfluxDB.Client.Core.Api.ClientUtils.ParameterToString(zapTraceSpan)); // header parameter
            }
            localVarRequestOptions.Data = onboardingRequest;

            // authentication (TokenAuthentication) required
            if (!string.IsNullOrEmpty(this.Configuration.GetApiKeyWithPrefix("Authorization")))
            {
                localVarRequestOptions.HeaderParameters.Add("Authorization", this.Configuration.GetApiKeyWithPrefix("Authorization"));
            }

            // make the HTTP request

            var localVarResponse = await this.AsynchronousClient.PostAsync<OnboardingResponse>("/api/v2/setup", localVarRequestOptions, this.Configuration, cancellationToken).ConfigureAwait(false);

            if (this.ExceptionFactory != null)
            {
                Exception _exception = this.ExceptionFactory("PostSetup", localVarResponse);
                if (_exception != null) throw _exception;
            }

            return localVarResponse;
        }

    }
}
