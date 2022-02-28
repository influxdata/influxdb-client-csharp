using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Numerics;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using NodaTime;

namespace InfluxDB.Client.Writes
{
    public partial class PointData
    {
        public sealed class Builder
        {
            private readonly string _measurementName;
            private readonly Dictionary<string, string> _tags = new Dictionary<string, string>();
            private readonly Dictionary<string, object> _fields = new Dictionary<string, object>();

            private WritePrecision _precision;
            private BigInteger? _time;

            private Builder(string measurementName)
            {
                Arguments.CheckNonEmptyString(measurementName, "Measurement name");

                _measurementName = measurementName;
                _precision = WritePrecision.Ns;
            }

            /// <summary>
            /// Create a new Point withe specified a measurement name.
            /// </summary>
            /// <param name="measurementName">the measurement name</param>
            /// <returns>the new Point</returns>
            public static Builder Measurement(string measurementName)
            {
                return new Builder(measurementName);
            }

            /// <summary>
            /// Adds or replaces a tag value for a point.
            /// </summary>
            /// <param name="name">the tag name</param>
            /// <param name="value">the tag value</param>
            /// <returns>this</returns>
            public Builder Tag(string name, string value)
            {
                var isEmptyValue = string.IsNullOrEmpty(value);
                if (isEmptyValue)
                {
                    if (_tags.ContainsKey(name))
                    {
                        Trace.TraceWarning(
                            $"Empty tags will cause deletion of, tag [{name}], measurement [{_measurementName}]");
                        _tags.Remove(name);
                    }
                    else
                    {
                        Trace.TraceWarning($"Empty tags has no effect, tag [{name}], measurement [{_measurementName}]");
                    }
                }
                else
                {
                    _tags[name] = value;
                }

                return this;
            }

            /// <summary>
            /// Add a field with a <see cref="byte"/> value.
            /// </summary>
            /// <param name="name">the field name</param>
            /// <param name="value">the field value</param>
            /// <returns>this</returns>
            public Builder Field(string name, byte value)
            {
                return PutField(name, value);
            }

            /// <summary>
            /// Add a field with a <see cref="float"/> value.
            /// </summary>
            /// <param name="name">the field name</param>
            /// <param name="value">the field value</param>
            /// <returns>this</returns>
            public Builder Field(string name, float value)
            {
                return PutField(name, value);
            }

            /// <summary>
            /// Add a field with a <see cref="double"/> value.
            /// </summary>
            /// <param name="name">the field name</param>
            /// <param name="value">the field value</param>
            /// <returns>this</returns>
            public Builder Field(string name, double value)
            {
                return PutField(name, value);
            }

            /// <summary>
            /// Add a field with a <see cref="decimal"/> value.
            /// </summary>
            /// <param name="name">the field name</param>
            /// <param name="value">the field value</param>
            /// <returns>this</returns>
            public Builder Field(string name, decimal value)
            {
                return PutField(name, value);
            }

            /// <summary>
            /// Add a field with a <see cref="long"/> value.
            /// </summary>
            /// <param name="name">the field name</param>
            /// <param name="value">the field value</param>
            /// <returns>this</returns>
            public Builder Field(string name, long value)
            {
                return PutField(name, value);
            }

            /// <summary>
            /// Add a field with a <see cref="ulong"/> value.
            /// </summary>
            /// <param name="name">the field name</param>
            /// <param name="value">the field value</param>
            /// <returns>this</returns>
            public Builder Field(string name, ulong value)
            {
                return PutField(name, value);
            }

            /// <summary>
            /// Add a field with a <see cref="uint"/> value.
            /// </summary>
            /// <param name="name">the field name</param>
            /// <param name="value">the field value</param>
            /// <returns>this</returns>
            public Builder Field(string name, uint value)
            {
                return PutField(name, value);
            }

