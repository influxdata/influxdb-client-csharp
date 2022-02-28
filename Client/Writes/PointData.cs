using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Numerics;
using System.Text;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using NodaTime;

namespace InfluxDB.Client.Writes
{
    /// <summary>
    /// Point defines the values that will be written to the database.
    /// <a href="http://bit.ly/influxdata-point">See Go Implementation</a>.
    /// </summary>
    public partial class PointData : IEquatable<PointData>
    {
        private static readonly DateTime EpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly string _measurementName;

        private readonly ImmutableSortedDictionary<string, string> _tags = ImmutableSortedDictionary<string, string>
            .Empty;

        private readonly ImmutableSortedDictionary<string, object> _fields =
            ImmutableSortedDictionary<string, object>.Empty;

        public readonly WritePrecision Precision;
        private readonly BigInteger? _time;

        private PointData(string measurementName)
        {
            Arguments.CheckNonEmptyString(measurementName, "Measurement name");

            _measurementName = measurementName;
            Precision = WritePrecision.Ns;
        }

        /// <summary>
        /// Create a new Point withe specified a measurement name.
        /// </summary>
        /// <param name="measurementName">the measurement name</param>
        /// <returns>the new Point</returns>
        public static PointData Measurement(string measurementName)
        {
            return new PointData(measurementName);
        }

        private PointData(string measurementName,
            WritePrecision precision,
            BigInteger? time,
            ImmutableSortedDictionary<string, string> tags,
            ImmutableSortedDictionary<string, object> fields)
        {
            _measurementName = measurementName;
            Precision = precision;
            _time = time;
            _tags = tags;
            _fields = fields;
        }

        /// <summary>
        /// Adds or replaces a tag value for a point.
        /// </summary>
        /// <param name="name">the tag name</param>
        /// <param name="value">the tag value</param>
        /// <returns>this</returns>
        public PointData Tag(string name, string value)
        {
            var isEmptyValue = string.IsNullOrEmpty(value);
            var tags = _tags;
            if (isEmptyValue)
            {
                if (tags.ContainsKey(name))
                {
                    Trace.TraceWarning(
                        $"Empty tags will cause deletion of, tag [{name}], measurement [{_measurementName}]");
                }
                else
                {
                    Trace.TraceWarning($"Empty tags has no effect, tag [{name}], measurement [{_measurementName}]");
                    return this;
                }
            }

            if (tags.ContainsKey(name))
            {
                tags = tags.Remove(name);
            }

            if (!isEmptyValue)
            {
                tags = tags.Add(name, value);
            }

            return new PointData(_measurementName,
                Precision,
                _time,
                tags,
                _fields);
        }

        /// <summary>
        /// Add a field with a <see cref="byte"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public PointData Field(string name, byte value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Add a field with a <see cref="float"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public PointData Field(string name, float value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Add a field with a <see cref="double"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public PointData Field(string name, double value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Add a field with a <see cref="decimal"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public PointData Field(string name, decimal value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Add a field with a <see cref="long"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public PointData Field(string name, long value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Add a field with a <see cref="ulong"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public PointData Field(string name, ulong value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Add a field with a <see cref="uint"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public PointData Field(string name, uint value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Add a field with a <see cref="string"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public PointData Field(string name, string value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Add a field with a <see cref="bool"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public PointData Field(string name, bool value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Add a field with an <see cref="object"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public PointData Field(string name, object value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Updates the timestamp for the point.
        /// </summary>
        /// <param name="timestamp">the timestamp</param>
        /// <param name="timeUnit">the timestamp precision</param>
        /// <returns></returns>
        public PointData Timestamp(long timestamp, WritePrecision timeUnit)
        {
            return new PointData(_measurementName,
                timeUnit,
                timestamp,
                _tags,
                _fields);
        }

        /// <summary>
        /// Updates the timestamp for the point represented by <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="timestamp">the timestamp</param>
        /// <param name="timeUnit">the timestamp precision</param>
        /// <returns></returns>
        public PointData Timestamp(TimeSpan timestamp, WritePrecision timeUnit)
        {
            var time = TimeSpanToBigInteger(timestamp, timeUnit);
            return new PointData(_measurementName,
                timeUnit,
                time,
                _tags,
                _fields);
        }

        /// <summary>
        /// Updates the timestamp for the point represented by <see cref="DateTime"/>.
        /// </summary>
        /// <param name="timestamp">the timestamp</param>
        /// <param name="timeUnit">the timestamp precision</param>
        /// <returns></returns>
        public PointData Timestamp(DateTime timestamp, WritePrecision timeUnit)
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
        public PointData Timestamp(DateTimeOffset timestamp, WritePrecision timeUnit)
        {
            return Timestamp(timestamp.UtcDateTime, timeUnit);
        }

