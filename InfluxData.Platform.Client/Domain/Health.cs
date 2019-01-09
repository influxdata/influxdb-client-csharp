using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// The health of Platform.
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