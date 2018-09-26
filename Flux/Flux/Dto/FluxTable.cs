using System.Collections.Generic;
using System.Text;

namespace Flux.flux.dto
{
/**
 * This class represents table structure of Flux CSV Response.
 *
 * <a href="https://github.com/influxdata/platform/blob/master/query/docs/SPEC.md#table">Specification</a>.
 */
    public class FluxTable
    {
        /**
        * Table column's labels and types.
        */
        public List<FluxColumn> Columns { get; set; } = new List<FluxColumn>();

        /**
        * Table records.
        */
        public List<FluxRecord> Records { get; set; } = new List<FluxRecord>();

        public override string ToString()
        {
            return new StringBuilder(GetType().Name + "[")
                .Append("columns=" + Columns.Count)
                .Append(", records=" + Records.Count)
                .Append("]")
                .ToString();
        }
    }
}