using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using InfluxDB.Client.Domain;
using Task = System.Threading.Tasks.Task;

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
        /// <returns>created Bucket</returns>
        public async Task<Bucket> CreateBucket(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return await _service.PostBucketsAsync(bucket);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="organization">owner of the bucket</param>
        /// <returns>created Bucket</returns>
        public async Task<Bucket> CreateBucket(string name, Organization organization)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNotNull(organization, nameof(organization));

            return await CreateBucket(name, organization.Id);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="bucketRetentionRules">retention rule of the bucket</param>
        /// <param name="organization">owner of the bucket</param>
        /// <returns>created Bucket</returns>
        public async Task<Bucket> CreateBucket(string name, BucketRetentionRules bucketRetentionRules,
            Organization organization)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNotNull(organization, nameof(organization));

            return await CreateBucket(name, bucketRetentionRules, organization.Id);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="orgId">owner of the bucket</param>
        /// <returns>created Bucket</returns>
        public async Task<Bucket> CreateBucket(string name, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return await CreateBucket(name, default(BucketRetentionRules), orgId);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="bucketRetentionRules">retention rule of the bucket</param>
        /// <param name="orgId">owner of the bucket</param>
        /// <returns>created Bucket</returns>
        public async Task<Bucket> CreateBucket(string name, BucketRetentionRules bucketRetentionRules, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var bucket = new Bucket(null, name, null, orgId, null, new List<BucketRetentionRules>());
            if (bucketRetentionRules != null) bucket.RetentionRules.Add(bucketRetentionRules);

            return await CreateBucket(bucket);
        }

        /// <summary>
        /// Update a bucket name and retention.
        /// </summary>
        /// <param name="bucket">bucket update to apply</param>
        /// <returns>bucket updated</returns>
        public async Task<Bucket> UpdateBucket(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return await _service.PatchBucketsIDAsync(bucket.Id, bucket);
        }

        /// <summary>
        /// Delete a bucket.
        /// </summary>
        /// <param name="bucketId">ID of bucket to delete</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteBucket(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            await _service.DeleteBucketsIDAsync(bucketId);
        }

        /// <summary>
        /// Delete a bucket.
        /// </summary>
        /// <param name="bucket">bucket to delete</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteBucket(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            await DeleteBucket(bucket.Id);
        }

        /// <summary>
        /// Clone a bucket.
        /// </summary>
        /// <param name="clonedName">name of cloned bucket</param>
        /// <param name="bucketId">ID of bucket to clone</param>
        /// <returns>cloned bucket</returns>
        public async Task<Bucket> CloneBucket(string clonedName, string bucketId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return await FindBucketById(bucketId).ContinueWith(t => t.Result)
                .ContinueWith(t => CloneBucket(clonedName, t.Result)).Unwrap();
        }

        /// <summary>
        /// Clone a bucket.
        /// </summary>
        /// <param name="clonedName">name of cloned bucket</param>
        /// <param name="bucket">bucket to clone</param>
        /// <returns>cloned bucket</returns>
        public async Task<Bucket> CloneBucket(string clonedName, Bucket bucket)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(bucket, nameof(bucket));

            var cloned = new Bucket(null, clonedName, null, bucket.OrgID, bucket.Rp, bucket.RetentionRules);

            return await CreateBucket(cloned).ContinueWith(created =>
            {
                //
                // Add labels
                //
                return GetLabels(bucket)
                    .ContinueWith(labels => { return labels.Result.Select(label => AddLabel(label, created.Result)); })
                    .ContinueWith(async tasks =>
                    {
                        await Task.WhenAll(tasks.Result);
                        return created.Result;
                    })
                    .Unwrap();
            }).Unwrap();
        }

        /// <summary>
        /// Retrieve a bucket.
        /// </summary>
        /// <param name="bucketId">ID of bucket to get</param>
        /// <returns>Bucket Details</returns>
        public async Task<Bucket> FindBucketById(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return await _service.GetBucketsIDAsync(bucketId);
        }

        /// <summary>
        /// Retrieve a bucket.
        /// </summary>
        /// <param name="bucketName">Name of bucket to get</param>
        /// <returns>Bucket Details</returns>
        public async Task<Bucket> FindBucketByName(string bucketName)
        {
            Arguments.CheckNonEmptyString(bucketName, nameof(bucketName));

            return await _service
                .GetBucketsAsync(null, null, null, null, null, bucketName)
                .ContinueWith(t => t.Result._Buckets.FirstOrDefault());
        }

        /// <summary>
        /// List all buckets for specified organization.
        /// </summary>
        /// <param name="organization">filter buckets to a specific organization</param>
        /// <returns>A list of buckets</returns>
        public async Task<List<Bucket>> FindBucketsByOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await FindBucketsByOrgName(organization.Name);
        }

        /// <summary>
        /// List all buckets for specified orgId.
        /// </summary>
        /// <param name="orgName">filter buckets to a specific organization</param>
        /// <returns>A list of buckets</returns>
        public async Task<List<Bucket>> FindBucketsByOrgName(string orgName)
        {
            var buckets = FindBuckets(orgName, new FindOptions());

            return await buckets.ContinueWith(t => t.Result._Buckets);
        }

        /// <summary>
        /// List all buckets.
        /// </summary>
        /// <returns>List all buckets</returns>
        public async Task<List<Bucket>> FindBuckets()
        {
            return await FindBucketsByOrgName(null);
        }

        /// <summary>
        /// List all buckets.
        /// </summary>
        /// <param name="findOptions">the find options</param>
        /// <returns>List all buckets</returns>
        public async Task<Buckets> FindBuckets(FindOptions findOptions)
        {
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return await FindBuckets(null, findOptions);
        }

        /// <summary>
        /// List all members of a bucket.
        /// </summary>
        /// <param name="bucket">bucket of the members</param>
        /// <returns>the List all members of a bucket</returns>
        public async Task<List<ResourceMember>> GetMembers(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return await GetMembers(bucket.Id);
        }

        /// <summary>
        /// List all members of a bucket.
        /// </summary>
        /// <param name="bucketId">ID of bucket to get members</param>
        /// <returns>the List all members of a bucket</returns>
        public async Task<List<ResourceMember>> GetMembers(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return await _service.GetBucketsIDMembersAsync(bucketId).ContinueWith(t => t.Result.Users);
        }

        /// <summary>
        /// Add a bucket member.
        /// </summary>
        /// <param name="member">the member of a bucket</param>
        /// <param name="bucket">the bucket of a member</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddMember(User member, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(member, nameof(member));

            return await AddMember(member.Id, bucket.Id);
        }

        /// <summary>
        /// Add a bucket member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddMember(string memberId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            var mapping = new AddResourceMemberRequestBody(memberId);

            return await _service.PostBucketsIDMembersAsync(bucketId, mapping);
        }

        /// <summary>
        /// Removes a member from a bucket.
        /// </summary>
        /// <param name="member">the member of a bucket</param>
        /// <param name="bucket">the bucket of a member</param>
        /// <returns>member removed</returns>
        public async Task DeleteMember(User member, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(member, nameof(member));

            await DeleteMember(member.Id, bucket.Id);
        }

        /// <summary>
        /// Removes a member from a bucket.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>member removed</returns>
        public async Task DeleteMember(string memberId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            await _service.DeleteBucketsIDMembersIDAsync(memberId, bucketId);
        }

        /// <summary>
        /// List all owners of a bucket.
        /// </summary>
        /// <param name="bucket">bucket of the owners</param>
        /// <returns>the List all owners of a bucket</returns>
        public async Task<List<ResourceOwner>> GetOwners(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return await GetOwners(bucket.Id);
        }

        /// <summary>
        /// List all owners of a bucket.
        /// </summary>
        /// <param name="bucketId">ID of a bucket to get owners</param>
        /// <returns>the List all owners of a bucket</returns>
        public async Task<List<ResourceOwner>> GetOwners(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return await _service.GetBucketsIDOwnersAsync(bucketId).ContinueWith(t => t.Result.Users);
        }

        /// <summary>
        /// Add a bucket owner.
        /// </summary>
        /// <param name="owner">the owner of a bucket</param>
        /// <param name="bucket">the bucket of a owner</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceOwner> AddOwner(User owner, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(owner, nameof(owner));

            return await AddOwner(owner.Id, bucket.Id);
        }

        /// <summary>
        /// Add a bucket owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceOwner> AddOwner(string ownerId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var mapping = new AddResourceMemberRequestBody(ownerId);

            return await _service.PostBucketsIDOwnersAsync(bucketId, mapping);
        }

        /// <summary>
        /// Removes a owner from a bucket.
        /// </summary>
        /// <param name="owner">the owner of a bucket</param>
        /// <param name="bucket">the bucket of a owner</param>
        /// <returns>owner removed</returns>
        public async Task DeleteOwner(User owner, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(owner, nameof(owner));

            await DeleteOwner(owner.Id, bucket.Id);
        }

        /// <summary>
        /// Removes a owner from a bucket.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>owner removed</returns>
        public async Task DeleteOwner(string ownerId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            await _service.DeleteBucketsIDOwnersIDAsync(ownerId, bucketId);
        }

        /// <summary>
        /// Retrieve a bucket's logs
        /// </summary>
        /// <param name="bucket">for retrieve logs</param>
        /// <returns>logs</returns>
        public async Task<List<OperationLog>> FindBucketLogs(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return await FindBucketLogs(bucket.Id);
        }

        /// <summary>
        /// Retrieve a bucket's logs
        /// </summary>
        /// <param name="bucket">for retrieve logs</param>
        /// <param name="findOptions">the find options</param>
        /// <returns>logs</returns>
        public async Task<OperationLogs> FindBucketLogs(Bucket bucket, FindOptions findOptions)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return await FindBucketLogs(bucket.Id, findOptions);
        }

        /// <summary>
        /// Retrieve a bucket's logs
        /// </summary>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>logs</returns>
        public async Task<List<OperationLog>> FindBucketLogs(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return await FindBucketLogs(bucketId, new FindOptions()).ContinueWith(t=> t.Result.Logs);
        }

        /// <summary>
        /// Retrieve a bucket's logs
        /// </summary>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <param name="findOptions">the find options</param>
        /// <returns>logs</returns>
        public async Task<OperationLogs> FindBucketLogs(string bucketId, FindOptions findOptions)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return await _service.GetBucketsIDLogsAsync(bucketId, null, findOptions.Offset, findOptions.Limit);
        }

        /// <summary>
        /// List all labels of a bucket.
        /// </summary>
        /// <param name="bucket">bucket of the labels</param>
        /// <returns>the List all labels of a bucket</returns>
        public async Task<List<Label>> GetLabels(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return await GetLabels(bucket.Id);
        }

        /// <summary>
        /// List all labels of a bucket.
        /// </summary>
        /// <param name="bucketId">ID of a bucket to get labels</param>
        /// <returns>the List all labels of a bucket</returns>
        public async Task<List<Label>> GetLabels(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return await _service.GetBucketsIDLabelsAsync(bucketId).ContinueWith(t => t.Result.Labels);
        }

        /// <summary>
        /// Add a bucket label.
        /// </summary>
        /// <param name="label">the label of a bucket</param>
        /// <param name="bucket">the bucket of a label</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabel(Label label, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(label, nameof(label));

            return await AddLabel(label.Id, bucket.Id);
        }

        /// <summary>
        /// Add a bucket label.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabel(string labelId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var mapping = new LabelMapping(labelId);

            return await _service.PostBucketsIDLabelsAsync(bucketId, mapping).ContinueWith(t => t.Result.Label);
        }

        /// <summary>
        /// Removes a label from a bucket.
        /// </summary>
        /// <param name="label">the label of a bucket</param>
        /// <param name="bucket">the bucket of a owner</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteLabel(Label label, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(label, nameof(label));

            await DeleteLabel(label.Id, bucket.Id);
        }

        /// <summary>
        /// Removes a label from a bucket.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteLabel(string labelId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            await _service.DeleteBucketsIDLabelsIDAsync(bucketId, labelId);
        }

        private async Task<Buckets> FindBuckets(string orgName, FindOptions findOptions)
        {
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return await _service.GetBucketsAsync(null, findOptions.Offset, findOptions.Limit, orgName);
        }
    }
}