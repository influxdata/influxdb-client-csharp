using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using InfluxData.Platform.Client.Client;
using Platform.Common.Platform;

namespace InfluxData.Platform.Client.Write
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

        public TimeUnit Precision { get; private set; }
        private double? _time;

        private Point(string measurementName)
        {
            Arguments.CheckNonEmptyString(measurementName, "Measurement name");

            _measurementName = measurementName;
            _fields = new SortedDictionary<string, object>(StringComparer.Ordinal);
            _tags = new SortedDictionary<string, string>(StringComparer.Ordinal);
            Precision = TimeUnit.Nanos;
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
        public Point Timestamp(long timestamp, TimeUnit timeUnit)
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
        public Point Timestamp(TimeSpan timestamp, TimeUnit timeUnit)
        {
            Precision = timeUnit;

            switch (timeUnit)
            {
                case TimeUnit.Nanos:
                    _time = timestamp.Ticks * 100;
                    break;
                case TimeUnit.Micros:
                    _time = timestamp.Ticks * 0.1;
                    break;
                case TimeUnit.Millis:
                    _time = timestamp.TotalMilliseconds;
                    break;
                case TimeUnit.Seconds:
                    _time = timestamp.TotalSeconds;
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
        public Point Timestamp(DateTime timestamp, TimeUnit timeUnit)
        {
            if (timestamp != null && timestamp.Kind != DateTimeKind.Utc)
            {
                throw new ArgumentException("Timestamps must be specified as UTC", nameof(timestamp));
            }

            TimeSpan timeSpan = timestamp.Subtract(EpochStart);
            
            return Timestamp(timeSpan, timeUnit);
        }

        /// <summary>
        /// Updates the timestamp for the point represented by <see cref="DateTime"/>.
        /// </summary>
        /// <param name="timestamp">the timestamp</param>
        /// <param name="timeUnit">the timestamp precision</param>
        /// <returns></returns>
        public Point Timestamp(DateTimeOffset timestamp, TimeUnit timeUnit)
        {
            return Timestamp(timestamp.UtcDateTime, timeUnit);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            EscapeKey(sb, _measurementName);
            AppendTags(sb);
            AppendFields(sb);
            AppendTime(sb);

            return sb.ToString();
        }

        private Point PutField(string name, object value)
        {
            Arguments.CheckNonEmptyString(name, "Field name");

            _fields[name] = value;

            return this;
        }

        private void AppendTags(StringBuilder writer)
        {
            foreach (var keyValue in _tags)
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

        private void AppendFields(StringBuilder sb)
        {
            bool removeLast = false;

            foreach (var keyValue in _fields)
            {
                var key = keyValue.Key;
                var value = keyValue.Value;

                if (value == null)
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
                removeLast = true;
            }

            if (removeLast)
            {
                sb.Remove(sb.Length - 1, 1);
            }
        }

        private void AppendTime(StringBuilder sb)
        {
            if (_time == null)
            {
                return;
            }

            sb.Append(' ');
            sb.Append(((IConvertible) _time).ToString(CultureInfo.InvariantCulture));
        }

        private void EscapeKey(StringBuilder sb, string key)
        {
            foreach (char c in key)
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
            foreach (char c in value)
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
    }
}