        /// <summary>
        /// Updates the timestamp for the point represented by <see cref="Instant"/>.
        /// </summary>
        /// <param name="timestamp">the timestamp</param>
        /// <param name="timeUnit">the timestamp precision</param>
        /// <returns></returns>
        public PointData Timestamp(Instant timestamp, WritePrecision timeUnit)
        {
            var time = InstantToBigInteger(timestamp, timeUnit);
            return new PointData(_measurementName,
                timeUnit,
                time,
                _tags,
                _fields);
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
        /// The Line Protocol
        /// </summary>
        /// <param name="pointSettings">with the default values</param>
        /// <returns></returns>
        public string ToLineProtocol(PointSettings pointSettings = null)
        {
            var sb = new StringBuilder();

            EscapeKey(sb, _measurementName, false);
            AppendTags(sb, pointSettings);
            var appendedFields = AppendFields(sb);
            if (!appendedFields)
            {
                return "";
            }

            AppendTime(sb);

            return sb.ToString();
        }

        private PointData PutField(string name, object value)
        {
            Arguments.CheckNonEmptyString(name, "Field name");

            var fields = _fields;
            if (fields.ContainsKey(name))
            {
                fields = fields.Remove(name);
            }

            fields = fields.Add(name, value);

            return new PointData(_measurementName,
                Precision,
                _time,
                _tags,
                fields);
        }

        private static BigInteger TimeSpanToBigInteger(TimeSpan timestamp, WritePrecision timeUnit)
        {
            BigInteger time;
            switch (timeUnit)
            {
                case WritePrecision.Ns:
                    time = timestamp.Ticks * 100;
                    break;
                case WritePrecision.Us:
                    time = (BigInteger)(timestamp.Ticks * 0.1);
                    break;
                case WritePrecision.Ms:
                    time = (BigInteger)timestamp.TotalMilliseconds;
                    break;
                case WritePrecision.S:
                    time = (BigInteger)timestamp.TotalSeconds;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeUnit), timeUnit,
                        "WritePrecision value is not supported");
            }

            return time;
        }

        private static BigInteger InstantToBigInteger(Instant timestamp, WritePrecision timeUnit)
        {
            BigInteger time;
            switch (timeUnit)
            {
                case WritePrecision.S:
                    time = timestamp.ToUnixTimeSeconds();
                    break;
                case WritePrecision.Ms:
                    time = timestamp.ToUnixTimeMilliseconds();
                    break;
                case WritePrecision.Us:
                    time = (long)(timestamp.ToUnixTimeTicks() * 0.1);
                    break;
                case WritePrecision.Ns:
                    time = (timestamp - NodaConstants.UnixEpoch).ToBigIntegerNanoseconds();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(timeUnit), timeUnit,
                        "WritePrecision value is not supported");
            }

