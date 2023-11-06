using System.Diagnostics;
using System.Linq;

namespace InfluxDB.Client.Core
{
    public class CategoryTraceFilter : TraceFilter
    {
        public const string CategoryInflux = "influx-client";

        private readonly string[] categoryToFilter;
        private readonly bool keep;

        public CategoryTraceFilter(string[] categoryToFilter, bool keep)
        {
            this.categoryToFilter = categoryToFilter;
            this.keep = keep;
        }

        public override bool ShouldTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
            string formatOrMessage, object[] args, object data, object[] dataArray)
        {
            return categoryToFilter.Any(x => x == source) ^ keep;
        }

        public static CategoryTraceFilter SuppressInflux()
        {
            return new CategoryTraceFilter(new string[]
            {
                CategoryInflux
            }, false);
        }
    }
}