using System;
using System.Collections.Generic;
using InfluxDB.Client.Core;

namespace Client.Linq.Test
{
    [Measurement("sensor")]
    class Sensor
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
        public float Value { get; set; }

        [Column(IsTimestamp = true)] public DateTime Timestamp { get; set; }
    }
    
    class SensorDateTimeOffset
    {
        [Column("sensor_id", IsTag = true)] public string SensorId { get; set; }

        /// <summary>
        /// Value measured by sensor
        /// </summary>
        [Column("data")]
        public float Value { get; set; }

        [Column(IsTimestamp = true)] public DateTimeOffset Timestamp { get; set; }
    }

    class SensorCustom
    {
        public Guid Id { get; set; }
        
        public DateTimeOffset Time { get; set; }
        
        public virtual ICollection<SensorAttribute> Attributes { get; set; }
    }

    class SensorAttribute
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}