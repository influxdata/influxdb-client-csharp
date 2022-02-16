using System.Reflection;
using InfluxDB.Client.Core.Flux.Internal;

namespace InfluxDB.Client.Linq
{
    /// <summary>
    /// Resolver to use customized tag and field names in LINQ queries.
    /// </summary>
    public interface IMemberNameResolver
    {
        /// <summary>
        /// Get Type Of Member
        /// </summary>
        /// <param name="memberInfo">for actual member</param>
        /// <returns>Type of member for actual member</returns>
        MemberType ResolveMemberType(MemberInfo memberInfo);

        /// <summary>
        /// Get name of property that will be use as a tag name or field name in InfluxDB.
        /// </summary>
        /// <param name="memberInfo">property</param>
        /// <returns>Returns name of field or tag in InfluxDB</returns>
        string GetColumnName(MemberInfo memberInfo);

        /// <summary>
        /// Get name of property for named field.
        /// </summary>
        /// <param name="memberInfo">property</param>
        /// <param name="value">value of expression</param>
        /// <returns>Return name of property.</returns>
        string GetNamedFieldName(MemberInfo memberInfo, object value);
    }

    public enum MemberType
    {
        Measurement,
        Tag,
        Field,
        Timestamp,

        // Member Value is used as a name of field
        NamedField,

        // Member Value is used as a value of NamedField
        NamedFieldValue
    }

    public class DefaultMemberNameResolver : IMemberNameResolver
    {
        private readonly AttributesCache _attributesCache = new AttributesCache();

        public MemberType ResolveMemberType(MemberInfo memberInfo)
        {
            var attribute = _attributesCache.GetAttribute(memberInfo as PropertyInfo);

            if (attribute != null)
            {
                if (attribute.IsMeasurement)
                {
                    return MemberType.Measurement;
                }

                if (attribute.IsTag)
                {
                    return MemberType.Tag;
                }

                if (attribute.IsTimestamp)
                {
                    return MemberType.Timestamp;
                }
            }

            return MemberType.Field;
        }

        public string GetColumnName(MemberInfo memberInfo)
        {
            var propertyInfo = memberInfo as PropertyInfo;
            var attribute = _attributesCache.GetAttribute(propertyInfo);

            return _attributesCache.GetColumnName(attribute, propertyInfo);
        }

        public string GetNamedFieldName(MemberInfo memberInfo, object value)
        {
            return GetColumnName(memberInfo);
        }
    }
}