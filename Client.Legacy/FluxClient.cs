using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Internal;
using InfluxDB.Client.Core.Internal;

namespace InfluxDB.Client.Flux
{
    public class FluxClient : AbstractQueryClient
    {
        private readonly LoggingHandler _loggingHandler;

        public FluxClient(FluxConnectionOptions options)
        {
            _loggingHandler = new LoggingHandler(LogLevel.None);

            Client.HttpClient = new HttpClient(_loggingHandler);
            Client.HttpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            Client.HttpClient.BaseAddress = new Uri(options.Url);
            Client.HttpClient.Timeout = options.Timeout;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously map whole response to <see cref="List{FluxTable}"/>.
        /// </summary>
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="Query(string, System.Action{ICancellable,FluxRecord},System.Action{System.Exception},Action)"/> for large data streaming.
        /// </para>
        /// <param name="query">the flux query to execute</param>
        /// <returns><see cref="List{FluxTable}"/> which are matched the query</returns>
        public async Task<List<FluxTable>> Query(string query)
        {
            Arguments.CheckNonEmptyString(query, "query");

            var consumer = new FluxCsvParser.FluxResponseConsumerTable();

            await Query(query, GetDefaultDialect(), consumer, ErrorConsumer, EmptyAction);

            return consumer.Tables;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously map whole response to list of object with
        /// given type.
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="Query{T}(string, System.Action{ICancellable,T},System.Action{System.Exception},Action)"/> for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns><see cref="List{T}"/> which are matched the query</returns>
        public async Task<List<T>> Query<T>(string query)
        {
            var measurements = new List<T>();

            var consumer = new FluxResponseConsumerPoco<T>((cancellable, poco) => { measurements.Add(poco); });

            await Query(query, GetDefaultDialect(), consumer, ErrorConsumer, EmptyAction);

            return measurements;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream <see cref="FluxRecord"/> to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <returns>async task</returns>
        public async Task Query(string query, Action<ICancellable, FluxRecord> onNext)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");

            await Query(query, onNext, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream result as POCO.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public async Task Query<T>(string query, Action<ICancellable, T> onNext)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");

            await Query(query, onNext, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream <see cref="FluxRecord"/> to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <returns>async task</returns>
        public async Task Query(string query, Action<ICancellable, FluxRecord> onNext, Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");
            Arguments.CheckNotNull(onError, "onError");

            await Query(query, onNext, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream result as POCO.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public async Task Query<T>(string query, Action<ICancellable, T> onNext, Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");
            Arguments.CheckNotNull(onError, "onError");

            await Query(query, onNext, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream <see cref="FluxRecord"/> to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <returns>async task</returns>
        public async Task Query(string query,
            Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            var consumer = new FluxResponseConsumerRecord(onNext);

            await Query(query, GetDefaultDialect(), consumer, onError, onComplete);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream result as POCO.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public async Task Query<T>(string query,
            Action<ICancellable, T> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            var consumer = new FluxResponseConsumerPoco<T>(onNext);

            await Query(query, GetDefaultDialect(), consumer, onError, onComplete);
        }

        private async Task Query(string query,
            string dialect,
            FluxCsvParser.IFluxResponseConsumer responseConsumer,
            Action<Exception> onError,
            Action onComplete)
        {
            var message = FluxService.Query(CreateBody(dialect, query));

            await Query(message, responseConsumer, onError, onComplete);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        /// <para>
        /// NOTE: This method is not intended for large responses, that do not fit into memory.
        /// Use <see cref="QueryRaw(string,string,Action{ICancellable, string},Action{Exception},Action)"/>
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute></param>
        /// <returns>the raw response that matched the query</returns>
        public async Task<string> QueryRaw(string query)
        {
            Arguments.CheckNonEmptyString(query, "query");

            return await QueryRaw(query, "");
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        /// <para>
        /// NOTE: This method is not intended for large responses, that do not fit into memory.
        /// Use <see cref="QueryRaw(string,string,Action{ICancellable, string},Action{Exception},Action)"/>
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute></param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <returns>the raw response that matched the query</returns>
        public async Task<string> QueryRaw(string query, string dialect)
        {
            Arguments.CheckNonEmptyString(query, "query");

            var rows = new List<string>();

            void Consumer(ICancellable cancellable, string row) => rows.Add(row);

            await QueryRaw(query, dialect, Consumer, ErrorConsumer, EmptyAction);

            return string.Join("\n", rows);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability to discontinue a streaming query.</param>
        /// <returns>async task</returns>
        public async Task QueryRaw(string query,
            Action<ICancellable, string> onResponse)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");

            await QueryRaw(query, null, onResponse);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onResponse">the callback to consume the response line by line with capability to discontinue a streaming query.</param>
        /// <returns>async task</returns>
        public async Task QueryRaw(string query,
            string dialect,
            Action<ICancellable, string> onResponse)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");

            await QueryRaw(query, dialect, onResponse, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability to discontinue a streaming query.</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <returns>async task</returns>
        public async Task QueryRaw(string query,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");
            Arguments.CheckNotNull(onError, "onError");

            await QueryRaw(query, onResponse, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onResponse">the callback to consume the response line by line with capability to discontinue a streaming query.</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <returns>async task</returns>
        public async Task QueryRaw(string query,
            string dialect,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");
            Arguments.CheckNotNull(onError, "onError");

            await QueryRaw(query, dialect, onResponse, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability to discontinue a streaming query.</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <returns>async task</returns>
        public async Task QueryRaw(string query,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            await QueryRaw(query, null, onResponse, onError, onComplete);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onResponse">the callback to consume the response line by line with capability to discontinue a streaming query.</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <returns>async task</returns>
        public async Task QueryRaw(string query,
            string dialect,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            var message = FluxService.Query(CreateBody(dialect, query));

            await QueryRaw(message, onResponse, onError, onComplete);
        }

        /// <summary>
        /// Check the status of InfluxDB Server.
        /// </summary>
        /// <returns>true if server is healthy otherwise return false</returns>
        public async Task<bool> Ping()
        {
            try
            {
                var responseHttp = await Client.DoRequest(FluxService.Ping()).ConfigureAwait(false);

                RaiseForInfluxError(responseHttp);

                return true;
            }
            catch (Exception e)
            {
                Trace.WriteLine($"Error: {e.Message}");
                return false;
            }
        }

        /// <summary>
        ///  Return the version of the connected InfluxDB Server.
        /// </summary>
        /// <returns>the version String, otherwise unknown</returns>
        /// <exception cref="InfluxException">throws when request did not succesfully ends</exception>
        public async Task<string> Version()
        {
            try
            {
                var responseHttp = await Client.DoRequest(FluxService.Ping()).ConfigureAwait(false);

                RaiseForInfluxError(responseHttp);

                return GetVersion(responseHttp);
            }
            catch (Exception e)
            {
                throw new InfluxException(e);
            }
        }

        /// <summary>
        /// Set the log level for the request and response information.
        /// </summary>
        /// <param name="logLevel">the log level to set</param>
        public void SetLogLevel(LogLevel logLevel)
        {
            Arguments.CheckNotNull(logLevel, nameof(logLevel));

            _loggingHandler.Level = logLevel;
        }

        /// <summary>
        /// Set the <see cref="LogLevel"/> that is used for logging requests and responses.
        /// </summary>
        /// <returns>Log Level</returns>
        public LogLevel GetLogLevel()
        {
            return _loggingHandler.Level;
        }

        private string GetVersion(RequestResult responseHttp)
        {
            Arguments.CheckNotNull(responseHttp, "responseHttp");

            IEnumerable<string> value;

            responseHttp.ResponseHeaders.TryGetValue("X-Influxdb-Version", out value);

            if (value != null)
            {
                var version = value.FirstOrDefault();

                if (!string.IsNullOrEmpty(version))
                {
                    return version;
                }
            }

            return "unknown";
        }
    }
}