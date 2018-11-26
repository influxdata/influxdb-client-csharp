using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Platform.Common.Platform.Rest;

namespace Platform.Common.Flux.Error
{
    public class InfluxException : Exception
    {
        private QueryErrorResponse _queryErrorResponse;

        /** <summary>
         * List of all errors sent by the server.
         * </summary>
         */
        public string Error => _queryErrorResponse.Error;

        public int StatusCode =>
            _queryErrorResponse.StatusCode;

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
                if (json.ContainsKey("msg"))
                {
                    return json.GetValue("msg").ToString();
                }
            }

            var keys = new [] {"X-Platform-Error-Code", "X-Influx-Error", "X-InfluxDb-Error"};

            var message = keys
                .Where(key => response.ResponseHeaders.ContainsKey(key))
                .Select(key => response.ResponseHeaders[key].First());
            
            return message.FirstOrDefault();
        }

        public InfluxException(string message) : this(new QueryErrorResponse(0, message))
        {
        }

        public InfluxException(QueryErrorResponse response)
        {
            _queryErrorResponse = response;
        }
    }

    public struct QueryErrorResponse
    {
        public int StatusCode { get; private set; }
        public string Error { get; private set; }

        public QueryErrorResponse(int statusCode, string error)
        {
            StatusCode = statusCode;
            Error = error;
        }
    }

    public class HttpException : InfluxException
    {
        public HttpException(QueryErrorResponse response) : base(response)
        {
        }
    }
}