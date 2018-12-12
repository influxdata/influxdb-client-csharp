using System.Runtime.Serialization;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// The status of the <see cref="Run"/>.
    /// </summary>
    public enum RunStatus
    {
        [EnumMember(Value = "scheduled")]
        Scheduled,

        [EnumMember(Value = "started")]
        Started,

        [EnumMember(Value = "failed")]
        Failed,

        [EnumMember(Value = "success")]
        Success,
        
        [EnumMember(Value = "canceled")]
        Canceled
    }
}