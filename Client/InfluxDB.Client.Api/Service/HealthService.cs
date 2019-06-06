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
    public interface IHealthService : IApiAccessor
    {
        #region Synchronous Operations
        /// <summary>
        /// Get the health of an instance anytime during execution. Allow us to check if the instance is still healthy.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>Check</returns>
        Check GetHealth (string zapTraceSpan = null);

        /// <summary>
        /// Get the health of an instance anytime during execution. Allow us to check if the instance is still healthy.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of Check</returns>
        ApiResponse<Check> GetHealthWithHttpInfo (string zapTraceSpan = null);
        #endregion Synchronous Operations
        #region Asynchronous Operations
        /// <summary>
        /// Get the health of an instance anytime during execution. Allow us to check if the instance is still healthy.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>Task of Check</returns>
        System.Threading.Tasks.Task<Check> GetHealthAsync (string zapTraceSpan = null);

        /// <summary>
        /// Get the health of an instance anytime during execution. Allow us to check if the instance is still healthy.
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>Task of ApiResponse (Check)</returns>
        System.Threading.Tasks.Task<ApiResponse<Check>> GetHealthAsyncWithHttpInfo (string zapTraceSpan = null);
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class HealthService : IHealthService
    {
        private InfluxDB.Client.Api.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthService"/> class.
        /// </summary>
        /// <returns></returns>
        public HealthService(String basePath)
        {
            this.Configuration = new InfluxDB.Client.Api.Client.Configuration { BasePath = basePath };

            ExceptionFactory = InfluxDB.Client.Api.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthService"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public HealthService(InfluxDB.Client.Api.Client.Configuration configuration = null)
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
        /// Get the health of an instance anytime during execution. Allow us to check if the instance is still healthy. 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>Check</returns>
        public Check GetHealth (string zapTraceSpan = null)
        {
             ApiResponse<Check> localVarResponse = GetHealthWithHttpInfo(zapTraceSpan);
             return localVarResponse.Data;
        }

        /// <summary>
        /// Get the health of an instance anytime during execution. Allow us to check if the instance is still healthy. 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of Check</returns>
        public ApiResponse< Check > GetHealthWithHttpInfo (string zapTraceSpan = null)
        {

            var localVarPath = "/health";
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

            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };

            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null && !localVarHeaderParams.ContainsKey("Accept"))
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) this.Configuration.ApiClient.CallApi(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("GetHealth", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<Check>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Check) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(Check)));
        }

        /// <summary>
        /// Get the health of an instance anytime during execution. Allow us to check if the instance is still healthy. 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of Check</returns>
        public async System.Threading.Tasks.Task<IRestResponse> GetHealthWithIRestResponseAsync (string zapTraceSpan = null)
        {

            var localVarPath = "/health";
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

            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };

            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null && !localVarHeaderParams.ContainsKey("Accept"))
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) await this.Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("GetHealth", localVarResponse);
                if (exception != null) throw exception;
            }

            return localVarResponse;
        }
        
        /// <summary>
        /// Get the health of an instance anytime during execution. Allow us to check if the instance is still healthy. 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of Check</returns>
        public RestRequest GetHealthWithRestRequest (string zapTraceSpan = null)
        {

            var localVarPath = "/health";
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

            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };

            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null && !localVarHeaderParams.ContainsKey("Accept"))
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);


            return this.Configuration.ApiClient.PrepareRequest(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);
        }

        /// <summary>
        /// Get the health of an instance anytime during execution. Allow us to check if the instance is still healthy. 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>Task of Check</returns>
        public async System.Threading.Tasks.Task<Check> GetHealthAsync (string zapTraceSpan = null)
        {
             ApiResponse<Check> localVarResponse = await GetHealthAsyncWithHttpInfo(zapTraceSpan);
             return localVarResponse.Data;

        }

        /// <summary>
        /// Get the health of an instance anytime during execution. Allow us to check if the instance is still healthy. 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>Task of ApiResponse (Check)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<Check>> GetHealthAsyncWithHttpInfo (string zapTraceSpan = null)
        {

            var localVarPath = "/health";
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

            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };

            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null && !localVarHeaderParams.ContainsKey("Accept"))
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);


            // make the HTTP request
            IRestResponse localVarResponse = (IRestResponse) await this.Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.GET, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("GetHealth", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<Check>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (Check) this.Configuration.ApiClient.Deserialize(localVarResponse, typeof(Check)));
        }

    }
}
