using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    /**
     * The wrapper for "/api/v2/orgs" response.
     */
    public class Organizations : AbstractHasLinks
    {
        [JsonProperty("orgs")]
        public List<Organization> Orgs { get; set; } = new List<Organization>();

        public override string ToString()
        {
            return new StringBuilder(GetType().Name + "[")
                            .Append("links=" + LinksToString())
                            .Append(", orgs=" + Orgs)
                            .Append("]").ToString();
        }
    }
}