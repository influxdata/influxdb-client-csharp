using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Platform.Common.Flux.Domain
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
        
        /**
        * A table's group key is subset of the entire columns dataset that assigned to the table.
        * As such, all records within a table will have the same values for each column that is part of the group key.
        */
        public List<FluxColumn> GetGroupKey()
        {
            return Columns.Where(column => column.Group).ToList();
        }

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