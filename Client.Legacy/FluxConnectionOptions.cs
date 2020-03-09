using System;

namespace InfluxDB.Client.Flux
{
    public class FluxConnectionOptions
    {
        public string Url { get; private set; }

        public TimeSpan Timeout { get; private set; }

        public string Username { get; }
        public char[] Password { get; }

        public FluxConnectionOptions(string url) : this(url, TimeSpan.FromSeconds(60))
        {
        }

        public FluxConnectionOptions(string url, string username = "", char[] password = null) : this(url,
            TimeSpan.FromSeconds(60), username, password)
        {
        }

        public FluxConnectionOptions(string url, TimeSpan timeout, string username = "", char[] password = null)
        {
            Url = url;
            Timeout = timeout;
            Username = username;
            Password = password;
        }
    }
}