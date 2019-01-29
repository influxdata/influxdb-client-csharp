using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;
using Task = System.Threading.Tasks.Task;

namespace InfluxData.Platform.Client.Client
{
    public class BucketClient : AbstractPlatformClient
    {
        protected internal BucketClient(DefaultClientIo client) : base(client)
        {
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id"/> with the new identifier.
        /// </summary>
        /// <param name="bucket">bucket to create</param>
        /// <returns>created Bucket</returns>
        public async Task<Bucket> CreateBucket(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, "Bucket");

            var response = await Post(bucket, "/api/v2/buckets");

            return Call<Bucket>(response);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id"/> with the new identifier.
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
        /// Creates a new bucket and sets <see cref="Bucket.Id"/> with the new identifier.
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
        /// Creates a new bucket and sets <see cref="Bucket.Id"/> with the new identifier.
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
        /// Creates a new bucket and sets <see cref="Bucket.Id"/> with the new identifier.
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
            if (retentionRule != null)
            {
                bucket.RetentionRules.Add(retentionRule);
            }

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

            var result = await Patch(bucket, $"/api/v2/buckets/{bucket.Id}");

            return Call<Bucket>(result);
        }
        
        /// <summary>
        /// Delete a bucket.
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
        /// Delete a bucket.
        /// </summary>
        /// <param name="bucket">bucket to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteBucket(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            await DeleteBucket(bucket.Id);
        }
        
        /// <summary>
        /// Retrieve a bucket.
        /// </summary>
        /// <param name="bucketId">ID of bucket to get</param>
        /// <returns>Bucket Details</returns>
        public async Task<Bucket> FindBucketById(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            var request = await Get($"/api/v2/buckets/{bucketId}");

            return Call<Bucket>(request, "bucket not found");
        }
        /// <summary>
        /// Retrieve a bucket.
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
        ///  List all buckets for specified organization.
        /// </summary>
        /// <param name="organization">filter buckets to a specific organization</param>
        /// <returns>A list of buckets</returns>
        public async Task<List<Bucket>> FindBucketsByOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            
            return await FindBucketsByOrgId(organization.Name);
        }

        /// <summary>
        /// List all buckets for specified orgId.
        /// </summary>
        /// <param name="orgId">filter buckets to a specific organization name</param>
        /// <returns>A list of buckets</returns>
        public async Task<List<Bucket>> FindBucketsByOrgId(string orgId)
        {
            var request = await Get($"/api/v2/buckets?org={orgId}");

            var buckets = Call<Buckets>(request);

            return buckets.BucketList;
        }

        /// <summary>
        /// List all buckets.
        /// </summary>
        /// <returns>List all buckets</returns>
        public async Task<List<Bucket>> FindBuckets()
        {
            return await FindBucketsByOrgId(null);
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

            var request = await Get($"/api/v2/buckets/{bucketId}/members");

            var response = Call<ResourceMembers>(request);

            return response?.Users;
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

            var user = new User {Id = memberId};

            var request = await Post(user, $"/api/v2/buckets/{bucketId}/members");

            return Call<ResourceMember>(request);
        }

        /// <summary>
        /// Removes a member from a bucket.
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
        /// Removes a member from a bucket.
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
        /// List all owners of a bucket.
        /// </summary>
        /// <param name="bucket">bucket of the owners</param>
        /// <returns>the List all owners of a bucket</returns>
        public async Task<List<ResourceMember>> GetOwners(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return await GetOwners(bucket.Id);
        }

        /// <summary>
        /// List all owners of a bucket.
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
        /// Add a bucket owner.
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
        /// Add a bucket owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddOwner(string ownerId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var user = new User {Id = ownerId};

            var request = await Post(user, $"/api/v2/buckets/{bucketId}/owners");

            return Call<ResourceMember>(request);
        }

        /// <summary>
        /// Removes a owner from a bucket.
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
        /// Removes a owner from a bucket.
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

            return await GetLabels(bucketId, "buckets");
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

            return await AddLabel(labelId, bucketId, "buckets", ResourceType.Buckets);
        }

        /// <summary>
        /// Removes a label from a bucket.
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
        /// Removes a label from a bucket.
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
    }
}