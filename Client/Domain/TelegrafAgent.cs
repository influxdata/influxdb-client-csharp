using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    /// TelegrafAgent is based telegraf/internal/config AgentConfig.
    /// </summary>
    public class TelegrafAgent
    {
        /// <summary>
        /// Default data collection interval for all inputs in milliseconds.
        /// </summary>
        [JsonProperty("collectionInterval")]
        public int CollectionInterval { get; set; }
    }
}