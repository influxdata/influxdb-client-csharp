using System.Runtime.Serialization;

namespace InfluxDB.Client.Domain
{
    public enum ScraperType
    {
        [EnumMember(Value = "prometheus")]
        Prometheus
    }
}