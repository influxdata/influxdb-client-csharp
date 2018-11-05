using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Platform.Common.Flux.Csv;

namespace Platform.Common.Platform.Rest
{
    /**
     * <summary>
     * Default client that handles all http connections using <see cref="HttpClient"/>.
     * </summary>
     */
    public class DefaultClientIo : IClientIo
    {
        public readonly HttpClient HttpClient;

        public DefaultClientIo()
        {
            HttpClient = new HttpClient();
            HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public Task<RequestResult> DoRequest(HttpRequestMessage message) =>
            DoRequestAsync(message);

        async Task<RequestResult> DoRequestAsync(HttpRequestMessage message)
        {
            var startTime = DateTime.UtcNow;

            var httpResponse = await HttpClient.SendAsync(message).ConfigureAwait(false);

            Stream response;

            if (httpResponse.Content.Headers.ContentEncoding.Any(encoding => encoding == "gzip"))
            {
                response = FluxCsvParser.ToStream(await DecompressGZip(httpResponse.Content).ConfigureAwait(false));
            }
            else
            {
                response = await httpResponse.Content.ReadAsStreamAsync().ConfigureAwait(false);
            }

            var endTime = DateTime.UtcNow;

            return new RequestResult(new BufferedStream(response), (int)httpResponse.StatusCode, 
                ToDictionary(httpResponse.Headers), startTime, endTime);
        }

        static async Task<string> DecompressGZip(HttpContent content)
        {
            using (var stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                using (var gzip = new GZipStream(stream, CompressionMode.Decompress, true))
                {
                    using (var reader = new StreamReader(gzip))
                        return await reader.ReadToEndAsync().ConfigureAwait(false);
                }
            }
        }

        static IReadOnlyDictionary<string, IEnumerable<string>> ToDictionary(HttpResponseHeaders headers) =>
            headers.ToDictionary(k => k.Key, v => v.Value);
    }
}