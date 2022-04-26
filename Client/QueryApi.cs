using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Internal;
using InfluxDB.Client.Core.Internal;
using RestSharp;

namespace InfluxDB.Client
{
    public class QueryApi : AbstractQueryClient
    {
        public static readonly Dialect Dialect = new Dialect
        {
            Header = true,
            Delimiter = ",",
            CommentPrefix = "#",
            Annotations = new List<Dialect.AnnotationsEnum>
            {
                Dialect.AnnotationsEnum.Datatype,
                Dialect.AnnotationsEnum.Group,
                Dialect.AnnotationsEnum.Default
            }
        };

        private readonly InfluxDBClientOptions _options;
        private readonly QueryService _service;

        protected internal QueryApi(InfluxDBClientOptions options, QueryService service, IFluxResultMapper mapper) :
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
        /// to <see cref="FluxTable"/>s.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync(string,System.Action{InfluxDB.Client.Core.Flux.Domain.FluxRecord},System.Action{System.Exception},System.Action,string,System.Threading.CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>FluxTables that are matched the query</returns>
        public Task<List<FluxTable>> QueryAsync(string query, string org = null,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QueryAsync(CreateQuery(query, Dialect), org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and synchronously map whole response
        /// to <see cref="FluxTable"/>s.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync(string,System.Action{InfluxDB.Client.Core.Flux.Domain.FluxRecord},System.Action{System.Exception},System.Action,string,System.Threading.CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>FluxTables that are matched the query</returns>
        public async Task<List<FluxTable>> QueryAsync(Query query, string org = null,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));

            var consumer = new FluxCsvParser.FluxResponseConsumerTable();

            await QueryAsync(query, consumer, ErrorConsumer, EmptyAction, org, cancellationToken)
                .ConfigureAwait(false);

            return consumer.Tables;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and synchronously map whole response
        /// to list of object with given type.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync{T}(string,System.Action{T},System.Action{System.Exception},System.Action,string,System.Threading.CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public Task<List<T>> QueryAsync<T>(string query, string org = null,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QueryAsync<T>(CreateQuery(query), org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and synchronously map whole response
        /// to list of object with given type.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync{T}(string,System.Action{T},System.Action{System.Exception},System.Action,string,System.Threading.CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public async Task<List<T>> QueryAsync<T>(Query query, string org = null,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));

            var measurements = new List<T>();

            var consumer = new FluxResponseConsumerPoco<T>(poco => { measurements.Add(poco); }, Mapper);

            await QueryAsync(query, consumer, ErrorConsumer, EmptyAction, org, cancellationToken)
                .ConfigureAwait(false);

            return measurements;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and asynchronously maps
        /// response to enumerable of objects of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public async IAsyncEnumerable<T> QueryAsyncEnumerable<T>(string query, string org = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            var requestMessage = CreateRequest(CreateQuery(query), org);

            await foreach (var record in QueryEnumerable(requestMessage, it => Mapper.ConvertToEntity<T>(it),
                               cancellationToken).ConfigureAwait(false))
                yield return record;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and asynchronously maps
        /// response to enumerable of objects of type <typeparamref name="T"/>. 
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public async IAsyncEnumerable<T> QueryAsyncEnumerable<T>(Query query, string org = null,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));

            var requestMessage = CreateRequest(query, org);

