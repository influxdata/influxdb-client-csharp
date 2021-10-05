using System;

namespace InfluxDB.Client.Core
{
    /// <summary>
    /// The annotation is used for mapping POCO class into line protocol.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class Measurement : Attribute
    {
        public string Name { get; }

        public Measurement(string name)
        {
            Name = name;
        }
    }

    /// <summary>
    /// The annotation is used to customize bidirectional mapping between POCO and Flux query result or Line Protocol.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class Column : Attribute
    {
        public string Name { get; }

        public bool IsMeasurement { get; set; }

        public bool IsTag { get; set; }

        public bool IsTimestamp { get; set; }

        public Column()
        {
        }

        public Column(string name)
        {
            Name = name;
        }
    }
}