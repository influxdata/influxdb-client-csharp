using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    /**
     * Bucket
     */
    public class Bucket : AbstractHasLinks
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("organizationID")]
        public string OrganizationId { get; set; }

        [JsonProperty("organization")]
        public string OrganizationName { get; set; }

        [JsonProperty("rp")]
        /**
         * For support V1 sources.
         */
        public string RetentionPolicyName { get; set; }

        /**
         * The retention rules.
         */
        public List<RetentionRule> RetentionRules { get; set;} = new List<RetentionRule>();
    }
}