using System;
using System.Linq;
using System.Linq.Expressions;
using InfluxDB.Client.Core;
using InfluxDB.Client.Linq.Internal;
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
        /// <param name="bucket">Specifies the source bucket.</param>
        /// <param name="org">Specifies the source organization.</param>
        /// <param name="queryApi">The underlying API to execute Flux Query.</param>
        /// <returns>new instance for of Queryable</returns>
        public static InfluxDBQueryable<T> Queryable(string bucket, string org, QueryApiSync queryApi)
        {
            return Queryable(bucket, org, queryApi, new DefaultMemberNameResolver());
        }
        
        /// <summary>
        /// Create a new instance of IQueryable.
        /// </summary>
        /// <param name="bucket">Specifies the source bucket.</param>
        /// <param name="org">Specifies the source organization.</param>
        /// <param name="queryApi">The underlying API to execute Flux Query.</param>
        /// <param name="memberResolver">Resolver for customized names.</param>
        /// <returns>new instance for of Queryable</returns>
        public static InfluxDBQueryable<T> Queryable(string bucket, string org, QueryApiSync queryApi, IMemberNameResolver memberResolver)
        {
            return new InfluxDBQueryable<T>(bucket, org, queryApi, memberResolver);
        }
        
        /// <summary>
        /// Create a new instance of IQueryable.
        /// </summary>
        /// <param name="bucket">Specifies the source bucket.</param>
        /// <param name="org">Specifies the source organization.</param>
        /// <param name="queryApi">The underlying API to execute Flux Query.</param>
        /// <param name="memberResolver">Resolver for customized names.</param>
        public InfluxDBQueryable(string bucket, string org, QueryApiSync queryApi, IMemberNameResolver memberResolver) : base(CreateQueryParser(),
            CreateExecutor(bucket, org, queryApi, memberResolver))
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
                throw new NotSupportedException("InfluxDBQueryable should use InfluxDBQueryExecutor");

            var parsedQuery = provider.QueryParser.GetParsedQuery(Expression);
            var generateQuery = executor.GenerateQuery(parsedQuery, out _);

            return generateQuery;
        }

        private static IQueryExecutor CreateExecutor(string bucket, string org, QueryApiSync queryApi, IMemberNameResolver memberResolver)
        {
            Arguments.CheckNonEmptyString(bucket, nameof(bucket));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(queryApi, nameof(queryApi));
                
            return new InfluxDBQueryExecutor(bucket, org, queryApi, memberResolver);
        }

        private static QueryParser CreateQueryParser()
        {
            return QueryParser.CreateDefault();
        }
    }
}