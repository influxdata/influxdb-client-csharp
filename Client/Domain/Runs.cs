using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    public class Runs : AbstractHasLinks
    {
        [JsonProperty("runs")]
        public List<Run> RunList { get; set; } = new List<Run>();
    }
}