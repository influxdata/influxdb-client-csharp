using InfluxDB.Client.Core.Internal;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    ///     ScraperTarget is a target to scrape.
    /// </summary>
    public class ScraperTarget : AbstractHasLinks
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("name")] public string Name { get; set; }

        [JsonProperty("status")]
        [JsonConverter(typeof(EnumConverter))]
        public ScraperType? Type { get; set; } = ScraperType.Prometheus;

        [JsonProperty("url")] public string Url { get; set; }

        [JsonProperty("orgID")] public string OrgId { get; set; }

        [JsonProperty("bucketID")] public string BucketId { get; set; }
    }
}