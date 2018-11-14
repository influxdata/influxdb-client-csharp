namespace Platform.Client.Domain
{
    /**
     * Source is an external Influx with time series data.
     */
    public class Source
    {
        /**
         * The unique ID of the source.
         */
        public string Id { get; set; }

        /**
         * The organization ID that resource belongs to.
         */
        private string OrganizationId { get; set; }

        /**
         * Specifies the default source for the application.
         */
        public bool DefaultSource { get; set; }

        /**
         * The user-defined name for the source.
         */
        public string Name { get; set; }

        /**
         * Type specifies which kinds of source (enterprise vs oss vs 2.0).
         */
        public SourceType Type { get; set; }

        /**
         * SourceType is a string for types of sources.
         */
        public enum SourceType
        {
            V2SourceType,

            V1SourceType,

            SelfSourceType
        }

        /**
         * URL are the connections to the source.
         */
        public string Url { get; set; }

        /**
         * InsecureSkipVerify as true means any certificate presented by the source is accepted.
         */
        private bool InsecureSkipVerify { get; set; }

        /**
         * Telegraf is the db telegraf is written to. By default it is "telegraf".
         */
        public string Telegraf { get; set; }

        /**
         * Token is the 2.0 authorization token associated with a source.
         */
        public string Token { get; set; }

        //
        // V1SourceFields are the fields for connecting to a 1.0 source (oss or enterprise)
        //

        /**
         * The username to connect to the source (V1SourceFields).
         */
        public string UserName { get; set; }

        /**
         * Password is in CLEARTEXT (V1SourceFields).
         */
        public string Password { get; set; }

        /**
         * The optional signing secret for Influx JWT authorization (V1SourceFields).
         */
        public string SharedSecret { get; set; }

        /**
         * The url for the meta node (V1SourceFields).
         */
        public string MetaUrl { get; set; }

        /**
         * The default retention policy used in database queries to this source (V1SourceFields).
         */
        public string DefaultRp { get; set; }
    }
}