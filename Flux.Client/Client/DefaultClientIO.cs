using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Flux.Flux.Options;
using Platform.Common.Platform.Rest;

namespace Flux.Client.Client
{
    /**
     * <summary>
     * Default client that handles all http connections using <see cref="HttpClient"/>.
     * </summary>
     */
    class DefaultClientIO : IClientIO
    {
        readonly HttpClient _client;

        internal DefaultClientIO(FluxConnectionOptions options)
        {
            _client = CreateClient(options);
        }

        public Task<RequestResult> DoRequest(HttpRequestMessage message) =>
            DoRequestAsync(message);

        async Task<RequestResult> DoRequestAsync(HttpRequestMessage message)
        {
            var startTime = DateTime.UtcNow;

            var httpResponse = await _client.SendAsync(message).ConfigureAwait(false);

            string response;

            if (httpResponse.Content.Headers.ContentEncoding.Any(encoding => encoding == "gzip"))
            {
                response = await DecompressGZip(httpResponse.Content).ConfigureAwait(false);
            }
            else
            {
                response = await httpResponse.Content.ReadAsStringAsync().ConfigureAwait(false);
            }

            var endTime = DateTime.UtcNow;

            return new RequestResult(response, (int)httpResponse.StatusCode, 
                ToDictionary(httpResponse.Headers), startTime, endTime);
        }

        static async Task<string> DecompressGZip(HttpContent content)
        {
            using (var stream = await content.ReadAsStreamAsync().ConfigureAwait(false))
            {
                using (var gzip = new GZipStream(stream, CompressionMode.Decompress))
                {
                    using (var reader = new StreamReader(gzip))
                        return await reader.ReadToEndAsync().ConfigureAwait(false);
                }
            }
        }

        static IReadOnlyDictionary<string, IEnumerable<string>> ToDictionary(HttpResponseHeaders headers) =>
            headers.ToDictionary(k => k.Key, v => v.Value);

        public static HttpClient CreateClient(FluxConnectionOptions options)
        {
            var client = new HttpClient();
            client.BaseAddress = new Uri(options.Url);
            client.Timeout = options.Timeout;
            client.DefaultRequestHeaders.AcceptEncoding.Add(new StringWithQualityHeaderValue("gzip"));
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            return client;
        }
    }
}