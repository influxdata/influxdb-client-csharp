using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    /// The health of InfluxDB 2.0.
    /// </summary>
    public class Health
    {
        private const string HealthyStatus = "pass";

        [JsonProperty("status")]
        public string Status { get; set; }
        
        [JsonProperty("message")]
        public string Message { get; set; }

        public bool IsHealthy() 
        {
            return HealthyStatus.Equals(Status);
        }
    }
}