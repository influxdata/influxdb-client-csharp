using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using InfluxDB.Client.Core;
using InfluxDB.Client.Linq.Internal;
using InfluxDB.Client.Linq.Internal.NodeTypes;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;
using Expression = System.Linq.Expressions.Expression;

namespace InfluxDB.Client.Linq
{
    /// <summary>
    /// The settings for a Query optimization.
    /// </summary>
    public class QueryableOptimizerSettings
    {
        public QueryableOptimizerSettings()
        {
            QueryMultipleTimeSeries = false;
            AlignFieldsWithPivot = true;
            AlignLimitFunctionAfterPivot = true;
            DropMeasurementColumn = true;
            DropStartColumn = true;
            DropStopColumn = true;
            RangeStartValue = null;
            RangeStopValue = null;
        }

        /// <summary>
        /// Gets or sets whether the drive is used to query multiple time series.
        /// Setting this variable to true will change how the produced Flux Query looks like:
        /// <list type="bullet">
        /// <item>Appends <a href="https://docs.influxdata.com/flux/v0.x/stdlib/universe/group/">group operator</a></item>
        /// <item>Enable use default sorting: <i>sort(columns: ["_time"], desc: false)</i></item>
        /// </list>
        /// </summary>
        public bool QueryMultipleTimeSeries { get; set; }

        /// <summary>
        /// Gets or set whether the drive should use a Flux <a href="https://docs.influxdata.com/flux/v0.x/stdlib/universe/pivot/">pivot()</a> function
        /// to align fields to tabular way.
        /// </summary>
        public bool AlignFieldsWithPivot { get; set; }

        /// <summary>
        /// Gets or set whether the drive should align <a>limit()</a> and <a>tail()</a> functions
        /// after <a href="https://docs.influxdata.com/flux/v0.x/stdlib/universe/pivot/">pivot()</a> function.
        /// </summary>
        public bool AlignLimitFunctionAfterPivot { get; set; }

        /// <summary>
        /// Gets or sets whether the _measurement column will be dropped from query results.
        /// Setting this variable to true will change how the produced Flux Query looks like:
        /// <list type="bullet">
        /// <item>Appends <a href="https://docs.influxdata.com/flux/v0.x/stdlib/universe/drop/">drop operator</a></item>
        /// <item>Drops the _measurement column: <i>drop(columns: ["_measurement"])</i></item>
        /// </list>
        /// </summary>
        public bool DropMeasurementColumn { get; set; }

        /// <summary>
        /// Gets or sets whether the _start column will be dropped from query results.
        /// Setting this variable to true will change how the produced Flux Query looks like:
        /// <list type="bullet">
        /// <item>Appends <a href="https://docs.influxdata.com/flux/v0.x/stdlib/universe/drop/">drop operator</a></item>
        /// <item>Drops the _start column: <i>drop(columns: ["_start"])</i></item>
        /// </list>
        /// </summary>
        public bool DropStartColumn { get; set; }

        /// <summary>
        /// Gets or sets whether the _stop column will be dropped from query results.
        /// Setting this variable to true will change how the produced Flux Query looks like:
        /// <list type="bullet">
        /// <item>Appends <a href="https://docs.influxdata.com/flux/v0.x/stdlib/universe/drop/">drop operator</a></item>
        /// <item>Drops the _stop column: <i>drop(columns: ["_stop"])</i></item>
        /// </list>
        /// </summary>
        public bool DropStopColumn { get; set; }

        /// <summary>
        /// Gets or sets the default value for a start parameter of a <a href="https://docs.influxdata.com/flux/v0.x/stdlib/universe/range/">range function</a>.
        /// The `start` is earliest time to include in results. Results include points that match the specified start time.
        /// Defaults to `0`.
        /// </summary>
        public DateTime? RangeStartValue { get; set; }

        /// <summary>
        /// Gets or sets the default value for a stop parameter of a <a href="https://docs.influxdata.com/flux/v0.x/stdlib/universe/range/">range function</a>.
        /// The `stop` is latest time to include in results. Results exclude points that match the specified stop time.
        /// Defaults to `now()`.
        /// </summary>
        public DateTime? RangeStopValue { get; set; }
    }

