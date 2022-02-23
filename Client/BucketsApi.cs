using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using InfluxDB.Client.Domain;

namespace InfluxDB.Client
{
    public class BucketsApi
    {
        private readonly BucketsService _service;

        protected internal BucketsApi(BucketsService service)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="bucket">bucket to create</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created Bucket</returns>
        public Task<Bucket> CreateBucketAsync(Bucket bucket, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            var postBucket = new PostBucketRequest(bucket.OrgID, bucket.Name, bucket.Description,
                bucket.Rp, bucket.RetentionRules);

            return _service.PostBucketsAsync(postBucket, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="bucket">bucket to create</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created Bucket</returns>
        public Task<Bucket> CreateBucketAsync(PostBucketRequest bucket, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return _service.PostBucketsAsync(bucket, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="organization">owner of the bucket</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created Bucket</returns>
        public Task<Bucket> CreateBucketAsync(string name, Organization organization,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNotNull(organization, nameof(organization));

            return CreateBucketAsync(name, organization.Id, cancellationToken);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="bucketRetentionRules">retention rule of the bucket</param>
        /// <param name="organization">owner of the bucket</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created Bucket</returns>
        public Task<Bucket> CreateBucketAsync(string name, BucketRetentionRules bucketRetentionRules,
            Organization organization, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNotNull(organization, nameof(organization));

            return CreateBucketAsync(name, bucketRetentionRules, organization.Id, cancellationToken);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="orgId">owner of the bucket</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created Bucket</returns>
        public Task<Bucket> CreateBucketAsync(string name, string orgId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return CreateBucketAsync(name, default, orgId, cancellationToken);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="bucketRetentionRules">retention rule of the bucket</param>
        /// <param name="orgId">owner of the bucket</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created Bucket</returns>
        public Task<Bucket> CreateBucketAsync(string name, BucketRetentionRules bucketRetentionRules, string orgId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var bucket = new Bucket(null, name, null, orgId, null, null, new List<BucketRetentionRules>());
            if (bucketRetentionRules != null)
            {
                bucket.RetentionRules.Add(bucketRetentionRules);
            }

            return CreateBucketAsync(bucket, cancellationToken);
        }

        /// <summary>
        /// Update a bucket name and retention.
        /// </summary>
        /// <param name="bucket">bucket update to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>bucket updated</returns>
        public Task<Bucket> UpdateBucketAsync(Bucket bucket, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            var retentionRules = bucket.RetentionRules.Select(rules =>
            {
                Enum.TryParse(rules.Type.ToString(), true, out PatchRetentionRule.TypeEnum type);
                return new PatchRetentionRule(type, rules.EverySeconds, rules.ShardGroupDurationSeconds);
            }).ToList();

            var request = new PatchBucketRequest(bucket.Name, bucket.Description, retentionRules);
            return _service.PatchBucketsIDAsync(bucket.Id, request, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Delete a bucket.
        /// </summary>
        /// <param name="bucketId">ID of bucket to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteBucketAsync(string bucketId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return _service.DeleteBucketsIDAsync(bucketId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Delete a bucket.
        /// </summary>
        /// <param name="bucket">bucket to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteBucketAsync(Bucket bucket, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return DeleteBucketAsync(bucket.Id, cancellationToken);
        }

        /// <summary>
        /// Clone a bucket.
        /// </summary>
        /// <param name="clonedName">name of cloned bucket</param>
        /// <param name="bucketId">ID of bucket to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned bucket</returns>
        public async Task<Bucket> CloneBucketAsync(string clonedName, string bucketId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            var bucket = await FindBucketByIdAsync(bucketId, cancellationToken).ConfigureAwait(false);
            return await CloneBucketAsync(clonedName, bucket, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a bucket.
        /// </summary>
        /// <param name="clonedName">name of cloned bucket</param>
        /// <param name="bucket">bucket to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned bucket</returns>
        public async Task<Bucket> CloneBucketAsync(string clonedName, Bucket bucket,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(bucket, nameof(bucket));

            var cloned = new Bucket(null, clonedName, null, bucket.OrgID, bucket.Rp, null, bucket.RetentionRules);

            var created = await CreateBucketAsync(cloned, cancellationToken).ConfigureAwait(false);

            var labels = await GetLabelsAsync(bucket, cancellationToken).ConfigureAwait(false);
            foreach (var label in labels) await AddLabelAsync(label, created, cancellationToken).ConfigureAwait(false);

            return created;
        }

        /// <summary>
        /// Retrieve a bucket.
        /// </summary>
        /// <param name="bucketId">ID of bucket to get</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Bucket Details</returns>
        public Task<Bucket> FindBucketByIdAsync(string bucketId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return _service.GetBucketsIDAsync(bucketId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Retrieve a bucket.
        /// </summary>
        /// <param name="bucketName">Name of bucket to get</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Bucket Details</returns>
        public async Task<Bucket> FindBucketByNameAsync(string bucketName,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucketName, nameof(bucketName));

            var buckets = await _service
                .GetBucketsAsync(null, null, null, null, null, null, bucketName, cancellationToken: cancellationToken)
                .ConfigureAwait(false);

            return buckets._Buckets.FirstOrDefault();
        }

        /// <summary>
        /// List all buckets for specified organization.
        /// </summary>
        /// <param name="organization">filter buckets to a specific organization</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of buckets</returns>
        public Task<List<Bucket>> FindBucketsByOrganizationAsync(Organization organization,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return FindBucketsByOrgNameAsync(organization.Name, cancellationToken);
        }

        /// <summary>
        /// List all buckets for specified orgId.
        /// </summary>
        /// <param name="orgName">filter buckets to a specific organization</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of buckets</returns>
        public Task<List<Bucket>> FindBucketsByOrgNameAsync(string orgName,
            CancellationToken cancellationToken = default)
        {
            return FindBucketsAsync(org: orgName, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// List all buckets.
        /// </summary>
        /// <param name="offset"> (optional)</param>
        /// <param name="limit"> (optional, default to 20)</param>
        /// <param name="after">The last resource ID from which to seek from (but not including). This is to be used instead of &#x60;offset&#x60;. (optional)</param>
        /// <param name="org">The name of the organization. (optional)</param>
        /// <param name="orgID">The organization ID. (optional)</param>
        /// <param name="name">Only returns buckets with a specific name. (optional)</param>
        /// <param name="id">Only returns buckets with a specific ID. (optional)</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List all buckets</returns>
        public async Task<List<Bucket>> FindBucketsAsync(int? offset = null, int? limit = null, string after = null,
            string org = null, string orgID = null, string name = null, string id = null,
            CancellationToken cancellationToken = default)
        {
            var buckets = await GetBucketsAsync(offset, limit, after, org, orgID, name, id, cancellationToken)
                .ConfigureAwait(false);
            return buckets._Buckets;
        }

        /// <summary>
        /// List all buckets.
        /// </summary>
        /// <param name="findOptions">the find options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List all buckets</returns>
        public Task<Buckets> FindBucketsAsync(FindOptions findOptions, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return GetBucketsAsync(findOptions.Offset, findOptions.Limit, findOptions.After,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// List all members of a bucket.
        /// </summary>
        /// <param name="bucket">bucket of the members</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all members of a bucket</returns>
        public Task<List<ResourceMember>> GetMembersAsync(Bucket bucket, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return GetMembersAsync(bucket.Id, cancellationToken);
        }

        /// <summary>
        /// List all members of a bucket.
        /// </summary>
        /// <param name="bucketId">ID of bucket to get members</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all members of a bucket</returns>
        public async Task<List<ResourceMember>> GetMembersAsync(string bucketId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            var members = await _service.GetBucketsIDMembersAsync(bucketId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return members.Users;
        }

        /// <summary>
        /// Add a bucket member.
        /// </summary>
        /// <param name="member">the member of a bucket</param>
        /// <param name="bucket">the bucket of a member</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created mapping</returns>
        public Task<ResourceMember> AddMemberAsync(User member, Bucket bucket,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(member, nameof(member));

            return AddMemberAsync(member.Id, bucket.Id, cancellationToken);
        }

        /// <summary>
        /// Add a bucket member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created mapping</returns>
        public Task<ResourceMember> AddMemberAsync(string memberId, string bucketId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            var mapping = new AddResourceMemberRequestBody(memberId);

            return _service.PostBucketsIDMembersAsync(bucketId, mapping, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Removes a member from a bucket.
        /// </summary>
        /// <param name="member">the member of a bucket</param>
        /// <param name="bucket">the bucket of a member</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>member removed</returns>
        public Task DeleteMemberAsync(User member, Bucket bucket, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(member, nameof(member));

            return DeleteMemberAsync(member.Id, bucket.Id, cancellationToken);
        }

        /// <summary>
        /// Removes a member from a bucket.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>member removed</returns>
        public Task DeleteMemberAsync(string memberId, string bucketId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return _service.DeleteBucketsIDMembersIDAsync(memberId, bucketId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// List all owners of a bucket.
        /// </summary>
        /// <param name="bucket">bucket of the owners</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all owners of a bucket</returns>
        public Task<List<ResourceOwner>> GetOwnersAsync(Bucket bucket, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return GetOwnersAsync(bucket.Id, cancellationToken);
        }

        /// <summary>
        /// List all owners of a bucket.
        /// </summary>
        /// <param name="bucketId">ID of a bucket to get owners</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all owners of a bucket</returns>
        public async Task<List<ResourceOwner>> GetOwnersAsync(string bucketId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            var members = await _service.GetBucketsIDOwnersAsync(bucketId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return members.Users;
        }

        /// <summary>
        /// Add a bucket owner.
        /// </summary>
        /// <param name="owner">the owner of a bucket</param>
        /// <param name="bucket">the bucket of a owner</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created mapping</returns>
        public Task<ResourceOwner> AddOwnerAsync(User owner, Bucket bucket,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(owner, nameof(owner));

            return AddOwnerAsync(owner.Id, bucket.Id, cancellationToken);
        }

        /// <summary>
        /// Add a bucket owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created mapping</returns>
        public Task<ResourceOwner> AddOwnerAsync(string ownerId, string bucketId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var mapping = new AddResourceMemberRequestBody(ownerId);

            return _service.PostBucketsIDOwnersAsync(bucketId, mapping, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Removes a owner from a bucket.
        /// </summary>
        /// <param name="owner">the owner of a bucket</param>
        /// <param name="bucket">the bucket of a owner</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>owner removed</returns>
        public Task DeleteOwnerAsync(User owner, Bucket bucket, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(owner, nameof(owner));

            return DeleteOwnerAsync(owner.Id, bucket.Id, cancellationToken);
        }

        /// <summary>
        /// Removes a owner from a bucket.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>owner removed</returns>
        public Task DeleteOwnerAsync(string ownerId, string bucketId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            return _service.DeleteBucketsIDOwnersIDAsync(ownerId, bucketId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// List all labels of a bucket.
        /// </summary>
        /// <param name="bucket">bucket of the labels</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all labels of a bucket</returns>
        public Task<List<Label>> GetLabelsAsync(Bucket bucket, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return GetLabelsAsync(bucket.Id, cancellationToken);
        }

        /// <summary>
        /// List all labels of a bucket.
        /// </summary>
        /// <param name="bucketId">ID of a bucket to get labels</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the List all labels of a bucket</returns>
        public async Task<List<Label>> GetLabelsAsync(string bucketId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            var response = await _service.GetBucketsIDLabelsAsync(bucketId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Labels;
        }

        /// <summary>
        /// Add a bucket label.
        /// </summary>
        /// <param name="label">the label of a bucket</param>
        /// <param name="bucket">the bucket of a label</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>added label</returns>
        public Task<Label> AddLabelAsync(Label label, Bucket bucket, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(label, nameof(label));

            return AddLabelAsync(label.Id, bucket.Id, cancellationToken);
        }

        /// <summary>
        /// Add a bucket label.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabelAsync(string labelId, string bucketId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var mapping = new LabelMapping(labelId);

            var response = await _service
                .PostBucketsIDLabelsAsync(bucketId, mapping, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Label;
        }

        /// <summary>
        /// Removes a label from a bucket.
        /// </summary>
        /// <param name="label">the label of a bucket</param>
        /// <param name="bucket">the bucket of a owner</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteLabelAsync(Label label, Bucket bucket, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(label, nameof(label));

            return DeleteLabelAsync(label.Id, bucket.Id, cancellationToken);
        }

        /// <summary>
        /// Removes a label from a bucket.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteLabelAsync(string labelId, string bucketId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return _service.DeleteBucketsIDLabelsIDAsync(bucketId, labelId, cancellationToken: cancellationToken);
        }

        private Task<Buckets> GetBucketsAsync(int? offset = null, int? limit = null, string after = null,
            string org = null, string orgID = null, string name = null, string id = null,
            CancellationToken cancellationToken = default)
        {
            return _service.GetBucketsAsync(offset: offset, limit: limit, after: after, org: org, orgID: orgID,
                name: name, id: id, cancellationToken: cancellationToken);
        }
    }
}