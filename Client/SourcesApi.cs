using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;

namespace InfluxDB.Client
{
    public class SourcesApi
    {
        private readonly SourcesService _service;

        protected internal SourcesApi(SourcesService service)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
        }

        /// <summary>
        /// Creates a new Source and sets <see cref="InfluxDB.Client.Api.Domain.Source.Id" /> with the new identifier.
        /// </summary>
        /// <param name="source">source to create</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created Source</returns>
        public Task<Source> CreateSourceAsync(Source source, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return _service.PostSourcesAsync(source, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Update a Source.
        /// </summary>
        /// <param name="source">source update to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>updated source</returns>
        public Task<Source> UpdateSourceAsync(Source source, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return _service.PatchSourcesIDAsync(source.Id, source, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Delete a source.
        /// </summary>
        /// <param name="sourceId">ID of source to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteSourceAsync(string sourceId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(sourceId, nameof(sourceId));

            return _service.DeleteSourcesIDAsync(sourceId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Delete a source.
        /// </summary>
        /// <param name="source">source to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteSourceAsync(Source source, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return DeleteSourceAsync(source.Id, cancellationToken);
        }

        /// <summary>
        /// Clone a source.
        /// </summary>
        /// <param name="clonedName">name of cloned source</param>
        /// <param name="sourceId">ID of source to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned source</returns>
        public async Task<Source> CloneSourceAsync(string clonedName, string sourceId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            var source = await FindSourceByIdAsync(sourceId, cancellationToken).ConfigureAwait(false);
            return await CloneSourceAsync(clonedName, source, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a source.
        /// </summary>
        /// <param name="clonedName">name of cloned source</param>
        /// <param name="source">source to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned source</returns>
        public Task<Source> CloneSourceAsync(string clonedName, Source source,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(source, nameof(source));

            var cloned = new Source
            {
                Name = clonedName,
                OrgID = source.OrgID,
                Default = source.Default,
                Type = source.Type,
                Url = source.Url,
                InsecureSkipVerify = source.InsecureSkipVerify,
                Telegraf = source.Telegraf,
                Token = source.Token,
                Username = source.Username,
                Password = source.Password,
                SharedSecret = source.SharedSecret,
                MetaUrl = source.MetaUrl,
                DefaultRP = source.DefaultRP
            };

            return CreateSourceAsync(cloned, cancellationToken);
        }

        /// <summary>
        /// Retrieve a source.
        /// </summary>
        /// <param name="sourceId">ID of source to get</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>source details</returns>
        public Task<Source> FindSourceByIdAsync(string sourceId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            return _service.GetSourcesIDAsync(sourceId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Get all sources.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of sources</returns>
        public async Task<List<Source>> FindSourcesAsync(CancellationToken cancellationToken = default)
        {
            var response = await _service.GetSourcesAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            return response._Sources;
        }

        /// <summary>
        /// Get a sources buckets (will return dbrps in the form of buckets if it is a v1 source).
        /// </summary>
        /// <param name="source">filter buckets to a specific source</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The buckets for source. If source does not exist than return null.</returns>
        public Task<List<Bucket>> FindBucketsBySourceAsync(Source source, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return FindBucketsBySourceIdAsync(source.Id, cancellationToken);
        }

        /// <summary>
        /// Get a sources buckets (will return dbrps in the form of buckets if it is a v1 source).
        /// </summary>
        /// <param name="sourceId">filter buckets to a specific source ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The buckets for source. If source does not exist than return null.</returns>
        public async Task<List<Bucket>> FindBucketsBySourceIdAsync(string sourceId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            var response = await _service.GetSourcesIDBucketsAsync(sourceId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response._Buckets;
        }

        /// <summary>
        /// Get a sources health.
        /// </summary>
        /// <param name="source">source to check health</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>health of source</returns>
        public Task<HealthCheck> HealthAsync(Source source, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return HealthAsync(source.Id, cancellationToken);
        }

        /// <summary>
        /// Get a sources health.
        /// </summary>
        /// <param name="sourceId">source to check health</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>health of source</returns>
        public Task<HealthCheck> HealthAsync(string sourceId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            return InfluxDBClient.GetHealthAsync(
                _service.GetSourcesIDHealthAsync(sourceId, cancellationToken: cancellationToken));
        }
    }
}