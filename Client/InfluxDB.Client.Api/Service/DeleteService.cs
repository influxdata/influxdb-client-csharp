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
    public interface IDeleteService : IApiAccessor
    {
        #region Synchronous Operations

        /// <summary>
        /// Delete data
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="deletePredicateRequest">Deletes data from an InfluxDB bucket.</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="org">Specifies the organization to delete data from. (optional)</param>
        /// <param name="bucket">Specifies the bucket to delete data from. (optional)</param>
        /// <param name="orgID">Specifies the organization ID of the resource. (optional)</param>
        /// <param name="bucketID">Specifies the bucket ID to delete data from. (optional)</param>
        /// <returns></returns>
        void PostDelete(DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null,
            string bucket = null, string orgID = null, string bucketID = null);

        /// <summary>
        /// Delete data
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="deletePredicateRequest">Deletes data from an InfluxDB bucket.</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="org">Specifies the organization to delete data from. (optional)</param>
        /// <param name="bucket">Specifies the bucket to delete data from. (optional)</param>
        /// <param name="orgID">Specifies the organization ID of the resource. (optional)</param>
        /// <param name="bucketID">Specifies the bucket ID to delete data from. (optional)</param>
        /// <returns>ApiResponse of Object(void)</returns>
        ApiResponse<object> PostDeleteWithHttpInfo(DeletePredicateRequest deletePredicateRequest,
            string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null,
            string bucketID = null);

        #endregion Synchronous Operations

        #region Asynchronous Operations

        /// <summary>
        /// Delete data
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="deletePredicateRequest">Deletes data from an InfluxDB bucket.</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="org">Specifies the organization to delete data from. (optional)</param>
        /// <param name="bucket">Specifies the bucket to delete data from. (optional)</param>
        /// <param name="orgID">Specifies the organization ID of the resource. (optional)</param>
        /// <param name="bucketID">Specifies the bucket ID to delete data from. (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task of void</returns>
        System.Threading.Tasks.Task PostDeleteAsync(DeletePredicateRequest deletePredicateRequest,
            string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null,
            string bucketID = null, CancellationToken cancellationToken = default);

        /// <summary>
        /// Delete data
        /// </summary>
        /// <remarks>
        /// 
        /// </remarks>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="deletePredicateRequest">Deletes data from an InfluxDB bucket.</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="org">Specifies the organization to delete data from. (optional)</param>
        /// <param name="bucket">Specifies the bucket to delete data from. (optional)</param>
        /// <param name="orgID">Specifies the organization ID of the resource. (optional)</param>
        /// <param name="bucketID">Specifies the bucket ID to delete data from. (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task of ApiResponse</returns>
        System.Threading.Tasks.Task<ApiResponse<object>> PostDeleteAsyncWithHttpInfo(
            DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null,
            string bucket = null, string orgID = null, string bucketID = null,
            CancellationToken cancellationToken = default);

        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class DeleteService : IDeleteService
    {
        private ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteService"/> class.
        /// </summary>
        /// <returns></returns>
        public DeleteService(string basePath)
        {
            Configuration = new Configuration {BasePath = basePath};

            ExceptionFactory = Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteService"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public DeleteService(Configuration configuration = null)
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
        /// Delete data 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="deletePredicateRequest">Deletes data from an InfluxDB bucket.</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="org">Specifies the organization to delete data from. (optional)</param>
        /// <param name="bucket">Specifies the bucket to delete data from. (optional)</param>
        /// <param name="orgID">Specifies the organization ID of the resource. (optional)</param>
        /// <param name="bucketID">Specifies the bucket ID to delete data from. (optional)</param>
        /// <returns></returns>
        public void PostDelete(DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null,
            string org = null, string bucket = null, string orgID = null, string bucketID = null)
        {
            PostDeleteWithHttpInfo(deletePredicateRequest, zapTraceSpan, org, bucket, orgID, bucketID);
        }

        /// <summary>
        /// Delete data 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="deletePredicateRequest">Deletes data from an InfluxDB bucket.</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="org">Specifies the organization to delete data from. (optional)</param>
        /// <param name="bucket">Specifies the bucket to delete data from. (optional)</param>
        /// <param name="orgID">Specifies the organization ID of the resource. (optional)</param>
        /// <param name="bucketID">Specifies the bucket ID to delete data from. (optional)</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public ApiResponse<object> PostDeleteWithHttpInfo(DeletePredicateRequest deletePredicateRequest,
            string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null,
            string bucketID = null)
        {
            // verify the required parameter 'deletePredicateRequest' is set
            if (deletePredicateRequest == null)
            {
                throw new ApiException(400,
                    "Missing required parameter 'deletePredicateRequest' when calling DeleteService->PostDelete");
            }

            var localVarPath = "/api/v2/delete";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new List<KeyValuePair<string, string>>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            var localVarHttpContentTypes = new string[]
            {
                "application/json"
            };
            var localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            if (org != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "org", org)); // query parameter
            }

            if (bucket != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "bucket", bucket)); // query parameter
            }

            if (orgID != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "orgID", orgID)); // query parameter
            }

            if (bucketID != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "bucketID", bucketID)); // query parameter
            }

            if (zapTraceSpan != null)
            {
                localVarHeaderParams.Add("Zap-Trace-Span",
                    Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            }

            if (deletePredicateRequest != null && deletePredicateRequest.GetType() != typeof(byte[]))
            {
                localVarPostBody =
                    Configuration.ApiClient.Serialize(deletePredicateRequest); // http body (model) parameter
            }
            else
            {
                localVarPostBody = deletePredicateRequest; // byte array
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
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            var localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                var exception = ExceptionFactory("PostDelete", localVarResponse);
                if (exception != null)
                {
                    throw exception;
                }
            }

            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Delete data 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="deletePredicateRequest">Deletes data from an InfluxDB bucket.</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="org">Specifies the organization to delete data from. (optional)</param>
        /// <param name="bucket">Specifies the bucket to delete data from. (optional)</param>
        /// <param name="orgID">Specifies the organization ID of the resource. (optional)</param>
        /// <param name="bucketID">Specifies the bucket ID to delete data from. (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public async System.Threading.Tasks.Task<RestResponse> PostDeleteWithIRestResponseAsync(
            DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null,
            string bucket = null, string orgID = null, string bucketID = null,
            CancellationToken cancellationToken = default)
        {
            // verify the required parameter 'deletePredicateRequest' is set
            if (deletePredicateRequest == null)
            {
                throw new ApiException(400,
                    "Missing required parameter 'deletePredicateRequest' when calling DeleteService->PostDelete");
            }

            var localVarPath = "/api/v2/delete";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new List<KeyValuePair<string, string>>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            var localVarHttpContentTypes = new string[]
            {
                "application/json"
            };
            var localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            if (org != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "org", org)); // query parameter
            }

            if (bucket != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "bucket", bucket)); // query parameter
            }

            if (orgID != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "orgID", orgID)); // query parameter
            }

            if (bucketID != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "bucketID", bucketID)); // query parameter
            }

            if (zapTraceSpan != null)
            {
                localVarHeaderParams.Add("Zap-Trace-Span",
                    Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            }

            if (deletePredicateRequest != null && deletePredicateRequest.GetType() != typeof(byte[]))
            {
                localVarPostBody =
                    Configuration.ApiClient.Serialize(deletePredicateRequest); // http body (model) parameter
            }
            else
            {
                localVarPostBody = deletePredicateRequest; // byte array
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
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams,
                localVarPathParams, localVarHttpContentType, cancellationToken).ConfigureAwait(false);

            var localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                var exception = ExceptionFactory("PostDelete", localVarResponse);
                if (exception != null)
                {
                    throw exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Delete data 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="deletePredicateRequest">Deletes data from an InfluxDB bucket.</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="org">Specifies the organization to delete data from. (optional)</param>
        /// <param name="bucket">Specifies the bucket to delete data from. (optional)</param>
        /// <param name="orgID">Specifies the organization ID of the resource. (optional)</param>
        /// <param name="bucketID">Specifies the bucket ID to delete data from. (optional)</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public RestResponse PostDeleteWithIRestResponse(DeletePredicateRequest deletePredicateRequest,
            string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null,
            string bucketID = null)
        {
            // verify the required parameter 'deletePredicateRequest' is set
            if (deletePredicateRequest == null)
            {
                throw new ApiException(400,
                    "Missing required parameter 'deletePredicateRequest' when calling DeleteService->PostDelete");
            }

            var localVarPath = "/api/v2/delete";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new List<KeyValuePair<string, string>>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            var localVarHttpContentTypes = new string[]
            {
                "application/json"
            };
            var localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            if (org != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "org", org)); // query parameter
            }

            if (bucket != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "bucket", bucket)); // query parameter
            }

            if (orgID != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "orgID", orgID)); // query parameter
            }

            if (bucketID != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "bucketID", bucketID)); // query parameter
            }

            if (zapTraceSpan != null)
            {
                localVarHeaderParams.Add("Zap-Trace-Span",
                    Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            }

            if (deletePredicateRequest != null && deletePredicateRequest.GetType() != typeof(byte[]))
            {
                localVarPostBody =
                    Configuration.ApiClient.Serialize(deletePredicateRequest); // http body (model) parameter
            }
            else
            {
                localVarPostBody = deletePredicateRequest; // byte array
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
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            var localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                var exception = ExceptionFactory("PostDelete", localVarResponse);
                if (exception != null)
                {
                    throw exception;
                }
            }

            return localVarResponse;
        }

        /// <summary>
        /// Delete data 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="deletePredicateRequest">Deletes data from an InfluxDB bucket.</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="org">Specifies the organization to delete data from. (optional)</param>
        /// <param name="bucket">Specifies the bucket to delete data from. (optional)</param>
        /// <param name="orgID">Specifies the organization ID of the resource. (optional)</param>
        /// <param name="bucketID">Specifies the bucket ID to delete data from. (optional)</param>
        /// <returns>ApiResponse of Object(void)</returns>
        public RestRequest PostDeleteWithRestRequest(DeletePredicateRequest deletePredicateRequest,
            string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null,
            string bucketID = null)
        {
            // verify the required parameter 'deletePredicateRequest' is set
            if (deletePredicateRequest == null)
            {
                throw new ApiException(400,
                    "Missing required parameter 'deletePredicateRequest' when calling DeleteService->PostDelete");
            }

            var localVarPath = "/api/v2/delete";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new List<KeyValuePair<string, string>>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            var localVarHttpContentTypes = new string[]
            {
                "application/json"
            };
            var localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            if (org != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "org", org)); // query parameter
            }

            if (bucket != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "bucket", bucket)); // query parameter
            }

            if (orgID != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "orgID", orgID)); // query parameter
            }

            if (bucketID != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "bucketID", bucketID)); // query parameter
            }

            if (zapTraceSpan != null)
            {
                localVarHeaderParams.Add("Zap-Trace-Span",
                    Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            }

            if (deletePredicateRequest != null && deletePredicateRequest.GetType() != typeof(byte[]))
            {
                localVarPostBody =
                    Configuration.ApiClient.Serialize(deletePredicateRequest); // http body (model) parameter
            }
            else
            {
                localVarPostBody = deletePredicateRequest; // byte array
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
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams,
                localVarPathParams, localVarHttpContentType);
        }

        /// <summary>
        /// Delete data 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="deletePredicateRequest">Deletes data from an InfluxDB bucket.</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="org">Specifies the organization to delete data from. (optional)</param>
        /// <param name="bucket">Specifies the bucket to delete data from. (optional)</param>
        /// <param name="orgID">Specifies the organization ID of the resource. (optional)</param>
        /// <param name="bucketID">Specifies the bucket ID to delete data from. (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task of void</returns>
        public System.Threading.Tasks.Task PostDeleteAsync(DeletePredicateRequest deletePredicateRequest,
            string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null,
            string bucketID = null, CancellationToken cancellationToken = default)
        {
            return PostDeleteAsyncWithHttpInfo(deletePredicateRequest, zapTraceSpan, org, bucket, orgID, bucketID,
                cancellationToken);
        }

        /// <summary>
        /// Delete data 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="deletePredicateRequest">Deletes data from an InfluxDB bucket.</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="org">Specifies the organization to delete data from. (optional)</param>
        /// <param name="bucket">Specifies the bucket to delete data from. (optional)</param>
        /// <param name="orgID">Specifies the organization ID of the resource. (optional)</param>
        /// <param name="bucketID">Specifies the bucket ID to delete data from. (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task of ApiResponse</returns>
        public async System.Threading.Tasks.Task<ApiResponse<object>> PostDeleteAsyncWithHttpInfo(
            DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null,
            string bucket = null, string orgID = null, string bucketID = null,
            CancellationToken cancellationToken = default)
        {
            // make the HTTP request
            var localVarResponse = await PostDeleteAsyncWithIRestResponse(deletePredicateRequest, zapTraceSpan, org,
                bucket, orgID, bucketID, cancellationToken).ConfigureAwait(false);

            var localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                var exception = ExceptionFactory("PostDelete", localVarResponse);
                if (exception != null)
                {
                    throw exception;
                }
            }

            return new ApiResponse<object>(localVarStatusCode,
                localVarResponse.Headers.ToDictionary(x => x.Name, x => x.Value.ToString()),
                null);
        }

        /// <summary>
        /// Delete data 
        /// </summary>
        /// <exception cref="InfluxDB.Client.Api.Client.ApiException">Thrown when fails to make API call</exception>
        /// <param name="deletePredicateRequest">Deletes data from an InfluxDB bucket.</param>
        /// <param name="zapTraceSpan">OpenTracing span context (optional)</param>
        /// <param name="org">Specifies the organization to delete data from. (optional)</param>
        /// <param name="bucket">Specifies the bucket to delete data from. (optional)</param>
        /// <param name="orgID">Specifies the organization ID of the resource. (optional)</param>
        /// <param name="bucketID">Specifies the bucket ID to delete data from. (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Task of RestResponse</returns>
        public async System.Threading.Tasks.Task<RestResponse> PostDeleteAsyncWithIRestResponse(
            DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null,
            string bucket = null, string orgID = null, string bucketID = null,
            CancellationToken cancellationToken = default)
        {
            // verify the required parameter 'deletePredicateRequest' is set
            if (deletePredicateRequest == null)
            {
                throw new ApiException(400,
                    "Missing required parameter 'deletePredicateRequest' when calling DeleteService->PostDelete");
            }

            var localVarPath = "/api/v2/delete";
            var localVarPathParams = new Dictionary<string, string>();
            var localVarQueryParams = new List<KeyValuePair<string, string>>();
            var localVarHeaderParams = new Dictionary<string, string>(Configuration.DefaultHeader);
            var localVarFormParams = new Dictionary<string, string>();
            var localVarFileParams = new Dictionary<string, FileParameter>();
            object localVarPostBody = null;

            // to determine the Content-Type header
            var localVarHttpContentTypes = new string[]
            {
                "application/json"
            };
            var localVarHttpContentType = Configuration.ApiClient.SelectHeaderContentType(localVarHttpContentTypes);

            if (org != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "org", org)); // query parameter
            }

            if (bucket != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "bucket", bucket)); // query parameter
            }

            if (orgID != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "orgID", orgID)); // query parameter
            }

            if (bucketID != null)
            {
                localVarQueryParams.AddRange(
                    Configuration.ApiClient.ParameterToKeyValuePairs("", "bucketID", bucketID)); // query parameter
            }

            if (zapTraceSpan != null)
            {
                localVarHeaderParams.Add("Zap-Trace-Span",
                    Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            }

            if (deletePredicateRequest != null && deletePredicateRequest.GetType() != typeof(byte[]))
            {
                localVarPostBody =
                    Configuration.ApiClient.Serialize(deletePredicateRequest); // http body (model) parameter
            }
            else
            {
                localVarPostBody = deletePredicateRequest; // byte array
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
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams,
                localVarFileParams,
                localVarPathParams, localVarHttpContentType, cancellationToken).ConfigureAwait(false);

            if (ExceptionFactory != null)
            {
                var exception = ExceptionFactory("PostDelete", localVarResponse);
                if (exception != null)
                {
                    throw exception;
                }
            }

            return localVarResponse;
        }
    }
}