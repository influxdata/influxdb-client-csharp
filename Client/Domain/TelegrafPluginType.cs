using System.Runtime.Serialization;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    /// Type is a telegraf plugin type.
    /// </summary>
    public enum TelegrafPluginType
    {
        [EnumMember(Value = "input")] 
        Input,
        
        [EnumMember(Value = "output")] 
        Output,
        
        [EnumMember(Value = "processor")] 
        Processor,
        
        [EnumMember(Value = "aggregator")] 
        Aggregator
    }
}