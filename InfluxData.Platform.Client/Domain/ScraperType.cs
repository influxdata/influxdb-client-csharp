using System.Runtime.Serialization;

namespace InfluxData.Platform.Client.Domain
{
    public enum ScraperType
    {
        [EnumMember(Value = "prometheus")]
        Prometheus
    }
}