using System.Runtime.Serialization;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// Status defines if a resource is active or inactive.
    /// </summary>
    public enum Status
    {
       /// <summary>
       /// Active status means that the resource can be used.
       /// </summary>
       [EnumMember(Value = "active")]
        Active,

        /// <summary>
        /// Inactive status means that the resource cannot be used.
        /// </summary>
        [EnumMember(Value = "inactive")]
        Inactive
    }
}