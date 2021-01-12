using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryAggregator
    {
        private readonly string _bucketAssignment;
        private readonly string _rangeStartAssignment;

        internal QueryAggregator(string bucketAssignment, string rangeStartAssignment)
        {
            _bucketAssignment = bucketAssignment;
            _rangeStartAssignment = rangeStartAssignment;
        }

        public string BuildFluxQuery()
        {
            var parts = new List<string>
            {
                BuildOperator("from", "bucket", _bucketAssignment),
                BuildOperator("range", "start", _rangeStartAssignment),
                "pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")"
            };

            return parts.Aggregate(new StringBuilder(), (builder, part) =>
            {
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
            var builder = new StringBuilder();
            builder.Append(operatorName);
            builder.Append("(");

            for (var i = 0; i < variables.Length; i += 2)
            {
                var variableName = variables[i];
                var variableAssignment = variables[i + 1];

                builder.Append($"{variableName}: {variableAssignment}");
            }

            builder.Append(")");
            return builder.ToString();
        }
    }
}