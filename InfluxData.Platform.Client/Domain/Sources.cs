using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// The wrapper for "/v2/sources" response.
    /// </summary>
    public class Sources : AbstractHasLinks 
    {
        [JsonProperty("sources")]
        private List<Source> SourceList { get; set; } = new List<Source>();
    }
}