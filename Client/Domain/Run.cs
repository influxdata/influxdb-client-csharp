using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InfluxDB.Client.Domain
{
    public class Run
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("taskID")]
        public string TaskId { get; set; }

        [JsonProperty("status"), JsonConverter(typeof(StringEnumConverter))]
        public RunStatus Status { get; set; }

        [JsonProperty("scheduledFor")]
        public DateTime ScheduledFor { get; set; }
        
        [JsonProperty("startedAt")]
        public DateTime StartedAt { get; set; }
        
        [JsonProperty("finishedAt")]
        public DateTime FinishedAt { get; set; }
        
        [JsonProperty("requestedAt")]
        public DateTime? RequestedAt { get; set; }

        [JsonProperty("log")]
        public string Log { get; set; }
    }
}