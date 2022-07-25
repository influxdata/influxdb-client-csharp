using System.Configuration;

namespace InfluxDB.Client.Configurations

{
    public class Influx2 : ConfigurationSection
    {
        /// <summary>
        /// The url to connect the InfluxDB.
        /// </summary>
        [ConfigurationProperty("url", IsKey = true, IsRequired = true)]
        public string Url
        {
            get => (string)base["url"];
            set => base["url"] = value;
        }

        /// <summary>
        /// Specify the default destination organization for writes and queries.
        /// </summary>
        [ConfigurationProperty("org", IsKey = true, IsRequired = false)]
        public string Org
        {
            get => (string)base["org"];
            set => base["org"] = value;
        }

        /// <summary>
        /// Specify the default destination bucket for writes.
        /// </summary>
        [ConfigurationProperty("bucket", IsKey = true, IsRequired = false)]
        public string Bucket
        {
            get => (string)base["bucket"];
            set => base["bucket"] = value;
        }

        /// <summary>
        /// The token to use for the authorization.
        /// </summary>
        [ConfigurationProperty("token", IsKey = true, IsRequired = false)]
        public string Token
        {
            get => (string)base["token"];
            set => base["token"] = value;
        }

        /// <summary>
        /// The log level for the request and response information.
        /// </summary>
        [ConfigurationProperty("logLevel", IsKey = true, IsRequired = false)]
        public string LogLevel
        {
            get => (string)base["logLevel"];
            set => base["logLevel"] = value;
        }

        /// <summary>
        /// The timespan to wait before the HTTP request times out.
        /// </summary>
        [ConfigurationProperty("timeout", IsKey = true, IsRequired = false)]
        public string Timeout
        {
            get => (string)base["timeout"];
            set => base["timeout"] = value;
        }

        /// <summary>
        /// Configure automatically following HTTP 3xx redirects.
        /// </summary>
        [ConfigurationProperty("allowHttpRedirects", IsKey = true, IsRequired = false)]
        public bool AllowHttpRedirects
        {
            get => (bool)base["allowHttpRedirects"];
            set => base["allowHttpRedirects"] = value;
        }

        /// <summary>
        /// Ignore Certificate Validation Errors when false
        /// </summary>
        [ConfigurationProperty("verifySsl", IsKey = true, IsRequired = false, DefaultValue = true)]
        public bool VerifySsl
        {
            get => (bool)base["verifySsl"];
            set => base["verifySsl"] = value;
        }

        [ConfigurationProperty("tags", IsRequired = false)]
        public TagCollection Tags
        {
            get => base["tags"] as TagCollection;
            set => base["tags"] = value;
        }

        [ConfigurationCollection(typeof(TagElement))]
        public class TagCollection : ConfigurationElementCollection
        {
            public override ConfigurationElementCollectionType CollectionType =>
                ConfigurationElementCollectionType.BasicMapAlternate;

            protected override string ElementName => "tag";

            protected override ConfigurationElement CreateNewElement()
            {
                return new TagElement();
            }

            protected override object GetElementKey(ConfigurationElement element)
            {
                return ((TagElement)element).Name;
            }
        }

        public class TagElement : ConfigurationElement
        {
            [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
            public string Name => (string)base["name"];

            [ConfigurationProperty("value", IsRequired = false)]
            public string Value => (string)base["value"];
        }
    }
}