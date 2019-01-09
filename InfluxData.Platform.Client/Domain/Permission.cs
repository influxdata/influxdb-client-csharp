using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// Permission defines an action and a resource.
    /// </summary>
    public class Permission
    {
        /// <summary>
        /// Action for reading.
        /// </summary>
        public static readonly string ReadAction = "read";

        /// <summary>
        /// Action for writing.
        /// </summary>
        public static readonly string WriteAction = "write";

        
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("resource"), JsonConverter(typeof(StringEnumConverter))]
        public PermissionResourceType Resource { get; set; }
        
        [JsonProperty("action")]
        public string Action { get; set; }
    }
}