using System.Collections.Generic;
using System.Text;

namespace Platform.Client.Domain
{
    /**
     * The wrapper for "/api/v2/orgs" response.
     */
    public class Organizations : AbstractHasLinks
    {
        public List<Organization> Orgs { get; set; } = new List<Organization>();

        public override string ToString()
        {
            return new StringBuilder(GetType().Name + "[")
                            .Append("links=" + Links)
                            .Append(", orgs=" + Orgs)
                            .Append("]").ToString();
        }
    }
}