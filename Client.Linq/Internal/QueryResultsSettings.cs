using System;
using System.Collections.Generic;
using System.Linq;
using Remotion.Linq;
using Remotion.Linq.Clauses.ResultOperators;

namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryResultsSettings
    {
        /// <summary>
        /// If true, indicates that result is aggregated scalar value. This applies to, for example, item counts.
        /// </summary>
        internal readonly bool ScalarAggregated;

        /// <summary>
        /// The function that transform results info required scalar value.
        /// </summary>
        internal readonly Func<IEnumerable<object>, object> AggregateFunction;

        internal QueryResultsSettings(QueryModel queryModel)
        {
            foreach (var resultOperator in queryModel.ResultOperators)
                //
                // Count
                //
                if (resultOperator is CountResultOperator || resultOperator is LongCountResultOperator)
                {
                    ScalarAggregated = true;
                    AggregateFunction = objects => objects
                        .Select(it => (long)Convert.ChangeType(it, typeof(long)))
                        .Sum();
                    return;
                }

            //
            // Default behaviour
            //
            ScalarAggregated = false;
            AggregateFunction = objects => objects;
        }
    }
}