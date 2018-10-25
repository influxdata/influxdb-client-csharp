using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
        public TimeSpan GetStart() 
        {
            return (TimeSpan) GetValueByKey("_start");
        }

        /**
         * @return the exclusive upper time bound of all records
         */
        public TimeSpan GetStop() 
        {
            return (TimeSpan) GetValueByKey("_stop");
        }

        /**
         * @return the time of the record
         */
        public TimeSpan GetTime() 
        {
            return (TimeSpan) GetValueByKey("_time");
        }

        /**
         * @return the value of the record
         */
        public Object GetValue() 
        {
            return (Object) GetValueByKey("_value");
        }

        /**
         * @return get value with key <i>_field</i>
         */
        public String GetField() 
        {
            return (String) GetValueByKey("_field");
        }

        /**
         * @return get value with key <i>_measurement</i>
         */
        public String GetMeasurement() 
        {
            return (String) GetValueByKey("_measurement");
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
            return Values[key];
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