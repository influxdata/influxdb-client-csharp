
using System;

namespace Flux.Flux.Options
{
    public class FluxConnectionOptions
    {
        public string Url { get; set; }
        
        public string OrgID { get; set; }

        public TimeSpan Timeout { get; set; }
        
        public FluxConnectionOptions(string url, string orgId)
        {
            Initialize(url, orgId, TimeSpan.FromSeconds(60));
        }

        public FluxConnectionOptions(string url, string orgId, TimeSpan timeout) 
        {
            Initialize(url, orgId, timeout);
        }

        private void Initialize(string url, string orgId, TimeSpan timeout)
        {
            Url = url;
            OrgID = orgId;
            Timeout = timeout;
        }
    }
}