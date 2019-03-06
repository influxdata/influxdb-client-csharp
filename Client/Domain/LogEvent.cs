using Newtonsoft.Json;
using NodaTime;

namespace InfluxDB.Client.Domain
{
    public class LogEvent
    {
        [JsonProperty("message")]
        public string Message { get; set; }
        
        [JsonProperty("time")]
        public OffsetDateTime Time { get; set; }
    }
}