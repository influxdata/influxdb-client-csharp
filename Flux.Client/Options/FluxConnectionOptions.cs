
using System;

namespace Flux.Client.Options
{
    public class FluxConnectionOptions
    {
        public string Url { get; set; }

        public TimeSpan Timeout { get; set; }
        
        public FluxConnectionOptions(string url)
        {
            Initialize(url, TimeSpan.FromSeconds(60));
        }

        public FluxConnectionOptions(string url, TimeSpan timeout) 
        {
            Initialize(url, timeout);
        }

        private void Initialize(string url, TimeSpan timeout)
        {
            Url = url;
            Timeout = timeout;
        }
    }
}