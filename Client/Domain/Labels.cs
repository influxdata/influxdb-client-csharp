using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    public class Labels: AbstractHasLinks
    {
        [JsonProperty("labels")]
        public List<Label> LabelList { get; set; } = new List<Label>();
    }
}