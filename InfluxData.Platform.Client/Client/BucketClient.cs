using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;
using Task = System.Threading.Tasks.Task;

namespace InfluxData.Platform.Client.Client
{
    public class BucketClient : AbstractClient
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
            Arguments.CheckNonEmptyString(name, "Bucket name");
            Arguments.CheckNotNull(organization, "Organization");
            
            return await CreateBucket(name, organization.Name);
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
            Arguments.CheckNonEmptyString(name, "Bucket name");
            Arguments.CheckNotNull(organization, "Organization");
            
            return await CreateBucket(name, retentionRule, organization.Name);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id"/> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="organizationName">owner of the bucket</param>
        /// <returns>created Bucket</returns>
        public async Task<Bucket> CreateBucket(string name, string organizationName)
        {
            Arguments.CheckNonEmptyString(name, "Bucket name");
            Arguments.CheckNonEmptyString(organizationName, "Organization name");
            
            return await CreateBucket(name, default(RetentionRule), organizationName);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id"/> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="retentionRule">retention rule of the bucket</param>
        /// <param name="organizationName">owner of the bucket</param>
        /// <returns>created Bucket</returns>
        public async Task<Bucket> CreateBucket(string name, RetentionRule retentionRule, string organizationName)
        {
            Arguments.CheckNonEmptyString(name, "Bucket name");
            Arguments.CheckNonEmptyString(organizationName, "Organization name");
            
            var bucket = new Bucket {Name = name, OrganizationName = organizationName};
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
            Arguments.CheckNotNull(bucket, "Bucket is required");

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
            Arguments.CheckNotNull(bucketId, "Bucket ID");

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
            Arguments.CheckNotNull(bucket, "bucket");

            await DeleteBucket(bucket.Id);
        }
        
        /// <summary>
        /// Retrieve a bucket.
        /// </summary>
        /// <param name="bucketId">ID of bucket to get</param>
        /// <returns>Bucket Details</returns>
        public async Task<Bucket> FindBucketById(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, "Bucket ID");

            var request = await Get($"/api/v2/buckets/{bucketId}");

            return Call<Bucket>(request, "bucket not found");
        }

        /// <summary>
        ///  List all buckets for specified organization.
        /// </summary>
        /// <param name="organization">filter buckets to a specific organization</param>
        /// <returns>A list of buckets</returns>
        public async Task<List<Bucket>> FindBucketsByOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, "organization");
            
            return await FindBucketsByOrganizationName(organization.Name);
        }

        /// <summary>
        /// List all buckets for specified organizationName.
        /// </summary>
        /// <param name="organizationName">filter buckets to a specific organization name</param>
        /// <returns>A list of buckets</returns>
        public async Task<List<Bucket>> FindBucketsByOrganizationName(string organizationName)
        {
            var request = await Get($"/api/v2/buckets?org={organizationName}");

            var buckets = Call<Buckets>(request);

            return buckets.BucketList;
        }

        /// <summary>
        /// List all buckets.
        /// </summary>
        /// <returns>List all buckets</returns>
        public async Task<List<Bucket>> FindBuckets()
        {
            return await FindBucketsByOrganizationName(null);
        }
    }
}