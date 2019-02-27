using System.Collections.Generic;
using InfluxDB.Client.Core.Internal;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    ///     Authorization
    /// </summary>
    public class Authorization : AbstractHasLinks
    {
        [JsonProperty("description")] public string Description;

        [JsonProperty("id")] public string Id { get; set; }

        [JsonProperty("token")] public string Token { get; set; }

        [JsonProperty("userID")] public string UserId { get; set; }

        [JsonProperty("user")] public string UserName { get; set; }

        [JsonProperty("orgID")] public string OrgId { get; set; }

        [JsonProperty("org")] public string OrgName { get; set; }

        [JsonProperty("status")]
        [JsonConverter(typeof(EnumConverter))]
        public Status? Status { get; set; }

        [JsonProperty("permissions")] public List<Permission> Permissions { get; set; } = new List<Permission>();
    }
}