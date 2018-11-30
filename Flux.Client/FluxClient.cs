using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flux.Client.Client;
using Flux.Client.Options;
using Platform.Common.Flux.Domain;
using Platform.Common.Flux.Error;
using Platform.Common.Flux.Parser;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;

namespace Flux.Client
{
    public class FluxClient : AbstractQueryClient
    {
        public FluxClient(FluxConnectionOptions options)
        {
            Client.HttpClient.BaseAddress = new Uri(options.Url);
            Client.HttpClient.Timeout = options.Timeout;
        }

        /**
         * Executes the Flux query against the InfluxDB and asynchronously map whole response to {@code List<FluxTable>}.
         * <p>
         *
         * @param query the flux query to execute
         * @return {@code List<FluxTable>} which are matched the query
         */
        public async Task<List<FluxTable>> Query(string query)
        {
            Arguments.CheckNonEmptyString(query, "query");
            
            var consumer = new FluxCsvParser.FluxResponseConsumerTable();

            await Query(query, GetDefaultDialect(), consumer, ErrorConsumer, EmptyAction);

            return consumer.Tables;
        }
        
        /**
         * Executes the Flux query against the InfluxDB and asynchronously map whole response to list of object with
         * given type.
         * <p>
         * NOTE: This method is not intended for large query results.
         * Use {@link FluxClient#query(String, Class, BiConsumer, Consumer, Runnable)} for large data streaming.
         *
         * @param query the flux query to execute
         * @param measurementType  the type of measurement
         * @return {@code List<FluxTable>} which are matched the query
         */
        public async Task<List<T>> Query<T>(string query)
        {
            List<T> measurements = new List<T>();
            
            var consumer = new FluxResponseConsumerPoco<T>((cancellable, poco) =>
            {
                measurements.Add(poco);
            });
            
            await Query(query, GetDefaultDialect(), consumer, ErrorConsumer, EmptyAction);
            
            return measurements;
        }

        /**
         * Executes the Flux query against the InfluxDB and asynchronously stream {@link FluxRecord}s
         * to {@code onNext} consumer.
         *
         * @param query  the flux query to execute
         * @param onNext the callback to consume the FluxRecord result with capability to discontinue a streaming query
         */
        public async Task Query(string query, Action<ICancellable, FluxRecord> onNext)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");

            await Query(query, onNext, ErrorConsumer);
        }
        
        /**
         * Executes the Flux query against the InfluxDB and asynchronously stream POCO classes
         * to {@code onNext} consumer.
         *
         * @param query  the flux query to execute
         * @param measurementType the measurement type (POCO)
         * @param onNext the callback to consume the FluxRecord result with capability to discontinue a streaming query
         * @param <T> the type of the measurement (POCO)
         */
        public async Task Query<T>(string query, Action<ICancellable, T> onNext)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");

