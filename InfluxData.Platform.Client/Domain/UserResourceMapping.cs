using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    public class UserResourceMapping
    {
        [JsonProperty("resource_id")]
        public string ResourceId { get; set; }

        [JsonProperty("resource_type")]
        public ResourceType ResourceType { get; set; }

        [JsonProperty("user_id")]
        public string UserId { get; set; }

        [JsonProperty("user_type")]
        public MemberType UserType { get; set; }

        /**
         * The user type.
         */
        public enum MemberType 
        {
            Owner,

            Member
        }
    }
}