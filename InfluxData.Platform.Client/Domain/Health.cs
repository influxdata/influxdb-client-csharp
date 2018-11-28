using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// The health of Platform.
    /// </summary>
    public class Health
    {
        public static readonly string HealthyStatus = "healthy";

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