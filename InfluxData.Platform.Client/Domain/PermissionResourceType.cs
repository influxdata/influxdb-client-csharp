using System.Runtime.Serialization;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// The type of resource. 
    /// </summary>
    public enum PermissionResourceType
    {
        [EnumMember(Value =  "authorizations")]
        Authorization,

        [EnumMember(Value =  "buckets")]
        Bucket,

        [EnumMember(Value =  "dashboards")]
        Dashboard,

        [EnumMember(Value =  "orgs")]
        Org,

        [EnumMember(Value =  "tasks")]
        Task,

        [EnumMember(Value =  "telegrafs")]
        Telegraf,

        [EnumMember(Value =  "sources")]
        Source,

        [EnumMember(Value = "users")]
        User
    }
}