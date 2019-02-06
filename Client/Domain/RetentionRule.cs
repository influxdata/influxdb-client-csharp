namespace InfluxDB.Client.Domain
{
    /// <summary>
    /// The retention rule action for a bucket. 
    /// </summary>
    public class RetentionRule
    {
        public string Type { get; set; }

        public long EverySeconds { get; set; }
    }
}