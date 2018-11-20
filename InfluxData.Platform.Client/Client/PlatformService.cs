using System.Net.Http;
using InfluxData.Platform.Client.Domain;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Platform.Common;

namespace InfluxData.Platform.Client.Client
{
    public class PlatformService
    {
        //
        // Organizations
        //        
        public static HttpRequestMessage CreateOrganization(Organization organization)
        {
            return new HttpRequestMessage(new HttpMethod(HttpMethodKind.Post.Name()), "/api/v2/orgs")
            {
                Content = new StringContent(JsonConvert.SerializeObject(organization))
            };
        }
        
        public static HttpRequestMessage FindOrganizations()
        {
            return new HttpRequestMessage(new HttpMethod(HttpMethodKind.Get.Name()), "/api/v2/orgs");
        }
    }
}