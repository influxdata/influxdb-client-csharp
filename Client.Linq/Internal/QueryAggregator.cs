using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryAggregator
    {
        private string _bucketAssignment;
        private string _rangeStartAssignment;
        private string _rangeStopAssignment;
        private string _limitNAssignment;
        private string _limitOffsetAssignment;
        private readonly List<string> _filters;
        private readonly List<(string, string)> _orders;

        internal QueryAggregator()
        {
            _filters = new List<string>();
            _orders = new List<(string, string)>();
        }

        internal void AddBucket(string bucket)
        {
            _bucketAssignment = bucket;
        }

        internal void AddRangeStart(string rangeStart)
        {
            _rangeStartAssignment = rangeStart;
        }
        
        internal void AddRangeStop(string rangeStop)
        {
            _rangeStopAssignment = rangeStop;
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

        internal void AddSubQueries(QueryAggregator aggregator)
        {
            _filters.AddRange(aggregator._filters);
            _orders.AddRange(aggregator._orders);
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
                BuildOperator("range", "start", _rangeStartAssignment, "stop", _rangeStopAssignment),
                "pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")",
                BuildFilter()
            };

            // https://docs.influxdata.com/influxdb/cloud/reference/flux/stdlib/built-in/transformations/sort/
            foreach (var (column, desc) in _orders)
            {
                parts.Add(BuildOperator("sort", "columns", new List<string> {column}, "desc", desc));
            }

            // https://docs.influxdata.com/influxdb/cloud/reference/flux/stdlib/built-in/transformations/limit/
            if (_limitNAssignment != null)
            {
                parts.Add(BuildOperator("limit", "n", _limitNAssignment, "offset", _limitOffsetAssignment));
            }

            return JoinList(parts, "|>");
        }

        internal string BuildFilter()
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

                var stringValue = Convert.ToString(filter);
                if (stringValue.Length == 0)
                {
                    return builder;
                }

                if (builder.Length != 0)
                {
                    builder.Append(" ");
                    builder.Append(delimiter);
                    builder.Append(" ");
                }

                builder.Append(stringValue);

                return builder;
            }).ToString();
        }
    }
}