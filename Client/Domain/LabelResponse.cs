using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    public class LabelResponse
    {
        [JsonProperty("label")]
        public Label Label { get; set; }
    }
}