using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    /// TelegrafConfig stores telegraf config for one telegraf instance.
    /// </summary>
    public class TelegrafConfig
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("organizationID")]
        public string OrgId { get; set; }
        
        [JsonProperty("description")]
        public string Description { get; set; }
        
        [JsonProperty("agent")]
        public TelegrafAgent Agent { get; set; }
        
        [JsonProperty("plugins")]
        public List<TelegrafPlugin> Plugins { get; set;} = new List<TelegrafPlugin>();
    }
}