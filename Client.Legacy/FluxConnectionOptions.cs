using System;
using System.Net;

namespace InfluxDB.Client.Flux
{
    public class FluxConnectionOptions
    {
        public enum AuthenticationType
        {
            UrlQueryParameters,
            BasicAuthentication
        }

        public string Url { get; private set; }

        public TimeSpan Timeout { get; private set; }

        public string Username { get; }
        public char[] Password { get; }

        public AuthenticationType Authentication { get; }

        public IWebProxy WebProxy { get; }

        public FluxConnectionOptions(string url) : this(url, TimeSpan.FromSeconds(60))
        {
        }

        public FluxConnectionOptions(string url, string username = "", char[] password = null,
            AuthenticationType authentication = AuthenticationType.UrlQueryParameters) : this(url,
            TimeSpan.FromSeconds(60), username, password, authentication)
        {
        }

        public FluxConnectionOptions(string url, TimeSpan timeout, string username = "", char[] password = null,
            AuthenticationType authentication = AuthenticationType.UrlQueryParameters,
            IWebProxy webProxy = null)
        {
            Url = url;
            Timeout = timeout;
            Username = username;
            Password = password;
            Authentication = authentication;
            WebProxy = webProxy;
        }
    }
}