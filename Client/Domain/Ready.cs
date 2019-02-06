using System;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    /// The readiness of the InfluxDB 2.0.
    /// </summary>
    public class Ready
    {
        [JsonProperty("status")] 
        public string Status { get; set; }
        
        [JsonProperty("started")]
        public DateTime Started { get; set; }
        
        [JsonProperty("up")]
        public string Up { get; set; }
    }
}