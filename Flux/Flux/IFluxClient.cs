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
         * Executes the Flux query against the InfluxDB and synchronously map whole response to {@code List<FluxTable>}.
         * <p>
         *
         * @param query the flux query to execute
         * @return {@code List<FluxTable>} which are matched the query
         */
        Task<List<FluxTable>> Query(string query);


        /**
         * Check the status of InfluxDB Server.
         *
         * @return {@link Boolean#TRUE} if server is healthy otherwise return {@link Boolean#FALSE}
         */
        Task<bool> Ping();

        /**
         * Return the version of the connected InfluxDB Server.
         *
         * @return the version String, otherwise unknown.
         */
        String Version();
    }
}