using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    public class TelegrafConfigs
    {
        [JsonProperty("configurations")] 
        public List<TelegrafConfig> Configs { get; set; } = new List<TelegrafConfig>();
    }
}