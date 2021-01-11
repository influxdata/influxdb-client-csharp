using System.Collections.Generic;
using System.Linq;
using Remotion.Linq;

namespace InfluxDB.Client.Linq
{
    /// <summary>
    /// Executor is called by ReLinq when query is executed.
    /// </summary>
    public class InfluxDBQueryExecutor : IQueryExecutor
    {
        private readonly QueryApi _queryApi;
        private readonly string _org;
        private readonly string _bucket;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="queryApi">The underlying API to execute Flux Query.</param>
        /// <param name="org">Specifies the source organization.</param>
        /// <param name="bucket">Specifies the source bucket.</param>
        public InfluxDBQueryExecutor(QueryApi queryApi, string org, string bucket)
        {
            _queryApi = queryApi;
            _org = org;
            _bucket = bucket;
        }

        /// <summary>
        /// Executes the given <paramref name="queryModel" /> as a scalar query,
        /// i.e. a query that ends with a aggregation operator such as Count, Sum, or Average.
        /// </summary>
        public T ExecuteScalar<T>(QueryModel queryModel)
        {
            return ExecuteCollection<T>(queryModel).Single();
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
            var visitor = new InfluxDBQueryVisitor(_bucket);
            visitor.VisitQueryModel(queryModel);

            var task = _queryApi.QueryAsync<T>(visitor.GenerateQuery(), _org);
            task.RunSynchronously();

            return task.Result;
        }
    }
}