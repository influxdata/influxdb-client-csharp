using System.Runtime.Serialization;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// The type of resource. 
    /// </summary>
    public enum ResourceType
    {
        [EnumMember(Value = "dashboard")]
        DashboardResourceType,

        [EnumMember(Value = "bucket")]
        BucketResourceType,

        [EnumMember(Value = "task")]
        TaskResourceType,

        [EnumMember(Value = "org")]
        OrgResourceType,

        [EnumMember(Value = "view")] 
        ViewResourceType
    }
}