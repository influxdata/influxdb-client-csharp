using System;
using System.Collections.Generic;
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
        /// Creates a new Source and sets <see cref="InfluxDBClient.A.Api.Source.Id" /> with the new identifier.
        /// </summary>
        /// <param name="source">source to create</param>
        /// <returns>created Source</returns>
        public Source CreateSource(Source source)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return _service.PostSources(source);
        }

        /// <summary>
        /// Update a Source.
        /// </summary>
        /// <param name="source">source update to apply</param>
        /// <returns>updated source</returns>
        public Source UpdateSource(Source source)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return _service.PatchSourcesID(source.Id, source);
        }

        /// <summary>
        /// Delete a source.
        /// </summary>
        /// <param name="sourceId">ID of source to delete</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteSource(string sourceId)
        {
            Arguments.CheckNotNull(sourceId, nameof(sourceId));

            _service.DeleteSourcesID(sourceId);
        }

        /// <summary>
        /// Delete a source.
        /// </summary>
        /// <param name="source">source to delete</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteSource(Source source)
        {
            Arguments.CheckNotNull(source, nameof(source));

            DeleteSource(source.Id);
        }

        /// <summary>
        /// Clone a source.
        /// </summary>
        /// <param name="clonedName">name of cloned source</param>
        /// <param name="sourceId">ID of source to clone</param>
        /// <returns>cloned source</returns>
        public Source CloneSource(string clonedName, string sourceId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            var source = FindSourceById(sourceId);
            if (source == null) throw new InvalidOperationException($"NotFound Source with ID: {sourceId}");

            return CloneSource(clonedName, source);
        }

        /// <summary>
        /// Clone a source.
        /// </summary>
        /// <param name="clonedName">name of cloned source</param>
        /// <param name="source">source to clone</param>
        /// <returns>cloned source</returns>
        public Source CloneSource(string clonedName, Source source)
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

            return CreateSource(cloned);
        }

        /// <summary>
        /// Retrieve a source.
        /// </summary>
        /// <param name="sourceId">ID of source to get</param>
        /// <returns>source details</returns>
        public Source FindSourceById(string sourceId)
        {
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            return _service.GetSourcesID(sourceId);
        }

        /// <summary>
        /// Get all sources.
        /// </summary>
        /// <returns>A list of sources</returns>
        public List<Source> FindSources()
        {
            return _service.GetSources()._Sources;
        }

        /// <summary>
        /// Get a sources buckets (will return dbrps in the form of buckets if it is a v1 source).
        /// </summary>
        /// <param name="source">filter buckets to a specific source</param>
        /// <returns>The buckets for source. If source does not exist than return null.</returns>
        public List<Bucket> FindBucketsBySource(Source source)
        {
            Arguments.CheckNotNull(source, nameof(source));
            
            return FindBucketsBySourceId(source.Id);
        }

        /// <summary>
        /// Get a sources buckets (will return dbrps in the form of buckets if it is a v1 source).
        /// </summary>
        /// <param name="sourceId">filter buckets to a specific source ID</param>
        /// <returns>The buckets for source. If source does not exist than return null.</returns>
        public List<Bucket> FindBucketsBySourceId(string sourceId)
        {
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            return _service.GetSourcesIDBuckets(sourceId)._Buckets;
        }

        /// <summary>
        /// Get a sources health.
        /// </summary>
        /// <param name="source">source to check health</param>
        /// <returns>health of source</returns>
        public Check Health(Source source)
        {
            Arguments.CheckNotNull(source, nameof(source));

            return Health(source.Id);
        }

        /// <summary>
        /// Get a sources health.
        /// </summary>
        /// <param name="sourceId">source to check health</param>
        /// <returns>health of source</returns>
        public Check Health(string sourceId)
        {
            Arguments.CheckNonEmptyString(sourceId, nameof(sourceId));

            return InfluxDBClient.GetHealth(_service.GetSourcesIDHealthAsync(sourceId));
        }
    }
}