            await Query(query, onNext, ErrorConsumer);
        }

        /**
         * Executes the Flux query against the InfluxDB and asynchronously stream {@link FluxRecord}s
         * to {@code onNext} consumer.
         *
         * @param query   the flux query to execute
         * @param onNext  the callback to consume FluxRecord result with capability to discontinue a streaming query
         * @param onError the callback to consume any error notification
         */
        public async Task Query(string query, Action<ICancellable, FluxRecord> onNext, Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");
            Arguments.CheckNotNull(onError, "onError");
            
            await Query(query, onNext, onError, EmptyAction);
        }
        
        /**
         * Executes the Flux query against the InfluxDB and asynchronously stream POCO classes
         * to {@code onNext} consumer.
         *
         * @param query   the flux query to execute
         * @param measurementType the measurement type (POCO)
         * @param onNext  the callback to consume POCO record with capability to discontinue a streaming query
         * @param onError the callback to consume any error notification
         * @param <T> the type of the measurement (POCO)
         */
        public async Task Query<T>(string query, Action<ICancellable, T> onNext, Action<Exception> onError)
        { 
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");
            Arguments.CheckNotNull(onError, "onError");

            await Query(query, onNext, onError, EmptyAction);
        }

        /**
         * Executes the Flux query against the InfluxDB and asynchronously stream {@link FluxRecord}s
         * to {@code onNext} consumer.
         *
         * @param query      the flux query to execute
         * @param onNext     the callback to consume FluxRecord result with capability to discontinue a streaming query
         * @param onError    the callback to consume any error notification
         * @param onComplete the callback to consume a notification about successfully end of stream
         */
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
        
        /**
         * Executes the Flux query and asynchronously stream result as POCO.
         *
         * @param query the flux query to execute
         * @param measurementType the measurement type (POCO)
         * @param onNext the callback to consume POCO record with capability to discontinue a streaming query
         * @param onError the callback to consume any error notification
         * @param onComplete the callback to consume a notification about successfully end of stream
         * @param <T> the type of the measurement (POCO)
         */
        public async Task Query<T>(string query, 
                        Action<ICancellable, T> onNext, 
                        Action<Exception> onError, 
                        Action onComplete)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");
            Arguments.CheckNotNull(onError, "onError");
            Arguments.CheckNotNull(onComplete, "onComplete");

            FluxResponseConsumerPoco<T> consumer = new FluxResponseConsumerPoco<T>(onNext);

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
        
        /**
         * Executes the Flux query against the InfluxDB and synchronously map whole response to {@link String} result.
         * <p>
         * NOTE: This method is not intended for large responses, that do not fit into memory.
         * Use {@link FluxClient#queryRaw(String, BiConsumer, Consumer, Runnable)} for large data streaming.
         *
         * @param query the flux query to execute
         * @return the raw response that matched the query
         */
        public async Task<string> QueryRaw(string query) 
        {
            Arguments.CheckNonEmptyString(query, "query");

            return await QueryRaw(query, "");
        }
        
        /**
         * Executes the Flux query against the InfluxDB and synchronously map whole response to {@link String} result.
         * <p>
         * NOTE: This method is not intended for large responses, that do not fit into memory.
         * Use {@link FluxClient#queryRaw(String, String, BiConsumer, Consumer, Runnable)} for large data streaming.
         *
         * @param query   the flux query to execute
         * @param dialect Dialect is an object defining the options to use when encoding the response.
         *                <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a>.
         * @return the raw response that matched the query
         */
        public async Task<string> QueryRaw(string query, string dialect)
        {
            Arguments.CheckNonEmptyString(query, "query");

            List<string> rows = new List<string>();

            void Consumer(ICancellable cancellable, string row) => rows.Add(row);

            await QueryRaw(query, dialect, Consumer, ErrorConsumer, EmptyAction);

            return string.Join("\n", rows);
        }

        /**
         * Executes the Flux query against the InfluxDB and asynchronously stream response
         * (line by line) to {@code onResponse}.
         *
         * @param query      the flux query to execute
         * @param onResponse callback to consume the response line by line with capability to discontinue a streaming query
         */
        public async Task QueryRaw(string query,
                        Action<ICancellable, string> onResponse) 
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");

            await QueryRaw(query, null, onResponse);
        }
        
        /**
         * Executes the Flux query against the InfluxDB and asynchronously stream response
         * (line by line) to {@code onResponse}.
         *
         * @param query      the flux query to execute
         * @param dialect    Dialect is an object defining the options to use when encoding the response.
         *                   <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a>.
         * @param onResponse the callback to consume the response line by line
         *                   with capability to discontinue a streaming query
         */
        public async Task QueryRaw(string query,
                        string dialect,
                        Action<ICancellable, string> onResponse) 
        {

            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");

            await QueryRaw(query, dialect, onResponse, ErrorConsumer);
        }
        
        /**
         * Executes the Flux query against the InfluxDB and asynchronously stream response
         * (line by line) to {@code onResponse}.
         *
         * @param query      the flux query to execute
         * @param onResponse the callback to consume the response line by line
         *                   with capability to discontinue a streaming query
         * @param onError    callback to consume any error notification
         */
        public async Task QueryRaw(string query,
                        Action<ICancellable, string> onResponse,
                        Action<Exception> onError)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onResponse, "onNext");
            Arguments.CheckNotNull(onError, "onError");

            await QueryRaw(query, onResponse, onError, EmptyAction);
        }
        
        /**
         * Executes the Flux query against the InfluxDB and asynchronously stream response
         * (line by line) to {@code onResponse}.
         *
         * @param query      the flux query to execute
         * @param dialect    Dialect is an object defining the options to use when encoding the response.
         *                   <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a>.
         * @param onResponse the callback to consume the response line by line
         *                   with capability to discontinue a streaming query
         * @param onError    callback to consume any error notification
         */
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
        
        /**
         * Executes the Flux query against the InfluxDB and asynchronously stream response
         * (line by line) to {@code onResponse}.
         *
         * @param query      the flux query to execute
         * @param onResponse the callback to consume the response line by line
         *                   with capability to discontinue a streaming query
         * @param onError    callback to consume any error notification
         * @param onComplete callback to consume a notification about successfully end of stream
         */
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
                
        /**
         * Executes the Flux query against the InfluxDB and asynchronously stream response
         * (line by line) to {@code onResponse}.
         *
         * @param query      the flux query to execute
         * @param dialect    Dialect is an object defining the options to use when encoding the response.
         *                   <a href="http://bit.ly/flux-dialect">See dialect SPEC.</a>.
         * @param onResponse the callback to consume the response line by line
         *                   with capability to discontinue a streaming query
         *                   The callback call contains the one line of the response.
         * @param onError    callback to consume any error notification
         * @param onComplete callback to consume a notification about successfully end of stream
         */
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

        /**
         * Check the status of InfluxDB Server.
         *
         * @return {@link Boolean#TRUE} if server is healthy otherwise return {@link Boolean#FALSE}
         */
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
                Console.WriteLine("Error: " + e.Message);
                return false;
            }
        }

        /**
         * Return the version of the connected InfluxDB Server.
         *
         * @return the version String, otherwise unknown.
         */
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
                throw new InfluxException(new QueryErrorResponse(0, e.Message));
            }
        }
        
        private string GetVersion(RequestResult responseHttp) 
        {
            Arguments.CheckNotNull(responseHttp, "responseHttp");

            IEnumerable<string> value;

            responseHttp.ResponseHeaders.TryGetValue("X-Influxdb-Version", out value);

            if (value != null)
            {
                string version = value.FirstOrDefault();

                if (!string.IsNullOrEmpty(version))
                {
                    return version;
                }
            }

            return "unknown";
        }
    }
}