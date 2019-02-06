using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    /// ScraperTarget is a target to scrape.
    /// </summary>
    public class ScraperTarget: AbstractHasLinks
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("status"), JsonConverter(typeof(StringEnumConverter))]
        public ScraperType Type { get; set; }
        
        [JsonProperty("url")]
        public string Url { get; set; }
        
        [JsonProperty("orgID")]
        public string OrgId { get; set; }
        
        [JsonProperty("bucketID")]
        public string BucketId { get; set; }
    }
}