    /// <summary>
    /// Main entry point to query InfluxDB by LINQ
    /// </summary>
    public class InfluxDBQueryable<T> : QueryableBase<T>
    {
        /// <summary>
        /// Create a new instance of IQueryable for synchronous Queries.
        /// </summary>
        /// <param name="bucket">Specifies the source bucket.</param>
        /// <param name="org">Specifies the source organization.</param>
        /// <param name="queryApi">The underlying API to execute Flux Query.</param>
        /// <param name="queryableOptimizerSettings">Settings for a Query optimization</param>
        /// <returns>new instance for of Queryable</returns>
        public static InfluxDBQueryable<T> Queryable(string bucket, string org, QueryApiSync queryApi,
            QueryableOptimizerSettings queryableOptimizerSettings = default)
        {
            return Queryable(bucket, org, queryApi, new DefaultMemberNameResolver(), queryableOptimizerSettings);
        }

        /// <summary>
        /// Create a new instance of IQueryable for asynchronous Queries.
        /// </summary>
        /// <param name="bucket">Specifies the source bucket.</param>
        /// <param name="org">Specifies the source organization.</param>
        /// <param name="queryApi">The underlying API to execute Flux Query.</param>
        /// <param name="queryableOptimizerSettings">Settings for a Query optimization</param>
        /// <returns>new instance for of Queryable</returns>
        public static InfluxDBQueryable<T> Queryable(string bucket, string org, QueryApi queryApi,
            QueryableOptimizerSettings queryableOptimizerSettings = default)
        {
            return Queryable(bucket, org, queryApi, new DefaultMemberNameResolver(), queryableOptimizerSettings);
        }

        /// <summary>
        /// Create a new instance of IQueryable for synchronous Queries.
        /// </summary>
        /// <param name="bucket">Specifies the source bucket.</param>
        /// <param name="org">Specifies the source organization.</param>
        /// <param name="queryApi">The underlying API to execute Flux Query.</param>
        /// <param name="memberResolver">Resolver for customized names.</param>
        /// <param name="queryableOptimizerSettings">Settings for a Query optimization</param>
        /// <returns>new instance for of Queryable</returns>
        public static InfluxDBQueryable<T> Queryable(string bucket, string org, QueryApiSync queryApi,
            IMemberNameResolver memberResolver, QueryableOptimizerSettings queryableOptimizerSettings = default)
        {
            return new InfluxDBQueryable<T>(bucket, org, queryApi, memberResolver, queryableOptimizerSettings);
        }

        /// <summary>
        /// Create a new instance of IQueryable for asynchronous Queries.
        /// </summary>
        /// <param name="bucket">Specifies the source bucket.</param>
        /// <param name="org">Specifies the source organization.</param>
        /// <param name="queryApi">The underlying API to execute Flux Query.</param>
        /// <param name="memberResolver">Resolver for customized names.</param>
        /// <param name="queryableOptimizerSettings">Settings for a Query optimization</param>
        /// <returns>new instance for of Queryable</returns>
        public static InfluxDBQueryable<T> Queryable(string bucket, string org, QueryApi queryApi,
            IMemberNameResolver memberResolver, QueryableOptimizerSettings queryableOptimizerSettings = default)
        {
            return new InfluxDBQueryable<T>(bucket, org, queryApi, memberResolver, queryableOptimizerSettings);
        }

        /// <summary>
        /// Create a new instance of IQueryable for synchronous Queries.
        /// </summary>
        /// <param name="bucket">Specifies the source bucket.</param>
        /// <param name="org">Specifies the source organization.</param>
        /// <param name="queryApi">The underlying API to execute Flux Query.</param>
        /// <param name="memberResolver">Resolver for customized names.</param>
        /// <param name="queryableOptimizerSettings">Settings for a Query optimization</param>
        public InfluxDBQueryable(string bucket, string org, QueryApiSync queryApi, IMemberNameResolver memberResolver,
            QueryableOptimizerSettings queryableOptimizerSettings = default) : base(CreateQueryParser(),
            CreateExecutor(bucket, org, queryApi, memberResolver, queryableOptimizerSettings))
        {
        }

        /// <summary>
        /// Create a new instance of IQueryable for asynchronous Queries.
        /// </summary>
        /// <param name="bucket">Specifies the source bucket.</param>
        /// <param name="org">Specifies the source organization.</param>
        /// <param name="queryApi">The underlying API to execute Flux Query.</param>
        /// <param name="memberResolver">Resolver for customized names.</param>
        /// <param name="queryableOptimizerSettings">Settings for a Query optimization</param>
        public InfluxDBQueryable(string bucket, string org, QueryApi queryApi, IMemberNameResolver memberResolver,
            QueryableOptimizerSettings queryableOptimizerSettings = default) : base(CreateQueryParser(),
            CreateExecutor(bucket, org, queryApi, memberResolver, queryableOptimizerSettings))
        {
        }

