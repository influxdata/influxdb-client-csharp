using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    public class ScraperTargetResponses : AbstractHasLinks
    {
        [JsonProperty("configurations")]
        public List<ScraperTargetResponse> TargetResponses { get; set; } = new List<ScraperTargetResponse>();
    }
}