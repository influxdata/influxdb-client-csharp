using System.Net.Http;
using System.Text;
using Flux.Flux.Options;
using Newtonsoft.Json.Linq;

namespace Flux.Flux.Client
{
    public class FluxService
    {
        public static HttpRequestMessage Query(string query)
        {
            return new HttpRequestMessage(new HttpMethod(HttpMethodKind.Post.Name()), "v2/query")
            {
                Content = new StringContent(query)
            };
        }

        public static HttpRequestMessage Ping()
        {
            return new HttpRequestMessage(new HttpMethod(HttpMethodKind.Get.Name()), "ping");
        }
        
        
        public static JObject GetDefaultDialect()
        {
            JObject json = new JObject();
            json.Add("header", true);
            json.Add("delimiter", ",");
            json.Add("quoteChar", "\"");
            json.Add("commentPrefix", "#");
            json.Add("annotations", new JArray("datatype", "group", "default"));

            return json;
        }

        public static string CreateBody(JObject dialect, string query)
        {
            Preconditions.CheckNonEmptyString(query, "Flux query");

            JObject json = new JObject();
            json.Add("query", query);

            if (dialect != null)
            {
                json.Add("dialect", dialect);
            }

            return json.ToString();
        }
    }
}