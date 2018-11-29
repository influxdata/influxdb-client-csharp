using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// Authorization
    /// </summary>
    public class Authorization : AbstractHasLinks
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("token")]
        public string Token { get; set; }
        
        [JsonProperty("userID")]
        public string UserId { get; set; }
        
        [JsonProperty("user")]
        public string UserName { get; set; }

        [JsonProperty("status"), JsonConverter(typeof(StringEnumConverter))]
        public Status Status { get; set; }
        
        [JsonProperty("permissions")]
        public List<Permission> Permissions { get; set; }

        public override string ToString() 
        {
            return new StringBuilder(GetType().Name + "[")
                            .Append("id='" + Id + "'")
                            .Append(", token='" + Token + "'")
                            .Append(", userID='" + UserId + "'")
                            .Append(", userName='" + UserName + "'")
                            .Append(", status=" + Status)
                            .Append(", permissions=" + Permissions)
                            .Append("]").ToString();
        }
    }
}