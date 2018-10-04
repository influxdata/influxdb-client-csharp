using System;
using System.Text;

namespace Flux.flux.dto
{
/**
 * This class represents column header specification of {@link FluxTable}.
 * <p>
 */
    public class FluxColumn
    {
        /**
         * Column index in record.
         */
        public int Index { get; set; }

        /**
         * The label of column (e.g., "_start", "_stop", "_time").
         */

        public string Label { get; set; }

        /**
         * The data type of column (e.g., "string", "long", "dateTime:RFC3339").
         */
        public string DataType { get; set; }

        /**
         * Boolean flag indicating if the column is part of the table's group key.
         */
        public bool Group { get; set; }

        /**
         * Default value to be used for rows whose string value is the empty string.
         */
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