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

    internal class LimitOffsetAssignment
    {
        internal string N;
        internal string Offset;
    }

    internal class QueryAggregator
    {
        private string _bucketAssignment;
        private string _rangeStartAssignment;
        private RangeExpressionType _rangeStartExpression;
        private string _rangeStopAssignment;
        private RangeExpressionType _rangeStopExpression;
        private readonly List<LimitOffsetAssignment> _limitNOffsetAssignments;
        private ResultFunction _resultFunction;
        private readonly List<string> _filterByTags;
        private readonly List<string> _filterByFields;
        private readonly List<(string, string)> _orders;

        internal QueryAggregator()
        {
            _resultFunction = ResultFunction.None;
            _limitNOffsetAssignments = new List<LimitOffsetAssignment>();
            _filterByTags = new List<string>();
            _filterByFields = new List<string>();
            _orders = new List<(string, string)>();
        }

        internal void AddBucket(string bucket)
        {
            _bucketAssignment = bucket;
        }

        internal void AddRangeStart(string rangeStart, RangeExpressionType expressionType)
        {
            _rangeStartAssignment = rangeStart;
            _rangeStartExpression = expressionType;
        }

        internal void AddRangeStop(string rangeStop, RangeExpressionType expressionType)
        {
            _rangeStopAssignment = rangeStop;
            _rangeStopExpression = expressionType;
        }

        internal void AddLimitN(string limitNAssignment)
        {
            if (_limitNOffsetAssignments.Count > 0 && _limitNOffsetAssignments.Last().N == null)
            {
                _limitNOffsetAssignments.Last().N = limitNAssignment;
            }
            else
            {
                _limitNOffsetAssignments.Add(new LimitOffsetAssignment {N = limitNAssignment});
            }
        }

        internal void AddLimitOffset(string limitOffsetAssignment)
        {
            if (_limitNOffsetAssignments.Count > 0)
            {
                _limitNOffsetAssignments.Last().Offset = limitOffsetAssignment;
            }
            else
            {
                _limitNOffsetAssignments.Add(new LimitOffsetAssignment {Offset = limitOffsetAssignment});
            }
        }

        internal void AddFilterByTags(string filter)
        {
            _filterByTags.Add(filter);
        }

        internal void AddFilterByFields(string filter)
        {
            _filterByFields.Add(filter);
        }

        internal void AddSubQueries(QueryAggregator aggregator)
        {
            _filterByTags.AddRange(aggregator._filterByTags);
            _filterByFields.AddRange(aggregator._filterByFields);
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
                BuildFilter(_filterByTags),
                "drop(columns: [\"_start\", \"_stop\", \"_measurement\"])",
                "pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")",
                "group()",
                BuildFilter(_filterByFields)
            };

            // https://docs.influxdata.com/influxdb/cloud/reference/flux/stdlib/built-in/transformations/sort/
            foreach (var (column, desc) in _orders)
            {
                parts.Add(BuildOperator("sort", "columns", new List<string> {column}, "desc", desc));
            }

            // https://docs.influxdata.com/influxdb/cloud/reference/flux/stdlib/built-in/transformations/limit/
            foreach (var limitNOffsetAssignment in _limitNOffsetAssignments)
            {
                if (limitNOffsetAssignment.N != null)
                {
                    parts.Add(BuildOperator("limit",
                        "n", limitNOffsetAssignment.N,
                        "offset", limitNOffsetAssignment.Offset));
                }
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
                var startShifted = $"start_shifted = int(v: time(v: {_rangeStartAssignment}))";
                if (_rangeStartExpression == RangeExpressionType.GreaterThan)
                {
                    startShifted += " + 1";
                }

                transforms.Add(startShifted);
                rangeStartShift = "time(v: start_shifted)";
            }

            if (_rangeStopAssignment != null)
            {
                var stopShifted = $"stop_shifted = int(v: time(v: {_rangeStopAssignment}))";
                if (_rangeStopExpression == RangeExpressionType.LessThanOrEqual ||
                    _rangeStopExpression == RangeExpressionType.Equal)
                {
                    stopShifted += " + 1";
                }

                transforms.Add(stopShifted);
                rangeStopShift = "time(v: stop_shifted)";
            }

            return BuildOperator("range", "start", rangeStartShift, "stop", rangeStopShift);
        }

        private string BuildFilter(IEnumerable<string> filterBy)
        {
            var filters = JoinList(filterBy, " and ");
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