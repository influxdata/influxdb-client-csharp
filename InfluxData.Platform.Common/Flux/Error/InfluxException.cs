using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;
using Platform.Common.Platform.Rest;

namespace Platform.Common.Flux.Error
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

        public static string GetErrorMessage(RequestResult response)
        {
            if (response == null)
            {
                throw new ArgumentNullException();
            }

            if (response.ResponseHeaders.ContainsKey("X-Platform-Error-Code"))
            {
                var readToEnd = new StreamReader(response.ResponseContent).ReadToEnd();

                var json = JObject.Parse(readToEnd);
                if (json.ContainsKey("message"))
                {
                    return json.GetValue("message").ToString();
                }
            }

            var keys = new [] {"X-Platform-Error-Code", "X-Influx-Error", "X-InfluxDb-Error"};

            var message = keys
                .Where(key => response.ResponseHeaders.ContainsKey(key))
                .Select(key => response.ResponseHeaders[key].First());
            
            return message.FirstOrDefault();
        }

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
        public HttpException(string message, int status) : base(message, 0)
        {
            Status = status;
        }
    }
}