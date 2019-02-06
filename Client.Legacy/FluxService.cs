using System.Net.Http;
using InfluxDB.Client.Core.Internal;

namespace InfluxDB.Client.Flux
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