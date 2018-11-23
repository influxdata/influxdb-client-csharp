using System.Net.Http;
using InfluxData.Platform.Client.Domain;
using Newtonsoft.Json;
using Platform.Common;

namespace InfluxData.Platform.Client.Client
{
    class PlatformService
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
                $"/api/v2/orgs/{organizationId}");
        }

        public static HttpRequestMessage UpdateOrganization(Organization organization)
        {
            return new HttpRequestMessage(new HttpMethod(HttpMethodKind.Patch.Name()),
                $"/api/v2/orgs/{organization.Id}")
            {
                Content = CreateBody(organization)
            };
        }

        public static HttpRequestMessage FindOrganizationById(string organizationId)
        {
            return new HttpRequestMessage(new HttpMethod(HttpMethodKind.Get.Name()), $"/api/v2/orgs/{organizationId}");
        }

        public static HttpRequestMessage FindOrganizations()
        {
            return new HttpRequestMessage(new HttpMethod(HttpMethodKind.Get.Name()), "/api/v2/orgs");
        }

        private static StringContent CreateBody(object content)
        {
            var serializer = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            return new StringContent(JsonConvert.SerializeObject(content, Formatting.None, serializer));
        }
    }
}