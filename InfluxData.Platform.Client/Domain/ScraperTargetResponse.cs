using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    public class ScraperTargetResponse: ScraperTarget
    {
        [JsonProperty("organization")]
        public string OrganizationName { get; set; }
        
        [JsonProperty("bucket")]
        public string BucketName { get; set; }
    }
}