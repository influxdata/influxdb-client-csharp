using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    public class Logs
    {
        [JsonProperty("events")]
        public List<LogEvent> Events { get; set; } = new List<LogEvent>();
    }
}