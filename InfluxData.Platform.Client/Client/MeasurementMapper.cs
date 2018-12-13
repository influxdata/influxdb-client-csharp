using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.CompilerServices;
using InfluxData.Platform.Client.Write;
using NodaTime;
using Platform.Common.Platform;

[assembly: InternalsVisibleTo("Platform.Client.Tests")]
namespace InfluxData.Platform.Client.Client
{
    internal class MeasurementMapper
    {
        internal Point ToPoint<TM>(TM measurement, TimeUnit precision)
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

            var point = Point.Measurement(measurementAttribute.Name);

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