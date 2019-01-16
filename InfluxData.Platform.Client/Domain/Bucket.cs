using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    
    /// <summary>
    /// Bucket
    /// </summary>
    public class Bucket : AbstractHasLinks
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("organizationID")]
        public string OrgId { get; set; }

        [JsonProperty("organization")]
        public string OrganizationName { get; set; }

        /// <summary>
        /// For support V1 sources. 
        /// </summary>
        [JsonProperty("rp")]
        public string RetentionPolicyName { get; set; }

        /// <summary>
        /// The retention rules.
        /// </summary>
        public List<RetentionRule> RetentionRules { get; set;} = new List<RetentionRule>();
    }
}