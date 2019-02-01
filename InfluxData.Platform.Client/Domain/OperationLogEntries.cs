using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    public class OperationLogEntries : AbstractPageLinks
    {
        [JsonProperty("log")] public List<OperationLogEntry> Logs { get; set; } = new List<OperationLogEntry>();
    }
}