            return time;
        }

        /// <summary>
        /// Appends the tags.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="pointSettings">The point settings.</param>
        private void AppendTags(StringBuilder writer, PointSettings pointSettings)
        {
            IReadOnlyDictionary<string, string> entries;

            if (pointSettings == null)
            {
                entries = _tags;
            }
            else
            {
                var defaultTags = pointSettings.GetDefaultTags();
                try
                {
                    entries = _tags.AddRange(defaultTags);
                }
                catch (ArgumentException)
                {
                    // Most cases don't expect to override existing content
                    // override don't consider as best practice
                    // therefore it a trade-off between being less efficient 
                    // on the default behavior or on the override scenario
                    var builder = _tags.ToBuilder();
                    foreach (var item in defaultTags)
                    {
                        var name = item.Key;
                        if (!builder.ContainsKey(name)) // existing tags overrides
                        {
                            builder.Add(name, item.Value);
                        }
                    }

                    entries = builder;
                }
            }

            foreach (var keyValue in entries)
            {
                var key = keyValue.Key;
                var value = keyValue.Value;

                if (string.IsNullOrEmpty(key) || string.IsNullOrEmpty(value))
                {
                    continue;
                }

                writer.Append(',');
                EscapeKey(writer, key);
                writer.Append('=');
                EscapeKey(writer, value);
            }

            writer.Append(' ');
        }

        /// <summary>
        /// Appends the fields.
        /// </summary>
        /// <param name="sb">The sb.</param>
        /// <returns></returns>
        private bool AppendFields(StringBuilder sb)
        {
            var appended = false;

            foreach (var keyValue in _fields)
            {
                var key = keyValue.Key;
                var value = keyValue.Value;

                if (IsNotDefined(value))
                {
                    continue;
                }

                EscapeKey(sb, key);
                sb.Append('=');

                if (value is double || value is float)
                {
                    sb.Append(((IConvertible)value).ToString(CultureInfo.InvariantCulture));
                }
                else if (value is uint || value is ulong || value is ushort)
                {
                    sb.Append(((IConvertible)value).ToString(CultureInfo.InvariantCulture));
                    sb.Append('u');
                }
                else if (value is byte || value is int || value is long || value is sbyte || value is short)
                {
                    sb.Append(((IConvertible)value).ToString(CultureInfo.InvariantCulture));
                    sb.Append('i');
                }
                else if (value is bool b)
                {
                    sb.Append(b ? "true" : "false");
                }
                else if (value is string s)
                {
                    sb.Append('"');
                    EscapeValue(sb, s);
                    sb.Append('"');
                }
                else if (value is IConvertible c)
                {
                    sb.Append(c.ToString(CultureInfo.InvariantCulture));
                }
                else
                {
                    sb.Append('"');
                    EscapeValue(sb, value.ToString());
                    sb.Append('"');
                }

                sb.Append(',');
                appended = true;
            }

            if (appended)
            {
                sb.Remove(sb.Length - 1, 1);
            }

            return appended;
        }

        /// <summary>
        /// Appends the time.
        /// </summary>
        /// <param name="sb">The sb.</param>
        private void AppendTime(StringBuilder sb)
        {
            if (_time == null)
            {
                return;
            }

            sb.Append(' ');
            sb.Append(((BigInteger)_time).ToString(CultureInfo.InvariantCulture));
        }

        /// <summary>
        /// Escapes the key.
        /// </summary>
        /// <param name="sb">The sb.</param>
        /// <param name="key">The key.</param>
        /// <param name="escapeEqual">Configure to escaping equal.</param>
        private void EscapeKey(StringBuilder sb, string key, bool escapeEqual = true)
        {
            foreach (var c in key)
            {
                switch (c)
                {
                    case '\n':
                        sb.Append("\\n");
                        continue;
                    case '\r':
                        sb.Append("\\r");
                        continue;
                    case '\t':
                        sb.Append("\\t");
                        continue;
                    case ' ':
                    case ',':
                        sb.Append("\\");
                        break;
                    case '=':
                        if (escapeEqual)
                        {
                            sb.Append("\\");
                        }

                        break;
                }

                sb.Append(c);
            }
        }

        /// <summary>
        /// Escapes the value.
        /// </summary>
        /// <param name="sb">The sb.</param>
        /// <param name="value">The value.</param>
        private void EscapeValue(StringBuilder sb, string value)
        {
            foreach (var c in value)
            {
                switch (c)
                {
                    case '\\':
                    case '\"':
                        sb.Append("\\");
                        break;
                }

                sb.Append(c);
            }
        }

        /// <summary>
        /// Determines whether [is not defined] [the specified value].
        /// </summary>
        /// <param name="value">The value.</param>
        /// <returns>
        ///   <c>true</c> if [is not defined] [the specified value]; otherwise, <c>false</c>.
        /// </returns>
        private bool IsNotDefined(object value)
        {
            return value == null
                   || value is double d && (double.IsInfinity(d) || double.IsNaN(d))
                   || value is float f && (float.IsInfinity(f) || float.IsNaN(f));
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" />, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="System.Object" /> to compare with this instance.</param>
        /// <returns>
        ///   <c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj)
        {
            return Equals(obj as PointData);
        }

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// true if the current object is equal to the <paramref name="other">other</paramref> parameter; otherwise, false.
        /// </returns>
        public bool Equals(PointData other)
        {
            if (other == null)
            {
                return false;
            }

            var otherTags = other._tags;

            var result = _tags.Count == otherTags.Count &&
                         _tags.All(pair =>
                         {
                             var key = pair.Key;
                             var value = pair.Value;
                             return otherTags.ContainsKey(key) &&
                                    otherTags[key] == value;
                         });
            var otherFields = other._fields;
            result = result && _fields.Count == otherFields.Count &&
                     _fields.All(pair =>
                     {
                         var key = pair.Key;
                         var value = pair.Value;
                         return otherFields.ContainsKey(key) &&
                                Equals(otherFields[key], value);
                     });

            result = result &&
                     _measurementName == other._measurementName &&
                     Precision == other.Precision &&
                     EqualityComparer<BigInteger?>.Default.Equals(_time, other._time);

            return result;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
        /// </returns>
        public override int GetHashCode()
        {
            var hashCode = 318335609;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(_measurementName);
            hashCode = hashCode * -1521134295 + Precision.GetHashCode();
            hashCode = hashCode * -1521134295 + _time.GetHashCode();

            foreach (var pair in _tags)
            {
                hashCode = hashCode * -1521134295 + pair.Key?.GetHashCode() ?? 0;
                hashCode = hashCode * -1521134295 + pair.Value?.GetHashCode() ?? 0;
            }

            foreach (var pair in _fields)
            {
                hashCode = hashCode * -1521134295 + pair.Key?.GetHashCode() ?? 0;
                hashCode = hashCode * -1521134295 + pair.Value?.GetHashCode() ?? 0;
            }

            return hashCode;
        }

        /// <summary>
        /// Implements the operator ==.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator ==(PointData left, PointData right)
        {
            return EqualityComparer<PointData>.Default.Equals(left, right);
        }

        /// <summary>
        /// Implements the operator !=.
        /// </summary>
        /// <param name="left">The left.</param>
        /// <param name="right">The right.</param>
        /// <returns>
        /// The result of the operator.
        /// </returns>
        public static bool operator !=(PointData left, PointData right)
        {
            return !(left == right);
        }
    }
}