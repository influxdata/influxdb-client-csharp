using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    public class ResourceMember
    {
        [JsonProperty("id")]
        public string UserId { get; set; }
        
        [JsonProperty("name")]
        public string UserName { get; set; }

        [JsonProperty("role")]
        public UserType Role { get; set; }

        /// <summary>
        /// The user type.
        /// </summary>
        public enum UserType 
        {
            Owner,

            Member
        }
    }
}