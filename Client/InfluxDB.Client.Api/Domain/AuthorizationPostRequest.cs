/*
 * InfluxDB OSS API Service
 *
 * The InfluxDB v2 API provides a programmatic interface for all interactions with InfluxDB. Access the InfluxDB API using the `/api/v2/` endpoint. 
 *
 * The version of the OpenAPI document: 2.0.0
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using FileParameter = InfluxDB.Client.Core.Api.FileParameter;
using OpenAPIDateConverter = InfluxDB.Client.Core.Api.OpenAPIDateConverter;

namespace InfluxDB.Client.Api.Domain
{
    /// <summary>
    /// AuthorizationPostRequest
    /// </summary>
    [DataContract(Name = "AuthorizationPostRequest")]
    public partial class AuthorizationPostRequest : IEquatable<AuthorizationPostRequest>
    {
        /// <summary>
        /// If inactive the token is inactive and requests using the token will be rejected.
        /// </summary>
        /// <value>If inactive the token is inactive and requests using the token will be rejected.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum StatusEnum
        {
            /// <summary>
            /// Enum Active for value: active
            /// </summary>
            [EnumMember(Value = "active")]
            Active = 1,

            /// <summary>
            /// Enum Inactive for value: inactive
            /// </summary>
            [EnumMember(Value = "inactive")]
            Inactive = 2

        }


        /// <summary>
        /// If inactive the token is inactive and requests using the token will be rejected.
        /// </summary>
        /// <value>If inactive the token is inactive and requests using the token will be rejected.</value>
        [DataMember(Name = "status", EmitDefaultValue = false)]
        public StatusEnum? Status { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationPostRequest" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected AuthorizationPostRequest() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="AuthorizationPostRequest" /> class.
        /// </summary>
        /// <param name="status">If inactive the token is inactive and requests using the token will be rejected. (default to StatusEnum.Active).</param>
        /// <param name="description">A description of the token..</param>
        /// <param name="orgID">ID of org that authorization is scoped to. (required).</param>
        /// <param name="userID">ID of user that authorization is scoped to..</param>
        /// <param name="permissions">List of permissions for an auth.  An auth must have at least one Permission. (required).</param>
        public AuthorizationPostRequest(StatusEnum? status = StatusEnum.Active, string description = default(string), string orgID = default(string), string userID = default(string), List<Permission> permissions = default(List<Permission>))
        {
            // to ensure "orgID" is required (not null)
            if (orgID == null) {
                throw new ArgumentNullException("orgID is a required property for AuthorizationPostRequest and cannot be null");
            }
            this.OrgID = orgID;
            // to ensure "permissions" is required (not null)
            if (permissions == null) {
                throw new ArgumentNullException("permissions is a required property for AuthorizationPostRequest and cannot be null");
            }
            this.Permissions = permissions;
            this.Status = status;
            this.Description = description;
            this.UserID = userID;
        }

        /// <summary>
        /// A description of the token.
        /// </summary>
        /// <value>A description of the token.</value>
        [DataMember(Name = "description", EmitDefaultValue = false)]
        public string Description { get; set; }

        /// <summary>
        /// ID of org that authorization is scoped to.
        /// </summary>
        /// <value>ID of org that authorization is scoped to.</value>
        [DataMember(Name = "orgID", IsRequired = true, EmitDefaultValue = false)]
        public string OrgID { get; set; }

        /// <summary>
        /// ID of user that authorization is scoped to.
        /// </summary>
        /// <value>ID of user that authorization is scoped to.</value>
        [DataMember(Name = "userID", EmitDefaultValue = false)]
        public string UserID { get; set; }

        /// <summary>
        /// List of permissions for an auth.  An auth must have at least one Permission.
        /// </summary>
        /// <value>List of permissions for an auth.  An auth must have at least one Permission.</value>
        [DataMember(Name = "permissions", IsRequired = true, EmitDefaultValue = false)]
        public List<Permission> Permissions { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class AuthorizationPostRequest {\n");
            sb.Append("  Status: ").Append(Status).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  OrgID: ").Append(OrgID).Append("\n");
            sb.Append("  UserID: ").Append(UserID).Append("\n");
            sb.Append("  Permissions: ").Append(Permissions).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as AuthorizationPostRequest);
        }

        /// <summary>
        /// Returns true if AuthorizationPostRequest instances are equal
        /// </summary>
        /// <param name="input">Instance of AuthorizationPostRequest to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(AuthorizationPostRequest input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Status == input.Status ||
                    this.Status.Equals(input.Status)
                ) && 
                (
                    this.Description == input.Description ||
                    (this.Description != null &&
                    this.Description.Equals(input.Description))
                ) && 
                (
                    this.OrgID == input.OrgID ||
                    (this.OrgID != null &&
                    this.OrgID.Equals(input.OrgID))
                ) && 
                (
                    this.UserID == input.UserID ||
                    (this.UserID != null &&
                    this.UserID.Equals(input.UserID))
                ) && 
                (
                    this.Permissions == input.Permissions ||
                    this.Permissions != null &&
                    input.Permissions != null &&
                    this.Permissions.SequenceEqual(input.Permissions)
                );
        }

        /// <summary>
        /// Gets the hash code
        /// </summary>
        /// <returns>Hash code</returns>
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                hashCode = hashCode * 59 + this.Status.GetHashCode();
                if (this.Description != null)
                    hashCode = hashCode * 59 + this.Description.GetHashCode();
                if (this.OrgID != null)
                    hashCode = hashCode * 59 + this.OrgID.GetHashCode();
                if (this.UserID != null)
                    hashCode = hashCode * 59 + this.UserID.GetHashCode();
                if (this.Permissions != null)
                    hashCode = hashCode * 59 + this.Permissions.GetHashCode();
                return hashCode;
            }
        }

    }

}
