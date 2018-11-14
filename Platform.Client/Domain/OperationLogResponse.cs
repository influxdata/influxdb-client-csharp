using System.Collections.Generic;

namespace Platform.Client.Domain
{
    public class OperationLogResponse : AbstractHasLinks
    {
        public List<OperationLogEntry> Log { get; set; } = new List<OperationLogEntry>();
    }
}