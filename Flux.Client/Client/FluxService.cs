using System.Net.Http;
using Flux.Flux.Options;
using Newtonsoft.Json.Linq;
using Platform.Common.Platform;

namespace Flux.Client.Client
{
    public class FluxService
    {
        public static HttpRequestMessage Query(string query)
        {
            return new HttpRequestMessage(new HttpMethod(HttpMethodKind.Post.Name()), "api/v2/query")
            {
                Content = new StringContent(query)
            };
        }

        public static HttpRequestMessage Ping()
        {
            return new HttpRequestMessage(new HttpMethod(HttpMethodKind.Get.Name()), "ping");
        }
    }
}