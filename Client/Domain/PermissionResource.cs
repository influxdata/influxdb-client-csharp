using InfluxDB.Client.Core.Internal;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    ///     Resource is an authorizable resource.
    /// </summary>
    public class PermissionResource
    {
        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("type")]
        [JsonConverter(typeof(EnumConverter))]
        public ResourceType? Type { get; set; }

        [JsonProperty("orgID")] public string OrgId { get; set; }
    }
}