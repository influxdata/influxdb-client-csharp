using System.Collections.Generic;

namespace InfluxData.Platform.Client.Domain
{
    public class OperationLogResponse : AbstractHasLinks
    {
        public List<OperationLogEntry> Log { get; set; } = new List<OperationLogEntry>();
    }
}