using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Flux.Flux.Core
{
    public class InfluxException : Exception
    {
        private QueryErrorResponse _queryErrorResponse;
        
        /** <summary>
         * List of all errors sent by the server.
         * </summary>
         */
        public IReadOnlyList<string> Errors =>
                        _queryErrorResponse.Errors;

        public int StatusCode =>
                        _queryErrorResponse.StatusCode;
        
        public static IEnumerable<string> GetErrorMessage(RequestResult response) 
        {
            if (response == null)
            {
                throw new ArgumentNullException();
            }

            IEnumerable<string> value;

            response.ResponseHeaders.TryGetValue("X-Influx-Error", out value);

            return value;
        }
        
        public InfluxException(QueryErrorResponse response)
        {
            _queryErrorResponse = response;
        }

        protected InfluxException(string message) : base(message) { }
    }
    
    public struct QueryErrorResponse
    {
        public int StatusCode { get; private set; }
        public IReadOnlyList<string> Errors { get; private set; }

        public QueryErrorResponse(int statusCode, IReadOnlyList<string> errors)
        {
            StatusCode = statusCode;
            Errors = errors;
        }
    }
    
    public class HttpException : InfluxException
    {
        internal HttpException(QueryErrorResponse response) : base(response) {}
    }
}