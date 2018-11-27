using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;

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
    }
}