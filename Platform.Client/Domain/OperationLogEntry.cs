using NodaTime;

namespace Platform.Client.Domain
{
    /**
     * OperationLogEntry is a record in an operation log.
     */
    public class OperationLogEntry : AbstractHasLinks
    {
        public string UserId { get; set; }

        public string Description { get; set; }
        
        public Instant Time { get; set; }
    }
}