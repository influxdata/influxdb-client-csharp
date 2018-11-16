using System.Collections.Generic;

namespace InfluxData.Platform.Client.Domain
{
    /**
     * Bucket
     */
    public class Bucket : AbstractHasLinks
    {
        public string Id { get; set; }

        public string Name { get; set; }

        public string OrganizationId { get; set; }

        public string OrganizationName { get; set; }

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