namespace Platform.Client.Domain
{
    /**
     * The health of Platform.
     */
    public class Health
    {
        public static readonly string HealthyStatus = "healthy";

        public string Status { get; set; }
        
        public string Message { get; set; }

        public bool IsHealthy() 
        {
            return HealthyStatus.Equals(Status);
        }
    }
}