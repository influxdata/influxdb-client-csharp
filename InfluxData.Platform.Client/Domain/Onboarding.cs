using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// Onboarding request, to setup initial user, org and bucket.
    /// </summary>
    public class Onboarding
    {
        [JsonProperty("username")] 
        public string Username { get; set; }
        
        [JsonProperty("password")] 
        public string Password { get; set; }
        
        [JsonProperty("org")] 
        public string Org { get; set; }
        
        [JsonProperty("bucket")] 
        public string Bucket { get; set; }
        
        [JsonProperty("retentionPeriodHrs")] 
        public int? RetentionPeriodHrs { get; set; }
        
        [JsonProperty("token")] 
        public string Token { get; set; }
    }
}