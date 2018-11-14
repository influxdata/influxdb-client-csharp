using System;
using System.Collections.Generic;
using System.Text;

namespace Platform.Client.Domain
{
/**
 * The wrapper for "/api/v2/authorizations" response.
 */
    public class Authorizations : AbstractHasLinks
    {
        public List<Authorization> Auths { get; set; } = new List<Authorization>();

        public override string ToString()
        {
            return new StringBuilder(GetType().Name + "[")
                            .Append("auths=" + Auths)
                            .Append("]").ToString();
        }
    }
}