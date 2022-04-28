using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Internal;
using InfluxDB.Client.Core.Internal;
using RestSharp;

namespace InfluxDB.Client
{
    /// <summary>
    /// The synchronous version of QueryApi.
    /// </summary>
    public class QueryApiSync : AbstractQueryClient
    {
        private readonly InfluxDBClientOptions _options;
        private readonly QueryService _service;

        protected internal QueryApiSync(InfluxDBClientOptions options, QueryService service, IFluxResultMapper mapper) :
            base(mapper)
        {
            Arguments.CheckNotNull(options, nameof(options));
            Arguments.CheckNotNull(service, nameof(service));

            _options = options;
            _service = service;
            RestClient = service.Configuration.ApiClient.RestClient;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and synchronously map whole response
        /// to list of object with given type.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public List<T> QuerySync<T>(string query, string org = null, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QuerySync<T>(QueryApi.CreateQuery(query, QueryApi.Dialect), org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and synchronously map whole response
        /// to list of object with given type.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public List<T> QuerySync<T>(Query query, string org = null, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));

            var optionsOrg = org ?? _options.Org;
            Arguments.CheckNonEmptyString(optionsOrg, OrgArgumentValidation);

            var measurements = new List<T>();

            var consumer = new FluxResponseConsumerPoco<T>(poco => { measurements.Add(poco); }, Mapper);

            RestRequest QueryFn(Func<HttpResponseMessage, RestResponse> advancedResponseWriter)
            {
                return _service
                    .PostQueryWithRestRequest(null, "application/json", null, optionsOrg, null, query)
                    .AddAdvancedResponseHandler(advancedResponseWriter);
            }

            QuerySync(QueryFn, consumer, ErrorConsumer, EmptyAction, cancellationToken);

            return measurements;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and synchronously map whole response
        /// to <see cref="FluxTable"/>s.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>FluxTables that are matched the query</returns>
        public List<FluxTable> QuerySync(string query, string org = null, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QuerySync(QueryApi.CreateQuery(query, QueryApi.Dialect), org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and synchronously map whole response
        /// to <see cref="FluxTable"/>s.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>FluxTables that are matched the query</returns>
        public List<FluxTable> QuerySync(Query query, string org = null, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));

            var consumer = new FluxCsvParser.FluxResponseConsumerTable();

            var optionsOrg = org ?? _options.Org;
            Arguments.CheckNonEmptyString(optionsOrg, OrgArgumentValidation);

            RestRequest QueryFn(Func<HttpResponseMessage, RestResponse> advancedResponseWriter)
            {
                return _service
                    .PostQueryWithRestRequest(null, "application/json", null, optionsOrg, null, query)
                    .AddAdvancedResponseHandler(advancedResponseWriter);
            }

            QuerySync(QueryFn, consumer, ErrorConsumer, EmptyAction, cancellationToken);

            return consumer.Tables;
        }

        protected override void BeforeIntercept(RestRequest request)
        {
            _service.Configuration.ApiClient.BeforeIntercept(request);
        }

        protected override T AfterIntercept<T>(int statusCode, Func<IEnumerable<HeaderParameter>> headers, T body)
        {
            return _service.Configuration.ApiClient.AfterIntercept(statusCode, headers, body);
        }
    }
}