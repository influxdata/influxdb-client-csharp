/* 
 * InfluxDB OSS API Service
 *
 * The InfluxDB v2 API provides a programmatic interface for all interactions with InfluxDB. Access the InfluxDB API using the `/api/v2/` endpoint. 
 *
 * OpenAPI spec version: 2.0.0
 * 
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
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
        /// Get the health of an instance
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>HealthCheck</returns>
        HealthCheck GetHealth(string zapTraceSpan = null);

        /// <summary>
        /// Get the health of an instance
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of HealthCheck</returns>
        ApiResponse<HealthCheck> GetHealthWithHttpInfo(string zapTraceSpan = null);

        #endregion Synchronous Operations

        #region Asynchronous Operations

        /// <summary>
        /// Get the health of an instance
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task of HealthCheck</returns>
        System.Threading.Tasks.Task<HealthCheck> GetHealthAsync(string zapTraceSpan = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get the health of an instance
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task of ApiResponse (HealthCheck)</returns>
        System.Threading.Tasks.Task<ApiResponse<HealthCheck>> GetHealthAsyncWithHttpInfo(string zapTraceSpan = null,
            CancellationToken cancellationToken = default);

        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class HealthService : IHealthService
    {
        private ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthService"/> class.
        /// </summary>
        /// <returns></returns>
        public HealthService(string basePath)
        {
            Configuration = new Configuration {BasePath = basePath};

            ExceptionFactory = Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="HealthService"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public HealthService(Configuration configuration = null)
        {
            if (configuration == null) // use the default one in Configuration
            {
                Configuration = Configuration.Default;
            }
            else
            {
                Configuration = configuration;
            }

            ExceptionFactory = Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Gets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        public string GetBasePath()
        {
            return Configuration.ApiClient.RestClientOptions.BaseUrl.ToString();
        }

        /// <summary>
        /// Sets the base path of the API client.
        /// </summary>
        /// <value>The base path</value>
        [Obsolete(
            "SetBasePath is deprecated, please do 'Configuration.ApiClient = new ApiClient(\"http://new-path\")' instead.")]
        public void SetBasePath(string basePath)
        {
            // do nothing
        }

        /// <summary>
        /// Gets or sets the configuration object
        /// </summary>
        /// <value>An instance of the Configuration</value>
        public Configuration Configuration { get; set; }

        /// <summary>
        /// Provides a factory method hook for the creation of exceptions.
        /// </summary>
        public ExceptionFactory ExceptionFactory
        {
            get
            {
                if (_exceptionFactory != null && _exceptionFactory.GetInvocationList().Length > 1)
                {
                    throw new InvalidOperationException("Multicast delegate for ExceptionFactory is unsupported.");
                }

                return _exceptionFactory;
            }
            set => _exceptionFactory = value;
        }

        /// <summary>
        /// Gets the default header.
        /// </summary>
        /// <returns>Dictionary of HTTP header</returns>
        [Obsolete("DefaultHeader is deprecated, please use Configuration.DefaultHeader instead.")]
        public IDictionary<string, string> DefaultHeader()
        {
            return new ReadOnlyDictionary<string, string>(Configuration.DefaultHeader);
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
            Configuration.AddDefaultHeader(key, value);
        }

        /// <summary>
        /// Get the health of an instance 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>HealthCheck</returns>
        public HealthCheck GetHealth(string zapTraceSpan = null)
        {
            var localVarResponse = GetHealthWithHttpInfo(zapTraceSpan);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get the health of an instance 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of HealthCheck</returns>
        public ApiResponse<HealthCheck> GetHealthWithHttpInfo(string zapTraceSpan = null)
        {
            var localVarPath = "/health";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new List<KeyValuePair<string, string>>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            var localVarHttpContentTypes = new string[]
            {
            };
            var localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            if (zapTraceSpan != null)
            {
                localVarHeaderParams.Add("Zap-Trace-Span",
                    Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            }

            // to determine the Accept header
            var localVarHttpHeaderAccepts = new string[]
            {
                "application/json"
            };

            var localVarHttpHeaderAccept = Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null && !localVarHeaderParams.ContainsKey("Accept"))
            {
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);
            }


            // make the HTTP request
            var localVarResponse = (RestResponse) Configuration.ApiClient.CallApi(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            var localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                var exception = ExceptionFactory("GetHealth", localVarResponse);
                if (exception != null)
                {
                    throw exception;
                }
            }

            return new ApiResponse<HealthCheck>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (HealthCheck) Configuration.ApiClient.Deserialize(localVarResponse, typeof(HealthCheck)));
        }

        /// <summary>
        /// Get the health of an instance 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ApiResponse of HealthCheck</returns>
        public async System.Threading.Tasks.Task<RestResponse> GetHealthWithIRestResponseAsync(
            string zapTraceSpan = null, CancellationToken cancellationToken = default)
        {
            var localVarPath = "/health";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new List<KeyValuePair<string, string>>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            var localVarHttpContentTypes = new string[]
            {
            };
            var localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            if (zapTraceSpan != null)
            {
                localVarHeaderParams.Add("Zap-Trace-Span",
                    Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            }

            // to determine the Accept header
            var localVarHttpHeaderAccepts = new string[]
            {
                "application/json"
            };

            var localVarHttpHeaderAccept = Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null && !localVarHeaderParams.ContainsKey("Accept"))
            {
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);
            }


            // make the HTTP request
            var localVarResponse = (RestResponse) await Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams,
                localVarPathParams, localVarHttpContentType, cancellationToken).ConfigureAwait(false);

            var localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                var exception = ExceptionFactory("GetHealth", localVarResponse);
                if (exception != null)
                {
                    throw exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get the health of an instance 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of HealthCheck</returns>
        public RestResponse GetHealthWithIRestResponse(string zapTraceSpan = null)
        {
            var localVarPath = "/health";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new List<KeyValuePair<string, string>>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            var localVarHttpContentTypes = new string[]
            {
            };
            var localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            if (zapTraceSpan != null)
            {
                localVarHeaderParams.Add("Zap-Trace-Span",
                    Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            }

            // to determine the Accept header
            var localVarHttpHeaderAccepts = new string[]
            {
                "application/json"
            };

            var localVarHttpHeaderAccept = Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null && !localVarHeaderParams.ContainsKey("Accept"))
            {
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);
            }


            // make the HTTP request
            var localVarResponse = (RestResponse) Configuration.ApiClient.CallApi(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            var localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                var exception = ExceptionFactory("GetHealth", localVarResponse);
                if (exception != null)
                {
                    throw exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Get the health of an instance 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <returns>ApiResponse of HealthCheck</returns>
        public RestRequest GetHealthWithRestRequest(string zapTraceSpan = null)
        {
            var localVarPath = "/health";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new List<KeyValuePair<string, string>>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            var localVarHttpContentTypes = new string[]
            {
            };
            var localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            if (zapTraceSpan != null)
            {
                localVarHeaderParams.Add("Zap-Trace-Span",
                    Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            }

            // to determine the Accept header
            var localVarHttpHeaderAccepts = new string[]
            {
                "application/json"
            };

            var localVarHttpHeaderAccept = Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null && !localVarHeaderParams.ContainsKey("Accept"))
            {
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);
            }


            return Configuration.ApiClient.PrepareRequest(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams,
                localVarPathParams, localVarHttpContentType);
        }

        /// <summary>
        /// Get the health of an instance 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task of HealthCheck</returns>
        public async System.Threading.Tasks.Task<HealthCheck> GetHealthAsync(string zapTraceSpan = null,
            CancellationToken cancellationToken = default)
        {
            var localVarResponse =
                await GetHealthAsyncWithHttpInfo(zapTraceSpan, cancellationToken).ConfigureAwait(false);
            return localVarResponse.Data;
        }

        /// <summary>
        /// Get the health of an instance 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task of ApiResponse (HealthCheck)</returns>
        public async System.Threading.Tasks.Task<ApiResponse<HealthCheck>> GetHealthAsyncWithHttpInfo(
            string zapTraceSpan = null, CancellationToken cancellationToken = default)
        {
            // make the HTTP request
            var localVarResponse =
                await GetHealthAsyncWithIRestResponse(zapTraceSpan, cancellationToken).ConfigureAwait(false);

            var localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                var exception = ExceptionFactory("GetHealth", localVarResponse);
                if (exception != null)
                {
                    throw exception;
                }
            }

            return new ApiResponse<HealthCheck>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                (HealthCheck) Configuration.ApiClient.Deserialize(localVarResponse, typeof(HealthCheck)));
        }

        /// <summary>
        /// Get the health of an instance 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task of RestResponse (HealthCheck)</returns>
        public async System.Threading.Tasks.Task<RestResponse> GetHealthAsyncWithIRestResponse(
            string zapTraceSpan = null, CancellationToken cancellationToken = default)
        {
            var localVarPath = "/health";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new List<KeyValuePair<string, string>>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            var localVarHttpContentTypes = new string[]
            {
            };
            var localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            if (zapTraceSpan != null)
            {
                localVarHeaderParams.Add("Zap-Trace-Span",
                    Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            }

            // to determine the Accept header
            var localVarHttpHeaderAccepts = new string[]
            {
                "application/json"
            };

            var localVarHttpHeaderAccept = Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null && !localVarHeaderParams.ContainsKey("Accept"))
            {
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);
            }


            // make the HTTP request
            var localVarResponse = (RestResponse) await Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.Get, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams,
                localVarPathParams, localVarHttpContentType, cancellationToken).ConfigureAwait(false);

            if (ExceptionFactory != null)
            {
                var exception = ExceptionFactory("GetHealth", localVarResponse);
                if (exception != null)
                {
                    throw exception;
                }
            }

            return localVarResponse;
        }
    }
}