using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Internal;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Generated.Domain;
using InfluxDB.Client.Internal;
using Bucket = InfluxDB.Client.Domain.Bucket;
using Buckets = InfluxDB.Client.Domain.Buckets;
using Organization = InfluxDB.Client.Domain.Organization;
using ResourceMember = InfluxDB.Client.Domain.ResourceMember;
using ResourceMembers = InfluxDB.Client.Domain.ResourceMembers;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client
{
    public class BucketsApi : AbstractInfluxDBClient
    {
        protected internal BucketsApi(DefaultClientIo client) : base(client)
        {
        }

        /// <summary>
        ///     Creates a new bucket and sets <see cref="Domain.Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="bucket">bucket to create</param>
        /// <returns>created Bucket</returns>
        public async Task<Bucket> CreateBucket(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            var response = await Post(bucket, "/api/v2/buckets");

            return Call<Bucket>(response);
        }

        /// <summary>
        ///     Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
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
        ///     Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="retentionRule">retention rule of the bucket</param>
        /// <param name="organization">owner of the bucket</param>
        /// <returns>created Bucket</returns>
        public async Task<Bucket> CreateBucket(string name, RetentionRule retentionRule, Organization organization)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNotNull(organization, nameof(organization));

            return await CreateBucket(name, retentionRule, organization.Id);
        }

        /// <summary>
        ///     Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="orgId">owner of the bucket</param>
        /// <returns>created Bucket</returns>
        public async Task<Bucket> CreateBucket(string name, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return await CreateBucket(name, default(RetentionRule), orgId);
        }

        /// <summary>
        ///     Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="retentionRule">retention rule of the bucket</param>
        /// <param name="orgId">owner of the bucket</param>
        /// <returns>created Bucket</returns>
        public async Task<Bucket> CreateBucket(string name, RetentionRule retentionRule, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var bucket = new Bucket {Name = name, OrgId = orgId};
            if (retentionRule != null) bucket.RetentionRules.Add(retentionRule);

            return await CreateBucket(bucket);
        }

        /// <summary>
        ///     Update a bucket name and retention.
        /// </summary>
        /// <param name="bucket">bucket update to apply</param>
        /// <returns>bucket updated</returns>
        public async Task<Bucket> UpdateBucket(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            var result = await Patch(bucket, $"/api/v2/buckets/{bucket.Id}");

            return Call<Bucket>(result);
        }

        /// <summary>
        ///     Delete a bucket.
        /// </summary>
        /// <param name="bucketId">ID of bucket to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteBucket(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            var request = await Delete($"/api/v2/buckets/{bucketId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        ///     Delete a bucket.
        /// </summary>
        /// <param name="bucket">bucket to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteBucket(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            await DeleteBucket(bucket.Id);
        }

        /// <summary>
        ///     Clone a bucket.
        /// </summary>
        /// <param name="clonedName">name of cloned bucket</param>
        /// <param name="bucketId">ID of bucket to clone</param>
        /// <returns>cloned bucket</returns>
        public async Task<Bucket> CloneBucket(string clonedName, string bucketId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            var bucket = await FindBucketById(bucketId);
            if (bucket == null) throw new InvalidOperationException($"NotFound Bucket with ID: {bucketId}");

            return await CloneBucket(clonedName, bucket);
        }

        /// <summary>
        ///     Clone a bucket.
        /// </summary>
        /// <param name="clonedName">name of cloned bucket</param>
        /// <param name="bucket">bucket to clone</param>
        /// <returns>cloned bucket</returns>
        public async Task<Bucket> CloneBucket(string clonedName, Bucket bucket)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(bucket, nameof(bucket));

            var cloned = new Bucket
            {
                Name = clonedName,
                OrgId = bucket.OrgId,
                OrgName = bucket.OrgName,
                RetentionPolicyName = bucket.RetentionPolicyName
            };
            cloned.RetentionRules.AddRange(bucket.RetentionRules);

            var created = await CreateBucket(cloned);

            foreach (var label in await GetLabels(bucket)) await AddLabel(label, created);

            return created;
        }

        /// <summary>
        ///     Retrieve a bucket.
        /// </summary>
        /// <param name="bucketId">ID of bucket to get</param>
        /// <returns>Bucket Details</returns>
        public async Task<Bucket> FindBucketById(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            var request = await Get($"/api/v2/buckets/{bucketId}");

            return Call<Bucket>(request, 404);
        }

        /// <summary>
        ///     Retrieve a bucket.
        /// </summary>
        /// <param name="bucketName">Name of bucket to get</param>
        /// <returns>Bucket Details</returns>
        public async Task<Bucket> FindBucketByName(string bucketName)
        {
            Arguments.CheckNonEmptyString(bucketName, nameof(bucketName));

            var request = await Get($"/api/v2/buckets?name={bucketName}");

            var buckets = Call<Buckets>(request);

            return buckets.BucketList.FirstOrDefault();
        }

        /// <summary>
        ///     List all buckets for specified organization.
        /// </summary>
        /// <param name="organization">filter buckets to a specific organization</param>
        /// <returns>A list of buckets</returns>
        public async Task<List<Bucket>> FindBucketsByOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await FindBucketsByOrgId(organization.Name);
        }

        /// <summary>
        ///     List all buckets for specified orgId.
        /// </summary>
        /// <param name="orgId">filter buckets to a specific organization ID</param>
        /// <returns>A list of buckets</returns>
        public async Task<List<Bucket>> FindBucketsByOrgId(string orgId)
        {
            var buckets = await FindBuckets(orgId, new FindOptions());

            return buckets.BucketList;
        }

        /// <summary>
        ///     List all buckets.
        /// </summary>
        /// <returns>List all buckets</returns>
        public async Task<List<Bucket>> FindBuckets()
        {
            return await FindBucketsByOrgId(null);
        }

        /// <summary>
        ///     List all buckets.
        /// </summary>
        /// <param name="findOptions">the find options</param>
        /// <returns>List all buckets</returns>
        public async Task<Buckets> FindBuckets(FindOptions findOptions)
        {
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            var buckets = await FindBuckets(null, findOptions);

            return buckets;
        }

        /// <summary>
        ///     List all members of a bucket.
        /// </summary>
        /// <param name="bucket">bucket of the members</param>
        /// <returns>the List all members of a bucket</returns>
        public async Task<List<ResourceMember>> GetMembers(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return await GetMembers(bucket.Id);
        }

        /// <summary>
        ///     List all members of a bucket.
        /// </summary>
        /// <param name="bucketId">ID of bucket to get members</param>
        /// <returns>the List all members of a bucket</returns>
        public async Task<List<ResourceMember>> GetMembers(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            var request = await Get($"/api/v2/buckets/{bucketId}/members");

            var response = Call<ResourceMembers>(request);

            return response?.Users;
        }

        /// <summary>
        ///     Add a bucket member.
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
        ///     Add a bucket member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddMember(string memberId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            var user = new User(memberId);

            var request = await Post(user, $"/api/v2/buckets/{bucketId}/members");

            return Call<ResourceMember>(request);
        }

        /// <summary>
        ///     Removes a member from a bucket.
        /// </summary>
        /// <param name="member">the member of a bucket</param>
        /// <param name="bucket">the bucket of a member</param>
        /// <returns>async task</returns>
        public async Task DeleteMember(User member, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(member, nameof(member));

            await DeleteMember(member.Id, bucket.Id);
        }

        /// <summary>
        ///     Removes a member from a bucket.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>async task</returns>
        public async Task DeleteMember(string memberId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            var request = await Delete($"/api/v2/buckets/{bucketId}/members/{memberId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        ///     List all owners of a bucket.
        /// </summary>
        /// <param name="bucket">bucket of the owners</param>
        /// <returns>the List all owners of a bucket</returns>
        public async Task<List<ResourceMember>> GetOwners(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return await GetOwners(bucket.Id);
        }

        /// <summary>
        ///     List all owners of a bucket.
        /// </summary>
        /// <param name="bucketId">ID of a bucket to get owners</param>
        /// <returns>the List all owners of a bucket</returns>
        public async Task<List<ResourceMember>> GetOwners(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            var request = await Get($"/api/v2/buckets/{bucketId}/owners");

            var response = Call<ResourceMembers>(request);

            return response?.Users;
        }

        /// <summary>
        ///     Add a bucket owner.
        /// </summary>
        /// <param name="owner">the owner of a bucket</param>
        /// <param name="bucket">the bucket of a owner</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddOwner(User owner, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(owner, nameof(owner));

            return await AddOwner(owner.Id, bucket.Id);
        }

        /// <summary>
        ///     Add a bucket owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddOwner(string ownerId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var user = new User(ownerId);

            var request = await Post(user, $"/api/v2/buckets/{bucketId}/owners");

            return Call<ResourceMember>(request);
        }

        /// <summary>
        ///     Removes a owner from a bucket.
        /// </summary>
        /// <param name="owner">the owner of a bucket</param>
        /// <param name="bucket">the bucket of a owner</param>
        /// <returns>async task</returns>
        public async Task DeleteOwner(User owner, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(owner, nameof(owner));

            await DeleteOwner(owner.Id, bucket.Id);
        }

        /// <summary>
        ///     Removes a owner from a bucket.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>async task</returns>
        public async Task DeleteOwner(string ownerId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var request = await Delete($"/api/v2/buckets/{bucketId}/owners/{ownerId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        ///     Retrieve a bucket's logs
        /// </summary>
        /// <param name="bucket">for retrieve logs</param>
        /// <returns>logs</returns>
        public async Task<List<OperationLogEntry>> FindBucketLogs(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return await FindBucketLogs(bucket.Id);
        }

        /// <summary>
        ///     Retrieve a bucket's logs
        /// </summary>
        /// <param name="bucket">for retrieve logs</param>
        /// <param name="findOptions">the find options</param>
        /// <returns>logs</returns>
        public async Task<OperationLogEntries> FindBucketLogs(Bucket bucket, FindOptions findOptions)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return await FindBucketLogs(bucket.Id, findOptions);
        }

        /// <summary>
        ///     Retrieve a bucket's logs
        /// </summary>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>logs</returns>
        public async Task<List<OperationLogEntry>> FindBucketLogs(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return (await FindBucketLogs(bucketId, new FindOptions())).Logs;
        }

        /// <summary>
        ///     Retrieve a bucket's logs
        /// </summary>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <param name="findOptions">the find options</param>
        /// <returns>logs</returns>
        public async Task<OperationLogEntries> FindBucketLogs(string bucketId, FindOptions findOptions)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            var request = await Get($"/api/v2/buckets/{bucketId}/log?" + CreateQueryString(findOptions));

            return GetOperationLogEntries(request);
        }

        /// <summary>
        ///     List all labels of a bucket.
        /// </summary>
        /// <param name="bucket">bucket of the labels</param>
        /// <returns>the List all labels of a bucket</returns>
        public async Task<List<Label>> GetLabels(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return await GetLabels(bucket.Id);
        }

        /// <summary>
        ///     List all labels of a bucket.
        /// </summary>
        /// <param name="bucketId">ID of a bucket to get labels</param>
        /// <returns>the List all labels of a bucket</returns>
        public async Task<List<Label>> GetLabels(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return await GetLabels(bucketId, "buckets");
        }

        /// <summary>
        ///     Add a bucket label.
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
        ///     Add a bucket label.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabel(string labelId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return await AddLabel(labelId, bucketId, "buckets", ResourceType.Buckets);
        }

        /// <summary>
        ///     Removes a label from a bucket.
        /// </summary>
        /// <param name="label">the label of a bucket</param>
        /// <param name="bucket">the bucket of a owner</param>
        /// <returns>async task</returns>
        public async Task DeleteLabel(Label label, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(label, nameof(label));

            await DeleteLabel(label.Id, bucket.Id);
        }

        /// <summary>
        ///     Removes a label from a bucket.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>async task</returns>
        public async Task DeleteLabel(string labelId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            await DeleteLabel(labelId, bucketId, "buckets");
        }

        private async Task<Buckets> FindBuckets(string orgId, FindOptions findOptions)
        {
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            var request = await Get($"/api/v2/buckets?org={orgId}&" + CreateQueryString(findOptions));

            return Call<Buckets>(request);
        }
    }
}