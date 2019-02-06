using NodaTime;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    /// OperationLogEntry is a record in an operation log.
    /// </summary>
    public class OperationLogEntry : AbstractHasLinks
    {
        public string UserId { get; set; }

        public string Description { get; set; }
        
        public Instant Time { get; set; }
    }
}