        /// <summary>
        /// Call by ReLinq.
        /// </summary>
        /// <param name="provider"></param>
        /// <param name="expression"></param>
        public InfluxDBQueryable(IQueryProvider provider, Expression expression) : base(provider, expression)
        {
        }

        /// <summary>
        /// Create a <see cref="Api.Domain.Query"/> object that will be used for Querying.
        /// </summary>
        /// <returns>Query that will be used to Querying</returns>
        public Api.Domain.Query ToDebugQuery()
        {
            var provider = Provider as DefaultQueryProvider;
            var executor = provider?.Executor as InfluxDBQueryExecutor;

            if (executor == null)
            {
                throw new NotSupportedException("InfluxDBQueryable should use InfluxDBQueryExecutor");
            }

            var parsedQuery = provider.QueryParser.GetParsedQuery(Expression);
            var generateQuery = executor.GenerateQuery(parsedQuery, out _);

            return generateQuery;
        }

        private static IQueryExecutor CreateExecutor(string bucket, string org, QueryApiSync queryApi,
            IMemberNameResolver memberResolver, QueryableOptimizerSettings queryableOptimizerSettings = default)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(queryApi, nameof(queryApi));

            return new InfluxDBQueryExecutor(bucket, org, queryApi, memberResolver,
                queryableOptimizerSettings ?? new QueryableOptimizerSettings());
        }

        private static IQueryExecutor CreateExecutor(string bucket, string org, QueryApi queryApi,
            IMemberNameResolver memberResolver, QueryableOptimizerSettings queryableOptimizerSettings = default)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(queryApi, nameof(queryApi));

            return new InfluxDBQueryExecutor(bucket, org, queryApi, memberResolver,
                queryableOptimizerSettings ?? new QueryableOptimizerSettings());
        }

        internal static QueryParser CreateQueryParser()
        {
            var queryParser = QueryParser.CreateDefault();
            var compoundNodeTypeProvider = queryParser.NodeTypeProvider as CompoundNodeTypeProvider;
            compoundNodeTypeProvider?.InnerProviders.Add(new InfluxDBNodeTypeProvider());
            return queryParser;
        }

        public IAsyncEnumerable<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            var provider = Provider as DefaultQueryProvider;

            if (!(provider?.Executor is InfluxDBQueryExecutor executor))
            {
                throw new NotSupportedException("InfluxDBQueryable should use InfluxDBQueryExecutor");
            }

            var parsedQuery = provider.QueryParser.GetParsedQuery(Expression);
            return executor.ExecuteCollectionAsync<T>(parsedQuery, cancellationToken);
        }
    }

    public static class QueryableExtensions
    {
        public static InfluxDBQueryable<T> ToInfluxQueryable<T>(this IQueryable<T> source)
        {
            if (source == null)
            {
                throw new InvalidCastException("Queryable source is null");
            }

            if (!(source is InfluxDBQueryable<T> queryable))
            {
                throw new InvalidCastException("Queryable should be InfluxDBQueryable");
            }

            return queryable;
        }

        /// <summary>
        /// The extension to use Flux window operator. For more info see https://docs.influxdata.com/flux/v0.x/stdlib/universe/aggregatewindow/
        ///
        /// <example>
        /// <code>
        /// var query = from s in InfluxDBQueryable&lt;Sensor&gt;.Queryable("my-bucket", "my-org", _queryApi)
        ///     where s.Timestamp.AggregateWindow(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(40), "mean")
        ///     where s.Value == 5
        ///     select s;
        /// </code>
        /// </example>
        /// </summary>
        /// <param name="timestamp">The entity value which is market as a Timestamp.</param>
        /// <param name="every">Duration of time between windows.</param>
        /// <param name="period">Duration of the window.</param>
        /// <param name="fn">Aggregate or selector function used to operate on each window of time.</param>
        /// <returns>NotSupportedException if it's called outside LINQ expression.</returns>
        /// <exception cref="NotSupportedException">Caused by calling outside of LINQ expression.</exception>
        // ReSharper disable UnusedParameter.Global
        public static bool AggregateWindow(this DateTime timestamp, TimeSpan every, TimeSpan? period = null,
            string fn = "mean")
        {
            throw new NotSupportedException("This should be used only in LINQ expression. " +
                                            "Something like: 'where s.Timestamp.AggregateWindow(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(40), \"mean\")'.");
        }
    }
}