using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;
using RestSharp;

namespace InfluxDB.Client.Core.Internal
{
    internal static class RestSharpExtensions
    {
        internal static IEnumerable<HeaderParameter> ToHeaderParameters(this HttpHeaders httpHeaders)
        {
            return httpHeaders
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
    }
}