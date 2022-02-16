using System;
using System.Linq;
using System.Web;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    public class FindOptions
    {
        public const string LimitKey = "limit";
        public const string OffsetKey = "offset";
        public const string SortByKey = "sortBy";
        public const string DescendingKey = "descending";
        public const string AfterKey = "after";

        [JsonProperty("limit")] public int? Limit { get; set; }

        [JsonProperty("offset")] public int? Offset { get; set; }

        [JsonProperty("sortBy")] public string SortBy { get; set; }

        [JsonProperty("descending")] public bool? Descending { get; set; }

        [JsonProperty("after")] public string After { get; set; }

        internal static FindOptions GetFindOptions(string link)
        {
            var options = new FindOptions();

            if (string.IsNullOrEmpty(link))
            {
                return options;
            }

            var qs = HttpUtility.ParseQueryString(link.Substring(link.LastIndexOf("?", StringComparison.Ordinal)));

            var keys = qs.AllKeys;
            if (!keys.Contains(LimitKey) && !keys.Contains(OffsetKey) &&
                !keys.Contains(SortByKey) && !keys.Contains(DescendingKey) && !keys.Contains(AfterKey))
            {
                return null;
            }

            var findOptions = new FindOptions();
            if (keys.Contains(LimitKey))
            {
                findOptions.Limit = int.Parse(qs.Get(LimitKey));
            }

            if (keys.Contains(OffsetKey))
            {
                findOptions.Offset = int.Parse(qs.Get(OffsetKey));
            }

            if (keys.Contains(SortByKey))
            {
                findOptions.SortBy = qs.Get(SortByKey);
            }

            if (keys.Contains(AfterKey))
            {
                findOptions.After = qs.Get(AfterKey);
            }

            if (keys.Contains(DescendingKey))
            {
                findOptions.Descending = bool.Parse(qs.Get(DescendingKey));
            }

            return findOptions;
        }
    }
}