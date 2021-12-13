using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Api;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Internal;
using InfluxDB.Client.Core.Internal;
using InfluxDB.Client.Flux.Internal;
using Newtonsoft.Json.Linq;

namespace InfluxDB.Client.Flux
{
    public class FluxClient : AbstractRestClient
    {
        private readonly ApiClient _apiClient;
        private readonly LoggingHandler _loggingHandler;
        private readonly LegacyQueryApi _queryApi;
        private readonly RequestOptions _authorizationParams = new RequestOptions();

        public FluxClient(FluxConnectionOptions options)
        {
            _loggingHandler = new LoggingHandler(LogLevel.None);

            var version = AssemblyHelper.GetVersion(typeof(FluxClient));

            _apiClient = options.ToApiClient(_loggingHandler);
            _apiClient.Configuration.UserAgent = $"influxdb-client-csharp/{version}";

            _queryApi = new LegacyQueryApi(_apiClient, _apiClient.ExceptionFactory, new FluxResultMapper());

            if (!string.IsNullOrEmpty(options.Username))
            {
                if (FluxConnectionOptions.AuthenticationType.BasicAuthentication.Equals(options.Authentication))
                {
                    var auth = Encoding.UTF8.GetBytes(options.Username + ":" + new string(options.Password));
                    _authorizationParams.HeaderParameters.Add("Authorization", "Basic " + Convert.ToBase64String(auth));
                }
                else
                {
                    _authorizationParams.QueryParameters.Add("u", options.Username);
                    _authorizationParams.QueryParameters.Add("p", new string(options.Password));
                }
            }
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously map whole response to <see cref="List{FluxTable}"/>.
        /// </summary>
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync(string,System.Action{CancellationToken,InfluxDB.Client.Core.Flux.Domain.FluxRecord},System.Action{System.Exception},System.Action)"/> for large data streaming.
        /// </para>
        /// <param name="query">the flux query to execute</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns><see cref="List{FluxTable}"/> which are matched the query</returns>
        public async Task<List<FluxTable>> QueryAsync(string query, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");

            var consumer = new FluxCsvParser.FluxResponseConsumerTable();

            await QueryAsync(query, AbstractQueryClient.GetDefaultDialect(), consumer,
                    AbstractQueryClient.ErrorConsumer, AbstractQueryClient.EmptyAction, cancellationToken)
                .ConfigureAwait(false);

            return consumer.Tables;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously map whole response to list of object with
        /// given type.
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync{T}(string,System.Action{CancellationToken,T},System.Action{System.Exception},System.Action,System.Threading.CancellationToken)"/> for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns><see cref="List{T}"/> which are matched the query</returns>
        public async Task<List<T>> QueryAsync<T>(string query, CancellationToken cancellationToken = default)
        {
            var measurements = new List<T>();

            var consumer =
                new AbstractQueryClient.FluxResponseConsumerPoco<T>((cancellable, poco) => { measurements.Add(poco); },
                    _queryApi.Mapper);

            await QueryAsync(query, AbstractQueryClient.GetDefaultDialect(), consumer,
                    AbstractQueryClient.ErrorConsumer, AbstractQueryClient.EmptyAction, cancellationToken)
                .ConfigureAwait(false);

            return measurements;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream <see cref="FluxRecord"/> to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>async task</returns>
        public Task QueryAsync(string query, Action<CancellationToken, FluxRecord> onNext,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");

            return QueryAsync(query, onNext, AbstractQueryClient.ErrorConsumer, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream result as POCO.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(string query, Action<CancellationToken, T> onNext,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");

            return QueryAsync(query, onNext, AbstractQueryClient.ErrorConsumer, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream <see cref="FluxRecord"/> to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>async task</returns>
        public Task QueryAsync(string query, Action<CancellationToken, FluxRecord> onNext, Action<Exception> onError,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");
            Arguments.CheckNotNull(onError, "onError");

            return QueryAsync(query, onNext, onError, AbstractQueryClient.EmptyAction, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream result as POCO.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(string query, Action<CancellationToken, T> onNext, Action<Exception> onError,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");
            Arguments.CheckNotNull(onError, "onError");

            return QueryAsync(query, onNext, onError, AbstractQueryClient.EmptyAction, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream <see cref="FluxRecord"/> to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>async task</returns>
        public Task QueryAsync(string query, Action<CancellationToken, FluxRecord> onNext, Action<Exception> onError,
            Action onComplete, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            var consumer = new AbstractQueryClient.FluxResponseConsumerRecord(onNext);

            return QueryAsync(query, AbstractQueryClient.GetDefaultDialect(), consumer, onError, onComplete,
                cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream result as POCO.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(string query, Action<CancellationToken, T> onNext, Action<Exception> onError,
            Action onComplete, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            var consumer = new AbstractQueryClient.FluxResponseConsumerPoco<T>(onNext, _queryApi.Mapper);

            return QueryAsync(query, AbstractQueryClient.GetDefaultDialect(), consumer, onError, onComplete,
                cancellationToken);
        }

        private Task QueryAsync(string query,
            string dialect,
            FluxCsvParser.IFluxResponseConsumer responseConsumer,
            Action<Exception> onError,
            Action onComplete, CancellationToken cancellationToken = default)
        {
            var message = QueryRequest(CreateBody(dialect, query));

            return _queryApi.Query(message, responseConsumer, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        /// <para>
        /// NOTE: This method is not intended for large responses, that do not fit into memory.
        /// Use <see cref="QueryRawAsync(string,string,System.Action{CancellationToken,string},System.Action{System.Exception},System.Action,System.Threading.CancellationToken)"/>
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute></param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>the raw response that matched the query</returns>
        public Task<string> QueryRawAsync(string query, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");

            return QueryRawAsync(query, "", cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        /// <para>
        /// NOTE: This method is not intended for large responses, that do not fit into memory.
        /// Use <see cref="QueryRawAsync(string,string,System.Action{CancellationToken,string},System.Action{System.Exception},System.Action,System.Threading.CancellationToken)"/>
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute></param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>the raw response that matched the query</returns>
        public async Task<string> QueryRawAsync(string query, string dialect,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");

            var rows = new List<string>();

            void Consumer(CancellationToken cancellable, string row) => rows.Add(row);

            await QueryRawAsync(query, dialect, Consumer, AbstractQueryClient.ErrorConsumer,
                AbstractQueryClient.EmptyAction, cancellationToken).ConfigureAwait(false);

            return string.Join("\n", rows);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability to discontinue a streaming query.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>async task</returns>
        public Task QueryRawAsync(string query, Action<CancellationToken, string> onResponse,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");

            return QueryRawAsync(query, null, onResponse, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onResponse">the callback to consume the response line by line with capability to discontinue a streaming query.</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>async task</returns>
        public Task QueryRawAsync(string query, string dialect, Action<CancellationToken, string> onResponse,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");

            return QueryRawAsync(query, dialect, onResponse, AbstractQueryClient.ErrorConsumer, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability to discontinue a streaming query.</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>async task</returns>
        public Task QueryRawAsync(string query, Action<CancellationToken, string> onResponse, Action<Exception> onError,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");
            Arguments.CheckNotNull(onError, "onError");

            return QueryRawAsync(query, onResponse, onError, AbstractQueryClient.EmptyAction, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onResponse">the callback to consume the response line by line with capability to discontinue a streaming query.</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>async task</returns>
        public Task QueryRawAsync(string query, string dialect, Action<CancellationToken, string> onResponse,
            Action<Exception> onError, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");
            Arguments.CheckNotNull(onError, "onError");

            return QueryRawAsync(query, dialect, onResponse, onError, AbstractQueryClient.EmptyAction,
                cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability to discontinue a streaming query.</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>async task</returns>
        public Task QueryRawAsync(string query, Action<CancellationToken, string> onResponse, Action<Exception> onError,
            Action onComplete, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            return QueryRawAsync(query, null, onResponse, onError, onComplete, cancellationToken);
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
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>async task</returns>
        public Task QueryRawAsync(string query, string dialect, Action<CancellationToken, string> onResponse,
            Action<Exception> onError, Action onComplete, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            var message = QueryRequest(CreateBody(dialect, query));

            return _queryApi.QueryRaw(message, onResponse, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Check the status of InfluxDB Server.
        /// </summary>
        /// <returns>true if server is healthy otherwise return false</returns>
        public async Task<bool> PingAsync()
        {
            return await PingAsync(PingRequest());
        }

        /// <summary>
        ///  Return the version of the connected InfluxDB Server.
        /// </summary>
        /// <returns>the version String, otherwise unknown</returns>
        /// <exception cref="InfluxException">throws when request did not succesfully ends</exception>
        public async Task<string> VersionAsync()
        {
            var pingWithHttpInfoAsync = await PingRequest()
                .ConfigureAwait(false);

            return VersionAsync(pingWithHttpInfoAsync.Headers);
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

        private Task<ApiResponse<object>> PingRequest()
        {
            var request = new RequestOptions();
            AddDefaults(request);

            return _apiClient.GetAsync<Object>("/ping", request, _apiClient.Configuration);
        }

        private RequestOptions QueryRequest(string query)
        {
            var request = new RequestOptions();
            AddDefaults(request);

            request.HeaderParameters.Add("Content-Type", "application/json");
            request.HeaderParameters.Add("Accept", "application/csv");

            request.Data = query;

            return request;
        }

        private void AddDefaults(RequestOptions request)
        {
            request.HeaderParameters.Add(_authorizationParams.HeaderParameters);
            request.QueryParameters.Add(_authorizationParams.QueryParameters);
        }

        private string CreateBody(string dialect, string query)
        {
            Arguments.CheckNonEmptyString(query, "Flux query");

            var json = new JObject();
            json.Add("query", query);

            if (!string.IsNullOrEmpty(dialect))
            {
                json.Add("dialect", JObject.Parse(dialect));
            }

            return json.ToString();
        }
    }

    internal class LegacyQueryApi : AbstractQueryClient
    {
        internal LegacyQueryApi(ApiClient apiClient, ExceptionFactory exceptionFactory, IFluxResultMapper mapper) :
            base(apiClient, exceptionFactory, mapper)
        {
        }
    }
}