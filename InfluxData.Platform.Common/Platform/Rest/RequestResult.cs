using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Platform.Common.Platform.Rest
{
    /** <summary>
     * Stores information about a single request and response.
     * </summary>
     */
    public class RequestResult: IDisposable
    {
        private HttpResponseMessage _httpResponse;
        
        /** <summary>
         * String returned by the server.
         * </summary>
         */
        public BufferedStream ResponseContent { get; }

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
            BufferedStream responseContent,
            HttpResponseMessage httpResponse,
            DateTime startTime,
            DateTime endTime)
        {
            _httpResponse = httpResponse;
            ResponseContent = responseContent;
            StatusCode = (int) httpResponse.StatusCode;
            ResponseHeaders = ToDictionary(httpResponse.Headers);
            StartTime = startTime;
            EndTime = endTime;
        }
        
        private IReadOnlyDictionary<string, IEnumerable<string>> ToDictionary(HttpResponseHeaders headers) =>
            headers.ToDictionary(k => k.Key, v => v.Value);

        public void Dispose()
        {
            _httpResponse.Dispose();
            ResponseContent.Dispose();
        }
    }
}