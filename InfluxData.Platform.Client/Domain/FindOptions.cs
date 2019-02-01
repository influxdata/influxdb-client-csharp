using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    public class FindOptions
    {
        public const string LimitKey = "limit";
        public const string OffsetKey = "offset";
        public const string SortByKey = "sortBy";
        public const string DescendingKey = "descending";

        [JsonProperty("limit")] public int? Limit { get; set; }

        [JsonProperty("offset")] public int? Offset { get; set; }

        [JsonProperty("sortBy")] public string SortBy { get; set; }

        [JsonProperty("descending")] public bool? Descending { get; set; }
    }
}