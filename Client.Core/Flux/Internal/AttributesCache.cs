using System;
using System.Collections.Concurrent;
using System.Reflection;

namespace InfluxDB.Client.Core.Flux.Internal
{
    /// <summary>
    /// The cache for DomainObject attributes. The attributes are used for mapping from/to DomainObject.
    /// </summary>
    public class AttributesCache
    {
        // Reflection results are cached for poco type property and attribute lookups as an optimization since
        // calls are invoked continuously for a given type and will not change over library lifetime
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache =
            new ConcurrentDictionary<Type, PropertyInfo[]>();

        private static readonly ConcurrentDictionary<PropertyInfo, Column> AttributeCache =
            new ConcurrentDictionary<PropertyInfo, Column>();

        /// <summary>
        /// Get properties for specified Type.
        /// </summary>
        /// <param name="type">type of DomainObject</param>
        /// <returns>properties for DomainObject</returns>
        public PropertyInfo[] GetProperties(Type type)
        {
            Arguments.CheckNotNull(type, nameof(type));

            return PropertyCache.GetOrAdd(type, _ => type.GetProperties());
        }

        /// <summary>
        /// Get Mapping attribute for specified property.
        /// </summary>
        /// <param name="property">property of DomainObject</param>
        /// <returns>Property Attribute</returns>
        public Column GetAttribute(PropertyInfo property)
        {
            Arguments.CheckNotNull(property, nameof(property));

            return AttributeCache.GetOrAdd(property, _ =>
            {
                var attributes = property.GetCustomAttributes(typeof(Column), false);
                return attributes.Length > 0 ? attributes[0] as Column : null;
            });
        }

        /// <summary>
        /// Get name of field or tag for specified attribute and property
        /// </summary>
        /// <param name="attribute">attribute of DomainObject</param>
        /// <param name="property">property of DomainObject</param>
        /// <returns>name used for mapping</returns>
        public string GetColumnName(Column attribute, PropertyInfo property)
        {
            Arguments.CheckNotNull(property, nameof(property));

            if (attribute != null && !string.IsNullOrEmpty(attribute.Name))
            {
                return attribute.Name;
            }

            return property.Name;
        }
    }
}