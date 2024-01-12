using System.Text;
using InfluxDB.Client.Core;
using NodaTime;

namespace Examples
{
    public class Cpu
    {
        public Instant Time { get; set; }

        [Column("_value")] public double Usage { get; set; }

        public string Host { get; set; }

        public override string ToString()
        {
            return new StringBuilder(GetType().Name + "[")
                .Append("time=" + Time)
                .Append(", usage=" + Usage)
                .Append(", host=" + Host)
                .Append("]").ToString();
        }
    }
}