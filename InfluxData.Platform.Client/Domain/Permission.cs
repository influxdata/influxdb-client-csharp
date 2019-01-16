using Newtonsoft.Json;

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
        public const string ReadAction = "read";

        /// <summary>
        /// Action for writing.
        /// </summary>
        public const string WriteAction = "write";

        [JsonProperty("action")]
        public string Action { get; set; }

        [JsonProperty("resource")]
        public PermissionResource Resource { get; set; }
    }
}