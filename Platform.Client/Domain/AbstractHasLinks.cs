using System.Collections.Generic;

namespace Platform.Client.Domain
{
    public abstract class AbstractHasLinks
    {
        /**
         * The URIs of resources.
         */
        public Dictionary<string, string> Links { get; set; } = new Dictionary<string, string>();
    }
}