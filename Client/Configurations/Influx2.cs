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
            get => (string) base["url"];
            set => base["url"] = value;
        }

        /// <summary>
        /// Specify the default destination organization for writes and queries.
        /// </summary>
        [ConfigurationProperty("org", IsKey = true, IsRequired = false)]
        public string Org
        {
            get => (string) base["org"];
            set => base["org"] = value;
        }

        /// <summary>
        /// Specify the default destination bucket for writes.
        /// </summary>
        [ConfigurationProperty("bucket", IsKey = true, IsRequired = false)]
        public string Bucket
        {
            get => (string) base["bucket"];
            set => base["bucket"] = value;
        }

        /// <summary>
        /// The token to use for the authorization.
        /// </summary>
        [ConfigurationProperty("token", IsKey = true, IsRequired = false)]
        public string Token
        {
            get => (string) base["token"];
            set => base["token"] = value;
        }

        /// <summary>
        /// The log level for the request and response information.
        /// </summary>
        [ConfigurationProperty("logLevel", IsKey = true, IsRequired = false)]
        public string LogLevel
        {
            get => (string) base["logLevel"];
            set => base["logLevel"] = value;
        }

        /// <summary>
        /// The timeout to read and write from the InfluxDB.
        /// </summary>
        [ConfigurationProperty("readWriteTimeout", IsKey = true, IsRequired = false)]
        public string ReadWriteTimeout
        {
            get => (string) base["readWriteTimeout"];
            set => base["readWriteTimeout"] = value;
        }

        /// <summary>
        /// The timeout to connect the InfluxDB.
        /// </summary>
        [ConfigurationProperty("timeout", IsKey = true, IsRequired = false)]
        public string Timeout
        {
            get => (string) base["timeout"];
            set => base["timeout"] = value;
        }
    }
}