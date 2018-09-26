using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Flux.Flux.Options;

namespace Flux.Flux.Client
{
    /** <summary>
     * Handles actual I/O for a <see cref="FluxClient"/>.
     * </summary>
     */
    public interface IClientIO
    {
        Task<RequestResult> DoRequest(HttpRequestMessage message);
    }
}