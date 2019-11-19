using System;
using System.Diagnostics;
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
    internal class MeasurementMapper
    {
        internal PointData ToPoint<TM>(TM measurement, WritePrecision precision)
        {
            Arguments.CheckNotNull(measurement, nameof(measurement));
            Arguments.CheckNotNull(precision, nameof(precision));

            var measurementAttribute = (Measurement) measurement.GetType()
                .GetCustomAttribute(typeof(Measurement));

            if (measurementAttribute == null)
            {
                throw new InvalidOperationException(
                    $"Measurement {measurement} does not have a {typeof(Measurement)} attribute.");
            }

            var point = PointData.Measurement(measurementAttribute.Name);

            foreach (var property in measurement.GetType().GetProperties())
            {
                var column = (Column) property.GetCustomAttribute(typeof(Column));
                if (column == null)
                {
                    continue;
                }

                var value = property.GetValue(measurement);
                if (value == null)
                {
                    continue;
                }

                var name = !string.IsNullOrEmpty(column.Name) ? column.Name : property.Name;
                if (column.IsTag)
                {
                    point.Tag(name, value.ToString());
                }
                else if (column.IsTimestamp)
                {
                    if (value is long l)
                    {
                        point.Timestamp(l, precision);
                    }
                    else if (value is TimeSpan span)
                    {
                        point.Timestamp(span, precision);
                    } 
                    else if (value is DateTime date)
                    {
                        point.Timestamp(date, precision);
                    } 
                    else if (value is DateTimeOffset offset)
                    {
                        point.Timestamp(offset, precision);
                    }
                    else if (value is Instant instant)
                    {
                        point.Timestamp(instant, precision);
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
                        point.Field(name, b);
                    }
                    else if (value is double d)
                    {
                        point.Field(name, d);
                    }
                    else if (value is float f)
                    {
                        point.Field(name, f);
                    }
                    else if (value is decimal dec)
                    {
                        point.Field(name, dec);
                    }
                    else if (value is long lng)
                    {
                        point.Field(name, lng);
                    }
                    else if (value is ulong ulng)
                    {
                        point.Field(name, ulng);
                    }
                    else if (value is int i)
                    {
                        point.Field(name, i);
                    }
                    else if (value is byte bt)
                    {
                        point.Field(name, bt);
                    }
                    else if (value is sbyte sb)
                    {
                        point.Field(name, sb);
                    }
                    else if (value is short sh)
                    {
                        point.Field(name, sh);
                    }
                    else if (value is uint ui)
                    {
                        point.Field(name, ui);
                    }
                    else if (value is ushort us)
                    {
                        point.Field(name, us);
                    }
                    else
                    {
                        point.Field(name, value.ToString());
                    }
                }
            }

            return point;
        }
    }
}