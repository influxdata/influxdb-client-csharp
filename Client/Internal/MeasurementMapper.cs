using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Writes;
using NodaTime;

[assembly: InternalsVisibleTo("Client.Test, PublicKey=002400000480000094000000060200000024000052534131" +
                              "0004000001000100efaac865f88dd35c90dc548945405aae34056eedbe42cad60971f89a861a78437e86d" +
                              "95804a1aeeb0de18ac3728782f9dc8dbae2e806167a8bb64c0402278edcefd78c13dbe7f8d13de36eb362" +
                              "21ec215c66ee2dfe7943de97b869c5eea4d92f92d345ced67de5ac8fc3cd2f8dd7e3c0c53bdb0cc433af8" +
                              "59033d069cad397a7")]

namespace InfluxDB.Client.Internal
{
    internal class PropertyInfoColumn
    {
        internal PropertyInfo Property;
        internal Column Column;
    }

    internal class MeasurementMapper
    {
        private IDictionary<string, PropertyInfoColumn[]> CACHE =
            new ConcurrentDictionary<string, PropertyInfoColumn[]>();

        internal PointData ToPoint<TM>(TM measurement, WritePrecision precision)
        {
            Arguments.CheckNotNull(measurement, nameof(measurement));
            Arguments.CheckNotNull(precision, nameof(precision));

            var measurementType = measurement.GetType();
            CacheMeasurementClass(measurementType);

            var measurementAttribute = (Measurement)measurementType.GetCustomAttribute(typeof(Measurement));
            var measurementColumn = CACHE[measurementType.Name].SingleOrDefault(p => p.Column.IsMeasurement);

            if (((measurementAttribute == null) ^ (measurementColumn == null)) == false)
            {
                throw new InvalidOperationException(
                    $"Unable to determine Measurement for {measurement}. Does it have a {typeof(Measurement)} or IsMeasurement {typeof(Column)} attribute?");
            }

            var measurementName =
                measurementAttribute == null
                    ? (string)measurementColumn.Property.GetValue(measurement)
                    : measurementAttribute.Name;

            var point = PointData.Measurement(measurementName);

            foreach (var propertyInfo in CACHE[measurementType.Name])
            {
                if (propertyInfo.Column.IsMeasurement)
                {
                    continue;
                }

                var value = propertyInfo.Property.GetValue(measurement);
                if (value == null)
                {
                    continue;
                }

                var name = !string.IsNullOrEmpty(propertyInfo.Column.Name)
                    ? propertyInfo.Column.Name
                    : propertyInfo.Property.Name;
                if (propertyInfo.Column.IsTag)
                {
                    point = point.Tag(name, value.ToString());
                }
                else if (propertyInfo.Column.IsTimestamp)
                {
                    if (value is long l)
                    {
                        point = point.Timestamp(l, precision);
                    }
                    else if (value is TimeSpan span)
                    {
                        point = point.Timestamp(span, precision);
                    }
                    else if (value is DateTime date)
                    {
                        point = point.Timestamp(date, precision);
                    }
                    else if (value is DateTimeOffset offset)
                    {
                        point = point.Timestamp(offset, precision);
                    }
                    else if (value is Instant instant)
                    {
                        point = point.Timestamp(instant, precision);
                    }
                    else
                    {
                        Trace.WriteLine($"{value} is not supported as Timestamp");
                    }
                }
                else
                {
                    if (value is bool b)
                    {
                        point = point.Field(name, b);
                    }
                    else if (value is double d)
                    {
                        point = point.Field(name, d);
                    }
                    else if (value is float f)
                    {
                        point = point.Field(name, f);
                    }
                    else if (value is decimal dec)
                    {
                        point = point.Field(name, dec);
                    }
                    else if (value is long lng)
                    {
                        point = point.Field(name, lng);
                    }
                    else if (value is ulong ulng)
                    {
                        point = point.Field(name, ulng);
                    }
                    else if (value is int i)
                    {
                        point = point.Field(name, i);
                    }
                    else if (value is byte bt)
                    {
                        point = point.Field(name, bt);
                    }
                    else if (value is sbyte sb)
                    {
                        point = point.Field(name, sb);
                    }
                    else if (value is short sh)
                    {
                        point = point.Field(name, sh);
                    }
                    else if (value is uint ui)
                    {
                        point = point.Field(name, ui);
                    }
                    else if (value is ushort us)
                    {
                        point = point.Field(name, us);
                    }
                    else
                    {
                        point = point.Field(name, value.ToString());
                    }
                }
            }

            return point;
        }

        private void CacheMeasurementClass(Type measurementType)
        {
            if (CACHE.ContainsKey(measurementType.Name))
            {
                return;
            }

            CACHE[measurementType.Name] = measurementType.GetProperties()
                .Select(property => new PropertyInfoColumn
                    { Column = (Column)property.GetCustomAttribute(typeof(Column)), Property = property })
                .Where(propertyInfo => propertyInfo.Column != null)
                .ToArray();
        }
    }
}