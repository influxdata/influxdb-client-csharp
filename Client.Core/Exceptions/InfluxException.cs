using System;
using System.IO;
using System.Linq;
using InfluxDB.Client.Core.Internal;
using Newtonsoft.Json.Linq;

namespace InfluxDB.Client.Core.Exceptions
{
    public class InfluxException : Exception
    {
        /// <summary>
        /// Gets the reference code unique to the error type. If the reference code is not present than return "0".
        /// </summary>
        public int Code { get; }
        
        /// <summary>
        /// Gets the HTTP status code of the unsuccessful response. If the response is not present than return "0".
        /// </summary>
        public int Status { get; set; }

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
    }

    public class HttpException : InfluxException
    {
        /// <summary>
        /// The JSON unsuccessful response body.
        /// </summary>
        public JObject ErrorBody { get; set; }
        
        /// <summary>
        /// The retry interval is used when the InfluxDB server does not specify "Retry-After" header.
        /// </summary>
        public int? RetryAfter { get; set; }

        public HttpException(string message, int status) : base(message, 0)
        {
            Status = status;
        }

        public static HttpException Create(RequestResult requestResult)
        {
            Arguments.CheckNotNull(requestResult, nameof(requestResult));

            string errorMessage = null;
            JObject errorBody;

            int? retryAfter = null;
            if (requestResult.ResponseHeaders.TryGetValue("Retry-After", out var retry))
            {
                retryAfter = Convert.ToInt32(retry.First());
            }

            var readToEnd = new StreamReader(requestResult.ResponseContent).ReadToEnd();
            if (string.IsNullOrEmpty(readToEnd))
            {
                errorBody = new JObject();
            }
            else
            {
                errorBody = JObject.Parse(readToEnd);
            }

            if (errorBody.ContainsKey("message"))
            {
                errorMessage = errorBody.GetValue("message").ToString();
            }

            var keys = new[] {"X-Platform-Error-Code", "X-Influx-Error", "X-InfluxDb-Error"};

            if (string.IsNullOrEmpty(errorMessage))
            {
                var message = requestResult.ResponseHeaders
                    .Where(header => keys.Contains(header.Key, StringComparer.OrdinalIgnoreCase))
                    .Select(header => header.Value.First());

                errorMessage = message.FirstOrDefault();
            }

            return new HttpException(errorMessage, requestResult.StatusCode) {ErrorBody = errorBody, RetryAfter = retryAfter};
        }
    }
}