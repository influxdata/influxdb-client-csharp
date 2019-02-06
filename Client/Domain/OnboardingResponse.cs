using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    /// Created default user, bucket, org.
    /// </summary>
    public class OnboardingResponse
    {
        [JsonProperty("user")] 
        public User User { get; set; }
        
        [JsonProperty("bucket")] 
        public Bucket Bucket { get; set; }
        
        [JsonProperty("org")] 
        public Organization Organization { get; set; }
        
        [JsonProperty("auth")] 
        public Authorization Authorization { get; set; }
    }
}