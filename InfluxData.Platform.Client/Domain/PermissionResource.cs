using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// Resource is an authorizable resource.
    /// </summary>
    public class PermissionResource
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter))]
        public PermissionResourceType Type { get; set; }
        
        [JsonProperty("orgID")]
        public string OrgId { get; set; }
    }
}