using System;
using System.Collections;
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
        private readonly List<string> _filters;
        private readonly List<(string, string)> _orders;

        internal QueryAggregator(string bucketAssignment, string rangeStartAssignment)
        {
            _bucketAssignment = bucketAssignment;
            _rangeStartAssignment = rangeStartAssignment;
            _filters = new List<string>();
            _orders = new List<(string, string)>();
        }

        internal void AddLimitN(string limitNAssignment)
        {
            _limitNAssignment = limitNAssignment;
        }

        internal void AddLimitOffset(string limitOffsetAssignment)
        {
            _limitOffsetAssignment = limitOffsetAssignment;
        }

        internal void AddFilter(string filter)
        {
            _filters.Add(filter);
        }

        public void AddOrder(string orderPart, string desc)
        {
            _orders.Add((orderPart, desc));
        }

        internal string BuildFluxQuery()
        {
            var parts = new List<string>
            {
                BuildOperator("from", "bucket", _bucketAssignment),
                BuildOperator("range", "start", _rangeStartAssignment),
                //"drop(columns: [\"_start\", \"_stop\", \"_measurement\"])",
                "pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")",
                BuildFilter()
            };

            // https://docs.influxdata.com/influxdb/cloud/reference/flux/stdlib/built-in/transformations/sort/
            foreach (var (column, desc) in _orders)
            {
                parts.Add(BuildOperator("sort", "columns", new List<string>{column}, "desc", desc));
            }

            // https://docs.influxdata.com/influxdb/cloud/reference/flux/stdlib/built-in/transformations/limit/
            if (_limitNAssignment != null)
            {
                parts.Add(BuildOperator("limit", "n", _limitNAssignment, "offset", _limitOffsetAssignment));
            }

            return JoinList(parts, "|>");
        }

        private string BuildFilter()
        {
            var filters = JoinList(_filters, "and");
            if (filters.Length == 0)
            {
                return null;
            }

            var filter = new StringBuilder();
            filter.Append("filter(fn: (r) => ");
            filter.Append(filters);
            filter.Append(")");

            return filter.ToString();
        }

        private string BuildOperator(string operatorName, params object[] variables)
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

                if (variableAssignment is IEnumerable<string> enumerable)
                {
                    builderVariables.Append("[");
                    builderVariables.Append(JoinList(enumerable, ","));
                    builderVariables.Append("]");
                }
                else
                {
                    builderVariables.Append(variableAssignment);
                }
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

        private string JoinList(IEnumerable<object> strings, string delimiter)
        {
            return strings.Aggregate(new StringBuilder(), (builder, filter) =>
            {
                if (filter == null)
                {
                    return builder;
                }

                if (builder.Length != 0)
                {
                    builder.Append(" ");
                    builder.Append(delimiter);
                    builder.Append(" ");
                }

                builder.Append(filter);

                return builder;
            }).ToString();
        }
    }
}