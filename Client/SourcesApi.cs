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
        public Task<Source> CreateSourceAsync(Source source)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return _service.PostSourcesAsync(source);
        }

        /// <summary>
        /// Update a Source.
        /// </summary>
        /// <param name="source">source update to apply</param>
        /// <returns>updated source</returns>
        public Task<Source> UpdateSourceAsync(Source source)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return _service.PatchSourcesIDAsync(source.Id, source);
        }

        /// <summary>
        /// Delete a source.
        /// </summary>
        /// <param name="sourceId">ID of source to delete</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteSourceAsync(string sourceId)
        {
            Arguments.CheckNotNull(sourceId, nameof(sourceId));

            return _service.DeleteSourcesIDAsync(sourceId);
        }

        /// <summary>
        /// Delete a source.
        /// </summary>
        /// <param name="source">source to delete</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteSourceAsync(Source source)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return DeleteSourceAsync(source.Id);
        }

        /// <summary>
        /// Clone a source.
        /// </summary>
        /// <param name="clonedName">name of cloned source</param>
        /// <param name="sourceId">ID of source to clone</param>
        /// <returns>cloned source</returns>
        public async Task<Source> CloneSourceAsync(string clonedName, string sourceId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            var source = await FindSourceByIdAsync(sourceId);
            return await CloneSourceAsync(clonedName, source);
        }

        /// <summary>
        /// Clone a source.
        /// </summary>
        /// <param name="clonedName">name of cloned source</param>
        /// <param name="source">source to clone</param>
        /// <returns>cloned source</returns>
        public Task<Source> CloneSourceAsync(string clonedName, Source source)
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

            return CreateSourceAsync(cloned);
        }

        /// <summary>
        /// Retrieve a source.
        /// </summary>
        /// <param name="sourceId">ID of source to get</param>
        /// <returns>source details</returns>
        public Task<Source> FindSourceByIdAsync(string sourceId)
        {
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            return _service.GetSourcesIDAsync(sourceId);
        }

        /// <summary>
        /// Get all sources.
        /// </summary>
        /// <returns>A list of sources</returns>
        public async Task<List<Source>> FindSourcesAsync()
        {
            var response = await _service.GetSourcesAsync();
            return response._Sources;
        }

        /// <summary>
        /// Get a sources buckets (will return dbrps in the form of buckets if it is a v1 source).
        /// </summary>
        /// <param name="source">filter buckets to a specific source</param>
        /// <returns>The buckets for source. If source does not exist than return null.</returns>
        public Task<List<Bucket>> FindBucketsBySourceAsync(Source source)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return FindBucketsBySourceIdAsync(source.Id);
        }

        /// <summary>
        /// Get a sources buckets (will return dbrps in the form of buckets if it is a v1 source).
        /// </summary>
        /// <param name="sourceId">filter buckets to a specific source ID</param>
        /// <returns>The buckets for source. If source does not exist than return null.</returns>
        public async Task<List<Bucket>> FindBucketsBySourceIdAsync(string sourceId)
        {
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            var response = await _service.GetSourcesIDBucketsAsync(sourceId);
            return response._Buckets;
        }

        /// <summary>
        /// Get a sources health.
        /// </summary>
        /// <param name="source">source to check health</param>
        /// <returns>health of source</returns>
        public Task<HealthCheck> HealthAsync(Source source)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return HealthAsync(source.Id);

        }

        /// <summary>
        /// Get a sources health.
        /// </summary>
        /// <param name="sourceId">source to check health</param>
        /// <returns>health of source</returns>
        public Task<HealthCheck> HealthAsync(string sourceId)
        {
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            return InfluxDBClient.GetHealthAsync(_service.GetSourcesIDHealthAsync(sourceId));
        }
    }
}