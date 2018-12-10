using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;

namespace Platform.Common.Platform.Rest
{
    /// <summary>
    /// The InfluxData Platform server Response.
    /// </summary>
    public class RequestResult: IDisposable
    {
        private HttpResponseMessage _httpResponse;
        
        /// <summary>
        /// Stream returned by the InfluxData Platform.
        /// </summary>
        public BufferedStream ResponseContent { get; }

        /// <summary>
        /// HTTP status code result of the request.
        /// </summary>
        public int StatusCode { get; }

        /// <summary>
        /// Response headers returned by the InfluxData Platform.
        /// </summary>
        public IReadOnlyDictionary<string, IEnumerable<string>> ResponseHeaders { get; }

        /// <summary>
        /// <see cref="DateTime"/> when the query was issued.
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// <see cref="DateTime"/> when the query finished.
        /// </summary>
        public DateTime EndTime { get; }

        /// <summary>
        /// Indicates how long the query took to execute.
        /// </summary>
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

        /// <summary>
        /// Returns true if the code is in [200..300).
        /// </summary>
        /// <returns>true for successfully ended request</returns>
        public bool IsSuccessful()
        {
            if (StatusCode >= 200 && StatusCode < 300)
            {
                return true;
            }

            return false;
        }
        
        public void Dispose()
        {
            _httpResponse.Dispose();
            ResponseContent.Dispose();
        }
    }
}