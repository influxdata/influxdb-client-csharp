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
        void PostDelete (DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null, string bucketID = null);

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
        ApiResponse<Object> PostDeleteWithHttpInfo (DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null, string bucketID = null);
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
        System.Threading.Tasks.Task PostDeleteAsync (DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null, string bucketID = null, CancellationToken cancellationToken = default);

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
        System.Threading.Tasks.Task<ApiResponse<Object>> PostDeleteAsyncWithHttpInfo (DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null, string bucketID = null, CancellationToken cancellationToken = default);
        #endregion Asynchronous Operations
    }

    /// <summary>
    /// Represents a collection of functions to interact with the API endpoints
    /// </summary>
    public partial class DeleteService : IDeleteService
    {
        private InfluxDB.Client.Api.Client.ExceptionFactory _exceptionFactory = (name, response) => null;

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteService"/> class.
        /// </summary>
        /// <returns></returns>
        public DeleteService(String basePath)
        {
            this.Configuration = new InfluxDB.Client.Api.Client.Configuration { BasePath = basePath };

            ExceptionFactory = InfluxDB.Client.Api.Client.Configuration.DefaultExceptionFactory;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeleteService"/> class
        /// using Configuration object
        /// </summary>
        /// <param name="configuration">An instance of Configuration</param>
        /// <returns></returns>
        public DeleteService(InfluxDB.Client.Api.Client.Configuration configuration = null)
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
            return this.Configuration.ApiClient.RestClientOptions.BaseUrl.ToString();
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
        public void PostDelete (DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null, string bucketID = null)
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
        public ApiResponse<Object> PostDeleteWithHttpInfo (DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null, string bucketID = null)
        {
            // verify the required parameter 'deletePredicateRequest' is set
            if (deletePredicateRequest == null)
                throw new ApiException(400, "Missing required parameter 'deletePredicateRequest' when calling DeleteService->PostDelete");

            var localVarPath = "/api/v2/delete";
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

            if (org != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "org", org)); // query parameter
            if (bucket != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "bucket", bucket)); // query parameter
            if (orgID != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "orgID", orgID)); // query parameter
            if (bucketID != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "bucketID", bucketID)); // query parameter
            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            if (deletePredicateRequest != null && deletePredicateRequest.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(deletePredicateRequest); // http body (model) parameter
            }
            else
            {
                localVarPostBody = deletePredicateRequest; // byte array
            }

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };

            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null && !localVarHeaderParams.ContainsKey("Accept"))
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);


            // make the HTTP request
            RestResponse localVarResponse = (RestResponse) this.Configuration.ApiClient.CallApi(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("PostDelete", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<Object>(localVarStatusCode,
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
        public async System.Threading.Tasks.Task<RestResponse> PostDeleteWithIRestResponseAsync (DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null, string bucketID = null, CancellationToken cancellationToken = default)
        {
            // verify the required parameter 'deletePredicateRequest' is set
            if (deletePredicateRequest == null)
                throw new ApiException(400, "Missing required parameter 'deletePredicateRequest' when calling DeleteService->PostDelete");

            var localVarPath = "/api/v2/delete";
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

            if (org != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "org", org)); // query parameter
            if (bucket != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "bucket", bucket)); // query parameter
            if (orgID != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "orgID", orgID)); // query parameter
            if (bucketID != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "bucketID", bucketID)); // query parameter
            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            if (deletePredicateRequest != null && deletePredicateRequest.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(deletePredicateRequest); // http body (model) parameter
            }
            else
            {
                localVarPostBody = deletePredicateRequest; // byte array
            }

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };

            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null && !localVarHeaderParams.ContainsKey("Accept"))
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);


            // make the HTTP request
            RestResponse localVarResponse = (RestResponse) await this.Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType, cancellationToken).ConfigureAwait(false);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("PostDelete", localVarResponse);
                if (exception != null) throw exception;
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
        public RestResponse PostDeleteWithIRestResponse (DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null, string bucketID = null)
        {
            // verify the required parameter 'deletePredicateRequest' is set
            if (deletePredicateRequest == null)
                throw new ApiException(400, "Missing required parameter 'deletePredicateRequest' when calling DeleteService->PostDelete");

            var localVarPath = "/api/v2/delete";
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

            if (org != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "org", org)); // query parameter
            if (bucket != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "bucket", bucket)); // query parameter
            if (orgID != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "orgID", orgID)); // query parameter
            if (bucketID != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "bucketID", bucketID)); // query parameter
            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            if (deletePredicateRequest != null && deletePredicateRequest.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(deletePredicateRequest); // http body (model) parameter
            }
            else
            {
                localVarPostBody = deletePredicateRequest; // byte array
            }

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };

            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null && !localVarHeaderParams.ContainsKey("Accept"))
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);


            // make the HTTP request
            RestResponse localVarResponse = (RestResponse) this.Configuration.ApiClient.CallApi(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("PostDelete", localVarResponse);
                if (exception != null) throw exception;
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
        public RestRequest PostDeleteWithRestRequest (DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null, string bucketID = null)
        {
            // verify the required parameter 'deletePredicateRequest' is set
            if (deletePredicateRequest == null)
                throw new ApiException(400, "Missing required parameter 'deletePredicateRequest' when calling DeleteService->PostDelete");

            var localVarPath = "/api/v2/delete";
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

            if (org != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "org", org)); // query parameter
            if (bucket != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "bucket", bucket)); // query parameter
            if (orgID != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "orgID", orgID)); // query parameter
            if (bucketID != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "bucketID", bucketID)); // query parameter
            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            if (deletePredicateRequest != null && deletePredicateRequest.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(deletePredicateRequest); // http body (model) parameter
            }
            else
            {
                localVarPostBody = deletePredicateRequest; // byte array
            }

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };

            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null && !localVarHeaderParams.ContainsKey("Accept"))
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);


            return this.Configuration.ApiClient.PrepareRequest(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
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
        public System.Threading.Tasks.Task PostDeleteAsync (DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null, string bucketID = null, CancellationToken cancellationToken = default)
        {
             return PostDeleteAsyncWithHttpInfo(deletePredicateRequest, zapTraceSpan, org, bucket, orgID, bucketID, cancellationToken);

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
        public async System.Threading.Tasks.Task<ApiResponse<Object>> PostDeleteAsyncWithHttpInfo (DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null, string bucketID = null, CancellationToken cancellationToken = default)
        {
            // make the HTTP request
            RestResponse localVarResponse = await PostDeleteAsyncWithIRestResponse(deletePredicateRequest, zapTraceSpan, org, bucket, orgID, bucketID, cancellationToken).ConfigureAwait(false);

            int localVarStatusCode = (int) localVarResponse.StatusCode;

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("PostDelete", localVarResponse);
                if (exception != null) throw exception;
            }

            return new ApiResponse<Object>(localVarStatusCode,
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
        public async System.Threading.Tasks.Task<RestResponse> PostDeleteAsyncWithIRestResponse (DeletePredicateRequest deletePredicateRequest, string zapTraceSpan = null, string org = null, string bucket = null, string orgID = null, string bucketID = null, CancellationToken cancellationToken = default)
        {
            // verify the required parameter 'deletePredicateRequest' is set
            if (deletePredicateRequest == null)
                throw new ApiException(400, "Missing required parameter 'deletePredicateRequest' when calling DeleteService->PostDelete");

            var localVarPath = "/api/v2/delete";
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

            if (org != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "org", org)); // query parameter
            if (bucket != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "bucket", bucket)); // query parameter
            if (orgID != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "orgID", orgID)); // query parameter
            if (bucketID != null) localVarQueryParams.AddRange(this.Configuration.ApiClient.ParameterToKeyValuePairs("", "bucketID", bucketID)); // query parameter
            if (zapTraceSpan != null) localVarHeaderParams.Add("Zap-Trace-Span", this.Configuration.ApiClient.ParameterToString(zapTraceSpan)); // header parameter
            if (deletePredicateRequest != null && deletePredicateRequest.GetType() != typeof(byte[]))
            {
                localVarPostBody = this.Configuration.ApiClient.Serialize(deletePredicateRequest); // http body (model) parameter
            }
            else
            {
                localVarPostBody = deletePredicateRequest; // byte array
            }

            // to determine the Accept header
            String[] localVarHttpHeaderAccepts = new String[] {
                "application/json"
            };

            String localVarHttpHeaderAccept = this.Configuration.ApiClient.SelectHeaderAccept(localVarHttpHeaderAccepts);
            if (localVarHttpHeaderAccept != null && !localVarHeaderParams.ContainsKey("Accept"))
                localVarHeaderParams.Add("Accept", localVarHttpHeaderAccept);


            // make the HTTP request
            RestResponse localVarResponse = (RestResponse) await this.Configuration.ApiClient.CallApiAsync(localVarPath,
                Method.Post, localVarQueryParams, localVarPostBody, localVarHeaderParams, localVarFormParams, localVarFileParams,
                localVarPathParams, localVarHttpContentType, cancellationToken).ConfigureAwait(false);

            if (ExceptionFactory != null)
            {
                Exception exception = ExceptionFactory("PostDelete", localVarResponse);
                if (exception != null) throw exception;
            }

            return localVarResponse;
        }

    }
}
