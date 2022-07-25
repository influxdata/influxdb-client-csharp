using System;
using System.Collections.Generic;
using System.Linq;
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
[assembly: InternalsVisibleTo("Client.Linq.Test, PublicKey=00240000048000009400000006020000002400005" +
                              "25341310004000001000100efaac865f88dd35c90dc548945405aae34056eedbe42cad60971f89a861a78" +
                              "437e86d95804a1aeeb0de18ac3728782f9dc8dbae2e806167a8bb64c0402278edcefd78c13dbe7f8d13de" +
                              "36eb36221ec215c66ee2dfe7943de97b869c5eea4d92f92d345ced67de5ac8fc3cd2f8dd7e3c0c53bdb0c" +
                              "c433af859033d069cad397a7")]
[assembly: InternalsVisibleTo("InfluxDB.Client, PublicKey=00240000048000009400000006020000002400005" +
                              "25341310004000001000100efaac865f88dd35c90dc548945405aae34056eedbe42cad60971f89a861a78" +
                              "437e86d95804a1aeeb0de18ac3728782f9dc8dbae2e806167a8bb64c0402278edcefd78c13dbe7f8d13de" +
                              "36eb36221ec215c66ee2dfe7943de97b869c5eea4d92f92d345ced67de5ac8fc3cd2f8dd7e3c0c53bdb0c" +
                              "c433af859033d069cad397a7")]
[assembly: InternalsVisibleTo("InfluxDB.Client.Flux, PublicKey=00240000048000009400000006020000002400005" +
                              "25341310004000001000100efaac865f88dd35c90dc548945405aae34056eedbe42cad60971f89a861a78" +
                              "437e86d95804a1aeeb0de18ac3728782f9dc8dbae2e806167a8bb64c0402278edcefd78c13dbe7f8d13de" +
                              "36eb36221ec215c66ee2dfe7943de97b869c5eea4d92f92d345ced67de5ac8fc3cd2f8dd7e3c0c53bdb0c" +
                              "c433af859033d069cad397a7")]

namespace InfluxDB.Client.Core.Flux.Internal
{
    internal class FluxResultMapper : IFluxResultMapper
    {
        private readonly AttributesCache _attributesCache = new AttributesCache();

        private readonly Dictionary<Tuple<Type, Type>, MethodInfo> _parseMethodCache =
            new Dictionary<Tuple<Type, Type>, MethodInfo>();

        public T ConvertToEntity<T>(FluxRecord fluxRecord)
        {
            return ToPoco<T>(fluxRecord);
        }

        public object ConvertToEntity(FluxRecord fluxRecord, Type type)
        {
            return ToPoco(fluxRecord, type);
        }

        /// <summary>
        /// Maps FluxRecord into custom POCO class.
        /// </summary>
        /// <param name="record">the Flux record</param>
        /// <param name="type">the POCO type</param>
        /// <returns>An POCO object</returns>
        /// <exception cref="InfluxException"></exception>
        internal object ToPoco(FluxRecord record, Type type)
        {
            Arguments.CheckNotNull(record, "Record is required");

            try
            {
                var poco = Activator.CreateInstance(type);

                // copy record to case insensitive dictionary (do this once)
                var recordValues =
                    new Dictionary<string, object>(record.Values, StringComparer.InvariantCultureIgnoreCase);

                var properties = _attributesCache.GetProperties(type);

                foreach (var property in properties)
                {
                    var attribute = _attributesCache.GetAttribute(property);

                    if (attribute != null && attribute.IsMeasurement)
                    {
                        SetFieldValue(poco, property, record.GetMeasurement());
                    }

                    if (attribute != null && attribute.IsTimestamp)
                    {
                        SetFieldValue(poco, property, record.GetTime());
                    }
                    else
                    {
                        var columnName = _attributesCache.GetColumnName(attribute, property);

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
                            {
                                SetFieldValue(poco, property, value);
                            }
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

        /// <summary>
        /// Maps FluxRecord into custom POCO class.
        /// </summary>
        /// <param name="record">the Flux record</param>
        /// <typeparam name="T">the POCO type</typeparam>
        /// <returns></returns>
        /// <exception cref="InfluxException"></exception>
        internal T ToPoco<T>(FluxRecord record)
        {
            return (T)ToPoco(record, typeof(T));
        }

        private void SetFieldValue<T>(T poco, PropertyInfo property, object value)
        {
            if (property == null || value == null || !property.CanWrite)
            {
                return;
            }

            try
            {
                // Nullable types cannot be used in type conversion, but we can use Nullable.GetUnderlyingType()
                // to determine whether the type is nullable and convert to the underlying type instead
                var propertyType = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;
                var valueType = value.GetType();

                // The same type
                if (propertyType == valueType)
                {
                    property.SetValue(poco, value);
                    return;
                }

                // Handle time primitives
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

                // Handle parseables
                var parseMethod = GetParseMethod(propertyType, valueType);
                if (parseMethod != null)
                {
                    var parsed = parseMethod.Invoke(null, new[] { value });
                    property.SetValue(poco, parsed);
                    return;
                }

                // Handle convertibles 
                if (value is IConvertible)
                {
                    property.SetValue(poco, Convert.ChangeType(value, propertyType));
                    return;
                }

                // Give up and try anyway
                property.SetValue(poco, value);
            }
            catch (InvalidCastException ex)
            {
                throw new InfluxException(
                    $"Class '{poco.GetType().Name}' field '{property.Name}' was defined with a different field type and caused an exception. " +
                    $"The correct type is '{value.GetType().Name}' (current field value: '{value}'). Exception: {ex.Message}",
                    ex);
            }
        }

        /// <summary>
        /// Gets the static Parse method on the target type taking the value type as a single parameter.
        /// </summary>
        /// <param name="parserType">The type declaring the Parse method.</param>
        /// <param name="valueType">The type that will be passed to the Parse method.</param>
        /// <returns>The matching Parse method.</returns>
        private MethodInfo GetParseMethod(Type parserType, Type valueType)
        {
            var key = new Tuple<Type, Type>(parserType, valueType);

            MethodInfo method;
            if (!_parseMethodCache.TryGetValue(key, out method))
            {
                method = parserType.GetMethods(BindingFlags.Public | BindingFlags.Static)
                    .Where(m => m.Name == "Parse")
                    .Where(m => m.ReturnType == parserType)
                    .Where(m =>
                    {
                        var parameters = m.GetParameters();
                        if (parameters.Length != 1)
                        {
                            return false;
                        }

                        var paramType = parameters[0].ParameterType;
                        if (valueType == paramType)
                        {
                            return true;
                        }

                        paramType = Nullable.GetUnderlyingType(paramType) ?? paramType;
                        return valueType == paramType;
                    })
                    .FirstOrDefault();
                _parseMethodCache[key] = method;
            }

            return method;
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

            if (value is IConvertible)
            {
                return (DateTime)Convert.ChangeType(value, typeof(DateTime));
            }

            throw new InvalidCastException(
                $"Object value of type {value.GetType().Name} cannot be converted to {nameof(DateTime)}");
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

            throw new InvalidCastException(
                $"Object value of type {value.GetType().Name} cannot be converted to {nameof(Instant)}");
        }
    }
}