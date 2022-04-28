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
    public class InvokableScriptsApi : AbstractQueryClient
    {
        private readonly InvokableScriptsService _service;

        protected internal InvokableScriptsApi(InvokableScriptsService service, IFluxResultMapper mapper) : base(mapper,
            new FluxCsvParser(FluxCsvParser.ResponseMode.OnlyNames))
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
            RestClient = service.Configuration.ApiClient.RestClient;
        }

        /// <summary>
        /// Create a script.
        /// </summary>
        /// <param name="createRequest">The script to create.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The created script.</returns>
        public Task<Script> CreateScriptAsync(ScriptCreateRequest createRequest,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(createRequest, nameof(createRequest));

            return _service.PostScriptsAsync(createRequest, cancellationToken);
        }

        /// <summary>
        /// Update a script.
        /// </summary>
        /// <param name="scriptId">The ID of the script to update. (required)</param>
        /// <param name="updateRequest">Script updates to apply (required)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The updated script.</returns>
        public Task<Script> UpdateScriptAsync(string scriptId, ScriptUpdateRequest updateRequest,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(scriptId, nameof(scriptId));
            Arguments.CheckNotNull(updateRequest, nameof(updateRequest));

            return _service.PatchScriptsIDAsync(scriptId, updateRequest, cancellationToken);
        }

        /// <summary>
        /// Delete a script.
        /// </summary>
        /// <param name="scriptId">The ID of the script to delete. (required)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteScriptAsync(string scriptId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(scriptId, nameof(scriptId));

            return _service.DeleteScriptsIDAsync(scriptId, cancellationToken);
        }

        /// <summary>
        /// List scripts.
        /// </summary>
        /// <param name="offset">The offset for pagination.</param>
        /// <param name="limit">The number of scripts to return.</param>
        /// <param name="cancellationToken"></param>
        /// <returns>[Script]</returns>
        public Task<List<Script>> FindScriptsAsync(int? offset = null, int? limit = null,
            CancellationToken cancellationToken = default)
        {
            return _service.GetScriptsAsync(limit, offset, cancellationToken)
                .ContinueWith(t => t.Result._Scripts, cancellationToken);
        }

        /// <summary>
        /// Invoke a script and return result as a [FluxTable].
        /// </summary>
        /// <param name="scriptId">The ID of the script to invoke. (required)</param>
        /// <param name="bindParams">Represent key/value pairs parameters to be injected into script</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>[FluxTable]</returns>
        public async Task<List<FluxTable>> InvokeScriptAsync(string scriptId,
            Dictionary<string, object> bindParams = default,
            CancellationToken cancellationToken = default)
        {
            var consumer = new FluxCsvParser.FluxResponseConsumerTable();

            await InvokeScript(scriptId, consumer, ErrorConsumer, EmptyAction, bindParams, cancellationToken)
                .ConfigureAwait(false);

            return consumer.Tables;
        }

        /// <summary>
        /// Invoke a script and return result as a [T].
        /// </summary>
        /// <param name="scriptId">The ID of the script to invoke. (required)</param>
        /// <param name="bindParams">Represent key/value pairs parameters to be injected into script</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>[T]</returns>
        public async Task<List<T>> InvokeScriptMeasurementsAsync<T>(string scriptId,
            Dictionary<string, object> bindParams = default,
            CancellationToken cancellationToken = default)
        {
            var measurements = new List<T>();

            var consumer = new FluxResponseConsumerPoco<T>(poco => { measurements.Add(poco); }, Mapper);

            await Query(CreateRequest(scriptId, bindParams), consumer, ErrorConsumer, EmptyAction, cancellationToken)
                .ConfigureAwait(false);

            return measurements;
        }

        /// <summary>
        /// Invoke a script and return result as a raw string.
        /// </summary>
        /// <param name="scriptId">The ID of the script to invoke. (required)</param>
        /// <param name="bindParams">Represent key/value pairs parameters to be injected into script</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>String</returns>
        public async Task<string> InvokeScriptRawAsync(string scriptId, Dictionary<string, object> bindParams = default,
            CancellationToken cancellationToken = default)
        {
            var rows = new List<string>();

            void Consumer(string row)
            {
                rows.Add(row);
            }

            await QueryRaw(CreateRequest(scriptId, bindParams), Consumer, ErrorConsumer, EmptyAction, cancellationToken)
                .ConfigureAwait(false);

            return string.Join("\n", rows);
        }

        /// <summary>
        /// Invoke a script and return result as a stream of FluxRecord.
        /// </summary>
        /// <param name="scriptId">The ID of the script to invoke. (required)</param>
        /// <param name="bindParams">Represent key/value pairs parameters to be injected into script</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>stream of FluxRecord</returns>
        public async IAsyncEnumerable<FluxRecord> InvokeScriptEnumerableAsync(string scriptId,
            Dictionary<string, object> bindParams = default,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var requestMessage = CreateRequest(scriptId, bindParams);

            await foreach (var record in QueryEnumerable(requestMessage, it => it, cancellationToken)
                               .ConfigureAwait(false))
                yield return record;
        }

        /// <summary>
        /// Invoke a script and return result as a stream of Measurement.
        /// </summary>
        /// <param name="scriptId">The ID of the script to invoke. (required)</param>
        /// <param name="bindParams">Represent key/value pairs parameters to be injected into script</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>stream of Measurement</returns>
        public async IAsyncEnumerable<T> InvokeScriptMeasurementsEnumerableAsync<T>(string scriptId,
            Dictionary<string, object> bindParams = default,
            [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            var requestMessage = CreateRequest(scriptId, bindParams);

            await foreach (var record in QueryEnumerable(requestMessage, it => Mapper.ConvertToEntity<T>(it),
                               cancellationToken).ConfigureAwait(false))
                yield return record;
        }

        protected override void BeforeIntercept(RestRequest request)
        {
            _service.Configuration.ApiClient.BeforeIntercept(request);
        }

        protected override T AfterIntercept<T>(int statusCode, Func<IEnumerable<HeaderParameter>> headers, T body)
        {
            return _service.Configuration.ApiClient.AfterIntercept(statusCode, headers, body);
        }

        private Task InvokeScript(string scriptId, FluxCsvParser.IFluxResponseConsumer consumer,
            Action<Exception> onError = null, Action onComplete = null, Dictionary<string, object> bindParams = default,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(scriptId, nameof(scriptId));
            Arguments.CheckNotNull(consumer, nameof(consumer));

            var requestMessage = CreateRequest(scriptId, bindParams);

            return Query(requestMessage, consumer, onError ?? ErrorConsumer, onComplete ?? EmptyAction,
                cancellationToken);
        }

        private Func<Func<HttpResponseMessage, RestResponse>, RestRequest> CreateRequest(string scriptId,
            Dictionary<string, object> bindParams = default)
        {
            Arguments.CheckNonEmptyString(scriptId, nameof(scriptId));

            return advancedResponseWriter => _service
                .PostScriptsIDInvokeWithRestRequest(scriptId, new ScriptInvocationParams(bindParams))
                .AddAdvancedResponseHandler(advancedResponseWriter);
        }
    }
}