            /// <summary>
            /// Add a field with a <see cref="string"/> value.
            /// </summary>
            /// <param name="name">the field name</param>
            /// <param name="value">the field value</param>
            /// <returns>this</returns>
            public Builder Field(string name, string value)
            {
                return PutField(name, value);
            }

            /// <summary>
            /// Add a field with a <see cref="bool"/> value.
            /// </summary>
            /// <param name="name">the field name</param>
            /// <param name="value">the field value</param>
            /// <returns>this</returns>
            public Builder Field(string name, bool value)
            {
                return PutField(name, value);
            }

            /// <summary>
            /// Add a field with an <see cref="object"/> value.
            /// </summary>
            /// <param name="name">the field name</param>
            /// <param name="value">the field value</param>
            /// <returns>this</returns>
            public Builder Field(string name, object value)
            {
                return PutField(name, value);
            }

            /// <summary>
            /// Updates the timestamp for the point.
            /// </summary>
            /// <param name="timestamp">the timestamp</param>
            /// <param name="timeUnit">the timestamp precision</param>
            /// <returns></returns>
            public Builder Timestamp(long timestamp, WritePrecision timeUnit)
            {
                _precision = timeUnit;
                _time = timestamp;
                return this;
            }

            /// <summary>
            /// Updates the timestamp for the point represented by <see cref="TimeSpan"/>.
            /// </summary>
            /// <param name="timestamp">the timestamp</param>
            /// <param name="timeUnit">the timestamp precision</param>
            /// <returns></returns>
            public Builder Timestamp(TimeSpan timestamp, WritePrecision timeUnit)
            {
                _time = TimeSpanToBigInteger(timestamp, timeUnit);
                _precision = timeUnit;
                return this;
            }

            /// <summary>
            /// Updates the timestamp for the point represented by <see cref="DateTime"/>.
            /// </summary>
            /// <param name="timestamp">the timestamp</param>
            /// <param name="timeUnit">the timestamp precision</param>
            /// <returns></returns>
            public Builder Timestamp(DateTime timestamp, WritePrecision timeUnit)
            {
                if (timestamp != null && timestamp.Kind != DateTimeKind.Utc)
                {
                    throw new ArgumentException("Timestamps must be specified as UTC", nameof(timestamp));
                }

                var timeSpan = timestamp.Subtract(EpochStart);

                return Timestamp(timeSpan, timeUnit);
            }

            /// <summary>
            /// Updates the timestamp for the point represented by <see cref="DateTime"/>.
            /// </summary>
            /// <param name="timestamp">the timestamp</param>
            /// <param name="timeUnit">the timestamp precision</param>
            /// <returns></returns>
            public Builder Timestamp(DateTimeOffset timestamp, WritePrecision timeUnit)
            {
                return Timestamp(timestamp.UtcDateTime, timeUnit);
            }

            /// <summary>
            /// Updates the timestamp for the point represented by <see cref="Instant"/>.
            /// </summary>
            /// <param name="timestamp">the timestamp</param>
            /// <param name="timeUnit">the timestamp precision</param>
            /// <returns></returns>
            public Builder Timestamp(Instant timestamp, WritePrecision timeUnit)
            {
                _time = InstantToBigInteger(timestamp, timeUnit);
                _precision = timeUnit;
                return this;
            }

            /// <summary>
            /// Has point any fields?
            /// </summary>
            /// <returns>true, if the point contains any fields, false otherwise.</returns>
            public bool HasFields()
            {
                return _fields.Count > 0;
            }

            /// <summary>
            /// The PointData
            /// </summary>
            /// <returns></returns>
            public PointData ToPointData()
            {
                return new PointData(_measurementName, _precision, _time,
                    ImmutableSortedDictionary.CreateRange(_tags),
                    ImmutableSortedDictionary.CreateRange(_fields));
            }

            private Builder PutField(string name, object value)
            {
                Arguments.CheckNonEmptyString(name, "Field name");

                _fields[name] = value;
                return this;
            }
        }
    }
}