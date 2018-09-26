using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flux.flux.dto;
using Flux.Flux.Options;

/**
 * The client for the Flux service.
 */
namespace Flux.Flux
{
    public interface IFluxClient
    {
        /**
         * Execute a Flux against the Flux service and synchronously map whole response to {@link FluxTable}s.
         *
         * @param query the flux query to execute
         * @return {@code List<FluxTable>} which are matched the query
         */
    
        List<FluxTable> Flux(string query);
    
        /**
         * Execute a Flux against the Flux service and return the Flux server HTTP response.
         *
         * @param query the flux query to execute
         * @return {@code Response<ResponseBody>} raw response which are matched the query
         */
        HttpResponseMessage FluxRaw(string query);
    
        /**
         * Execute a Flux against the Flux service and return the Flux server HTTP response.
         *
         * @param options the options for the query
         * @param query   the flux query to execute
         * @return {@code Response<ResponseBody>} raw response which are matched the query
         */
        HttpResponseMessage FluxRaw(string query, FluxOptions options);
    
        /**
         * Execute a Flux against the Flux service and asynchronous stream HTTP response to {@code onResponse}.
         * <p>
         * The callback is call only once.
         *
         * @param query      the flux query to execute
         * @param onResponse callback to consume raw response which are matched the query
         */
        void FluxRaw(string query, Action<HttpResponseMessage> onResponse);
    
        /**
         * Execute a Flux against the Flux service and asynchronous stream HTTP response to {@code onResponse}.
         * <p>
         * The callback is call only once.
         *
         * @param query      the flux query to execute
         * @param onResponse callback to consume raw response which are matched the query
         * @param onFailure  callback to consume error notification invoked when a network exception occurred
         *                   talking to the server
         */
        void FluxRaw(string query, Action<HttpResponseMessage> onResponse, Action onFailure);
    
        /**
         * Execute a Flux against the Flux service.
         * <p>
         * The callback is call only once.
         *
         * @param query      the flux query to execute
         * @param onResponse callback to consume raw response which are matched the query
         * @param options    the options for the query
         */
        void FluxRaw(string query, FluxOptions options, Action<HttpResponseMessage> onResponse);
    
        /**
         * Execute a Flux against the Flux service.
         * <p>
         * The callback is call only once.
         *
         * @param query      the flux query to execute
         * @param onResponse callback to consume raw response which are matched the query
         * @param options    the options for the query
         * @param onFailure  callback to consume error notification invoked when a network exception occurred
         *                   talking to the server
         */
        void FluxRaw(string query, FluxOptions options, Action<HttpResponseMessage> onResponse, Action onFailure);
    
        /**
         * Check the status of Flux Server.
         *
         * @return {@link Boolean#TRUE} if server is healthy otherwise return {@link Boolean#FALSE}
         */
        Task <bool> Ping();
    }
}