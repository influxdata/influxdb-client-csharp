using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;

namespace InfluxData.Platform.Client.Client
{
    public class SourceClient: AbstractClient
    {
        protected internal SourceClient(DefaultClientIo client) : base(client)
        {
        }
        
        /// <summary>
        /// Creates a new Source and sets <see cref="Source.Id"/> with the new identifier.
        /// </summary>
        /// <param name="source">source to create</param>
        /// <returns>created Source</returns>
        public async Task<Source> CreateSource(Source source)
        {
            Arguments.CheckNotNull(source, "source");

            var response = await Post(source, "/api/v2/sources");

            return Call<Source>(response);
        }
    }
}