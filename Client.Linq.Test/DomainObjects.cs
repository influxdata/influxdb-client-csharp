using System;
using System.Collections.Generic;
using InfluxDB.Client.Core;

namespace Client.Linq.Test
{
    [Measurement("test_point")]
    internal class InfluxPoint
    {
        [Column("test_tag", IsTag = true)] 
        public string TestTag { get; set; }
        
        [Column("int_value")]
        public int IntValue { get; set; }
        
        [Column("short_value")]
        public short ShortValue { get; set; }
        
        [Column("long_value")]
        public long LongValue { get; set; }
        
        [Column("double_value")]
        public double DoubleValue { get; set; }
        
        [Column("float_value")]
        public float FloatValue { get; set; }
    
        [Column("_time", IsTimestamp = true)] 
        public DateTime Timestamp { get; set; }
    }
    
    [Measurement("sensor")]
    internal class Sensor
    {
        [Column("sensor_id", IsTag = true)] public string SensorId { get; set; }

        /// <summary>
        /// "production" or "testing"
        /// </summary>
        [Column("deployment", IsTag = true)]
        public string Deployment { get; set; }

        /// <summary>
        /// Value measured by sensor
        /// </summary>
        [Column("data")]
        public int Value { get; set; }

        [Column(IsTimestamp = true)] public DateTime Timestamp { get; set; }
    }

    internal class SensorDateTimeOffset
    {
        [Column("sensor_id", IsTag = true)] public string SensorId { get; set; }

        /// <summary>
        /// Value measured by sensor
        /// </summary>
        [Column("data")]
        public float Value { get; set; }

        [Column(IsTimestamp = true)] public DateTimeOffset Timestamp { get; set; }
    }

    internal class SensorWithCustomMeasurement
    {
        [Column(IsMeasurement = true)] public string Measurement { get; set; }

        [Column("sensor_id", IsTag = true)] public string SensorId { get; set; }

        [Column("data")] public int Value { get; set; }
    }

    internal class SensorCustom
    {
        public Guid Id { get; set; }

        public float Value { get; set; }

        public DateTimeOffset Time { get; set; }

        public virtual ICollection<SensorAttribute> Attributes { get; set; }
    }

    internal class SensorAttribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }

    [Measurement(nameof(TagIsNotDefinedAsString))]
    internal class TagIsNotDefinedAsString
    {
        [Column(IsTag = true)] public int Id { get; set; }

        [Column(IsTag = true)] public string Tag { get; set; }

        [Column(nameof(Energy))] public decimal Energy { get; set; }

        [Column(IsTimestamp = true)] public DateTime Timestamp { get; set; }
    }

    public class DataEntityWithLong
    {
        public long EndWithTicks { get; set; }
    }

    internal class SensorDateTimeAsField
    {
        [Column("data")] public int Value { get; set; }

        [Column("dataTime")] public DateTime DateTimeField { get; set; }
    }
}