using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InfluxDB.Client.Domain
{
    public class LabelMapping
    {
        [JsonProperty("labelID")]
        public string LabelId { get; set; }

        [JsonProperty("resourceType"), JsonConverter(typeof(StringEnumConverter))]
        public ResourceType  ResourceType { get; set; }
    }
}