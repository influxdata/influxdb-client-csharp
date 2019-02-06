using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    /// Check if database has default user, org, bucket created, returns true if not.
    /// </summary>
    public class IsOnboarding
    {
        [JsonProperty("allowed")] 
        public bool Allowed { get; set; }
    }
}