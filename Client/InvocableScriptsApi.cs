using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;

namespace InfluxDB.Client
{
    public class InvocableScriptsApi
    {
        private readonly InvocableScriptsService _service;

        protected internal InvocableScriptsApi(InvocableScriptsService service)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
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
    }
}