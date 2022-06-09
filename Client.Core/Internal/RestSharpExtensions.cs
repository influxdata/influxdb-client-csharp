using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RestSharp;

namespace InfluxDB.Client.Core.Internal
{
    internal static class RestSharpExtensions
    {
        /// <summary>
        /// Transform `HttpHeaders` to `HeaderParameter` type.
        /// </summary>
        /// <param name="httpHeaders"></param>
        /// <param name="httpContentHeaders">Additionally content Headers</param>
        /// <returns>IEnumerable&lt;HeaderParameter&gt;</returns>
        internal static IEnumerable<HeaderParameter> ToHeaderParameters(this HttpHeaders httpHeaders,
            HttpContentHeaders httpContentHeaders = null)
        {
            return httpHeaders
                .Concat(httpContentHeaders ?? Enumerable.Empty<KeyValuePair<string, IEnumerable<string>>>())
                .SelectMany(x => x.Value.Select(y => (x.Key, y)))
                .Select(x => new HeaderParameter(x.Key, x.y));
        }

        internal static RestRequest AddAdvancedResponseHandler(this RestRequest restRequest,
            Func<HttpResponseMessage, RestResponse> advancedResponseWriter)
        {
            var field = restRequest.GetType()
                .GetField("_advancedResponseHandler", BindingFlags.Instance | BindingFlags.NonPublic);
            field!.SetValue(restRequest, advancedResponseWriter);

            return restRequest;
        }

        //
        internal static RestResponse ExecuteSync(this RestClient client,
            RestRequest request, CancellationToken cancellationToken = default)
        {
            // return client.Execute(request);
            return client.ExecuteAsync(request, cancellationToken).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}