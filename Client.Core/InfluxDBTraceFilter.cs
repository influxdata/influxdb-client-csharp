using System.Diagnostics;
using System.Linq;

namespace InfluxDB.Client.Core
{
    /// <summary>
    /// The <see cref="InfluxDBTraceFilter"/> is used to filter client trace messages by category.
    /// </summary>
    public class InfluxDBTraceFilter : TraceFilter
    {
        public const string CategoryInflux = "influx-client";
        public const string CategoryInfluxError = "influx-client-error";
        public const string CategoryInfluxQuery = "influx-client-query";
        public const string CategoryInfluxQueryError = "influx-client-query-error";
        public const string CategoryInfluxWrite = "influx-client-write";
        public const string CategoryInfluxWriteError = "influx-client-write-error";
        public const string CategoryInfluxLogger = "influx-client-logger";

        private readonly string[] _categoryToFilter;
        private readonly bool _keep;

        public InfluxDBTraceFilter(string[] categoryToFilter, bool keep)
        {
            _categoryToFilter = categoryToFilter;
            _keep = keep;
        }

        public override bool ShouldTrace(TraceEventCache eventCache, string source, TraceEventType eventType, int id,
            string formatOrMessage, object[] args, object data, object[] dataArray)
        {
            return _categoryToFilter.Any(x => x == source) ^ _keep;
        }

        /// <summary>
        /// Suppress all client trace messages.
        /// </summary>
        /// <returns>Trace Filter</returns>
        public static InfluxDBTraceFilter SuppressInflux()
        {
            return new InfluxDBTraceFilter(new string[]
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

        /// <summary>
        /// Suppress all client trace messages except <see cref="CategoryInfluxError"/>, <see cref="CategoryInfluxQueryError"/>, <see cref="CategoryInfluxWriteError"/>.
        /// </summary>
        /// <returns>Trace Filter</returns>
        public static InfluxDBTraceFilter SuppressInfluxVerbose()
        {
            return new InfluxDBTraceFilter(new string[]
            {
                CategoryInflux,
                CategoryInfluxQuery,
                CategoryInfluxWrite,
                CategoryInfluxLogger
            }, false);
        }
    }
}