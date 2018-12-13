using System;
using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// The readiness of the InfluxData Platform.
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