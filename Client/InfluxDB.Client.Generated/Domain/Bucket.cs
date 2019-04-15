/* 
 * Influx API Service
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * OpenAPI spec version: 0.1.0
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
using System.ComponentModel.DataAnnotations;
using OpenAPIDateConverter = InfluxDB.Client.Generated.Client.OpenAPIDateConverter;

namespace InfluxDB.Client.Generated.Domain
{
    /// <summary>
    /// Bucket
    /// </summary>
    [DataContract]
    public partial class Bucket :  IEquatable<Bucket>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Bucket" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected Bucket() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Bucket" /> class.
        /// </summary>
        /// <param name="links">links.</param>
        /// <param name="name">name (required).</param>
        /// <param name="organizationID">organizationID.</param>
        /// <param name="organization">organization.</param>
        /// <param name="rp">rp.</param>
        /// <param name="retentionRules">rules to expire or retain data.  No rules means data never expires. (required).</param>
        /// <param name="labels">labels.</param>
        public Bucket(BucketLinks links = default(BucketLinks), string name = default(string), string organizationID = default(string), string organization = default(string), string rp = default(string), List<BucketRetentionRules> retentionRules = default(List<BucketRetentionRules>), Labels labels = default(Labels))
        {
            // to ensure "name" is required (not null)
            if (name == null)
            {
                throw new InvalidDataException("name is a required property for Bucket and cannot be null");
            }
            else
            {
                this.Name = name;
            }
            // to ensure "retentionRules" is required (not null)
            if (retentionRules == null)
            {
                throw new InvalidDataException("retentionRules is a required property for Bucket and cannot be null");
            }
            else
            {
                this.RetentionRules = retentionRules;
            }
            this.Links = links;
            this.OrganizationID = organizationID;
            this.Organization = organization;
            this.Rp = rp;
            this.Labels = labels;
        }

        /// <summary>
        /// Gets or Sets Links
        /// </summary>
        [DataMember(Name="links", EmitDefaultValue=false)]
        public BucketLinks Links { get; set; }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name="id", EmitDefaultValue=false)]
        public string Id { get; private set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name="name", EmitDefaultValue=false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets OrganizationID
        /// </summary>
        [DataMember(Name="organizationID", EmitDefaultValue=false)]
        public string OrganizationID { get; set; }

        /// <summary>
        /// Gets or Sets Organization
        /// </summary>
        [DataMember(Name="organization", EmitDefaultValue=false)]
        public string Organization { get; set; }

        /// <summary>
        /// Gets or Sets Rp
        /// </summary>
        [DataMember(Name="rp", EmitDefaultValue=false)]
        public string Rp { get; set; }

        /// <summary>
        /// rules to expire or retain data.  No rules means data never expires.
        /// </summary>
        /// <value>rules to expire or retain data.  No rules means data never expires.</value>
        [DataMember(Name="retentionRules", EmitDefaultValue=false)]
        public List<BucketRetentionRules> RetentionRules { get; set; }

        /// <summary>
        /// Gets or Sets Labels
        /// </summary>
        [DataMember(Name="labels", EmitDefaultValue=false)]
        public Labels Labels { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Bucket {\n");
            sb.Append("  Links: ").Append(Links).Append("\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  OrganizationID: ").Append(OrganizationID).Append("\n");
            sb.Append("  Organization: ").Append(Organization).Append("\n");
            sb.Append("  Rp: ").Append(Rp).Append("\n");
            sb.Append("  RetentionRules: ").Append(RetentionRules).Append("\n");
            sb.Append("  Labels: ").Append(Labels).Append("\n");
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
            return this.Equals(input as Bucket);
        }

        /// <summary>
        /// Returns true if Bucket instances are equal
        /// </summary>
        /// <param name="input">Instance of Bucket to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Bucket input)
        {
            if (input == null)
                return false;

            return 
                (
                    
                    (this.Links != null &&
                    this.Links.Equals(input.Links))
                ) && 
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
                    this.OrganizationID == input.OrganizationID ||
                    (this.OrganizationID != null &&
                    this.OrganizationID.Equals(input.OrganizationID))
                ) && 
                (
                    this.Organization == input.Organization ||
                    (this.Organization != null &&
                    this.Organization.Equals(input.Organization))
                ) && 
                (
                    this.Rp == input.Rp ||
                    (this.Rp != null &&
                    this.Rp.Equals(input.Rp))
                ) && 
                (
                    this.RetentionRules == input.RetentionRules ||
                    this.RetentionRules != null &&
                    this.RetentionRules.SequenceEqual(input.RetentionRules)
                ) && 
                (
                    
                    (this.Labels != null &&
                    this.Labels.Equals(input.Labels))
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
                if (this.Links != null)
                    hashCode = hashCode * 59 + this.Links.GetHashCode();
                if (this.Id != null)
                    hashCode = hashCode * 59 + this.Id.GetHashCode();
                if (this.Name != null)
                    hashCode = hashCode * 59 + this.Name.GetHashCode();
                if (this.OrganizationID != null)
                    hashCode = hashCode * 59 + this.OrganizationID.GetHashCode();
                if (this.Organization != null)
                    hashCode = hashCode * 59 + this.Organization.GetHashCode();
                if (this.Rp != null)
                    hashCode = hashCode * 59 + this.Rp.GetHashCode();
                if (this.RetentionRules != null)
                    hashCode = hashCode * 59 + this.RetentionRules.GetHashCode();
                if (this.Labels != null)
                    hashCode = hashCode * 59 + this.Labels.GetHashCode();
                return hashCode;
            }
        }

    }

}