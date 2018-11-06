using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NodaTime;

namespace Platform.Common.Flux.Domain
{
/**
 * A record is a tuple of values. Each record in the table represents a single point in the series.
 *
 * <a href="https://github.com/influxdata/platform/blob/master/query/docs/SPEC.md#record">Specification</a>.
 */
    public class FluxRecord
    {
        /**
        * The Index of the table that the record belongs.
        */
        public int Table { get; set; }

        /**
        * The record's values.
        */
        public Dictionary<string, object> Values { get; } = new Dictionary<string, object>();

        public FluxRecord(int table)
        {
            Table = table;
        }
        
        /**
         * @return the inclusive lower time bound of all records
         */
        public Instant? GetStart() 
        {
            return (Instant?) GetValueByKey("_start");
        }

        /**
         * @return the exclusive upper time bound of all records
         */
        public Instant? GetStop() 
        {
            return (Instant?) GetValueByKey("_stop");
        }

        /**
         * @return the time of the record
         */
        public Instant? GetTime() 
        {
            return (Instant?) GetValueByKey("_time");
        }

        /**
         * @return the value of the record
         */
        public Object GetValue() 
        {
            return GetValueByKey("_value");
        }

        /**
         * @return get value with key <i>_field</i>
         */
        public string GetField() 
        {
            return (string) GetValueByKey("_field");
        }

        /**
         * @return get value with key <i>_measurement</i>
         */
        public string GetMeasurement() 
        {
            return (string) GetValueByKey("_measurement");
        }

        /**
        * Get FluxRecord value by index.
        *
        * @param index of value in CSV response
        * @return value
        */
        public object GetValueByIndex(int index)
        {
            return Values.Values.ToList()[index];
        }

        /**
        * Get FluxRecord value by key.
        *
        * @param key of value in CSV response
        * @return value
        */
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