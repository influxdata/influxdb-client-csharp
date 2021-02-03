using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Internal;
using InfluxDB.Client.Core.Internal;
using RestSharp;

namespace InfluxDB.Client.Flux
{
    public class FluxClient : AbstractQueryClient
    {
        private readonly LoggingHandler _loggingHandler;

        public FluxClient(FluxConnectionOptions options) : base(new RestClient())
        {
            _loggingHandler = new LoggingHandler(LogLevel.None);

            var version = AssemblyHelper.GetVersion(typeof(FluxClient));
            
            RestClient.BaseUrl = new Uri(options.Url);
            RestClient.Timeout = options.Timeout.Milliseconds;
            RestClient.AddDefaultHeader("Accept", "application/json");
            if (!string.IsNullOrEmpty(options.Username))
            {
                if (FluxConnectionOptions.AuthenticationType.BasicAuthentication.Equals(options.Authentication))
                {
                    var auth = System.Text.Encoding.UTF8.GetBytes(options.Username + ":" + new string(options.Password));
                    RestClient.AddDefaultHeader("Authorization", "Basic " + Convert.ToBase64String(auth));
                }
                else
                {
                    RestClient.AddDefaultQueryParameter("u", options.Username);
                    RestClient.AddDefaultQueryParameter("p", new string(options.Password));
                }
            }
            RestClient.UserAgent = $"influxdb-client-csharp/{version}";
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously map whole response to <see cref="List{FluxTable}"/>.
        /// </summary>
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync(string,System.Action{InfluxDB.Client.Core.ICancellable,InfluxDB.Client.Core.Flux.Domain.FluxRecord},System.Action{System.Exception},System.Action)"/> for large data streaming.
        /// </para>
        /// <param name="query">the flux query to execute</param>
        /// <returns><see cref="List{FluxTable}"/> which are matched the query</returns>
        public async Task<List<FluxTable>> QueryAsync(string query)
        {
            Arguments.CheckNonEmptyString(query, "query");

            var consumer = new FluxCsvParser.FluxResponseConsumerTable();

            await QueryAsync(query, GetDefaultDialect(), consumer, ErrorConsumer, EmptyAction);

            return consumer.Tables;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously map whole response to list of object with
        /// given type.
        /// <para>
        /// NOTE: This method is not intended for large query results.
        /// Use <see cref="QueryAsync{T}(string,System.Action{InfluxDB.Client.Core.ICancellable,T},System.Action{System.Exception},System.Action)"/> for large data streaming.
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns><see cref="List{T}"/> which are matched the query</returns>
        public async Task<List<T>> QueryAsync<T>(string query)
        {
            var measurements = new List<T>();

            var consumer = new FluxResponseConsumerPoco<T>((cancellable, poco) => { measurements.Add(poco); });

            await QueryAsync(query, GetDefaultDialect(), consumer, ErrorConsumer, EmptyAction);

            return measurements;
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream <see cref="FluxRecord"/> to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <returns>async task</returns>
        public Task QueryAsync(string query, Action<ICancellable, FluxRecord> onNext)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");

            return QueryAsync(query, onNext, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream result as POCO.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(string query, Action<ICancellable, T> onNext)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");

            return QueryAsync(query, onNext, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream <see cref="FluxRecord"/> to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <returns>async task</returns>
        public Task QueryAsync(string query, Action<ICancellable, FluxRecord> onNext, Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");
            Arguments.CheckNotNull(onError, "onError");

            return QueryAsync(query, onNext, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream result as POCO.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <typeparam name="T">the type of measurement</typeparam>
        /// <returns>async task</returns>
        public Task QueryAsync<T>(string query, Action<ICancellable, T> onNext, Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");
            Arguments.CheckNotNull(onError, "onError");

            return QueryAsync(query, onNext, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream <see cref="FluxRecord"/> to <see cref="onNext"/> consumer.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onNext">the callback to consume the FluxRecord result with capability to discontinue a streaming query</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <returns>async task</returns>
        public Task QueryAsync(string query,
            Action<ICancellable, FluxRecord> onNext,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            var consumer = new FluxResponseConsumerRecord(onNext);

            return QueryAsync(query, GetDefaultDialect(), consumer, onError, onComplete);
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
        public Task QueryAsync<T>(string query, Action<ICancellable, T> onNext, Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            var consumer = new FluxResponseConsumerPoco<T>(onNext);

            return QueryAsync(query, GetDefaultDialect(), consumer, onError, onComplete);
        }

        private Task QueryAsync(string query,
            string dialect,
            FluxCsvParser.IFluxResponseConsumer responseConsumer,
            Action<Exception> onError,
            Action onComplete)
        {
            var message = QueryRequest(CreateBody(dialect, query));

            return Query(message, responseConsumer, onError, onComplete);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        /// <para>
        /// NOTE: This method is not intended for large responses, that do not fit into memory.
        /// Use <see cref="QueryRawAsync(string,string,System.Action{InfluxDB.Client.Core.ICancellable,string},System.Action{System.Exception},System.Action)"/>
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute></param>
        /// <returns>the raw response that matched the query</returns>
        public Task<string> QueryRawAsync(string query)
        {
            Arguments.CheckNonEmptyString(query, "query");

            return QueryRawAsync(query, "");
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and synchronously map whole response to <see cref="string"/> result.
        /// <para>
        /// NOTE: This method is not intended for large responses, that do not fit into memory.
        /// Use <see cref="QueryRawAsync(string,string,System.Action{InfluxDB.Client.Core.ICancellable,string},System.Action{System.Exception},System.Action)"/>
        /// </para>
        /// </summary>
        /// <param name="query">the flux query to execute></param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <returns>the raw response that matched the query</returns>
        public async Task<string> QueryRawAsync(string query, string dialect)
        {
            Arguments.CheckNonEmptyString(query, "query");

            var rows = new List<string>();

            void Consumer(ICancellable cancellable, string row) => rows.Add(row);

            await QueryRawAsync(query, dialect, Consumer, ErrorConsumer, EmptyAction);

            return string.Join("\n", rows);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability to discontinue a streaming query.</param>
        /// <returns>async task</returns>
        public Task QueryRawAsync(string query,
            Action<ICancellable, string> onResponse)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");

            return QueryRawAsync(query, null, onResponse);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="dialect">Dialect is an object defining the options to use when encoding the response.
        /// <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a></param>
        /// <param name="onResponse">the callback to consume the response line by line with capability to discontinue a streaming query.</param>
        /// <returns>async task</returns>
        public Task QueryRawAsync(string query,
            string dialect,
            Action<ICancellable, string> onResponse)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");

            return QueryRawAsync(query, dialect, onResponse, ErrorConsumer);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability to discontinue a streaming query.</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <returns>async task</returns>
        public Task QueryRawAsync(string query,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");
            Arguments.CheckNotNull(onError, "onError");

            return QueryRawAsync(query, onResponse, onError, EmptyAction);
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
        public Task QueryRawAsync(string query,
            string dialect,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");
            Arguments.CheckNotNull(onError, "onError");

            return QueryRawAsync(query, dialect, onResponse, onError, EmptyAction);
        }

        /// <summary>
        /// Executes the Flux query against the InfluxDB and asynchronously stream response (line by line) to <see cref="onResponse"/>.
        /// </summary>
        /// <param name="query">the flux query to execute</param>
        /// <param name="onResponse">the callback to consume the response line by line with capability to discontinue a streaming query.</param>
        /// <param name="onError">the callback to consume any error notification</param>
        /// <param name="onComplete">the callback to consume a notification about successfully end of stream</param>
        /// <returns>async task</returns>
        public Task QueryRawAsync(string query,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            return QueryRawAsync(query, null, onResponse, onError, onComplete);
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
        public Task QueryRawAsync(string query,
            string dialect,
            Action<ICancellable, string> onResponse,
            Action<Exception> onError,
            Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            var message = QueryRequest(CreateBody(dialect, query));

            return QueryRaw(message, onResponse, onError, onComplete);
        }

        /// <summary>
        /// Check the status of InfluxDB Server.
        /// </summary>
        /// <returns>true if server is healthy otherwise return false</returns>
        public async Task<bool> PingAsync()
        {
            try
            {
                await ExecuteAsync(PingRequest());

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
        public async Task<string> VersionAsync()
        {
            try
            {
                var response = await ExecuteAsync(PingRequest());

                return GetVersion(response);
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

        private async Task<IRestResponse> ExecuteAsync(RestRequest request)
        {
            BeforeIntercept(request);

            var response = await Task.Run(() => RestClient.Execute(request));

            RaiseForInfluxError(response, response.Content);

            response.Content = AfterIntercept(
                (int) response.StatusCode,
                () => LoggingHandler.ToHeaders(response.Headers),
                response.Content);

            return response;
        }

        protected override void BeforeIntercept(RestRequest request)
        {
            _loggingHandler.BeforeIntercept(request);
        }

        protected override T AfterIntercept<T>(int statusCode, Func<IList<HttpHeader>> headers, T body)
        {
            return (T) _loggingHandler.AfterIntercept(statusCode, headers, body);
        }

        private string GetVersion(IRestResponse responseHttp)
        {
            Arguments.CheckNotNull(responseHttp, "responseHttp");

            var value = responseHttp.Headers
                .Where(header => header.Name.Equals("X-Influxdb-Version"))
                .Select(header => header.Value.ToString())
                .FirstOrDefault();

            if (value != null)
            {
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }

            return "unknown";
        }

        private RestRequest PingRequest()
        {
            return new RestRequest("ping", Method.GET);
        }

        private RestRequest QueryRequest(string query)
        {
            var restRequest = new RestRequest("api/v2/query", Method.POST);
            restRequest.AddParameter("application/json", query, ParameterType.RequestBody);

            return restRequest;
        }
    }
}