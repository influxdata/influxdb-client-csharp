using System;
using System.Collections.Generic;
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
        private static readonly Dialect DefaultDialect = new Dialect();
        
        private readonly QueryService _service;

        protected internal QueryApi(QueryService service) : base(service.Configuration.ApiClient.RestClient)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
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
        /// <param name="orgId">specifies the source organization</param>
        /// <returns>FluxTables that are matched the query</returns>
        public List<FluxTable> Query(string query, string orgId)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var consumer = new FluxCsvParser.FluxResponseConsumerTable();

            Query(CreateQuery(query, DefaultDialect), orgId);

            return consumer.Tables;
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
        /// <param name="orgId">specifies the source organization</param>
        /// <returns>FluxTables that are matched the query</returns>
        public List<FluxTable> Query(Query query, string orgId)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var consumer = new FluxCsvParser.FluxResponseConsumerTable();

            Query(query, orgId, consumer, ErrorConsumer, EmptyAction);

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
        /// <param name="orgId">specifies the source organization</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public List<T> Query<T>(string query, string orgId)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            
            return Query<T>(CreateQuery(query), orgId);
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
        /// <param name="orgId">specifies the source organization</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>Measurements which are matched the query</returns>
        public List<T> Query<T>(Query query, string orgId)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            
            var measurements = new List<T>();

            var consumer = new FluxResponseConsumerPoco<T>((cancellable, poco) => { measurements.Add(poco); });

            Query(query, orgId, consumer, ErrorConsumer, EmptyAction);

            return measurements;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability
        /// to discontinue a streaming query</param>
        /// <returns>async task</returns>
        public void Query(string query, string orgId, Action<ICancellable, FluxRecord> onNext)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            Query(query, orgId, onNext, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability
        /// to discontinue a streaming query</param>
        /// <returns>async task</returns>
        public void Query(Query query, string orgId, Action<ICancellable, FluxRecord> onNext)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            Query(query, orgId, onNext, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public void Query<T>(string query, string orgId, Action<ICancellable, T> onNext)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            Query(query, orgId, onNext, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public void Query<T>(Query query, string orgId, Action<ICancellable, T> onNext)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));

            Query(query, orgId, onNext, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <returns>async task</returns>
        public void Query(string query, string orgId, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            Query(query, orgId, onNext, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <returns>async task</returns>
        public void Query(Query query, string orgId, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            Query(query, orgId, onNext, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public void Query<T>(string query, string orgId, Action<ICancellable, T> onNext,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            Query(query, orgId, onNext, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public void Query<T>(Query query, string orgId, Action<ICancellable, T> onNext,
            Action<Exception> onError)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));

            Query(query, orgId, onNext, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <returns>async task</returns>
        public void Query(string query, string orgId, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var consumer = new FluxResponseConsumerRecord(onNext);

            Query(CreateQuery(query, DefaultDialect), orgId, consumer, onError, onComplete);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream <see cref="FluxRecord"/>
        /// to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <returns>async task</returns>
        public void Query(Query query, string orgId, Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var consumer = new FluxResponseConsumerRecord(onNext);

            Query(query, orgId, consumer, onError, onComplete);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public void Query<T>(string query, string orgId, Action<ICancellable, T> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var consumer = new FluxResponseConsumerPoco<T>(onNext);

            Query(CreateQuery(query, DefaultDialect), orgId, consumer, onError, onComplete);
        }
        
        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream Measurements
        /// to a <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onNext">the callback to consume the mapped Measurements with capability
        /// to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public void Query<T>(Query query, string orgId, Action<ICancellable, T> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNotNull(onNext, nameof(onNext));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var consumer = new FluxResponseConsumerPoco<T>(onNext);

            Query(query, orgId, consumer, onError, onComplete);
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
        /// <param name="orgId">specifies the source organization</param>
        /// <returns>the raw response that matched the query</returns>
        public string QueryRaw(string query, string orgId)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            
            return QueryRaw(query, orgId, DefaultDialect);
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
        /// <param name="orgId">specifies the source organization</param>
        /// <returns>the raw response that matched the query</returns>
        public string QueryRaw(Query query, string orgId)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            
            var rows = new List<string>();

            void Consumer(ICancellable cancellable, string row) => rows.Add(row);

            QueryRaw(query, orgId, Consumer, ErrorConsumer, EmptyAction);

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
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <returns>the raw response that matched the query</returns>
        public string QueryRaw(string query, string orgId, Dialect dialect)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            
            return QueryRaw(CreateQuery(query, dialect), orgId);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <returns></returns>
        public void QueryRaw(string query, string orgId, Action<ICancellable, string> onResponse
        )
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            
            QueryRaw(query, orgId, onResponse, ErrorConsumer);
        }
        
        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <returns></returns>
        public void QueryRaw(Query query, string orgId, Action<ICancellable, string> onResponse
        )
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            
            QueryRaw(query, orgId, onResponse, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <returns></returns>
        public void QueryRaw(string query, string orgId, Dialect dialect,
            Action<ICancellable, string> onResponse
        )
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            
            QueryRaw(query, orgId, dialect, onResponse, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <returns></returns>
        public void QueryRaw(string query, string orgId, Action<ICancellable, string> onResponse,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            
            QueryRaw(query, orgId, null, onResponse, onError, EmptyAction);
        }


        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <returns></returns>
        public void QueryRaw(Query query, string orgId, Action<ICancellable, string> onResponse,
            Action<Exception> onError)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            
            QueryRaw(query, orgId, onResponse, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <returns></returns>
        public void QueryRaw(string query, string orgId, Dialect dialect,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            
            QueryRaw(query, orgId, dialect, onResponse, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="onComplete">callback to consume a notification about successfully end of stream</param>
        /// <returns></returns>
        public void QueryRaw(string query, string orgId, Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            
            QueryRaw(query, orgId, null, onResponse, onError, onComplete);
        }
        
        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="onComplete">callback to consume a notification about successfully end of stream</param>
        /// <returns></returns>
        public void QueryRaw(Query query, string orgId, Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            
            var requestMessage = CreateRequest(query, orgId);

            QueryRaw(requestMessage, onResponse, onError, onComplete);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB 2.0 and asynchronously stream response
        /// (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="orgId">specifies the source organization</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onResponse">the callback to consume the response line by line with capability
        /// to discontinue a streaming query.</param>
        /// <param name="onError">callback to consume any error notification</param>
        /// <param name="onComplete">callback to consume a notification about successfully end of stream</param>
        /// <returns></returns>
        public void QueryRaw(string query, string orgId, Dialect dialect,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(onResponse, nameof(onResponse));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            QueryRaw(CreateQuery(query, dialect), orgId, onResponse, onError, onComplete);
        }
        
        private void Query(Query query, string orgId, FluxCsvParser.IFluxResponseConsumer consumer,
            Action<Exception> onError,
            Action onComplete)

        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(consumer, nameof(consumer));
            Arguments.CheckNotNull(onError, nameof(onError));
            Arguments.CheckNotNull(onComplete, nameof(onComplete));

            var requestMessage = CreateRequest(query, orgId);

            Query(requestMessage, consumer, onError, onComplete);
        }

        private RestRequest CreateRequest(Query query, string orgId)
        {
            Arguments.CheckNotNull(query, nameof(query));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var request = _service.QueryPostWithRestRequest(null, "text/csv",
                "application/json", null, orgId, query);
            _service.Configuration.ApiClient.Intercept(request);
            return request;
        }
        
        private Query CreateQuery(string query, Dialect dialect = null)
        {
            Arguments.CheckNonEmptyString(query, nameof(query));

            var created = new Query(null, query);
            created.Dialect = dialect ?? DefaultDialect;
            
            return created;
        }

    }
}