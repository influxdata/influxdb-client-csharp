using System.Runtime.Serialization;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    ///     The type of resource.
    /// </summary>
    public enum ResourceType
    {
        [EnumMember(Value = "authorizations")] Authorizations,

        [EnumMember(Value = "buckets")] Buckets,

        [EnumMember(Value = "dashboards")] Dashboards,

        [EnumMember(Value = "orgs")] Orgs,

        [EnumMember(Value = "sources")] Sources,

        [EnumMember(Value = "tasks")] Tasks,

        [EnumMember(Value = "telegrafs")] Telegrafs,

        [EnumMember(Value = "users")] Users,

        [EnumMember(Value = "variables")] Variables,

        [EnumMember(Value = "scrapers")] Scrapers,

        [EnumMember(Value = "secrets")] Secrets,

        [EnumMember(Value = "labels")] Labels,

        [EnumMember(Value = "views")] Views
    }
}