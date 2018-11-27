namespace InfluxData.Platform.Client.Domain
{
    /**
     * The retention rule action for a bucket.
     */
    public class RetentionRule
    {
        public string Type { get; set; }

        public long EverySeconds { get; set; }
    }
}