using System;
using System.Collections.Generic;
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
        private readonly Dialect _defaultDialect;

        private readonly InfluxDBClientOptions _options;
        private readonly QueryService _service;

        protected internal QueryApi(InfluxDBClientOptions options, QueryService service) : base(service.Configuration
            .ApiClient.RestClient)
        {
            Arguments.CheckNotNull(options, nameof(options));
            Arguments.CheckNotNull(service, nameof(service));

            _options = options;
            _service = service;
            _defaultDialect = new Dialect
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
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to <see cref="FluxTable"/>s.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="Query(string,string,System.Action{ICancellable,FluxRecord},System.Action{System.Exception}"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <returns>FluxTables that are matched the query</returns>
        public async Task<List<FluxTable>> Query(string query)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return await Query(query, _options.Org);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to <see cref="FluxTable"/>s.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="Query(string,string,System.Action{ICancellable,FluxRecord},System.Action{System.Exception}"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <returns>FluxTables that are matched the query</returns>
        public async Task<List<FluxTable>> Query(string query, string org)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return await Query(CreateQuery(query, _defaultDialect), org);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to <see cref="FluxTable"/>s.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="Query(string,string,System.Action{ICancellable,FluxRecord},System.Action{System.Exception}"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <returns>FluxTables that are matched the query</returns>
        public async Task<List<FluxTable>> Query(Query query)
        {
            Arguments.CheckNotNull(query, nameof(query));

            return await Query(query, _options.Org);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to <see cref="FluxTable"/>s.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="Query(string,string,System.Action{ICancellable,FluxRecord},System.Action{System.Exception}"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <returns>FluxTables that are matched the query</returns>
        public async Task<List<FluxTable>> Query(Query query, string org)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            var consumer = new FluxCsvParser.FluxResponseConsumerTable();

            await Query(query, org, consumer, ErrorConsumer, EmptyAction);

            return consumer.Tables;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to list of object with given type.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="Query{T}(string, string, System.Action{ICancellable,T},System.Action{System.Exception},Action)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public async Task<List<T>> Query<T>(string query)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return await Query<T>(query, _options.Org);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to list of object with given type.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="Query{T}(string, string, System.Action{ICancellable,T},System.Action{System.Exception},Action)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public async Task<List<T>> Query<T>(string query, string org)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return await Query<T>(CreateQuery(query), org);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to list of object with given type.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="Query{T}(string, string, System.Action{ICancellable,T},System.Action{System.Exception},Action)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public async Task<List<T>> Query<T>(Query query)
        {
            Arguments.CheckNotNull(query, nameof(query));

            return await Query<T>(query, _options.Org);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and synchronously map whole response
        /// to list of object with given type.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="Query{T}(string, string, System.Action{ICancellable,T},System.Action{System.Exception},Action)"/>
        /// for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public async Task<List<T>> Query<T>(Query query, string org)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            var measurements = new List<T>();

            var consumer = new FluxResponseConsumerPoco<T>((cancellable, poco) => { measurements.Add(poco); });

            await Query(query, org, consumer, ErrorConsumer, EmptyAction);

            return measurements;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability
        /// to discontinue a streaming query</param>
        /// <returns>async task</returns>
        public async Task Query(string query, Action<ICancellable, FluxRecord> onNext)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            await Query(query, _options.Org, onNext);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability
        /// to discontinue a streaming query</param>
        /// <returns>async task</returns>
        public async Task Query(string query, string org, Action<ICancellable, FluxRecord> onNext)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            await Query(query, org, onNext, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability
        /// to discontinue a streaming query</param>
        /// <returns>async task</returns>
        public async Task Query(Query query, Action<ICancellable, FluxRecord> onNext)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            await Query(query, _options.Org, onNext);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability
        /// to discontinue a streaming query</param>
        /// <returns>async task</returns>
        public async Task Query(Query query, string org, Action<ICancellable, FluxRecord> onNext)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            await Query(query, org, onNext, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public async Task Query<T>(string query, Action<ICancellable, T> onNext)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            await Query(query, _options.Org, onNext);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public async Task Query<T>(string query, string org, Action<ICancellable, T> onNext)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            await Query(query, org, onNext, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public async Task Query<T>(Query query, Action<ICancellable, T> onNext)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            await Query(query, _options.Org, onNext);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public async Task Query<T>(Query query, string org, Action<ICancellable, T> onNext)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            await Query(query, org, onNext, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <returns>async task</returns>
        public async Task Query(string query, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            await Query(query, _options.Org, onNext, onError);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <returns>async task</returns>
        public async Task Query(string query, string org, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            await Query(query, org, onNext, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <returns>async task</returns>
        public async Task Query(Query query, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            await Query(query, _options.Org, onNext, onError);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <returns>async task</returns>
        public async Task Query(Query query, string org, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            await Query(query, org, onNext, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public async Task Query<T>(string query, Action<ICancellable, T> onNext,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            await Query(query, _options.Org, onNext, onError);
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
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public async Task Query<T>(string query, string org, Action<ICancellable, T> onNext,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            await Query(query, org, onNext, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public async Task Query<T>(Query query, Action<ICancellable, T> onNext,
            Action<Exception> onError)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            await Query(query, _options.Org, onNext, onError);
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
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public async Task Query<T>(Query query, string org, Action<ICancellable, T> onNext,
            Action<Exception> onError)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            await Query(query, org, onNext, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <returns>async task</returns>
        public async Task Query(string query, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            await Query(query, _options.Org, onNext, onError, onComplete);
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
        /// <returns>async task</returns>
        public async Task Query(string query, string org, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var consumer = new FluxResponseConsumerRecord(onNext);

            await Query(CreateQuery(query, _defaultDialect), org, consumer, onError, onComplete);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <returns>async task</returns>
        public async Task Query(Query query, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            await Query(query, _options.Org, onNext, onError, onComplete);
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
        /// <returns>async task</returns>
        public async Task Query(Query query, string org, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var consumer = new FluxResponseConsumerRecord(onNext);

            await Query(query, org, consumer, onError, onComplete);
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
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public async Task Query<T>(string query, Action<ICancellable, T> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            await Query(query, _options.Org, onNext, onError, onComplete);
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
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public async Task Query<T>(string query, string org, Action<ICancellable, T> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var consumer = new FluxResponseConsumerPoco<T>(onNext);

            await Query(CreateQuery(query, _defaultDialect), org, consumer, onError, onComplete);
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
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public async Task Query<T>(Query query, Action<ICancellable, T> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            await Query(query, _options.Org, onNext, onError, onComplete);
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
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public async Task Query<T>(Query query, string org, Action<ICancellable, T> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var consumer = new FluxResponseConsumerPoco<T>(onNext);

            await Query(query, org, consumer, onError, onComplete);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryRaw(string,string,Action{ICancellable, string},Action{Exception},Action)"/>
        /// for large data streaming.
        /// </para>
        /// 
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <returns>the raw response that matched the query</returns>
        public async Task<string> QueryRaw(string query)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return await QueryRaw(query, _options.Org);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryRaw(string,string,Action{ICancellable, string},Action{Exception},Action)"/>
        /// for large data streaming.
        /// </para>
        /// 
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <returns>the raw response that matched the query</returns>
        public async Task<string> QueryRaw(string query, string org)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return await QueryRaw(query, org, _defaultDialect);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryRaw(string,string,Action{ICancellable, string},Action{Exception},Action)"/>
        /// for large data streaming.
        /// </para>
        /// 
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <returns>the raw response that matched the query</returns>
        public async Task<string> QueryRaw(Query query)
        {
            Arguments.CheckNotNull(query, nameof(query));

            return await QueryRaw(query, _options.Org);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryRaw(string,string,Action{ICancellable, string},Action{Exception},Action)"/>
        /// for large data streaming.
        /// </para>
        /// 
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <returns>the raw response that matched the query</returns>
        public async Task<string> QueryRaw(Query query, string org)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            var rows = new List<string>();

            void Consumer(ICancellable cancellable, string row) => rows.Add(row);

            await QueryRaw(query, org, Consumer, ErrorConsumer, EmptyAction);

            return string.Join("\n", rows);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryRaw(string,string,Action{ICancellable, string},Action{Exception},Action)"/>
        /// for large data streaming.
        /// </para>
        /// 
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <returns>the raw response that matched the query</returns>
        public async Task<string> QueryRaw(string query, Dialect dialect)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            return await QueryRaw(query, _options.Org, dialect);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        ///
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryRaw(string,string,Action{ICancellable, string},Action{Exception},Action)"/>
        /// for large data streaming.
        /// </para>
        /// 
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <returns>the raw response that matched the query</returns>
        public async Task<string> QueryRaw(string query, string org, Dialect dialect)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            return await QueryRaw(CreateQuery(query, dialect), org);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <returns></returns>
        public async Task QueryRaw(string query, Action<ICancellable, string> onResponse
        )
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            await QueryRaw(query, _options.Org, onResponse);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <returns></returns>
        public async Task QueryRaw(string query, string org, Action<ICancellable, string> onResponse
        )
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            await QueryRaw(query, org, onResponse, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <returns></returns>
        public async Task QueryRaw(Query query, Action<ICancellable, string> onResponse)
        {
            Arguments.CheckNotNull(query, nameof(query));

            await QueryRaw(query, _options.Org, onResponse);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="org">specifies the source organization</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <returns></returns>
        public async Task QueryRaw(Query query, string org, Action<ICancellable, string> onResponse
        )
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            await QueryRaw(query, org, onResponse, ErrorConsumer);
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
        /// <returns></returns>
        public async Task QueryRaw(string query, Dialect dialect, Action<ICancellable, string> onResponse)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            await QueryRaw(query, _options.Org, dialect, onResponse);
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
        /// <returns></returns>
        public async Task QueryRaw(string query, string org, Dialect dialect,
            Action<ICancellable, string> onResponse
        )
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            await QueryRaw(query, org, dialect, onResponse, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <returns></returns>
        public async Task QueryRaw(string query, Action<ICancellable, string> onResponse,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            await QueryRaw(query, _options.Org, null, onResponse, onError);
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
        /// <returns></returns>
        public async Task QueryRaw(string query, string org, Action<ICancellable, string> onResponse,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            await QueryRaw(query, org, null, onResponse, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <returns></returns>
        public async Task QueryRaw(Query query, Action<ICancellable, string> onResponse,
            Action<Exception> onError)
        {
            Arguments.CheckNotNull(query, nameof(query));

            await QueryRaw(query, _options.Org, onResponse, onError);
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
        /// <returns></returns>
        public async Task QueryRaw(Query query, string org, Action<ICancellable, string> onResponse,
            Action<Exception> onError)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            await QueryRaw(query, org, onResponse, onError, EmptyAction);
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
        /// <returns></returns>
        public async Task QueryRaw(string query, Dialect dialect,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            await QueryRaw(query, _options.Org, dialect, onResponse, onError);
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
        /// <returns></returns>
        public async Task QueryRaw(string query, string org, Dialect dialect,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            await QueryRaw(query, org, dialect, onResponse, onError, EmptyAction);
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
        /// <returns></returns>
        public async Task QueryRaw(string query, Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            await QueryRaw(query, _options.Org, onResponse, onError, onComplete);
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
        /// <returns></returns>
        public async Task QueryRaw(string query, string org, Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            await QueryRaw(query, org, null, onResponse, onError, onComplete);
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
        /// <returns></returns>
        public async Task QueryRaw(Query query, Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNotNull(query, nameof(query));


            await QueryRaw(query, _options.Org, onResponse, onError, onComplete);
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
        /// <returns></returns>
        public async Task QueryRaw(Query query, string org, Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            var requestMessage = CreateRequest(query, org);

            await QueryRaw(requestMessage, onResponse, onError, onComplete);
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
        /// <returns></returns>
        public async Task QueryRaw(string query, Dialect dialect,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onResponse, nameof(onResponse));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            await QueryRaw(query, _options.Org, dialect, onResponse, onError, onComplete);
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
        /// <returns></returns>
        public async Task QueryRaw(string query, string org, Dialect dialect,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(onResponse, nameof(onResponse));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            await QueryRaw(CreateQuery(query, dialect), org, onResponse, onError, onComplete);
        }

        protected override void BeforeIntercept(RestRequest request)
        {
            _service.Configuration.ApiClient.BeforeIntercept(request);
        }

        protected override T AfterIntercept<T>(int statusCode, Func<IList<HttpHeader>> headers, T body)
        {
            return _service.Configuration.ApiClient.AfterIntercept(statusCode, headers, body);
        }

        private async Task Query(Query query, string org, FluxCsvParser.IFluxResponseConsumer consumer,
            Action<Exception> onError,
            Action onComplete)

        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));
            Arguments.CheckNotNull(consumer, nameof(consumer));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var requestMessage = CreateRequest(query, org);

            await Query(requestMessage, consumer, onError, onComplete);
        }

        private RestRequest CreateRequest(Query query, string org)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(org, nameof(org));

            var request = _service.PostQueryWithRestRequest(null, "application/json", null, org, null, query);
            return request;
        }

        private Query CreateQuery(string query, Dialect dialect = null)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            var created = new Query(null, query);
            created.Dialect = dialect ?? _defaultDialect;

            return created;
        }
    }
}