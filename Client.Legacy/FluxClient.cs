using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Internal;
using InfluxDB.Client.Core.Internal;
using RestSharp;

namespace InfluxDB.Client.Flux
{
    public class FluxClient : AbstractQueryClient, IDisposable
    {
        private readonly LoggingHandler _loggingHandler;

        public FluxClient(FluxConnectionOptions options) : base(new FluxResultMapper())
        {
            _loggingHandler = new LoggingHandler(LogLevel.None);

            var version = AssemblyHelper.GetVersion(typeof(FluxClient));
            var restClientOptions = new RestClientOptions(options.Url)
            {
                Timeout = (int)options.Timeout.TotalMilliseconds,
                UserAgent = $"influxdb-client-csharp/{version}",
                Proxy = options.WebProxy
            };
            RestClient = new RestClient(restClientOptions);
            RestClient.AddDefaultHeader("Accept", "application/json");
            if (!string.IsNullOrEmpty(options.Username))
            {
                if (FluxConnectionOptions.AuthenticationType.BasicAuthentication.Equals(options.Authentication))
                {
                    var auth = Encoding.UTF8.GetBytes(options.Username + ":" + new string(options.Password));
                    RestClient.AddDefaultHeader("Authorization", "Basic " + Convert.ToBase64String(auth));
                }
                else
                {
                    RestClient.AddDefaultQueryParameter("u", options.Username);
                    RestClient.AddDefaultQueryParameter("p", new string(options.Password));
                }
            }
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously map whole response to <see cref="List{FluxTable}"/>.
        /// </summary>
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync(string,System.Action{InfluxDB.Client.Core.Flux.Domain.FluxRecord},System.Action{System.Exception},System.Action,System.Threading.CancellationToken)"/> for large data streaming.
        /// </para>
        /// <param name="query">the flux query to execute</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns><see cref="List{FluxTable}"/> which are matched the query</returns>
        public async Task<List<FluxTable>> QueryAsync(string query, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");

            var consumer = new FluxCsvParser.FluxResponseConsumerTable();

            await QueryAsync(query, GetDefaultDialect(), consumer, ErrorConsumer, EmptyAction, cancellationToken)
                .ConfigureAwait(false);

            return consumer.Tables;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously map whole response to list of object with
        /// given type.
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync{T}(string,System.Action{T},System.Action{System.Exception},System.Action,System.Threading.CancellationToken)"/> for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns><see cref="List{T}"/> which are matched the query</returns>
        public async Task<List<T>> QueryAsync<T>(string query, CancellationToken cancellationToken = default)
        {
            var measurements = new List<T>();

            var consumer = new FluxResponseConsumerPoco<T>((poco) => { measurements.Add(poco); }, Mapper);

            await QueryAsync(query, GetDefaultDialect(), consumer, ErrorConsumer, EmptyAction, cancellationToken)
                .ConfigureAwait(false);

            return measurements;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream <see cref="FluxRecord"/> to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>async task</returns>
        public Task QueryAsync(string query, Action<FluxRecord> onNext, Action<Exception> onError = null,
            Action onComplete = null, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");

            var consumer = new FluxResponseConsumerRecord(onNext);

            return QueryAsync(query, GetDefaultDialect(), consumer, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream result as POCO.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(string query, Action<T> onNext, Action<Exception> onError = null,
            Action onComplete = null, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");

            var consumer = new FluxResponseConsumerPoco<T>(onNext, Mapper);

            return QueryAsync(query, GetDefaultDialect(), consumer, onError, onComplete, cancellationToken);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        /// <para>
        /// NOTE: This method is not intended for large responses, that do not fit into memory.
        /// Use <see cref="QueryRawAsync(string,System.Action{string},string,System.Action{System.Exception},System.Action,System.Threading.CancellationToken)"/>
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute></param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>the raw response that matched the query</returns>
        public async Task<string> QueryRawAsync(string query, string dialect = null,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");

            var rows = new List<string>();

            void Consumer(string row)
            {
                rows.Add(row);
            }

            await QueryRawAsync(query, Consumer, dialect, ErrorConsumer, EmptyAction, cancellationToken)
                .ConfigureAwait(false);

            return string.Join("\n", rows);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response. <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <param name="cancellationToken">Token that enables callers to cancel the request.</param>
        /// <returns>async task</returns>
        public Task QueryRawAsync(string query, Action<string> onResponse, string dialect = null,
            Action<Exception> onError = null, Action onComplete = null, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");

            var message = QueryRequest(CreateBody(dialect, query));

            return QueryRaw(message, onResponse, onError ?? ErrorConsumer, onComplete ?? EmptyAction,
                cancellationToken);
        }

        /// <summary>
        /// Check the status of InfluxDB Server.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>true if server is healthy otherwise return false</returns>
        public async Task<bool> PingAsync(CancellationToken cancellationToken = default)
        {
            var request = ExecuteAsync(PingRequest(), cancellationToken);

            return await PingAsync(request);
        }

        /// <summary>
        ///  Return the version of the connected InfluxDB Server.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the version String, otherwise unknown</returns>
        /// <exception cref="InfluxException">throws when request did not succesfully ends</exception>
        public async Task<string> VersionAsync(CancellationToken cancellationToken = default)
        {
            var request = ExecuteAsync(PingRequest(), cancellationToken);

            return await VersionAsync(request);
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

        public void Dispose()
        {
            // 
            // Dispose HttpClient 
            // 
            RestClient.Dispose();
        }

        private Task QueryAsync(string query,
            string dialect,
            FluxCsvParser.IFluxResponseConsumer responseConsumer,
            Action<Exception> onError = null,
            Action onComplete = null,
            CancellationToken cancellationToken = default)
        {
            var message = QueryRequest(CreateBody(dialect, query));

            return Query(message, responseConsumer, onError ?? ErrorConsumer, onComplete ?? EmptyAction,
                cancellationToken);
        }

        private async Task<RestResponse> ExecuteAsync(RestRequest request,
            CancellationToken cancellationToken = default)
        {
            BeforeIntercept(request);

            var response = await RestClient.ExecuteAsync(request, cancellationToken).ConfigureAwait(false);

            RaiseForInfluxError(response, response.Content);

            AfterIntercept(
                (int)response.StatusCode,
                () => response.Headers,
                response.Content);

            return response;
        }

        protected override void BeforeIntercept(RestRequest request)
        {
            _loggingHandler.BeforeIntercept(request);
        }

        protected override T AfterIntercept<T>(int statusCode, Func<IEnumerable<HeaderParameter>> headers, T body)
        {
            return (T)_loggingHandler.AfterIntercept(statusCode, headers, body);
        }

        private RestRequest PingRequest()
        {
            return new RestRequest("ping");
        }

        private Func<Func<HttpResponseMessage, RestResponse>, RestRequest> QueryRequest(string query)
        {
            return advancedResponseWriter => new RestRequest("api/v2/query", Method.Post)
                .AddAdvancedResponseHandler(advancedResponseWriter)
                .AddParameter(new BodyParameter("application/json", query, "application/json"));
        }
    }
}