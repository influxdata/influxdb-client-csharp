using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using InfluxDB.Client.Core.Internal;
using Newtonsoft.Json.Linq;
using RestSharp;

namespace InfluxDB.Client.Core.Exceptions
{
    public class InfluxException : Exception
    {
        public InfluxException(string message) : this(message, 0)
        {
        }

        public InfluxException(Exception exception) : base(exception.Message, exception)
        {
            Code = 0;
        }

        public InfluxException(string message, int code) : base(message)
        {
            Code = code;
            Status = 0;
        }

        /// <summary>
        ///     Gets the reference code unique to the error type. If the reference code is not present than return "0".
        /// </summary>
        public int Code { get; }

        /// <summary>
        ///     Gets the HTTP status code of the unsuccessful response. If the response is not present than return "0".
        /// </summary>
        public int Status { get; set; }
    }

    public class HttpException : InfluxException
    {
        public HttpException(string message, int status) : base(message, 0)
        {
            Status = status;
        }

        /// <summary>
        ///     The JSON unsuccessful response body.
        /// </summary>
        public JObject ErrorBody { get; set; }

        /// <summary>
        ///     The retry interval is used when the InfluxDB server does not specify "Retry-After" header.
        /// </summary>
        public int? RetryAfter { get; set; }

        public static HttpException Create(IRestResponse requestResult)
        {
            Arguments.CheckNotNull(requestResult, nameof(requestResult));

            var httpHeaders = LoggingHandler.ToHeaders(requestResult.Headers);
            
            return Create(requestResult.Content, httpHeaders, requestResult.ErrorMessage, requestResult.StatusCode);
        }

        public static HttpException Create(IHttpResponse requestResult)
        {
            Arguments.CheckNotNull(requestResult, nameof(requestResult));

            return Create(requestResult.Content, requestResult.Headers, requestResult.ErrorMessage, requestResult.StatusCode);
        }
        
        public static HttpException Create(string content, IList<HttpHeader> headers, string ErrorMessage, HttpStatusCode statusCode)
        {
            string errorMessage = null;
            JObject errorBody;

            int? retryAfter = null;
            {
                var retryHeader = headers.FirstOrDefault(header => header.Name.Equals("Retry-After"));
                if (retryHeader != null) retryAfter = Convert.ToInt32(retryHeader.Value);
            }

            if (string.IsNullOrEmpty(content))
                errorBody = new JObject();
            else
                errorBody = JObject.Parse(content);

            if (errorBody.ContainsKey("message")) errorMessage = errorBody.GetValue("message").ToString();

            var keys = new[] {"X-Platform-Error-Code", "X-Influx-Error", "X-InfluxDb-Error"};

            if (string.IsNullOrEmpty(errorMessage))
                errorMessage = headers
                    .Where(header => keys.Contains(header.Name, StringComparer.OrdinalIgnoreCase))
                    .Select(header => header.Value.ToString()).FirstOrDefault();

            if (string.IsNullOrEmpty(errorMessage)) errorMessage = ErrorMessage;

            return new HttpException(errorMessage, (int) statusCode)
                {ErrorBody = errorBody, RetryAfter = retryAfter};
        }
    }
}