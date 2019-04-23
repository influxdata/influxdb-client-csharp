/* 
 * Influx API Service
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * OpenAPI spec version: 0.1.0
 * 
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using RestSharp;
using InfluxDB.Client.Api.Client;
using InfluxDB.Client.Api.Domain;

namespace InfluxDB.Client.Api.Service
{
    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public interface ISetupService : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// check if database has default user, org, bucket created, returns true if not.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>IsOnboarding</returns>
        IsOnboarding SetupGet (string zapTraceSpan = null);

        /// <summary>
        /// check if database has default user, org, bucket created, returns true if not.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of IsOnboarding</returns>
        ApiResponse<IsOnboarding> SetupGetWithHttpInfo (string zapTraceSpan = null);
        /// <summary>
        /// post onboarding request, to setup initial user, org and bucket
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>OnboardingResponse</returns>
        OnboardingResponse SetupPost (OnboardingRequest onboardingRequest, string zapTraceSpan = null);

        /// <summary>
        /// post onboarding request, to setup initial user, org and bucket
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of OnboardingResponse</returns>
        ApiResponse<OnboardingResponse> SetupPostWithHttpInfo (OnboardingRequest onboardingRequest, string zapTraceSpan = null);
        #endregion Synchronous Operations
        #region Asynchronous Operations
        /// <summary>
        /// check if database has default user, org, bucket created, returns true if not.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>Task of IsOnboarding</returns>
        System.Threading.Tasks.Task<IsOnboarding> SetupGetAsync (string zapTraceSpan = null);

        /// <summary>
        /// check if database has default user, org, bucket created, returns true if not.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>Task of ApiResponse (IsOnboarding)</returns>
        System.Threading.Tasks.Task<ApiResponse<IsOnboarding>> SetupGetAsyncWithHttpInfo (string zapTraceSpan = null);
        /// <summary>
        /// post onboarding request, to setup initial user, org and bucket
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>Task of OnboardingResponse</returns>
        System.Threading.Tasks.Task<OnboardingResponse> SetupPostAsync (OnboardingRequest onboardingRequest, string zapTraceSpan = null);

        /// <summary>
        /// post onboarding request, to setup initial user, org and bucket
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>Task of ApiResponse (OnboardingResponse)</returns>
        System.Threading.Tasks.Task<ApiResponse<OnboardingResponse>> SetupPostAsyncWithHttpInfo (OnboardingRequest onboardingRequest, string zapTraceSpan = null);
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class SetupService : ISetupService
    {
        private InfluxDB.Client.Api.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupService"/> class.
        /// </summary>
        /// <returns></returns>
        public SetupService(String basePath)
        {
            this.Configuration = new InfluxDB.Client.Api.Client.Configuration { BasePath = basePath };

            ExceptionFactory = InfluxDB.Client.Api.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SetupService"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public SetupService(InfluxDB.Client.Api.Client.Configuration configuration = null)
        {
            if (configuration == null) // use the default one in Configuration
                this.Configuration = InfluxDB.Client.Api.Client.Configuration.Default;
            else
                this.Configuration = configuration;

            ExceptionFactory = InfluxDB.Client.Api.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public String GetBasePath()
        {
            return this.Configuration.ApiClient.RestClient.BaseUrl.ToString();
        }

        /// <summary>
        /// Sets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        [Obsolete("SetBasePath is deprecated, please do 'Configuration.ApiClient = new ApiClient(\"http://new-path\")' instead.")]
        public void SetBasePath(String basePath)
        {
            // do nothing
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public InfluxDB.Client.Api.Client.Configuration Configuration {get; set;}

        /// <summary>
        /// Provides a factory method hook for the creation of exceptions.
        /// </summary>
        public InfluxDB.Client.Api.Client.ExceptionFactory ExceptionFactory
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
        /// Gets the default header.
        /// </summary>
        /// <returns>Dictionary of HTTP header</returns>
        [Obsolete("DefaultHeader is deprecated, please use Configuration.DefaultHeader instead.")]
        public IDictionary<String, String> DefaultHeader()
        {
            return new ReadOnlyDictionary<string, string>(this.Configuration.DefaultHeader);
        }

        /// <summary>
        /// Add default header.
        /// </summary>
        /// <param name="key">Header field name.</param>
        /// <param name="value">Header field value.</param>
        /// <returns></returns>
        [Obsolete("AddDefaultHeader is deprecated, please use Configuration.AddDefaultHeader instead.")]
        public void AddDefaultHeader(string key, string value)
        {
            this.Configuration.AddDefaultHeader(key, value);
        }

        /// <summary>
        /// check if database has default user, org, bucket created, returns true if not. 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>IsOnboarding</returns>
        public IsOnboarding SetupGet (string zapTraceSpan = null)
        {
             ApiResponse<IsOnboarding> localVarResponse = SetupGetWithHttpInfo(zapTraceSpan);
             return localVarResponse.Data;
        }

        /// <summary>
        /// check if database has default user, org, bucket created, returns true if not. 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of IsOnboarding</returns>
        public ApiResponse< IsOnboarding > SetupGetWithHttpInfo (string zapTraceSpan = null)
        {

            var localVarPath = "/api/v2/setup";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) this.Configuration.ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("SetupGet", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<IsOnboarding>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (IsOnboarding) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(IsOnboarding)));
        }

        /// <summary>
        /// check if database has default user, org, bucket created, returns true if not. 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of IsOnboarding</returns>
        public IRestResponse SetupGetWithIRestResponse (string zapTraceSpan = null)
        {

            var localVarPath = "/api/v2/setup";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) this.Configuration.ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("SetupGet", localVarResponse);
                if (exception != null) throw exception;
            }

            return localVarResponse;
        }
        
        /// <summary>
        /// check if database has default user, org, bucket created, returns true if not. 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of IsOnboarding</returns>
        public RestRequest SetupGetWithRestRequest (string zapTraceSpan = null)
        {

            var localVarPath = "/api/v2/setup";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter


            return this.Configuration.ApiClient.PrepareRequest(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);
        }

        /// <summary>
        /// check if database has default user, org, bucket created, returns true if not. 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>Task of IsOnboarding</returns>
        public async System.Threading.Tasks.Task<IsOnboarding> SetupGetAsync (string zapTraceSpan = null)
        {
             ApiResponse<IsOnboarding> localVarResponse = await SetupGetAsyncWithHttpInfo(zapTraceSpan);
             return localVarResponse.Data;

        }

        /// <summary>
        /// check if database has default user, org, bucket created, returns true if not. 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>Task of ApiResponse (IsOnboarding)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<IsOnboarding>> SetupGetAsyncWithHttpInfo (string zapTraceSpan = null)
        {

            var localVarPath = "/api/v2/setup";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) await this.Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("SetupGet", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<IsOnboarding>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (IsOnboarding) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(IsOnboarding)));
        }

        /// <summary>
        /// post onboarding request, to setup initial user, org and bucket 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>OnboardingResponse</returns>
        public OnboardingResponse SetupPost (OnboardingRequest onboardingRequest, string zapTraceSpan = null)
        {
             ApiResponse<OnboardingResponse> localVarResponse = SetupPostWithHttpInfo(onboardingRequest, zapTraceSpan);
             return localVarResponse.Data;
        }

        /// <summary>
        /// post onboarding request, to setup initial user, org and bucket 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of OnboardingResponse</returns>
        public ApiResponse< OnboardingResponse > SetupPostWithHttpInfo (OnboardingRequest onboardingRequest, string zapTraceSpan = null)
        {
            // verify the required parameter 'onboardingRequest' is set
            if (onboardingRequest == null)
                throw new ApiException(400, "Missing required parameter 'onboardingRequest' when calling SetupService->SetupPost");

            var localVarPath = "/api/v2/setup";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
                "application/json"
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            if (onboardingRequest != null && onboardingRequest.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(onboardingRequest); // http body (model) parameter
            }
            else
            {
                localVarPostBody = onboardingRequest; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) this.Configuration.ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("SetupPost", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<OnboardingResponse>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (OnboardingResponse) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(OnboardingResponse)));
        }

        /// <summary>
        /// post onboarding request, to setup initial user, org and bucket 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of OnboardingResponse</returns>
        public IRestResponse SetupPostWithIRestResponse (OnboardingRequest onboardingRequest, string zapTraceSpan = null)
        {
            // verify the required parameter 'onboardingRequest' is set
            if (onboardingRequest == null)
                throw new ApiException(400, "Missing required parameter 'onboardingRequest' when calling SetupService->SetupPost");

            var localVarPath = "/api/v2/setup";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
                "application/json"
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            if (onboardingRequest != null && onboardingRequest.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(onboardingRequest); // http body (model) parameter
            }
            else
            {
                localVarPostBody = onboardingRequest; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) this.Configuration.ApiClient.CallApi(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("SetupPost", localVarResponse);
                if (exception != null) throw exception;
            }

            return localVarResponse;
        }
        
        /// <summary>
        /// post onboarding request, to setup initial user, org and bucket 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of OnboardingResponse</returns>
        public RestRequest SetupPostWithRestRequest (OnboardingRequest onboardingRequest, string zapTraceSpan = null)
        {
            // verify the required parameter 'onboardingRequest' is set
            if (onboardingRequest == null)
                throw new ApiException(400, "Missing required parameter 'onboardingRequest' when calling SetupService->SetupPost");

            var localVarPath = "/api/v2/setup";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
                "application/json"
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            if (onboardingRequest != null && onboardingRequest.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(onboardingRequest); // http body (model) parameter
            }
            else
            {
                localVarPostBody = onboardingRequest; // byte array
            }


            return this.Configuration.ApiClient.PrepareRequest(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);
        }

        /// <summary>
        /// post onboarding request, to setup initial user, org and bucket 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>Task of OnboardingResponse</returns>
        public async System.Threading.Tasks.Task<OnboardingResponse> SetupPostAsync (OnboardingRequest onboardingRequest, string zapTraceSpan = null)
        {
             ApiResponse<OnboardingResponse> localVarResponse = await SetupPostAsyncWithHttpInfo(onboardingRequest, zapTraceSpan);
             return localVarResponse.Data;

        }

        /// <summary>
        /// post onboarding request, to setup initial user, org and bucket 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="onboardingRequest">source to create</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>Task of ApiResponse (OnboardingResponse)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<OnboardingResponse>> SetupPostAsyncWithHttpInfo (OnboardingRequest onboardingRequest, string zapTraceSpan = null)
        {
            // verify the required parameter 'onboardingRequest' is set
            if (onboardingRequest == null)
                throw new ApiException(400, "Missing required parameter 'onboardingRequest' when calling SetupService->SetupPost");

            var localVarPath = "/api/v2/setup";
            var localVarPathParams = new Dictionary<String, String>();
            var localVarQueryParams = new List<KeyValuePair<String, String>>();
            var localVarHeaderParams = new Dictionary<String, String>(this.Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<String, String>();
            var localVarFileParams = new Dictionary<String, FileParameter>();
            Object localVarPostBody = null;

            // to determine the Content-Type header
            String[] localVarHttpContentTypes = new String[] {
                "application/json"
            };
            String localVarHttpContentType = this.Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };
            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null)
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);

            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            if (onboardingRequest != null && onboardingRequest.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(onboardingRequest); // http body (model) parameter
            }
            else
            {
                localVarPostBody = onboardingRequest; // byte array
            }


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) await this.Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.POST, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("SetupPost", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<OnboardingResponse>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (OnboardingResponse) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(OnboardingResponse)));
        }

    }
}
