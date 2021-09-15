using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Flux.Domain;
using Remotion.Linq;

[assembly: InternalsVisibleTo("Client.Linq.Test, PublicKey=002400000480000094000000060200000024000052534131" +
                              "0004000001000100efaac865f88dd35c90dc548945405aae34056eedbe42cad60971f89a861a78437e86d" +
                              "95804a1aeeb0de18ac3728782f9dc8dbae2e806167a8bb64c0402278edcefd78c13dbe7f8d13de36eb362" +
                              "21ec215c66ee2dfe7943de97b869c5eea4d92f92d345ced67de5ac8fc3cd2f8dd7e3c0c53bdb0cc433af8" +
                              "59033d069cad397a7")]

namespace InfluxDB.Client.Linq.Internal
{
    /// <summary>
    /// Executor is called by ReLinq when query is executed.
    /// </summary>
    internal class InfluxDBQueryExecutor : IQueryExecutor
    {
        private readonly string _bucket;
        private readonly string _org;
        private readonly QueryApiSync _queryApiSync;
        private readonly QueryApi _queryApi;
        private readonly IMemberNameResolver _memberResolver;
        private readonly QueryableOptimizerSettings _queryableOptimizerSettings;

        /// <summary>
        /// Create InfluxDBQuery Executor for synchronous Queries.
        /// </summary>
        /// <param name="bucket">Specifies the source bucket.</param>
        /// <param name="org">Specifies the source organization.</param>
        /// <param name="queryApi">The underlying API to execute Flux Query.</param>
        /// <param name="memberResolver">Resolver for customized names.</param>
        /// <param name="queryableOptimizerSettings">Settings for a Query optimization</param>
        public InfluxDBQueryExecutor(string bucket, string org, QueryApiSync queryApi,
            IMemberNameResolver memberResolver, QueryableOptimizerSettings queryableOptimizerSettings)
        {
            _bucket = bucket;
            _org = org;
            _queryApiSync = queryApi;
            _memberResolver = memberResolver;
            _queryableOptimizerSettings = queryableOptimizerSettings;
        }

        /// <summary>
        /// Create InfluxDBQuery Executor for asynchronous Queries.
        /// </summary>
        /// <param name="bucket">Specifies the source bucket.</param>
        /// <param name="org">Specifies the source organization.</param>
        /// <param name="queryApi">The underlying API to execute Flux Query.</param>
        /// <param name="memberResolver">Resolver for customized names.</param>
        /// <param name="queryableOptimizerSettings">Settings for a Query optimization</param>
        public InfluxDBQueryExecutor(string bucket, string org, QueryApi queryApi,
            IMemberNameResolver memberResolver, QueryableOptimizerSettings queryableOptimizerSettings)
        {
            _bucket = bucket;
            _org = org;
            _queryApi = queryApi;
            _memberResolver = memberResolver;
            _queryableOptimizerSettings = queryableOptimizerSettings;
        }

        /// <summary>
        /// Executes the given <paramref name="queryModel" /> as a scalar query,
        /// i.e. a query that ends with a aggregation operator such as Count, Sum, or Average.
        /// </summary>
        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            return ExecuteSingle<T>(queryModel, false);
        }

        /// <summary>
        /// Executes the given <paramref name="queryModel" /> as a scalar query,
        /// i.e. a query that ends with a result operator such as First, Last, Single, Min, or Max.
        /// </summary>
        public T ExecuteSingle<T>(QueryModel queryModel, bool returnDefaultWhenEmpty)
        {
            return returnDefaultWhenEmpty
                ? ExecuteCollection<T>(queryModel).SingleOrDefault()
                : ExecuteCollection<T>(queryModel).Single();
        }

        /// <summary>
        /// Executes a query with a collection result.
        /// </summary>
        public IEnumerable<T> ExecuteCollection<T>(QueryModel queryModel)
        {
            var query = GenerateQuery(queryModel, out var queryResultsSettings);

            if (_queryApiSync == null)
            {
                throw new ArgumentException("The 'QueryApiSync' has to be configured for sync queries.");
            }

            if (queryResultsSettings.ScalarAggregated)
            {
                var result = ApplyAggregate<T>(_queryApiSync.QuerySync(query, _org), queryResultsSettings);

                return new List<T> { result };
            }

            return _queryApiSync.QuerySync<T>(query, _org);
        }

        /// <summary>
        /// Executes an async query with a collection result.
        /// </summary>
        public IAsyncEnumerable<T> ExecuteCollectionAsync<T>(QueryModel queryModel,
            CancellationToken cancellationToken = new CancellationToken())
        {
            var query = GenerateQuery(queryModel, out var queryResultsSettings);

            if (_queryApi == null)
            {
                throw new ArgumentException("The 'QueryApi' has to be configured for Async queries.");
            }

            if (queryResultsSettings.ScalarAggregated)
            {
                return AggregateAsync<T>(_queryApi.QueryAsync(query, _org), queryResultsSettings, cancellationToken);
            }

            return _queryApi.QueryAsyncEnumerable<T>(query, _org, cancellationToken);
        }

        /// <summary>
        /// Create a <see cref="Api.Domain.Query"/> object that will be used for Querying.
        /// </summary>
        /// <param name="queryModel">Expression Tree of LINQ Query</param>
        /// <param name="settings">Defines how to handle query results</param>
        /// <returns>Query to Invoke</returns>
        internal Query GenerateQuery(QueryModel queryModel, out QueryResultsSettings settings)
        {
            var visitor = QueryVisitor(queryModel);

            settings = new QueryResultsSettings(queryModel);
            return visitor.GenerateQuery();
        }

        /// <summary>
        /// Create QueryVisitor for specified model.
        /// </summary>
        /// <param name="queryModel">Query Model</param>
        /// <returns>Query Visitor</returns>
        internal InfluxDBQueryVisitor QueryVisitor(QueryModel queryModel)
        {
            var visitor = new InfluxDBQueryVisitor(_bucket, _memberResolver, _queryableOptimizerSettings);
            visitor.VisitQueryModel(queryModel);
            return visitor;
        }

        private async IAsyncEnumerable<T> AggregateAsync<T>(Task<List<FluxTable>> tables,
            QueryResultsSettings queryResultsSettings,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            var result = tables
                .ContinueWith(t => ApplyAggregate<T>(t.Result, queryResultsSettings), cancellationToken);

            yield return await result.ConfigureAwait(false);
        }

        private static T ApplyAggregate<T>(IEnumerable<FluxTable> tables, QueryResultsSettings queryResultsSettings)
        {
            var enumerable = tables
                .SelectMany(it => it.Records)
                .Select(it => it.GetValueByKey("linq_result_column"));

            var aggregated = queryResultsSettings.AggregateFunction(enumerable);

            return (T)Convert.ChangeType(aggregated, typeof(T));
        }
    }
}