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
    /// StackResources
    /// </summary>
    [DataContract]
    public partial class StackResources :  IEquatable<StackResources>
    {
        /// <summary>
        /// Gets or Sets Kind
        /// </summary>
        [DataMember(Name="kind", EmitDefaultValue=false)]
        public TemplateKind? Kind { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="StackResources" /> class.
        /// </summary>
        /// <param name="apiVersion">apiVersion.</param>
        /// <param name="resourceID">resourceID.</param>
        /// <param name="kind">kind.</param>
        /// <param name="templateMetaName">templateMetaName.</param>
        /// <param name="associations">associations.</param>
        /// <param name="links">links.</param>
        public StackResources(string apiVersion = default(string), string resourceID = default(string), TemplateKind? kind = default(TemplateKind?), string templateMetaName = default(string), List<StackAssociations> associations = default(List<StackAssociations>), StackLinks links = default(StackLinks))
        {
            this.ApiVersion = apiVersion;
            this.ResourceID = resourceID;
            this.Kind = kind;
            this.TemplateMetaName = templateMetaName;
            this.Associations = associations;
            this.Links = links;
        }

        /// <summary>
        /// Gets or Sets ApiVersion
        /// </summary>
        [DataMember(Name="apiVersion", EmitDefaultValue=false)]
        public string ApiVersion { get; set; }

        /// <summary>
        /// Gets or Sets ResourceID
        /// </summary>
        [DataMember(Name="resourceID", EmitDefaultValue=false)]
        public string ResourceID { get; set; }


        /// <summary>
        /// Gets or Sets TemplateMetaName
        /// </summary>
        [DataMember(Name="templateMetaName", EmitDefaultValue=false)]
        public string TemplateMetaName { get; set; }

        /// <summary>
        /// Gets or Sets Associations
        /// </summary>
        [DataMember(Name="associations", EmitDefaultValue=false)]
        public List<StackAssociations> Associations { get; set; }

        /// <summary>
        /// Gets or Sets Links
        /// </summary>
        [DataMember(Name="links", EmitDefaultValue=false)]
        public StackLinks Links { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class StackResources {\n");
            sb.Append("  ApiVersion: ").Append(ApiVersion).Append("\n");
            sb.Append("  ResourceID: ").Append(ResourceID).Append("\n");
            sb.Append("  Kind: ").Append(Kind).Append("\n");
            sb.Append("  TemplateMetaName: ").Append(TemplateMetaName).Append("\n");
            sb.Append("  Associations: ").Append(Associations).Append("\n");
            sb.Append("  Links: ").Append(Links).Append("\n");
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
            return this.Equals(input as StackResources);
        }

        /// <summary>
        /// Returns true if StackResources instances are equal
        /// </summary>
        /// <param name="input">Instance of StackResources to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(StackResources input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.ApiVersion == input.ApiVersion ||
                    (this.ApiVersion != null &&
                    this.ApiVersion.Equals(input.ApiVersion))
                ) && 
                (
                    this.ResourceID == input.ResourceID ||
                    (this.ResourceID != null &&
                    this.ResourceID.Equals(input.ResourceID))
                ) && 
                (
                    this.Kind == input.Kind ||
                    (this.Kind != null &&
                    this.Kind.Equals(input.Kind))
                ) && 
                (
                    this.TemplateMetaName == input.TemplateMetaName ||
                    (this.TemplateMetaName != null &&
                    this.TemplateMetaName.Equals(input.TemplateMetaName))
                ) && 
                (
                    this.Associations == input.Associations ||
                    this.Associations != null &&
                    this.Associations.SequenceEqual(input.Associations)
                ) && 
                (
                    
                    (this.Links != null &&
                    this.Links.Equals(input.Links))
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
                if (this.ApiVersion != null)
                    hashCode = hashCode * 59 + this.ApiVersion.GetHashCode();
                if (this.ResourceID != null)
                    hashCode = hashCode * 59 + this.ResourceID.GetHashCode();
                if (this.Kind != null)
                    hashCode = hashCode * 59 + this.Kind.GetHashCode();
                if (this.TemplateMetaName != null)
                    hashCode = hashCode * 59 + this.TemplateMetaName.GetHashCode();
                if (this.Associations != null)
                    hashCode = hashCode * 59 + this.Associations.GetHashCode();
                if (this.Links != null)
                    hashCode = hashCode * 59 + this.Links.GetHashCode();
                return hashCode;
            }
        }

    }

}
