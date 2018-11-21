using System.Net.Http;
using InfluxData.Platform.Client.Domain;
using Microsoft.Win32.SafeHandles;
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
                            Content = CreateBody(organization)
            };
        }

        public static HttpRequestMessage DeleteOrganization(string organizationId)
        {
            return new HttpRequestMessage(new HttpMethod(HttpMethodKind.Delete.Name()),
                            string.Format("/api/v2/orgs/{0}", organizationId));
        }
        
        public static HttpRequestMessage UpdateOrganization(Organization organization)
        {
            return new HttpRequestMessage(new HttpMethod(HttpMethodKind.Patch.Name()),
                            string.Format("/api/v2/orgs/{0}", organization.Id))
            {
                            Content = CreateBody(organization)
            };
        }
        
        public static HttpRequestMessage FindOrganizationById(string organizationId)
        {
            return new HttpRequestMessage(new HttpMethod(HttpMethodKind.Get.Name()),
                            string.Format("/api/v2/orgs/{0}", organizationId));
        }
        
        public static HttpRequestMessage FindOrganizations()
        {
            return new HttpRequestMessage(new HttpMethod(HttpMethodKind.Get.Name()), "/api/v2/orgs");
        }

        private static StringContent CreateBody(object content)
        {
            return new StringContent(JsonConvert.SerializeObject(content,
                            Formatting.None, new JsonSerializerSettings
                            {
                                            NullValueHandling = NullValueHandling.Ignore
                            }));
        }
    }
}