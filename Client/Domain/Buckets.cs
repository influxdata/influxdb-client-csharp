using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    ///     he wrapper for "/api/v2/buckets" response.
    /// </summary>
    public class Buckets : AbstractPageLinks
    {
        [JsonProperty("buckets")] public List<Bucket> BucketList { get; set; } = new List<Bucket>();
    }
}