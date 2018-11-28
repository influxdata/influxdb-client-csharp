using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;
using Task = System.Threading.Tasks.Task;

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
        
        /// <summary>
        /// Update a Source.
        /// </summary>
        /// <param name="source">source update to apply</param>
        /// <returns>updated source</returns>
        public async Task<Source> UpdateSource(Source source)
        {
            Arguments.CheckNotNull(source, "source");

            var result = await Patch(source, $"/api/v2/sources/{source.Id}");

            return Call<Source>(result);
        }
        
        /// <summary>
        /// Delete a source.
        /// </summary>
        /// <param name="sourceId">ID of source to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteSource(string sourceId)
        {
            Arguments.CheckNotNull(sourceId, "Source ID");

            var request = await Delete($"/api/v2/sources/{sourceId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        /// Delete a source.
        /// </summary>
        /// <param name="source">source to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteSource(Source source)
        {
            Arguments.CheckNotNull(source, "source");

            await DeleteSource(source.Id);
        }
        
         
        /// <summary>
        /// Retrieve a source.
        /// </summary>
        /// <param name="sourceId">ID of source to get</param>
        /// <returns>source details</returns>
        public async Task<Source> FindSourceById(string sourceId)
        {
            Arguments.CheckNonEmptyString(sourceId, "Source ID");

            var request = await Get($"/api/v2/sources/{sourceId}");

            return Call<Source>(request, "source not found");
        }
    }
}