using System;
using System.Collections.Generic;
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
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client
{
    public class QueryApi : AbstractQueryClient
    {
        internal static readonly Dialect DefaultDialect = new Dialect
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

        protected internal QueryApi(InfluxDBClientOptions options, QueryService service, IFluxResultMapper mapper) : base(service.Configuration
            .ApiClient.RestClient, mapper)
        {
            Arguments.CheckNotNull(options, nameof(options));
            Arguments.CheckNotNull(service, nameof(service));

            _options = options;
            _service = service;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to <see cref="FluxTable"/>s.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync(string,string,System.Action{InfluxDB.Client.Core.ICancellable,InfluxDB.Client.Core.Flux.Domain.FluxRecord},System.Action{System.Exception}, CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>FluxTables that are matched the query</returns>
        public Task<List<FluxTable>> QueryAsync(string query, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QueryAsync(query, _options.Org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to <see cref="FluxTable"/>s.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync(string,string,System.Action{InfluxDB.Client.Core.ICancellable,InfluxDB.Client.Core.Flux.Domain.FluxRecord},System.Action{System.Exception}, CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>FluxTables that are matched the query</returns>
        public Task<List<FluxTable>> QueryAsync(string query, string org, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return QueryAsync(CreateQuery(query, DefaultDialect), org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to <see cref="FluxTable"/>s.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync(string,string,System.Action{InfluxDB.Client.Core.ICancellable,InfluxDB.Client.Core.Flux.Domain.FluxRecord},System.Action{System.Exception}, CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>FluxTables that are matched the query</returns>
        public Task<List<FluxTable>> QueryAsync(Query query, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));

            return QueryAsync(query, _options.Org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to <see cref="FluxTable"/>s.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync(string,string,System.Action{InfluxDB.Client.Core.ICancellable,InfluxDB.Client.Core.Flux.Domain.FluxRecord},System.Action{System.Exception}, CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>FluxTables that are matched the query</returns>
        public async Task<List<FluxTable>> QueryAsync(Query query, string org, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            var consumer = new FluxCsvParser.FluxResponseConsumerTable();

            await QueryAsync(query, org, consumer, ErrorConsumer, EmptyAction, cancellationToken).ConfigureAwait(false);

            return consumer.Tables;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to list of object with given type.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync{T}(string,string,System.Action{InfluxDB.Client.Core.ICancellable,T},System.Action{System.Exception},System.Action,CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public Task<List<T>> QueryAsync<T>(string query, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QueryAsync<T>(query, _options.Org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to list of object with given type.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync{T}(string,string,System.Action{InfluxDB.Client.Core.ICancellable,T},System.Action{System.Exception},System.Action,CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public Task<List<T>> QueryAsync<T>(string query, string org, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return QueryAsync<T>(CreateQuery(query), org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously maps
        /// response to enumerable of objects of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public async IAsyncEnumerable<T> QueryAsyncEnumerable<T>(string query, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            var requestMessage = CreateRequest(CreateQuery(query), _options.Org);

            await foreach (var record in QueryEnumerable<T>(requestMessage, cancellationToken).ConfigureAwait(false))
                yield return record;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously maps
        /// response to enumerable of objects of type <typeparamref name="T"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public async IAsyncEnumerable<T> QueryAsyncEnumerable<T>(string query, string org, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            var requestMessage = CreateRequest(CreateQuery(query), org);

            await foreach (var record in QueryEnumerable<T>(requestMessage, cancellationToken).ConfigureAwait(false))
                yield return record;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to list of object with given type.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync{T}(string,string,System.Action{InfluxDB.Client.Core.ICancellable,T},System.Action{System.Exception},System.Action,CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public Task<List<T>> QueryAsync<T>(Query query, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));

            return QueryAsync<T>(query, _options.Org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to list of object with given type.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync{T}(string,string,System.Action{InfluxDB.Client.Core.ICancellable,T},System.Action{System.Exception},System.Action,CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public async Task<List<T>> QueryAsync<T>(Query query, string org, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            var measurements = new List<T>();

            var consumer = new FluxResponseConsumerPoco<T>((cancellable, poco) => { measurements.Add(poco); }, Mapper);

            await QueryAsync(query, org, consumer, ErrorConsumer, EmptyAction, cancellationToken).ConfigureAwait(false);

            return measurements;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability
        /// to discontinue a streaming query</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns>async task</returns>
        public Task QueryAsync(string query, Action<ICancellable, FluxRecord> onNext, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            return QueryAsync(query, _options.Org, onNext, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability
        /// to discontinue a streaming query</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns>async task</returns>
        public Task QueryAsync(string query, string org, Action<ICancellable, FluxRecord> onNext, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            return QueryAsync(query, org, onNext, ErrorConsumer, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability
        /// to discontinue a streaming query</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns>async task</returns>
        public Task QueryAsync(Query query, Action<ICancellable, FluxRecord> onNext, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            return QueryAsync(query, _options.Org, onNext, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability
        /// to discontinue a streaming query</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns>async task</returns>
        public Task QueryAsync(Query query, string org, Action<ICancellable, FluxRecord> onNext, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            return QueryAsync(query, org, onNext, ErrorConsumer, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(string query, Action<ICancellable, T> onNext, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            return QueryAsync(query, _options.Org, onNext, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(string query, string org, Action<ICancellable, T> onNext, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            return QueryAsync(query, org, onNext, ErrorConsumer, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(Query query, Action<ICancellable, T> onNext, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            return QueryAsync(query, _options.Org, onNext, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(Query query, string org, Action<ICancellable, T> onNext, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            return QueryAsync(query, org, onNext, ErrorConsumer, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns>async task</returns>
        public Task QueryAsync(string query, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            return QueryAsync(query, _options.Org, onNext, onError, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns>async task</returns>
        public Task QueryAsync(string query, string org, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            return QueryAsync(query, org, onNext, onError, EmptyAction, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns>async task</returns>
        public Task QueryAsync(Query query, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            return QueryAsync(query, _options.Org, onNext, onError, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="cancellationToken"></param>
        /// <returns>async task</returns>
        public Task QueryAsync(Query query, string org, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            return QueryAsync(query, org, onNext, onError, EmptyAction, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(string query, Action<ICancellable, T> onNext,
            Action<Exception> onError, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            return QueryAsync(query, _options.Org, onNext, onError, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(string query, string org, Action<ICancellable, T> onNext,
            Action<Exception> onError, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            return QueryAsync(query, org, onNext, onError, EmptyAction, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(Query query, Action<ICancellable, T> onNext,
            Action<Exception> onError, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            return QueryAsync(query, _options.Org, onNext, onError, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(Query query, string org, Action<ICancellable, T> onNext,
            Action<Exception> onError, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            return QueryAsync(query, org, onNext, onError, EmptyAction, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns>async task</returns>
        public Task QueryAsync(string query, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError,
            Action onComplete,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            return QueryAsync(query, _options.Org, onNext, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns>async task</returns>
        public Task QueryAsync(string query, string org, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError,
            Action onComplete, 
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var consumer = new FluxResponseConsumerRecord(onNext);

            return QueryAsync(CreateQuery(query, DefaultDialect), org, consumer, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns>async task</returns>
        public Task QueryAsync(Query query, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError,
            Action onComplete, 
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            return QueryAsync(query, _options.Org, onNext, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns>async task</returns>
        public Task QueryAsync(Query query, string org, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError,
            Action onComplete, 
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var consumer = new FluxResponseConsumerRecord(onNext);

            return QueryAsync(query, org, consumer, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(string query, Action<ICancellable, T> onNext,
            Action<Exception> onError,
            Action onComplete, 
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            return QueryAsync(query, _options.Org, onNext, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(string query, string org, Action<ICancellable, T> onNext,
            Action<Exception> onError,
            Action onComplete, 
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var consumer = new FluxResponseConsumerPoco<T>(onNext, Mapper);

            return QueryAsync(CreateQuery(query, DefaultDialect), org, consumer, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(Query query, Action<ICancellable, T> onNext,
            Action<Exception> onError,
            Action onComplete, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            return QueryAsync(query, _options.Org, onNext, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(Query query, string org, Action<ICancellable, T> onNext,
            Action<Exception> onError,
            Action onComplete, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var consumer = new FluxResponseConsumerPoco<T>(onNext, Mapper);

            return QueryAsync(query, org, consumer, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryRawAsync(string,string,System.Action{InfluxDB.Client.Core.ICancellable,string},System.Action{System.Exception},System.Action,CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// 
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>the raw response that matched the query</returns>
        public Task<string> QueryRawAsync(string query, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QueryRawAsync(query, _options.Org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryRawAsync(string,string,System.Action{InfluxDB.Client.Core.ICancellable,string},System.Action{System.Exception},System.Action,CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// 
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>the raw response that matched the query</returns>
        public Task<string> QueryRawAsync(string query, string org, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return QueryRawAsync(query, org, DefaultDialect, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryRawAsync(string,string,System.Action{InfluxDB.Client.Core.ICancellable,string},System.Action{System.Exception},System.Action,CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// 
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>the raw response that matched the query</returns>
        public Task<string> QueryRawAsync(Query query, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));

            return QueryRawAsync(query, _options.Org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryRawAsync(string,string,System.Action{InfluxDB.Client.Core.ICancellable,string},System.Action{System.Exception},System.Action,CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// 
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>the raw response that matched the query</returns>
        public async Task<string> QueryRawAsync(Query query, string org, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            var rows = new List<string>();

            void Consumer(ICancellable cancellable, string row) => rows.Add(row);

            await QueryRawAsync(query, org, Consumer, ErrorConsumer, EmptyAction, cancellationToken).ConfigureAwait(false);

            return string.Join("\n", rows);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryRawAsync(string,string,System.Action{InfluxDB.Client.Core.ICancellable,string},System.Action{System.Exception},System.Action,CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// 
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>the raw response that matched the query</returns>
        public Task<string> QueryRawAsync(string query, Dialect dialect, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QueryRawAsync(query, _options.Org, dialect, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        /// 
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryRawAsync(string,string,System.Action{InfluxDB.Client.Core.ICancellable,string},System.Action{System.Exception},System.Action,CancellationToken)"/>
        /// for large data streaming.
        /// </para>
        /// 
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="cancellationToken">cancellation token</param>
        /// <returns>the raw response that matched the query</returns>
        public Task<string> QueryRawAsync(string query, string org, Dialect dialect, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return QueryRawAsync(CreateQuery(query, dialect), org, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(string query, Action<ICancellable, string> onResponse, 
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QueryRawAsync(query, _options.Org, onResponse, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(string query, string org, Action<ICancellable, string> onResponse, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return QueryRawAsync(query, org, onResponse, ErrorConsumer, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(Query query, Action<ICancellable, string> onResponse, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));

            return QueryRawAsync(query, _options.Org, onResponse, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(Query query, string org, Action<ICancellable, string> onResponse, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return QueryRawAsync(query, org, onResponse, ErrorConsumer, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(string query, Dialect dialect, Action<ICancellable, string> onResponse, 
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QueryRawAsync(query, _options.Org, dialect, onResponse, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(string query, string org, Dialect dialect,
            Action<ICancellable, string> onResponse, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return QueryRawAsync(query, org, dialect, onResponse, ErrorConsumer, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(string query, Action<ICancellable, string> onResponse,
            Action<Exception> onError, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QueryRawAsync(query, _options.Org, null, onResponse, onError, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(string query, string org, Action<ICancellable, string> onResponse,
            Action<Exception> onError, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return QueryRawAsync(query, org, null, onResponse, onError, EmptyAction, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(Query query, Action<ICancellable, string> onResponse,
            Action<Exception> onError, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));

            return QueryRawAsync(query, _options.Org, onResponse, onError, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(Query query, string org, Action<ICancellable, string> onResponse,
            Action<Exception> onError, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return QueryRawAsync(query, org, onResponse, onError, EmptyAction, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(string query, Dialect dialect,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QueryRawAsync(query, _options.Org, dialect, onResponse, onError, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(string query, string org, Dialect dialect,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return QueryRawAsync(query, org, dialect, onResponse, onError, EmptyAction, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="onComplete">callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(string query, Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return QueryRawAsync(query, _options.Org, onResponse, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="onComplete">callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(string query, string org, Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return QueryRawAsync(query, org, null, onResponse, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="onComplete">callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(Query query, Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));

            return QueryRawAsync(query, _options.Org, onResponse, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="onComplete">callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(Query query, string org, Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            var requestMessage = CreateRequest(query, org);

            return QueryRaw(requestMessage, onResponse, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="onComplete">callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(string query, Dialect dialect,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete, 
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onResponse, nameof(onResponse));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            return QueryRawAsync(query, _options.Org, dialect, onResponse, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="onComplete">callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">cancellation token, useful for cancellation a request before parsing response from server</param>
        /// <returns></returns>
        public Task QueryRawAsync(string query, string org, Dialect dialect,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete, 
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(onResponse, nameof(onResponse));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            return QueryRawAsync(CreateQuery(query, dialect), org, onResponse, onError, onComplete, cancellationToken);
        }

        protected override void BeforeIntercept(RestRequest request)
        {
            _service.Configuration.ApiClient.BeforeIntercept(request);
        }

        protected override T AfterIntercept<T>(int statusCode, Func<IList<HttpHeader>> headers, T body)
        {
            return _service.Configuration.ApiClient.AfterIntercept(statusCode, headers, body);
        }

        private Task QueryAsync(Query query, string org, FluxCsvParser.IFluxResponseConsumer consumer,
            Action<Exception> onError,
            Action onComplete, 
            CancellationToken cancellationToken = default)

        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(consumer, nameof(consumer));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var requestMessage = CreateRequest(query, org);

            return Query(requestMessage, consumer, onError, onComplete, cancellationToken);
        }

        private RestRequest CreateRequest(Query query, string org)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            var request = _service.PostQueryWithRestRequest(null, "application/json", null, org, null, query);
            return request;
        }

        internal static Query CreateQuery(string query, Dialect dialect = null)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            var created = new Query(null, query);
            created.Dialect = dialect ?? DefaultDialect;

            return created;
        }
    }
}