using System.Diagnostics;
using System.Linq;

namespace InfluxDB.Client.Core
{
    public class CategoryTraceFilter : TraceFilter
    {
        public const string CategoryInflux = "influx-client";
        public const string CategoryInfluxError = "influx-client-error";
        public const string CategoryInfluxQuery = "influx-client-query";
        public const string CategoryInfluxQueryError = "influx-client-query-error";
        public const string CategoryInfluxWrite = "influx-client-write";
        public const string CategoryInfluxWriteError = "influx-client-write-error";
        public const string CategoryInfluxLogger = "influx-client-logger";

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
                CategoryInflux,
                CategoryInfluxError,
                CategoryInfluxQuery,
                CategoryInfluxQueryError,
                CategoryInfluxWrite,
                CategoryInfluxWriteError,
                CategoryInfluxLogger
            }, false);
        }

        public static CategoryTraceFilter SuppressInfluxVerbose()
        {
            return new CategoryTraceFilter(new string[]
            {
                CategoryInflux,
                CategoryInfluxQuery,
                CategoryInfluxWrite,
                CategoryInfluxLogger
            }, false);
        }
    }
}