using System;
using System.Linq;
using System.Web;
using InfluxDB.Client.Core;

namespace InfluxDB.Client.Domain
{
    public abstract class AbstractPageLinks : AbstractHasLinks
    {
        public FindOptions GetPrevPage()
        {
            return GetFindOptions("prev");
        }

        public FindOptions GetSelfPage()
        {
            return GetFindOptions("self");
        }

        public FindOptions GetNextPage()
        {
            return GetFindOptions("next");
        }

        private FindOptions GetFindOptions(string key)
        {
            Arguments.CheckNonEmptyString(key, nameof(key));

            if (!Links.ContainsKey(key)) return null;

            var pageLink = Links[key];
            var qs = HttpUtility.ParseQueryString(pageLink.Substring(Links[key]
                .LastIndexOf("?", StringComparison.Ordinal)));

            var keys = qs.AllKeys;
            if (!keys.Contains(FindOptions.LimitKey) && !keys.Contains(FindOptions.OffsetKey) &&
                !keys.Contains(FindOptions.SortByKey) && !keys.Contains(FindOptions.DescendingKey))
                return null;

            var findOptions = new FindOptions();
            if (keys.Contains(FindOptions.LimitKey)) findOptions.Limit = int.Parse(qs.Get(FindOptions.LimitKey));
            if (keys.Contains(FindOptions.OffsetKey)) findOptions.Offset = int.Parse(qs.Get(FindOptions.OffsetKey));
            if (keys.Contains(FindOptions.SortByKey)) findOptions.SortBy = qs.Get(FindOptions.SortByKey);
            if (keys.Contains(FindOptions.DescendingKey))
                findOptions.Descending = bool.Parse(qs.Get(FindOptions.DescendingKey));

            return findOptions;
        }
    }
}