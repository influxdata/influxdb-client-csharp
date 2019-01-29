using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    public class LabelResponse
    {
        [JsonProperty("label")]
        public Label Label { get; set; }
    }
}