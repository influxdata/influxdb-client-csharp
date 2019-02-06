using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    public class OperationLogEntries : AbstractPageLinks
    {
        [JsonProperty("log")] public List<OperationLogEntry> Logs { get; set; } = new List<OperationLogEntry>();
    }
}