/* 
 * InfluxDB OSS API Service
 *
 * The InfluxDB v2 API provides a programmatic interface for all interactions with InfluxDB. Access the InfluxDB API using the `/api/v2/` endpoint. 
 *
 * OpenAPI spec version: 2.0.0
 * 
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */

using System;
using System.Linq;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OpenAPIDateConverter = InfluxDB.Client.Api.Client.OpenAPIDateConverter;

namespace InfluxDB.Client.Api.Domain
{
    /// <summary>
    /// RemoteConnection
    /// </summary>
    [DataContract]
    public partial class RemoteConnection :  IEquatable<RemoteConnection>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteConnection" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected RemoteConnection() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="RemoteConnection" /> class.
        /// </summary>
        /// <param name="id">id (required).</param>
        /// <param name="name">name (required).</param>
        /// <param name="orgID">orgID (required).</param>
        /// <param name="description">description.</param>
        /// <param name="remoteURL">remoteURL (required).</param>
        /// <param name="remoteOrgID">remoteOrgID (required).</param>
        /// <param name="allowInsecureTLS">allowInsecureTLS (required) (default to false).</param>
        public RemoteConnection(string id = default(string), string name = default(string), string orgID = default(string), string description = default(string), string remoteURL = default(string), string remoteOrgID = default(string), bool? allowInsecureTLS = false)
        {
            // to ensure "id" is required (not null)
            if (id == null)
            {
                throw new InvalidDataException("id is a required property for RemoteConnection and cannot be null");
            }
            else
            {
                this.Id = id;
            }
            // to ensure "name" is required (not null)
            if (name == null)
            {
                throw new InvalidDataException("name is a required property for RemoteConnection and cannot be null");
            }
            else
            {
                this.Name = name;
            }
            // to ensure "orgID" is required (not null)
            if (orgID == null)
            {
                throw new InvalidDataException("orgID is a required property for RemoteConnection and cannot be null");
            }
            else
            {
                this.OrgID = orgID;
            }
            // to ensure "remoteURL" is required (not null)
            if (remoteURL == null)
            {
                throw new InvalidDataException("remoteURL is a required property for RemoteConnection and cannot be null");
            }
            else
            {
                this.RemoteURL = remoteURL;
            }
            // to ensure "remoteOrgID" is required (not null)
            if (remoteOrgID == null)
            {
                throw new InvalidDataException("remoteOrgID is a required property for RemoteConnection and cannot be null");
            }
            else
            {
                this.RemoteOrgID = remoteOrgID;
            }
            // to ensure "allowInsecureTLS" is required (not null)
            if (allowInsecureTLS == null)
            {
                throw new InvalidDataException("allowInsecureTLS is a required property for RemoteConnection and cannot be null");
            }
            else
            {
                this.AllowInsecureTLS = allowInsecureTLS;
            }
            this.Description = description;
        }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name="id", EmitDefaultValue=false)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name="name", EmitDefaultValue=false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets OrgID
        /// </summary>
        [DataMember(Name="orgID", EmitDefaultValue=false)]
        public string OrgID { get; set; }

        /// <summary>
        /// Gets or Sets Description
        /// </summary>
        [DataMember(Name="description", EmitDefaultValue=false)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or Sets RemoteURL
        /// </summary>
        [DataMember(Name="remoteURL", EmitDefaultValue=false)]
        public string RemoteURL { get; set; }

        /// <summary>
        /// Gets or Sets RemoteOrgID
        /// </summary>
        [DataMember(Name="remoteOrgID", EmitDefaultValue=false)]
        public string RemoteOrgID { get; set; }

        /// <summary>
        /// Gets or Sets AllowInsecureTLS
        /// </summary>
        [DataMember(Name="allowInsecureTLS", EmitDefaultValue=false)]
        public bool? AllowInsecureTLS { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class RemoteConnection {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  OrgID: ").Append(OrgID).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  RemoteURL: ").Append(RemoteURL).Append("\n");
            sb.Append("  RemoteOrgID: ").Append(RemoteOrgID).Append("\n");
            sb.Append("  AllowInsecureTLS: ").Append(AllowInsecureTLS).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as RemoteConnection);
        }

        /// <summary>
        /// Returns true if RemoteConnection instances are equal
        /// </summary>
        /// <param name="input">Instance of RemoteConnection to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(RemoteConnection input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Id == input.Id ||
                    (this.Id != null &&
                    this.Id.Equals(input.Id))
                ) && 
                (
                    this.Name == input.Name ||
                    (this.Name != null &&
                    this.Name.Equals(input.Name))
                ) && 
                (
                    this.OrgID == input.OrgID ||
                    (this.OrgID != null &&
                    this.OrgID.Equals(input.OrgID))
                ) && 
                (
                    this.Description == input.Description ||
                    (this.Description != null &&
                    this.Description.Equals(input.Description))
                ) && 
                (
                    this.RemoteURL == input.RemoteURL ||
                    (this.RemoteURL != null &&
                    this.RemoteURL.Equals(input.RemoteURL))
                ) && 
                (
                    this.RemoteOrgID == input.RemoteOrgID ||
                    (this.RemoteOrgID != null &&
                    this.RemoteOrgID.Equals(input.RemoteOrgID))
                ) && 
                (
                    this.AllowInsecureTLS == input.AllowInsecureTLS ||
                    (this.AllowInsecureTLS != null &&
                    this.AllowInsecureTLS.Equals(input.AllowInsecureTLS))
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
                if (this.Id != null)
                    hashCode = hashCode * 59 + this.Id.GetHashCode();
                if (this.Name != null)
                    hashCode = hashCode * 59 + this.Name.GetHashCode();
                if (this.OrgID != null)
                    hashCode = hashCode * 59 + this.OrgID.GetHashCode();
                if (this.Description != null)
                    hashCode = hashCode * 59 + this.Description.GetHashCode();
                if (this.RemoteURL != null)
                    hashCode = hashCode * 59 + this.RemoteURL.GetHashCode();
                if (this.RemoteOrgID != null)
                    hashCode = hashCode * 59 + this.RemoteOrgID.GetHashCode();
                if (this.AllowInsecureTLS != null)
                    hashCode = hashCode * 59 + this.AllowInsecureTLS.GetHashCode();
                return hashCode;
            }
        }

    }

}