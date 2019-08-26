using System;
using System.Collections.Generic;
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
    public class Point
    {
        private static readonly DateTime EpochStart = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        private readonly string _measurementName;
        private readonly SortedDictionary<string, string> _tags;
        private readonly SortedDictionary<string, object> _fields;

        public WritePrecision Precision { get; private set; }
        private BigInteger? _time;

        private Point(string measurementName)
        {
            Arguments.CheckNonEmptyString(measurementName, "Measurement name");

            _measurementName = measurementName;
            _fields = new SortedDictionary<string, object>(StringComparer.Ordinal);
            _tags = new SortedDictionary<string, string>(StringComparer.Ordinal);
            Precision = WritePrecision.Ns;
        }

        /// <summary>
        /// Create a new Point withe specified a measurement name.
        /// </summary>
        /// <param name="measurementName">the measurement name</param>
        /// <returns>the new Point</returns>
        public static Point Measurement(string measurementName)
        {
            return new Point(measurementName);
        }

        /// <summary>
        /// Adds or replaces a tag value for a point.
        /// </summary>
        /// <param name="name">the tag name</param>
        /// <param name="value">the tag value</param>
        /// <returns>this</returns>
        public Point Tag(string name, string value)
        {
            _tags[name] = value;

            return this;
        }

        /// <summary>
        /// Add a field with a <see cref="float"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public Point Field(string name, float value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Add a field with a <see cref="double"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public Point Field(string name, double value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Add a field with a <see cref="decimal"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public Point Field(string name, decimal value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Add a field with a <see cref="long"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public Point Field(string name, long value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Add a field with a <see cref="ulong"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public Point Field(string name, ulong value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Add a field with a <see cref="string"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public Point Field(string name, string value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Add a field with a <see cref="bool"/> value.
        /// </summary>
        /// <param name="name">the field name</param>
        /// <param name="value">the field value</param>
        /// <returns>this</returns>
        public Point Field(string name, bool value)
        {
            return PutField(name, value);
        }

        /// <summary>
        /// Updates the timestamp for the point.
        /// </summary>
        /// <param name="timestamp">the timestamp</param>
        /// <param name="timeUnit">the timestamp precision</param>
        /// <returns></returns>
        public Point Timestamp(long timestamp, WritePrecision timeUnit)
        {
            Precision = timeUnit;
            _time = timestamp;

            return this;
        }

        /// <summary>
        /// Updates the timestamp for the point represented by <see cref="TimeSpan"/>.
        /// </summary>
        /// <param name="timestamp">the timestamp</param>
        /// <param name="timeUnit">the timestamp precision</param>
        /// <returns></returns>
        public Point Timestamp(TimeSpan timestamp, WritePrecision timeUnit)
        {
            Precision = timeUnit;

            switch (timeUnit)
            {
                case WritePrecision.Ns:
                    _time = timestamp.Ticks * 100;
                    break;
                case WritePrecision.Us:
                    _time = (BigInteger) (timestamp.Ticks * 0.1);
                    break;
                case WritePrecision.Ms:
                    _time = (BigInteger) timestamp.TotalMilliseconds;
                    break;
                case WritePrecision.S:
                    _time = (BigInteger) timestamp.TotalSeconds;
                    break;
            }

            return this;
        }

        /// <summary>
        /// Updates the timestamp for the point represented by <see cref="DateTime"/>.
        /// </summary>
        /// <param name="timestamp">the timestamp</param>
        /// <param name="timeUnit">the timestamp precision</param>
        /// <returns></returns>
        public Point Timestamp(DateTime timestamp, WritePrecision timeUnit)
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
        public Point Timestamp(DateTimeOffset timestamp, WritePrecision timeUnit)
        {
            return Timestamp(timestamp.UtcDateTime, timeUnit);
        }

        /// <summary>
        /// Updates the timestamp for the point represented by <see cref="Instant"/>.
        /// </summary>
        /// <param name="timestamp">the timestamp</param>
        /// <param name="timeUnit">the timestamp precision</param>
        /// <returns></returns>
        public Point Timestamp(Instant timestamp, WritePrecision timeUnit)
        {
            Precision = timeUnit;

            switch (timeUnit)
            {
                case WritePrecision.S:
                    _time = timestamp.ToUnixTimeSeconds();
                    break;
                case WritePrecision.Ms:
                    _time = timestamp.ToUnixTimeMilliseconds();
                    break;
                case WritePrecision.Us:
                    _time = (long) (timestamp.ToUnixTimeTicks() * 0.1);
                    break;
                default:
                    _time = (timestamp - NodaConstants.UnixEpoch).ToBigIntegerNanoseconds();
                    break;
            }

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
        /// The Line Protocol
        /// </summary>
        /// <param name="pointSettings">with the default values</param>
        /// <returns></returns>
        public string ToLineProtocol(PointSettings pointSettings = null)
        {
            var sb = new StringBuilder();

            EscapeKey(sb, _measurementName);
            AppendTags(sb, pointSettings);
            var appendedFields = AppendFields(sb);
            if (!appendedFields)
            {
                return "";
            }

            AppendTime(sb);

            return sb.ToString();
        }

        private Point PutField(string name, object value)
        {
            Arguments.CheckNonEmptyString(name, "Field name");

            _fields[name] = value;

            return this;
        }

        private void AppendTags(StringBuilder writer, PointSettings pointSettings)
        {
            IDictionary<string, string> entries = _tags;
            if (pointSettings != null)
            {
                var defaultTags = pointSettings.GetDefaultTags();
                if (defaultTags.Count > 0)
                {
                    var list = new List<IDictionary<string, string>> {_tags, defaultTags};

                    entries = list.SelectMany(dict => dict)
                        .Where(pair => !string.IsNullOrEmpty(pair.Value))
                        .ToLookup(pair => pair.Key, pair => pair.Value)
                        .ToDictionary(group => group.Key, group =>
                        {
                            var first = group.First();
                            return string.IsNullOrEmpty(first) ? group.Last() : first;
                        })
                        .ToSortedDictionary(StringComparer.Ordinal);
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
                    sb.Append(((IConvertible) value).ToString(CultureInfo.InvariantCulture));
                }
                else if (value is byte || value is int || value is long || value is sbyte || value is short ||
                         value is uint || value is ulong || value is ushort)
                {
                    sb.Append(((IConvertible) value).ToString(CultureInfo.InvariantCulture));
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
                    sb.Append(value);
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

        private void AppendTime(StringBuilder sb)
        {
            if (_time == null)
            {
                return;
            }

            sb.Append(' ');
            sb.Append(((BigInteger) _time).ToString(CultureInfo.InvariantCulture));
        }

        private void EscapeKey(StringBuilder sb, string key)
        {
            foreach (var c in key)
            {
                switch (c)
                {
                    case ' ':
                    case ',':
                    case '=':
                        sb.Append("\\");
                        break;
                }

                sb.Append(c);
            }
        }

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

        private bool IsNotDefined(object value)
        {
            return value == null
                   || (value is double d && (double.IsInfinity(d) || double.IsNaN(d)))
                   || (value is float f && (float.IsInfinity(f) || float.IsNaN(f)));
        }
    }

    internal static class DictionaryExtensions
    {
        public static SortedDictionary<TK, TV> ToSortedDictionary<TK, TV>(this Dictionary<TK, TV> existing,
            IComparer<TK> comparer)
        {
            return new SortedDictionary<TK, TV>(existing, comparer);
        }
    }
}