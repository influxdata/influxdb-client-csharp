using System.Text;

namespace InfluxDB.Client.Core.Flux.Domain
{
    /// <summary>
    /// This class represents column header specification of <see cref="FluxTable"/>.
    /// </summary>
    public class FluxColumn
    {
        /// <summary>
        /// Column index in record.
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// The label of column (e.g., "_start", "_stop", "_time").
        /// </summary>
        public string Label { get; set; }

        /// <summary>
        /// The data type of column (e.g., "string", "long", "dateTime:RFC3339").
        /// </summary>
        public string DataType { get; set; }

        /// <summary>
        /// Boolean flag indicating if the column is part of the table's group key.
        /// </summary>
        public bool Group { get; set; }

        /// <summary>
        /// Default value to be used for rows whose string value is the empty string.
        /// </summary>
        public string DefaultValue { get; set; }

        public override string ToString()
        {
            return new StringBuilder(GetType().Name + "[")
                .Append("index=" + Index)
                .Append(", label='" + Label + "'")
                .Append(", dataType='" + DataType + "'")
                .Append(", group=" + Group)
                .Append(", defaultValue='" + DefaultValue + "'")
                .Append("]").ToString();
        }
    }
}