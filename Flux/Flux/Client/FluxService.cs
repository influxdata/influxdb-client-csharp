using System.Net.Http;
using Flux.Flux.Options;

namespace Flux.Flux.Client
{
    public class FluxService
    {
        public static HttpRequestMessage Query(string orgId, string query)
        {
            return new HttpRequestMessage(new HttpMethod(HttpMethodKind.Post.Name()), "query")
            {
                
            };
        }

        public static HttpRequestMessage Ping()
        {
            return new HttpRequestMessage(new HttpMethod(HttpMethodKind.Get.Name()), "ping");
        }
    }
}