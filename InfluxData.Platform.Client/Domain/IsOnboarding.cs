using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
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