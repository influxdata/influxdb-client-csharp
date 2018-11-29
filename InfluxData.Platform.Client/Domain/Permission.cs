using System.Text;
using Newtonsoft.Json;
using Platform.Common.Platform;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// Permission defines an action and a resource.
    /// </summary>
    public class Permission
    {
        /// <summary>
        /// Action for reading.
        /// </summary>
        public static readonly string ReadAction = "read";

        /// <summary>
        /// Action for writing.
        /// </summary>
        public static readonly string WriteAction = "write";

        /// <summary>
        /// Action for creating new resources.
        /// </summary>
        public static readonly string CreateAction = "create";

        /// <summary>
        /// Deleting an existing resource.
        /// </summary>
        public static readonly string DeleteAction = "delete";

        /// <summary>
        /// Represents the user resource actions can apply to.
        /// </summary>
        public static readonly string UserResource = "user";

        /// <summary>
        /// Represents the org resource actions can apply to.
        /// </summary>
        public static readonly string OrganizationResource = "org";

        [JsonProperty("resource")]
        public string Resource { get; set; }
        
        [JsonProperty("action")]
        public string Action { get; set; }

        
        /// <summary>
        /// Represents the task resource scoped to an organization.
        /// </summary>
        /// <param name="orgId">organization id</param>
        /// <returns>the task resource</returns>
        public static string TaskResource(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, "orgId");

            return $"org/{orgId}/task";
        }

        /// <summary>
        /// BucketResource constructs a bucket resource.
        /// </summary>
        /// <param name="bucketId">bucket id</param>
        /// <returns>the bucket resource</returns>
        public static string BucketResource(string bucketId)
        {
            Arguments.CheckNonEmptyString(bucketId, "bucketID");

            return $"bucket/{bucketId}";
        }
        
        public override string ToString()
        {
           return new StringBuilder(GetType().Name + "[")
               .Append("resource='" + Resource + "'")
               .Append(", action='" + Action + "'")
               .Append("]").ToString();
        }
    }
}