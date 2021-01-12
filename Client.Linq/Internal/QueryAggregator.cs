using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryAggregator
    {
        private readonly string _bucketAssignment;
        private readonly string _rangeStartAssignment;
        private string _limitNAssignment;
        private string _limitOffsetAssignment;

        internal QueryAggregator(string bucketAssignment, string rangeStartAssignment)
        {
            _bucketAssignment = bucketAssignment;
            _rangeStartAssignment = rangeStartAssignment;
        }

        internal void AddLimitN(string limitNAssignment)
        {
            _limitNAssignment = limitNAssignment;
        }

        internal void AddLimitOffset(string limitOffsetAssignment)
        {
            _limitOffsetAssignment = limitOffsetAssignment;
        }

        internal string BuildFluxQuery()
        {
            var parts = new List<string>
            {
                BuildOperator("from", "bucket", _bucketAssignment),
                BuildOperator("range", "start", _rangeStartAssignment),
                "pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")"
            };
            
            // https://docs.influxdata.com/influxdb/cloud/reference/flux/stdlib/built-in/transformations/limit/
            if (_limitNAssignment != null)
            {
                parts.Add(BuildOperator("limit", "n", _limitNAssignment, "offset", _limitOffsetAssignment));
            }

            return parts.Aggregate(new StringBuilder(), (builder, part) =>
            {
                if (part == null)
                {
                    return builder;
                }

                if (builder.Length != 0)
                {
                    builder.Append(" |> ");
                }

                builder.Append(part);

                return builder;
            }).ToString();
        }

        private string BuildOperator(string operatorName, params string[] variables)
        {
            var builderVariables = new StringBuilder();

            for (var i = 0; i < variables.Length; i += 2)
            {
                var variableName = variables[i];
                var variableAssignment = variables[i + 1];

                if (variableAssignment == null)
                {
                    continue;
                }

                if (builderVariables.Length != 0)
                {
                    builderVariables.Append(", ");
                }

                builderVariables.Append(variableName);
                builderVariables.Append(": ");
                builderVariables.Append(variableAssignment);
            }

            if (builderVariables.Length == 0)
            {
                return null;
            }

            var builder = new StringBuilder();
            builder.Append(operatorName);
            builder.Append("(");
            builder.Append(builderVariables);
            builder.Append(")");
            return builder.ToString();
        }
    }
}