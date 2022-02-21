using System;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;

namespace InfluxDB.Client
{
    public class DeleteApi
    {
        private readonly DeleteService _service;

        protected internal DeleteApi(DeleteService service)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
        }

        /// <summary>
        /// Delete Time series data from InfluxDB.
        /// </summary>
        /// <param name="start">Start time</param>
        /// <param name="stop">Stop time</param>
        /// <param name="predicate">Sql where like delete statement</param>
        /// <param name="bucket">The bucket from which data will be deleted</param>
        /// <param name="org">The organization of the above bucket</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task Delete(DateTime start, DateTime stop, string predicate, Bucket bucket, Organization org,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(start, "Start is required");
            Arguments.CheckNotNull(stop, "Stop is required");
            Arguments.CheckNotNull(predicate, "Predicate is required");
            Arguments.CheckNotNull(bucket, "Bucket is required");
            Arguments.CheckNotNull(org, "Organization is required");

            return Delete(start, stop, predicate, bucket.Id, org.Id, cancellationToken);
        }

        /// <summary>
        /// Delete Time series data from InfluxDB.
        /// </summary>
        /// <param name="start">Start time</param>
        /// <param name="stop">Stop time</param>
        /// <param name="predicate">Sql where like delete statement</param>
        /// <param name="bucket">The bucket from which data will be deleted</param>
        /// <param name="org">The organization of the above bucket</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task Delete(DateTime start, DateTime stop, string predicate, string bucket, string org,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(start, "Start is required");
            Arguments.CheckNotNull(stop, "Stop is required");
            Arguments.CheckNotNull(predicate, "Predicate is required");
            Arguments.CheckNonEmptyString(bucket, "Bucket is required");
            Arguments.CheckNonEmptyString(org, "Organization is required");

            var predicateRequest = new DeletePredicateRequest(start, stop, predicate);

            return Delete(predicateRequest, bucket, org, cancellationToken);
        }

        /// <summary>
        /// Delete Time series data from InfluxDB.
        /// </summary>
        /// <param name="predicate">Predicate delete request</param>
        /// <param name="bucket">The bucket from which data will be deleted</param>
        /// <param name="org">The organization of the above bucket</param>
        /// <param name="cancellationToken">Cancellation token</param>
        public Task Delete(DeletePredicateRequest predicate, string bucket, string org,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(predicate, "Predicate is required");
            Arguments.CheckNonEmptyString(bucket, "Bucket is required");
            Arguments.CheckNonEmptyString(org, "Organization is required");

            return _service.PostDeleteAsync(predicate, null, org, bucket, null, null, cancellationToken);
        }
    }
}