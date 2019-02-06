using System.Net.Http;
using System.Threading.Tasks;

namespace InfluxDB.Client.Core.Internal
{
    /// <summary>
    /// Handles actual I/O for a <see cref="FluxClient"/>.
    /// </summary>
    public interface IClientIo
    {
        Task<RequestResult> DoRequest(HttpRequestMessage message);
    }
}