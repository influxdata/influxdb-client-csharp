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
        internal string FluxFunction;
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
        private readonly List<LimitOffsetAssignment> _limitTailNOffsetAssignments;
        private ResultFunction _resultFunction;
        private readonly List<string> _filterByTags;
        private readonly List<string> _filterByFields;
        private readonly List<(string, string, bool, string)> _orders;
        private (string Every, string Period, string Fn)? _aggregateWindow;

        internal QueryAggregator()
        {
            _resultFunction = ResultFunction.None;
            _limitTailNOffsetAssignments = new List<LimitOffsetAssignment>();
            _filterByTags = new List<string>();
            _filterByFields = new List<string>();
            _orders = new List<(string, string, bool, string)>();
            _aggregateWindow = null;
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

        internal void AddAggregateWindow(string everyVariable, string periodVariable, string fnVariable)
        {
            _aggregateWindow = (everyVariable, periodVariable, fnVariable);
        }


        internal void AddLimitTailN(string limitNAssignment, string fluxFunction)
        {
            if (_limitTailNOffsetAssignments.Count > 0 && _limitTailNOffsetAssignments.Last().N == null)
            {
                _limitTailNOffsetAssignments.Last().FluxFunction = fluxFunction;
                _limitTailNOffsetAssignments.Last().N = limitNAssignment;
            }
            else
            {
                _limitTailNOffsetAssignments.Add(new LimitOffsetAssignment
                    { FluxFunction = fluxFunction, N = limitNAssignment });
            }
        }

        internal void AddLimitTailOffset(string limitOffsetAssignment)
        {
            if (_limitTailNOffsetAssignments.Count > 0)
            {
                _limitTailNOffsetAssignments.Last().Offset = limitOffsetAssignment;
            }
            else
            {
                _limitTailNOffsetAssignments.Add(new LimitOffsetAssignment { Offset = limitOffsetAssignment });
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

        internal void AddOrder(string column, string columnVariable, bool descending, string descendingVariable)
        {
            _orders.Add((column, columnVariable, descending, descendingVariable));
        }

        internal void AddResultFunction(ResultFunction resultFunction)
        {
            Arguments.CheckNotNull(resultFunction, nameof(resultFunction));

            _resultFunction = resultFunction;
        }

        internal string BuildFluxQuery(QueryableOptimizerSettings settings)
        {
            Arguments.CheckNotNull(settings, nameof(settings));

            var transforms = new List<string>();
            var parts = new List<string>
            {
                BuildOperator("from", "bucket", _bucketAssignment),
                BuildRange(transforms),
                BuildFilter(_filterByTags),
                BuildAggregateWindow(_aggregateWindow)
            };

            if (!settings.AlignLimitFunctionAfterPivot)
            {
                AddLimitFunctions(parts);
            }

            if (settings.AlignFieldsWithPivot)
            {
                parts.Add("pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")");
            }

            var drop = BuildDrop(settings);
            if (!string.IsNullOrEmpty(drop))
            {
                parts.Add(drop);
            }

            parts.Add(settings.QueryMultipleTimeSeries ? "group()" : "");
            parts.Add(BuildFilter(_filterByFields));

            // https://docs.influxdata.com/flux/v0.x/stdlib/universe/sort/
            foreach (var ((column, columnVariable, descending, descendingVariable), index) in _orders.Select(
                         (value, i) => (value, i)))
            {
                // skip default sorting if don't query to multiple time series
                if (!settings.QueryMultipleTimeSeries && index == 0 && column == "_time" && !descending)
                {
                    continue;
                }

                parts.Add(BuildOperator("sort", "columns", new List<string> { columnVariable }, "desc",
                    descendingVariable));
            }

            if (settings.AlignLimitFunctionAfterPivot)
            {
                AddLimitFunctions(parts);
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

        private void AddLimitFunctions(List<string> parts)
        {
            // https://docs.influxdata.com/flux/latest/stdlib/universe/limit/
            // https://docs.influxdata.com/flux/latest/stdlib/universe/tail/
            foreach (var limitNOffsetAssignment in _limitTailNOffsetAssignments)
                if (limitNOffsetAssignment.N != null)
                {
                    parts.Add(BuildOperator(limitNOffsetAssignment.FluxFunction,
                        "n", limitNOffsetAssignment.N,
                        "offset", limitNOffsetAssignment.Offset));
                }
        }

        private string BuildAggregateWindow((string Every, string Period, string Fn)? aggregateWindow)
        {
            if (aggregateWindow == null)
            {
                return null;
            }

            var (every, period, fn) = aggregateWindow.Value;
            var list = new List<string>
            {
                $"every: {every}",
                period != null ? $"period: {period}" : null,
                $"fn: {fn}"
            };


            return $"aggregateWindow({JoinList(list, ", ")})";
        }

        private string BuildDrop(QueryableOptimizerSettings settings)
        {
            var columns = new List<string>();

            if (settings.DropStartColumn)
            {
                columns.Add("\"_start\"");
            }

            if (settings.DropStopColumn)
            {
                columns.Add("\"_stop\"");
            }

            if (settings.DropMeasurementColumn)
            {
                columns.Add("\"_measurement\"");
            }

            if (columns.Count == 0)
            {
                return null;
            }

            return $"drop(columns: [{string.Join(", ", columns)}])";
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