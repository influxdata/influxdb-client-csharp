using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    /// The wrapper for "/api/v2/orgs" response. 
    /// </summary>
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