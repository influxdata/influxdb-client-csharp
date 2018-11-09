using System.Net.Http;
using Flux.Client.Options;

namespace Flux.Client.Client
{
    public static class FluxService
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