            await foreach (var record in QueryEnumerable(requestMessage, it => Mapper.ConvertToEntity<T>(it),
                               cancellationToken).ConfigureAwait(false))
                yield return record;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>async task</returns>
        public Task QueryAsync(string query, Action<FluxRecord> onNext, Action<Exception> onError = null,
            Action onComplete = null, string org = null, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            var consumer = new FluxResponseConsumerRecord(onNext);

            return QueryAsync(CreateQuery(query, Dialect), consumer, onError, onComplete, org,
                cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>async task</returns>
        public Task QueryAsync(Query query, Action<FluxRecord> onNext, Action<Exception> onError = null,
            Action onComplete = null, string org = null, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            var consumer = new FluxResponseConsumerRecord(onNext);

            return QueryAsync(query, consumer, onError, onComplete, org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the mapped Measurements</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(string query, Action<T> onNext, Action<Exception> onError = null,
            Action onComplete = null, string org = null, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            var consumer = new FluxResponseConsumerPoco<T>(onNext, Mapper);

            return QueryAsync(CreateQuery(query, Dialect), consumer, onError, onComplete, org,
                cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the mapped Measurements</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(Query query, Action<T> onNext, Action<Exception> onError = null,
            Action onComplete = null, string org = null, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            var consumer = new FluxResponseConsumerPoco<T>(onNext, Mapper);

            return QueryAsync(query, consumer, onError, onComplete, org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and synchronously map whole response
        /// to list of object with given type.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync(string,System.Type,System.Action{object},System.Action{System.Exception},System.Action,string,System.Threading.CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="pocoType">the type of measurement</param>
        /// <param name="org">the organization</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>Measurements which are matched the query</returns>
        public Task<List<object>> QueryAsync(string query, Type pocoType, string org = null,
            CancellationToken cancellationToken = default)
        {
            return QueryAsync(CreateQuery(query), pocoType, org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and synchronously map whole response
        /// to list of object with given type.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync(string,System.Type,System.Action{object},System.Action{System.Exception},System.Action,string,System.Threading.CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="pocoType">the type of measurement</param>
        /// <param name="org">the organization</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>Measurements which are matched the query</returns>
        public async Task<List<object>> QueryAsync(Query query, Type pocoType, string org = null,
            CancellationToken cancellationToken = default)
        {
            var measurements = new List<object>();
            var consumer = new FluxResponseConsumerPoco(poco => { measurements.Add(poco); }, Mapper, pocoType);
            await QueryAsync(query, consumer, ErrorConsumer, EmptyAction, org, cancellationToken)
                .ConfigureAwait(false);

            return measurements;
        }


        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="pocoType">the type of measurement</param>
        /// <param name="onNext">the callback to consume the mapped Measurements</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>async task</returns>
        public Task QueryAsync(string query, Type pocoType, Action<object> onNext,
            Action<Exception> onError = null, Action onComplete = null, string org = null,
            CancellationToken cancellationToken = default)
        {
            return QueryAsync(CreateQuery(query, Dialect), pocoType,
                onNext, onError, onComplete, org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="pocoType">the type of measurement</param>
        /// <param name="onNext">the callback to consume the mapped Measurements</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>async task</returns>
        public Task QueryAsync(Query query, Type pocoType, Action<object> onNext,
            Action<Exception> onError = null, Action onComplete = null, string org = null,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var consumer = new FluxResponseConsumerPoco(onNext, Mapper, pocoType);

            return QueryAsync(query, consumer, onError, onComplete, org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryRawAsync(string,System.Action{string},InfluxDB.Client.Api.Domain.Dialect,System.Action{System.Exception},System.Action,string,System.Threading.CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// 
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response. <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>the raw response that matched the query</returns>
        public Task<string> QueryRawAsync(string query, Dialect dialect = null, string org = null,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QueryRawAsync(CreateQuery(query, dialect), org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryRawAsync(InfluxDB.Client.Api.Domain.Query,System.Action{string},System.Action{System.Exception},System.Action,string,System.Threading.CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// 
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>the raw response that matched the query</returns>
        public async Task<string> QueryRawAsync(Query query, string org = null,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));

            var rows = new List<string>();

            void Consumer(string row)
            {
                rows.Add(row);
            }

            await QueryRawAsync(query, Consumer, ErrorConsumer, EmptyAction, org, cancellationToken)
                .ConfigureAwait(false);

            return string.Join("\n", rows);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response. <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="onComplete">callback to consume a notification about successfully end of stream</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns></returns>
        public Task QueryRawAsync(string query, Action<string> onResponse, Dialect dialect = null,
            Action<Exception> onError = null, Action onComplete = null, string org = null,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onResponse, nameof(onResponse));

            return QueryRawAsync(CreateQuery(query, dialect), onResponse, onError, onComplete, org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.x and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="onComplete">callback to consume a notification about successfully end of stream</param>
        /// <param name="org">specifies the source organization. If the org is not specified then is used config from <see cref="InfluxDBClientOptions.Org" />.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns></returns>
        public Task QueryRawAsync(Query query, Action<string> onResponse, Action<Exception> onError = null,
            Action onComplete = null, string org = null, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onResponse, nameof(onResponse));

            var requestMessage = CreateRequest(query, org);

            return QueryRaw(requestMessage, onResponse, onError, onComplete, cancellationToken);
        }

        protected override void BeforeIntercept(RestRequest request)
        {
            _service.Configuration.ApiClient.BeforeIntercept(request);
        }

        protected override T AfterIntercept<T>(int statusCode, Func<IEnumerable<HeaderParameter>> headers, T body)
        {
            return _service.Configuration.ApiClient.AfterIntercept(statusCode, headers, body);
        }

        private Task QueryAsync(Query query, FluxCsvParser.IFluxResponseConsumer consumer,
            Action<Exception> onError = null, Action onComplete = null, string org = null,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(consumer, nameof(consumer));

            var requestMessage = CreateRequest(query, org);

            return Query(requestMessage, consumer, onError ?? ErrorConsumer, onComplete ?? EmptyAction,
                cancellationToken);
        }

        private Func<Func<HttpResponseMessage, RestResponse>, RestRequest> CreateRequest(Query query, string org = null)
        {
            Arguments.CheckNotNull(query, nameof(query));

            var optionsOrg = org ?? _options.Org;
            Arguments.CheckNonEmptyString(optionsOrg, OrgArgumentValidation);

            return advancedResponseWriter => _service
                .PostQueryWithRestRequest(null, "application/json", null, optionsOrg, null, query)
                .AddAdvancedResponseHandler(advancedResponseWriter);
        }

        internal static Query CreateQuery(string query, Dialect dialect = null)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            var created = new Query(query: query, dialect: dialect ?? Dialect);

            return created;
        }
    }
}