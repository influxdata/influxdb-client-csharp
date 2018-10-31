using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Flux.Client.Client;
using Flux.Flux.Options;
using Newtonsoft.Json;
using Platform.Common.Flux.Csv;
using Platform.Common.Flux.Domain;
using Platform.Common.Flux.Error;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;

namespace Flux.Client
{
    public class FluxClient : AbstractClient
    {
        private readonly FluxConnectionOptions _options;

        public FluxClient(FluxConnectionOptions options) : base(new DefaultClientIO(options))
        {
            _options = options ?? throw new ArgumentException("options");
        }

        /**
         * Executes the Flux query against the InfluxDB and synchronously map whole response to {@code List<FluxTable>}.
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
         * Executes the Flux query against the InfluxDB and asynchronously stream {@link FluxRecord}s
         * to {@code onNext} consumer.
         *
         * @param query  the flux query to execute
         * @param onNext the callback to consume the FluxRecord result with capability to discontinue a streaming query
         */
        /*public async Task Query(string query, Action<ICancellable, FluxRecord> onNext)
        {
            Arguments.CheckNonEmptyString(query, "query");
            Arguments.CheckNotNull(onNext, "onNext");

            await Query(query, onNext, onError, )
        }

        /**
         * Executes the Flux query against the InfluxDB and asynchronously stream {@link FluxRecord}s
         * to {@code onNext} consumer.
         *
         * @param query   the flux query to execute
         * @param onNext  the callback to consume FluxRecord result with capability to discontinue a streaming query
         * @param onError the callback to consume any error notification
         */
        /*public async Task Query(string query, Action<ICancellable, FluxRecord> onNext, Action onError)
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

            var consumer = new OnNextConsumer(onNext);

            await Query(query, GetDefaultDialect(), consumer, onError, onComplete);
        }
        
        public async Task Query(string query, 
                        string dialect, 
                        FluxCsvParser.IFluxResponseConsumer responseConsumer,
                        Action<Exception> onError,
                        Action onComplete)
        {
            var message = FluxService.Query(CreateBody(dialect, query));

            await Query(message, responseConsumer, onError, onComplete);
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
                var responseHttp = await _client.DoRequest(FluxService.Ping()).ConfigureAwait(false);
                
                AbstractClient.RaiseForInfluxError(responseHttp);
                
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
        public string Version()
        {
            throw new NotImplementedException();
        }
    }
}