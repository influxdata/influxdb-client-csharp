using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    /// The wrapper for "/v2/sources" response.
    /// </summary>
    public class Sources : AbstractHasLinks 
    {
        [JsonProperty("sources")]
        public List<Source> SourceList { get; set; } = new List<Source>();
    }
}