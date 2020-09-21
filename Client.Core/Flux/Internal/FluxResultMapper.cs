using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Flux.Domain;
using NodaTime;

[assembly: InternalsVisibleTo("Client.Legacy.Test, PublicKey=00240000048000009400000006020000002400005" +
                              "25341310004000001000100efaac865f88dd35c90dc548945405aae34056eedbe42cad60971f89a861a78" +
                              "437e86d95804a1aeeb0de18ac3728782f9dc8dbae2e806167a8bb64c0402278edcefd78c13dbe7f8d13de" +
                              "36eb36221ec215c66ee2dfe7943de97b869c5eea4d92f92d345ced67de5ac8fc3cd2f8dd7e3c0c53bdb0c" +
                              "c433af859033d069cad397a7")]
namespace InfluxDB.Client.Core.Flux.Internal
{
    internal class FluxResultMapper
    {
        // Reflection results are cached for poco type property and attribute lookups as an optimization since
        // calls are invoked continuously for a given type and will not change over library lifetime
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropertyCache = new ConcurrentDictionary<Type, PropertyInfo[]>();
        private static readonly ConcurrentDictionary<PropertyInfo, Column> AttributeCache = new ConcurrentDictionary<PropertyInfo, Column>();

        /// <summary>
        /// Maps FluxRecord into custom POCO class.
        /// </summary>
        /// <param name="record">the Flux record</param>
        /// <typeparam name="T">the POCO type</typeparam>
        /// <returns></returns>
        /// <exception cref="InfluxException"></exception>
        internal T ToPoco<T>(FluxRecord record)
        {
            Arguments.CheckNotNull(record, "Record is required");

            try
            {
                var type = typeof(T);
                var poco = (T)Activator.CreateInstance(type);

                // copy record to case insensitive dictionary (do this once)
                var recordValues =
                    new Dictionary<string, object>(record.Values, StringComparer.InvariantCultureIgnoreCase);

                var properties = PropertyCache.GetOrAdd(type, _ => type.GetProperties());

                foreach (var property in properties)
                {
                    Column attribute = AttributeCache.GetOrAdd(property, _ =>
                    {
                        var attributes = property.GetCustomAttributes(typeof(Column), false);
                        return attributes.Length > 0 ? attributes[0] as Column : null;
                    });

                    if (attribute != null && attribute.IsTimestamp)
                    {
                        SetFieldValue(poco, property, record.GetTimeInDateTime());
                    }
                    else
                    {
                        var columnName = property.Name;

                        if (attribute != null && !string.IsNullOrEmpty(attribute.Name))
                        {
                            columnName = attribute.Name;
                        }

                        string col = null;

                        if (recordValues.ContainsKey(columnName))
                        {
                            col = columnName;
                        }
                        else if (recordValues.ContainsKey("_" + columnName))
                        {
                            col = "_" + columnName;
                        }

                        if (!string.IsNullOrEmpty(col))
                        {
                            // No need to set field value when column does not exist (default poco field value will be the same)
                            if (recordValues.TryGetValue(col, out var value))
                                SetFieldValue(poco, property, value);
                        }
                    }
                }

                return poco;
            }
            catch (Exception e)
            {
                throw new InfluxException(e);
            }
        }

        private void SetFieldValue<T>(T poco, PropertyInfo property, object value)
        {
            if (property == null || value == null || !property.CanWrite)
            {
                return;
            }

            try
            {
                var propertyType = property.PropertyType;

                //the same type
                if (propertyType == value.GetType())
                {
                    property.SetValue(poco, value);
                    return;
                }

                //handle time primitives
                if (propertyType == typeof(DateTime))
                {
                    property.SetValue(poco, ToDateTimeValue(value));
                    return;
                }

                if (propertyType == typeof(Instant))
                {
                    property.SetValue(poco, ToInstantValue(value));
                    return;
                }

                if (value is IConvertible)
                {
                    // Nullable types cannot be used in type conversion, but we can use Nullable.GetUnderlyingType()
                    // to determine whether the type is nullable and convert to the underlying type instead
                    Type targetType = Nullable.GetUnderlyingType(propertyType) ?? propertyType;
                    property.SetValue(poco, Convert.ChangeType(value, targetType));
                }
                else
                {
                    property.SetValue(poco, value);
                }
            }
            catch (InvalidCastException ex)
            {
                throw new InfluxException(
                    $"Class '{poco.GetType().Name}' field '{property.Name}' was defined with a different field type and caused an exception. " +
                    $"The correct type is '{value.GetType().Name}' (current field value: '{value}'). Exception: {ex.Message}", ex);
            }
        }

        private DateTime ToDateTimeValue(object value)
        {
            if (value is DateTime dateTime)
            {
                return dateTime;
            }

            if (value is Instant instant)
            {
                return instant.InUtc().ToDateTimeUtc();
            }

            return (DateTime) value;
        }

        private Instant ToInstantValue(object value)
        {
            if (value is Instant instant)
            {
                return instant;
            }

            if (value is DateTime dateTime)
            {
                return Instant.FromDateTimeUtc(dateTime);
            }

            return (Instant) value;
        }
    }
}