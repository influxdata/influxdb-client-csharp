using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// The wrapper for "/api/v2/orgs/{organizationId}/secrets" response.
    /// </summary>
    public class Secrets : AbstractHasLinks
    {
        [JsonProperty("secrets")]
        public List<string> SecretList { get; set; } = new List<string>();
    }
}