using System;
using System.Collections.Generic;
using Flux.Flux.Options;

namespace Flux.Flux
{
    /** <summary>
     * Stores information about a single request and response.
     * </summary>
     */
    public class RequestResult
    {
        /** <summary>
         * String returned by the server.
         * </summary>
         */
        public string ResponseContent { get; }

        /** <summary>
         * Http status code result of the request.
         * </summary>
         */
        public int StatusCode { get; }

        /** <summary>
         * Response headers returned by the FaunaDB server.
         * </summary>
         */
        public IReadOnlyDictionary<string, IEnumerable<string>> ResponseHeaders { get; }

        /** <summary>
         * <see cref="DateTime"/> when the query was issued.
         * </summary>
         */
        public DateTime StartTime { get; }

        /** <summary>
         * <see cref="DateTime"/> when the query finished.
         * </summary>
         */
        public DateTime EndTime { get; }

        /** <summary>
         * Indicates how long the query took to execute.
         * </summary>
         */
        public TimeSpan TimeTaken
        {
            get { return EndTime - StartTime; }
        }

        public RequestResult(
            string responseContent,
            int statusCode,
            IReadOnlyDictionary<string, IEnumerable<string>> responseHeaders,
            DateTime startTime,
            DateTime endTime)
        {
            ResponseContent = responseContent;
            StatusCode = statusCode;
            ResponseHeaders = responseHeaders;
            StartTime = startTime;
            EndTime = endTime;
        }
    }
}