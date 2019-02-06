using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfluxDB.Client.Core.Flux.Domain
{
    /// <summary>
    /// This class represents table structure of Flux CSV Response.
    ///<a href="http://bit.ly/flux-spec">Specification</a>.
    /// </summary>
    public class FluxTable
    {
        /// <summary>
        /// Table column's labels and types.
        /// </summary>
        public List<FluxColumn> Columns { get; } = new List<FluxColumn>();

        /// <summary>
        /// Table records.
        /// </summary>
        public List<FluxRecord> Records { get; } = new List<FluxRecord>();

        /// <summary>
        /// A table's group key is subset of the entire columns dataset that assigned to the table.
        /// As such, all records within a table will have the same values for each column that is part of the group key.
        /// </summary>
        /// <returns></returns>
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