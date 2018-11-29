using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// The wrapper for "/api/v2/authorizations" response. 
    /// </summary>
    public class Authorizations : AbstractHasLinks
    {
        [JsonProperty("auths")]
        public List<Authorization> Auths { get; set; } = new List<Authorization>();

        public override string ToString()
        {
            return new StringBuilder(GetType().Name + "[")
                .Append("auths=" + Auths)
                .Append("]").ToString();
        }
    }
}