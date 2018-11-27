using System.Runtime.Serialization;

namespace InfluxData.Platform.Client.Domain
{
    /**
     * The type of resource.
     */
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