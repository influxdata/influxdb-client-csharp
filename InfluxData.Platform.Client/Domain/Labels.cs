using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    public class Labels: AbstractHasLinks
    {
        [JsonProperty("labels")]
        public List<Label> LabelList { get; set; } = new List<Label>();
    }
}