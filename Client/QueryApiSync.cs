using System.Collections.Generic;
using System.Threading;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Api;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Internal;
using InfluxDB.Client.Core.Internal;

namespace InfluxDB.Client
{
    /// <summary>
    /// The synchronous version of QueryApi.
    /// </summary>
    public class QueryApiSync: AbstractQueryClient
    {
        private readonly InfluxDBClientOptions _options;
        private readonly QueryService _service;

        protected internal QueryApiSync(InfluxDBClientOptions options, QueryService service, ApiClient apiClient, ExceptionFactory exceptionFactory, IFluxResultMapper mapper) :
            base(apiClient, exceptionFactory, mapper)
        {
            Arguments.CheckNotNull(options, nameof(options));
            Arguments.CheckNotNull(service, nameof(service));

            _options = options;
            _service = service;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to list of object with given type.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public List<T> QuerySync<T>(string query, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QuerySync<T>(query, _options.Org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to list of object with given type.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public List<T> QuerySync<T>(string query, string org, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return QuerySync<T>(QueryApi.CreateQuery(query, QueryApi.DefaultDialect), org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to list of object with given type.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public List<T> QuerySync<T>(Query query, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));

            return QuerySync<T>(query, _options.Org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to list of object with given type.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public List<T> QuerySync<T>(Query query, string org, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            var measurements = new List<T>();

            var consumer = new FluxResponseConsumerPoco<T>(poco => { measurements.Add(poco); }, Mapper);

            var requestMessage = _service.PostQueryWithRequestOptions(null, "application/json", null, org, null, query);

            QuerySync(requestMessage, consumer, ErrorConsumer, EmptyAction, cancellationToken);

            return measurements;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to <see cref="FluxTable"/>s.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>FluxTables that are matched the query</returns>
        public List<FluxTable> QuerySync(string query, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QuerySync(query, _options.Org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to <see cref="FluxTable"/>s.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>FluxTables that are matched the query</returns>
        public List<FluxTable> QuerySync(string query, string org, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return QuerySync(QueryApi.CreateQuery(query, QueryApi.DefaultDialect), org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to <see cref="FluxTable"/>s.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>FluxTables that are matched the query</returns>
        public List<FluxTable> QuerySync(Query query, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));

            return QuerySync(query, _options.Org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to <see cref="FluxTable"/>s.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>FluxTables that are matched the query</returns>
        public List<FluxTable> QuerySync(Query query, string org, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            var consumer = new FluxCsvParser.FluxResponseConsumerTable();
            
            var requestMessage = _service.PostQueryWithRequestOptions(null, "application/json", null, org, null, query);

            QuerySync(requestMessage, consumer, ErrorConsumer, EmptyAction, cancellationToken);

            return consumer.Tables;
        }
    }
}