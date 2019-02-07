using InfluxDB.Client.Core.Internal;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    public class LabelMapping
    {
        [JsonProperty("labelID")] public string LabelId { get; set; }

        [JsonProperty("resourceType")]
        [JsonConverter(typeof(EnumConverter))]
        public ResourceType? ResourceType { get; set; }
    }
}