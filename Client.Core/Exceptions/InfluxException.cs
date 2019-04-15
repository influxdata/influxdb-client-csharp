using System;
using System.IO;
using System.Linq;
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

        //TODO remove
        public static HttpException Create(RequestResult requestResult)
        {
            Arguments.CheckNotNull(requestResult, nameof(requestResult));

            string errorMessage = null;
            JObject errorBody;

            int? retryAfter = null;
            if (requestResult.ResponseHeaders.TryGetValue("Retry-After", out var retry))
                retryAfter = Convert.ToInt32(retry.First());

            var readToEnd = new StreamReader(requestResult.ResponseContent).ReadToEnd();
            if (string.IsNullOrEmpty(readToEnd))
                errorBody = new JObject();
            else
                errorBody = JObject.Parse(readToEnd);

            if (errorBody.ContainsKey("message")) errorMessage = errorBody.GetValue("message").ToString();

            var keys = new[] {"X-Platform-Error-Code", "X-Influx-Error", "X-InfluxDb-Error"};

            if (string.IsNullOrEmpty(errorMessage))
            {
                var message = requestResult.ResponseHeaders
                    .Where(header => keys.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
                    .Select(header => header.Value.First());

                errorMessage = message.FirstOrDefault();
            }

            return new HttpException(errorMessage, requestResult.StatusCode)
                {ErrorBody = errorBody, RetryAfter = retryAfter};
        }

        public static HttpException Create(IRestResponse requestResult)
        {
            Arguments.CheckNotNull(requestResult, nameof(requestResult));

            string errorMessage = null;
            JObject errorBody;

            int? retryAfter = null;
            {
                var retryHeader = requestResult.Headers.FirstOrDefault(header => header.Name.Equals("Retry-After"));
                if (retryHeader != null) retryAfter = Convert.ToInt32(retryHeader.Value);
            }

            var readToEnd = requestResult.Content;
            if (string.IsNullOrEmpty(readToEnd))
                errorBody = new JObject();
            else
                errorBody = JObject.Parse(readToEnd);

            if (errorBody.ContainsKey("message")) errorMessage = errorBody.GetValue("message").ToString();

            var keys = new[] {"X-Platform-Error-Code", "X-Influx-Error", "X-InfluxDb-Error"};

            if (string.IsNullOrEmpty(errorMessage))
                errorMessage = requestResult.Headers
                    .Where(header => keys.Contains(header.Name, StringComparer.OrdinalIgnoreCase))
                    .Select(header => header.Value.ToString()).FirstOrDefault();

            if (string.IsNullOrEmpty(errorMessage)) errorMessage = requestResult.ErrorMessage;

            return new HttpException(errorMessage, (int) requestResult.StatusCode)
                {ErrorBody = errorBody, RetryAfter = retryAfter};
        }
    }
}