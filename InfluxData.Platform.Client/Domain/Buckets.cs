using System.Collections.Generic;

namespace InfluxData.Platform.Client.Domain
{
    /**
     * The wrapper for "/api/v2/buckets" response.
     */
    public class Buckets : AbstractHasLinks
    {
        public List<Bucket> BucketList { get; set; } = new List<Bucket>();
    }
}