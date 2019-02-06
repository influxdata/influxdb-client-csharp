using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    public class Users : AbstractHasLinks
    {
        [JsonProperty("users")]
        public List<User> UserList { get; set; } = new List<User>();
    }
}