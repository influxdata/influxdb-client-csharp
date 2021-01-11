using System.Linq;
using System.Linq.Expressions;
using Remotion.Linq;
using Remotion.Linq.Parsing.Structure;

namespace InfluxDB.Client.Linq
{
    /// <summary>
    /// Main entry point to query InfluxDB by LINQ
    /// </summary>
    public class InfluxDBQueryable<T> : QueryableBase<T>
    {
        /// <summary>
        /// Create a new instance of IQueryable.
        /// </summary>
        /// <param name="queryApi">The underlying API to execute Flux Query.</param>
        /// <param name="org">Specifies the source organization.</param>
        /// <param name="bucket">Specifies the source bucket.</param>
        public InfluxDBQueryable(QueryApi queryApi, string org, string bucket) : base(CreateQueryParser(),
            CreateExecutor(queryApi, org, bucket))
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

        private static IQueryExecutor CreateExecutor(QueryApi queryApi, string org, string bucket)
        {
            return new InfluxDBQueryExecutor(queryApi, org, bucket);
        }

        private static QueryParser CreateQueryParser()
        {
            return QueryParser.CreateDefault();
        }
    }
}