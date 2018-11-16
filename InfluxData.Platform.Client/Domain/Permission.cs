using System;
using System.Text;
using Platform.Common.Platform;

namespace InfluxData.Platform.Client.Domain
{
    /**
     * Permission defines an action and a resource.
     */
    public class Permission
    {
        /**
         * Action for reading.
         */
        public static readonly string ReadAction = "read";

        /**
         * Action for writing.
         */
        public static readonly string WriteAction = "write";

        /**
         * Action for creating new resources.
         */
        public static readonly string CreateAction = "create";

        /**
         * Deleting an existing resource.
         */
        public static readonly string DeleteAction = "delete";

        /**
         * Represents the user resource actions can apply to.
         */
        public static readonly string UserResource = "user";

        /**
         * Represents the org resource actions can apply to.
         */
        public static readonly string OrganizationResource = "org";


        /**
         * Represents the task resource scoped to an organization.
         *
         * @param orgId organization scope
         * @return task resource
         */
        public static string TaskResource(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, "orgId");

            return string.Format("org/{0}/task", orgId);
        }

        /**
         * BucketResource constructs a bucket resource.
         *
         * @param bucketId bucket scope
         * @return bucket resource
         */
        public static string BucketResource(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, "bucketID");

            return string.Format("bucket/{0}", bucketId);
        }

        public string Resource { get; set; }
        public string Action { get; set; }

        public override string ToString()
        {
           return new StringBuilder(GetType().Name + "[")
               .Append("resource='" + Resource + "'")
               .Append(", action='" + Action + "'")
               .Append("]").ToString();
        }
    }
}