using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// he wrapper for "/api/v2/buckets" response.
    /// </summary>
    public class Buckets : AbstractHasLinks
    {
        [JsonProperty("buckets")]
        public List<Bucket> BucketList { get; set; } = new List<Bucket>();
    }
}