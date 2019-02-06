using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Flux.Domain;
using NodaTime;

[assembly: InternalsVisibleTo("Client.Legacy.Test")]
namespace InfluxDB.Client.Core.Flux.Internal
{
    internal class FluxResultMapper
    {
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
                var poco = (T) Activator.CreateInstance(type);

                var properties = type.GetProperties();

                foreach (var property in properties)
                {
                    var attributes = property.GetCustomAttributes(typeof(Column), false);

                    Column attribute = null;

                    if (attributes.Length > 0)
                    {
                        attribute = (Column) attributes.First();
                    }

                    var columnName = property.Name;

                    if (attribute != null && !string.IsNullOrEmpty(attribute.Name))
                    {
                        columnName = attribute.Name;
                    }

                    // copy record to case insensitive dictionary
                    var recordValues =
                        new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);

                    foreach (var entry in record.Values)
                    {
                        recordValues.Add(entry.Key, entry.Value);
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
                        recordValues.TryGetValue(col, out var value);

                        SetFieldValue(poco, property, value);
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

                //convert primitives
                if (typeof(double).IsAssignableFrom(propertyType) || typeof(Double).IsAssignableFrom(propertyType))
                {
                    property.SetValue(poco, ToDoubleValue(value));
                    return;
                }

                if (typeof(long).IsAssignableFrom(propertyType))
                {
                    property.SetValue(poco, ToLongValue(value));
                    return;
                }

                if (typeof(int).IsAssignableFrom(propertyType))
                {
                    property.SetValue(poco, ToIntValue(value));
                    return;
                }

                if (typeof(bool).IsAssignableFrom(propertyType))
                {
                    property.SetValue(poco, bool.TryParse(value.ToString(), out var v) && v);
                    return;
                }

                if (typeof(DateTime).IsAssignableFrom(propertyType))
                {
                    property.SetValue(poco, ToDateTimeValue(value));
                    return;
                }

                property.SetValue(poco, value);
            }
            catch (InvalidCastException)
            {
                throw new InfluxException(
                    $"Class '{poco.GetType().Name}' field '{property.Name}' was defined with a different field type and caused a InvalidCastException. " +
                    $"The correct type is '{value.GetType().Name}' (current field value: '{value}').");
            }
        }

        private double ToDoubleValue(object value)
        {
            if (value.GetType().IsAssignableFrom(typeof(double)))
            {
                return (double) value;
            }

            return (double) value;
        }

        private long ToLongValue(object value)
        {
            if (value.GetType().IsAssignableFrom(typeof(long)))
            {
                return (long) value;
            }

            return (long) value;
        }

        private int ToIntValue(object value)
        {
            if (value.GetType().IsAssignableFrom(typeof(int)))
            {
                return (int) value;
            }

            return (int) value;
        }
        
        private DateTime ToDateTimeValue(object value)
        {
            if (value is Instant instant)
            {
                return instant.InUtc().ToDateTimeUtc();
            }
            
            return (DateTime) value;
        }
    }
}