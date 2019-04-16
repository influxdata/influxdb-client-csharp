using System;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Client.Core;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Generated.Domain;
using InfluxDB.Client.Generated.Service;
using ResourceMember = InfluxDB.Client.Generated.Domain.ResourceMember;

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
        /// Creates a new bucket and sets <see cref="InfluxDB.Client.Generated.Domain.Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="bucket">bucket to create</param>
        /// <returns>created Bucket</returns>
        public Bucket CreateBucket(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return _service.BucketsPost(bucket);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="organization">owner of the bucket</param>
        /// <returns>created Bucket</returns>
        public Bucket CreateBucket(string name, Organization organization)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNotNull(organization, nameof(organization));

            return CreateBucket(name, organization.Id);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="bucketRetentionRules">retention rule of the bucket</param>
        /// <param name="organization">owner of the bucket</param>
        /// <returns>created Bucket</returns>
        public Bucket CreateBucket(string name, BucketRetentionRules bucketRetentionRules, Organization organization)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNotNull(organization, nameof(organization));

            return CreateBucket(name, bucketRetentionRules, organization.Id);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="orgId">owner of the bucket</param>
        /// <returns>created Bucket</returns>
        public Bucket CreateBucket(string name, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return CreateBucket(name, default(BucketRetentionRules), orgId);
        }

        /// <summary>
        /// Creates a new bucket and sets <see cref="Bucket.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name">name of the bucket</param>
        /// <param name="bucketRetentionRules">retention rule of the bucket</param>
        /// <param name="orgId">owner of the bucket</param>
        /// <returns>created Bucket</returns>
        public Bucket CreateBucket(string name, BucketRetentionRules bucketRetentionRules, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var bucket = new Bucket(null, name, orgId, null, null, new List<BucketRetentionRules>());
            if (bucketRetentionRules != null) bucket.RetentionRules.Add(bucketRetentionRules);

            return CreateBucket(bucket);
        }

        /// <summary>
        /// Update a bucket name and retention.
        /// </summary>
        /// <param name="bucket">bucket update to apply</param>
        /// <returns>bucket updated</returns>
        public Bucket UpdateBucket(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return _service.BucketsBucketIDPatch(bucket.Id, bucket);
        }

        /// <summary>
        /// Delete a bucket.
        /// </summary>
        /// <param name="bucketId">ID of bucket to delete</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteBucket(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            _service.BucketsBucketIDDelete(bucketId);
        }

        /// <summary>
        /// Delete a bucket.
        /// </summary>
        /// <param name="bucket">bucket to delete</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteBucket(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            DeleteBucket(bucket.Id);
        }

        /// <summary>
        /// Clone a bucket.
        /// </summary>
        /// <param name="clonedName">name of cloned bucket</param>
        /// <param name="bucketId">ID of bucket to clone</param>
        /// <returns>cloned bucket</returns>
        public Bucket CloneBucket(string clonedName, string bucketId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            var bucket = FindBucketById(bucketId);
            if (bucket == null) throw new InvalidOperationException($"NotFound Bucket with ID: {bucketId}");

            return CloneBucket(clonedName, bucket);
        }

        /// <summary>
        /// Clone a bucket.
        /// </summary>
        /// <param name="clonedName">name of cloned bucket</param>
        /// <param name="bucket">bucket to clone</param>
        /// <returns>cloned bucket</returns>
        public Bucket CloneBucket(string clonedName, Bucket bucket)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(bucket, nameof(bucket));

            var cloned = new Bucket(null, clonedName, bucket.OrganizationID,
                bucket.Organization, bucket.Rp, bucket.RetentionRules);

            var created = CreateBucket(cloned);

            foreach (var label in GetLabels(bucket)) AddLabel(label, created);

            return created;
        }

        /// <summary>
        /// Retrieve a bucket.
        /// </summary>
        /// <param name="bucketId">ID of bucket to get</param>
        /// <returns>Bucket Details</returns>
        public Bucket FindBucketById(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return _service.BucketsBucketIDGet(bucketId);
        }

        /// <summary>
        /// Retrieve a bucket.
        /// </summary>
        /// <param name="bucketName">Name of bucket to get</param>
        /// <returns>Bucket Details</returns>
        public Bucket FindBucketByName(string bucketName)
        {
            Arguments.CheckNonEmptyString(bucketName, nameof(bucketName));

            return _service
                .BucketsGet(null, null, null, null, null, bucketName)
                ._Buckets
                .FirstOrDefault();
        }

        /// <summary>
        /// List all buckets for specified organization.
        /// </summary>
        /// <param name="organization">filter buckets to a specific organization</param>
        /// <returns>A list of buckets</returns>
        public List<Bucket> FindBucketsByOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return FindBucketsByOrgName(organization.Name);
        }

        /// <summary>
        /// List all buckets for specified orgId.
        /// </summary>
        /// <param name="orgName">filter buckets to a specific organization</param>
        /// <returns>A list of buckets</returns>
        public List<Bucket> FindBucketsByOrgName(string orgName)
        {
            var buckets = FindBuckets(orgName, new FindOptions());

            return buckets._Buckets;
        }

        /// <summary>
        /// List all buckets.
        /// </summary>
        /// <returns>List all buckets</returns>
        public List<Bucket> FindBuckets()
        {
            return FindBucketsByOrgName(null);
        }

        /// <summary>
        /// List all buckets.
        /// </summary>
        /// <param name="findOptions">the find options</param>
        /// <returns>List all buckets</returns>
        public Buckets FindBuckets(FindOptions findOptions)
        {
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return FindBuckets(null, findOptions);
        }

        /// <summary>
        /// List all members of a bucket.
        /// </summary>
        /// <param name="bucket">bucket of the members</param>
        /// <returns>the List all members of a bucket</returns>
        public List<ResourceMember> GetMembers(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return GetMembers(bucket.Id);
        }

        /// <summary>
        /// List all members of a bucket.
        /// </summary>
        /// <param name="bucketId">ID of bucket to get members</param>
        /// <returns>the List all members of a bucket</returns>
        public List<ResourceMember> GetMembers(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return _service.BucketsBucketIDMembersGet(bucketId).Users;
        }

        /// <summary>
        /// Add a bucket member.
        /// </summary>
        /// <param name="member">the member of a bucket</param>
        /// <param name="bucket">the bucket of a member</param>
        /// <returns>created mapping</returns>
        public ResourceMember AddMember(User member, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(member, nameof(member));

            return AddMember(member.Id, bucket.Id);
        }

        /// <summary>
        /// Add a bucket member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>created mapping</returns>
        public ResourceMember AddMember(string memberId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            var mapping = new AddResourceMemberRequestBody(memberId);

            return _service.BucketsBucketIDMembersPost(bucketId, mapping);
        }

        /// <summary>
        /// Removes a member from a bucket.
        /// </summary>
        /// <param name="member">the member of a bucket</param>
        /// <param name="bucket">the bucket of a member</param>
        /// <returns>member removed</returns>
        public void DeleteMember(User member, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(member, nameof(member));

            DeleteMember(member.Id, bucket.Id);
        }

        /// <summary>
        /// Removes a member from a bucket.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>member removed</returns>
        public void DeleteMember(string memberId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            _service.BucketsBucketIDMembersUserIDDelete(memberId, bucketId);
        }

        /// <summary>
        /// List all owners of a bucket.
        /// </summary>
        /// <param name="bucket">bucket of the owners</param>
        /// <returns>the List all owners of a bucket</returns>
        public List<ResourceOwner> GetOwners(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return GetOwners(bucket.Id);
        }

        /// <summary>
        /// List all owners of a bucket.
        /// </summary>
        /// <param name="bucketId">ID of a bucket to get owners</param>
        /// <returns>the List all owners of a bucket</returns>
        public List<ResourceOwner> GetOwners(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return _service.BucketsBucketIDOwnersGet(bucketId).Users;
        }

        /// <summary>
        /// Add a bucket owner.
        /// </summary>
        /// <param name="owner">the owner of a bucket</param>
        /// <param name="bucket">the bucket of a owner</param>
        /// <returns>created mapping</returns>
        public ResourceOwner AddOwner(User owner, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(owner, nameof(owner));

            return AddOwner(owner.Id, bucket.Id);
        }

        /// <summary>
        /// Add a bucket owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>created mapping</returns>
        public ResourceOwner AddOwner(string ownerId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var mapping = new AddResourceMemberRequestBody(ownerId);

            return _service.BucketsBucketIDOwnersPost(bucketId, mapping);
        }

        /// <summary>
        /// Removes a owner from a bucket.
        /// </summary>
        /// <param name="owner">the owner of a bucket</param>
        /// <param name="bucket">the bucket of a owner</param>
        /// <returns>owner removed</returns>
        public void DeleteOwner(User owner, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(owner, nameof(owner));

            DeleteOwner(owner.Id, bucket.Id);
        }

        /// <summary>
        /// Removes a owner from a bucket.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>owner removed</returns>
        public void DeleteOwner(string ownerId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            _service.BucketsBucketIDOwnersUserIDDelete(ownerId, bucketId);
        }

        /// <summary>
        /// Retrieve a bucket's logs
        /// </summary>
        /// <param name="bucket">for retrieve logs</param>
        /// <returns>logs</returns>
        public List<OperationLog> FindBucketLogs(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return FindBucketLogs(bucket.Id);
        }

        /// <summary>
        /// Retrieve a bucket's logs
        /// </summary>
        /// <param name="bucket">for retrieve logs</param>
        /// <param name="findOptions">the find options</param>
        /// <returns>logs</returns>
        public OperationLogs FindBucketLogs(Bucket bucket, FindOptions findOptions)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return FindBucketLogs(bucket.Id, findOptions);
        }

        /// <summary>
        /// Retrieve a bucket's logs
        /// </summary>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>logs</returns>
        public List<OperationLog> FindBucketLogs(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return FindBucketLogs(bucketId, new FindOptions()).Logs;
        }

        /// <summary>
        /// Retrieve a bucket's logs
        /// </summary>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <param name="findOptions">the find options</param>
        /// <returns>logs</returns>
        public OperationLogs FindBucketLogs(string bucketId, FindOptions findOptions)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return _service.BucketsBucketIDLogsGet(bucketId, null, findOptions.Offset, findOptions.Limit);
        }

        /// <summary>
        /// List all labels of a bucket.
        /// </summary>
        /// <param name="bucket">bucket of the labels</param>
        /// <returns>the List all labels of a bucket</returns>
        public List<Label> GetLabels(Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));

            return GetLabels(bucket.Id);
        }

        /// <summary>
        /// List all labels of a bucket.
        /// </summary>
        /// <param name="bucketId">ID of a bucket to get labels</param>
        /// <returns>the List all labels of a bucket</returns>
        public List<Label> GetLabels(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return _service.BucketsBucketIDLabelsGet(bucketId).Labels;
        }

        /// <summary>
        /// Add a bucket label.
        /// </summary>
        /// <param name="label">the label of a bucket</param>
        /// <param name="bucket">the bucket of a label</param>
        /// <returns>added label</returns>
        public Label AddLabel(Label label, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(label, nameof(label));

            return AddLabel(label.Id, bucket.Id);
        }

        /// <summary>
        /// Add a bucket label.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>added label</returns>
        public Label AddLabel(string labelId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var mapping = new LabelMapping(labelId);

            return _service.BucketsBucketIDLabelsPost(bucketId, mapping).Label;
        }

        /// <summary>
        /// Removes a label from a bucket.
        /// </summary>
        /// <param name="label">the label of a bucket</param>
        /// <param name="bucket">the bucket of a owner</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteLabel(Label label, Bucket bucket)
        {
            Arguments.CheckNotNull(bucket, nameof(bucket));
            Arguments.CheckNotNull(label, nameof(label));

            DeleteLabel(label.Id, bucket.Id);
        }

        /// <summary>
        /// Removes a label from a bucket.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="bucketId">the ID of a bucket</param>
        /// <returns>delete has been accepted</returns>
        public void DeleteLabel(string labelId, string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            _service.BucketsBucketIDLabelsLabelIDDelete(bucketId, labelId);
        }

        private Buckets FindBuckets(string orgName, FindOptions findOptions)
        {
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return _service.BucketsGet(null, findOptions.Offset, findOptions.Limit, orgName);
        }
    }
}