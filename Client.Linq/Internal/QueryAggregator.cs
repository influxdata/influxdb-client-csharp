using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfluxDB.Client.Core;

namespace InfluxDB.Client.Linq.Internal
{
    internal enum ResultFunction
    {
        /// <summary>
        /// Without result function.
        /// </summary>
        None,
        
        /// <summary>
        /// Count result function.
        /// </summary>
        Count
    }

    internal enum RangeExpressionType
    {
        /// <summary>
        /// Equality comparison
        /// </summary>
        Equal,
        /// <summary>
        /// "Less than" comparison
        /// </summary>
        LessThan,
        /// <summary>
        /// "Less than or equal" comparison
        /// </summary>
        LessThanOrEqual,
        /// <summary>
        /// "Greater than" comparison
        /// </summary>
        GreaterThan,
        /// <summary>
        /// "Greater than or equal" comparison
        /// </summary>
        GreaterThanOrEqual
    }

    internal class QueryAggregator
    {
        private string _bucketAssignment;
        private string _rangeStartAssignment;
        private string _rangeStopAssignment;
        private string _limitNAssignment;
        private string _limitOffsetAssignment;
        private ResultFunction _resultFunction;
        private readonly List<string> _filters;
        private readonly List<(string, string)> _orders;

        internal QueryAggregator()
        {
            _resultFunction = ResultFunction.None;
            _filters = new List<string>();
            _orders = new List<(string, string)>();
        }

        internal void AddBucket(string bucket)
        {
            _bucketAssignment = bucket;
        }

        internal void AddRangeStart(string rangeStart, RangeExpressionType expressionType)
        {
            _rangeStartAssignment = rangeStart;
        }
        
        internal void AddRangeStop(string rangeStop, RangeExpressionType expressionType)
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

        internal void AddOrder(string orderPart, string desc)
        {
            _orders.Add((orderPart, desc));
        }

        internal void AddResultFunction(ResultFunction resultFunction)
        {
            Arguments.CheckNotNull(resultFunction, nameof(resultFunction));
            
            _resultFunction = resultFunction;
        }

        internal string BuildFluxQuery()
        {
            var transforms = new List<string>();
            var parts = new List<string>
            {
                BuildOperator("from", "bucket", _bucketAssignment),
                BuildRange(transforms),
                "drop(columns: [\"_start\", \"_stop\", \"_measurement\"])",
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

            if (_resultFunction != ResultFunction.None)
            {
                if (_resultFunction == ResultFunction.Count)
                {
                    parts.Add("stateCount(fn: (r) => true, column: \"linq_result_column\")");
                    parts.Add("last(column: \"linq_result_column\")");
                    parts.Add("keep(columns: [\"linq_result_column\"])");
                }
            }

            var query = new StringBuilder();

            query.Append(JoinList(transforms, "\n"));
            query.Append("\n\n");
            query.Append(JoinList(parts, " |> "));

            return query.ToString();
        }

        private string BuildRange(List<string> transforms)
        {
            string rangeStartShift = null;
            string rangeStopShift = null;
            
            if (_rangeStartAssignment != null)
            {
                transforms.Add($"start_shifted = int(v: time(v: {_rangeStartAssignment}))");
                rangeStartShift = "time(v: start_shifted)";
            }

            if (_rangeStopAssignment != null)
            {
                transforms.Add($"stop_shifted = int(v: time(v: {_rangeStopAssignment}))");
                rangeStopShift = "time(v: stop_shifted)";
            }
            
            return BuildOperator("range", "start", rangeStartShift, "stop", rangeStopShift);
        }

        private string BuildFilter()
        {
            var filters = JoinList(_filters, " and ");
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
                    builderVariables.Append(JoinList(enumerable, ", "));
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

        private StringBuilder JoinList(IEnumerable<object> strings, string delimiter)
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
                    builder.Append(delimiter);
                }

                builder.Append(stringValue);

                return builder;
            });
        }
    }
}