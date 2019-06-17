using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using Task = System.Threading.Tasks.Task;

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
        /// <returns>created Source</returns>
        public async Task<Source> CreateSource(Source source)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return await _service.PostSourcesAsync(source);
        }

        /// <summary>
        /// Update a Source.
        /// </summary>
        /// <param name="source">source update to apply</param>
        /// <returns>updated source</returns>
        public async Task<Source> UpdateSource(Source source)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return await _service.PatchSourcesIDAsync(source.Id, source);
        }

        /// <summary>
        /// Delete a source.
        /// </summary>
        /// <param name="sourceId">ID of source to delete</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteSource(string sourceId)
        {
            Arguments.CheckNotNull(sourceId, nameof(sourceId));

            await _service.DeleteSourcesIDAsync(sourceId);
        }

        /// <summary>
        /// Delete a source.
        /// </summary>
        /// <param name="source">source to delete</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteSource(Source source)
        {
            Arguments.CheckNotNull(source, nameof(source));

            await DeleteSource(source.Id);
        }

        /// <summary>
        /// Clone a source.
        /// </summary>
        /// <param name="clonedName">name of cloned source</param>
        /// <param name="sourceId">ID of source to clone</param>
        /// <returns>cloned source</returns>
        public async Task<Source> CloneSource(string clonedName, string sourceId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            return await FindSourceById(sourceId).ContinueWith(t => CloneSource(clonedName, t.Result)).Unwrap();
        }

        /// <summary>
        /// Clone a source.
        /// </summary>
        /// <param name="clonedName">name of cloned source</param>
        /// <param name="source">source to clone</param>
        /// <returns>cloned source</returns>
        public async Task<Source> CloneSource(string clonedName, Source source)
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

            return await CreateSource(cloned);
        }

        /// <summary>
        /// Retrieve a source.
        /// </summary>
        /// <param name="sourceId">ID of source to get</param>
        /// <returns>source details</returns>
        public async Task<Source> FindSourceById(string sourceId)
        {
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            return await _service.GetSourcesIDAsync(sourceId);
        }

        /// <summary>
        /// Get all sources.
        /// </summary>
        /// <returns>A list of sources</returns>
        public async Task<List<Source>> FindSources()
        {
            return await _service.GetSourcesAsync().ContinueWith(t => t.Result._Sources);
        }

        /// <summary>
        /// Get a sources buckets (will return dbrps in the form of buckets if it is a v1 source).
        /// </summary>
        /// <param name="source">filter buckets to a specific source</param>
        /// <returns>The buckets for source. If source does not exist than return null.</returns>
        public async Task<List<Bucket>> FindBucketsBySource(Source source)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return await FindBucketsBySourceId(source.Id);
        }

        /// <summary>
        /// Get a sources buckets (will return dbrps in the form of buckets if it is a v1 source).
        /// </summary>
        /// <param name="sourceId">filter buckets to a specific source ID</param>
        /// <returns>The buckets for source. If source does not exist than return null.</returns>
        public async Task<List<Bucket>> FindBucketsBySourceId(string sourceId)
        {
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            return await _service.GetSourcesIDBucketsAsync(sourceId).ContinueWith(t => t.Result._Buckets);
        }

        /// <summary>
        /// Get a sources health.
        /// </summary>
        /// <param name="source">source to check health</param>
        /// <returns>health of source</returns>
        public async Task<Check> Health(Source source)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return await Health(source.Id);

        }

        /// <summary>
        /// Get a sources health.
        /// </summary>
        /// <param name="sourceId">source to check health</param>
        /// <returns>health of source</returns>
        public async Task<Check> Health(string sourceId)
        {
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            return await InfluxDBClient.GetHealth(_service.GetSourcesIDHealthAsync(sourceId));
        }
    }
}