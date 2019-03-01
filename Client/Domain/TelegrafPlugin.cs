using System.Collections.Generic;
using InfluxDB.Client.Core.Internal;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    /// TelegrafPlugin is the general wrapper of the telegraf plugin config.
    /// </summary>
    public class TelegrafPlugin
    {
        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("comment")]
        public string Comment { get; set; }
        
        [JsonProperty("type")]
        [JsonConverter(typeof(EnumConverter))]
        public TelegrafPluginType Type { get; set; }
        
        [JsonProperty("config")]
        public Dictionary<string, object> Config { get; set; } = new Dictionary<string, object>();
    }
}