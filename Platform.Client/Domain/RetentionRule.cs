namespace Platform.Client.Domain
{
    /**
     * The retention rule action for a bucket.
     */
    public class RetentionRule
    {
        public string Type { get; set; }
        
        private long EverySeconds { get; set; }
    }
}