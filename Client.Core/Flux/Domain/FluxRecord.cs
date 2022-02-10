using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodaTime;

namespace InfluxDB.Client.Core.Flux.Domain
{
    /// <summary>
    /// A record is a tuple of values. Each record in the table represents a single point in the series.
    ///
    /// <para><a href="http://bit.ly/flux-spec#record">Specification</a>.</para>
    /// </summary>
    public class FluxRecord
    {
        /// <summary>
        /// The Index of the table that the record belongs.
        /// </summary>
        public int Table { get; set; }

        /// <summary>
        /// The record's values.
        /// </summary>
        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        public FluxRecord(int table)
        {
            Table = table;
        }

        /// <returns>the inclusive lower time bound of all records</returns>
        public Instant? GetStart()
        {
            return (Instant?)GetValueByKey("_start");
        }

        /// <returns>the exclusive upper time bound of all records</returns>
        public Instant? GetStop()
        {
            return (Instant?)GetValueByKey("_stop");
        }

        ///<summary>
        /// The timestamp as a <see cref="Instant"/>
        /// </summary>
        /// <returns>the time of the record</returns>
        public Instant? GetTime()
        {
            return (Instant?)GetValueByKey("_time");
        }

        ///<summary>
        /// The timestamp as a <see cref="DateTime"/>
        /// </summary>
        /// <returns>the time of the record</returns>
        public DateTime? GetTimeInDateTime()
        {
            var time = GetTime();

            return time?.InUtc().ToDateTimeUtc() ?? default(DateTime);
        }

        /// <returns>the value of the record</returns>
        public object GetValue()
        {
            return GetValueByKey("_value");
        }

        /// <returns>get value with key <i>_field</i></returns>
        public string GetField()
        {
            return (string)GetValueByKey("_field");
        }

        /// <returns>get value with key <i>_measurement</i></returns>
        public string GetMeasurement()
        {
            return (string)GetValueByKey("_measurement");
        }

        /// <summary>
        /// Get FluxRecord value by index.
        /// </summary>
        /// <param name="index">index of value in CSV response</param>
        /// <returns>value</returns>
        public object GetValueByIndex(int index)
        {
            return Values.Values.ToList()[index];
        }

        /// <summary>
        /// Get FluxRecord value by key.
        /// </summary>
        /// <param name="key">the key of value in CSV response</param>
        /// <returns>value</returns>
        public object GetValueByKey(string key)
        {
            object value;

            if (Values.TryGetValue(key, out value))
            {
                return value;
            }

            return null;
        }

        public override string ToString()
        {
            return new StringBuilder(GetType().Name + "[")
                .Append("table=" + Table)
                .Append(", values=" + Values.Count)
                .Append("]")
                .ToString();
        }
    }
}