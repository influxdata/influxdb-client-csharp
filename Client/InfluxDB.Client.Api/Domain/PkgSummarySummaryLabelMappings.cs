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
using OpenAPIDateConverter = InfluxDB.Client.Api.Client.OpenAPIDateConverter;

namespace InfluxDB.Client.Api.Domain
{
    /// <summary>
    /// PkgSummarySummaryLabelMappings
    /// </summary>
    [DataContract]
    public partial class PkgSummarySummaryLabelMappings :  IEquatable<PkgSummarySummaryLabelMappings>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PkgSummarySummaryLabelMappings" /> class.
        /// </summary>
        /// <param name="resourceName">resourceName.</param>
        /// <param name="resourceID">resourceID.</param>
        /// <param name="resourceType">resourceType.</param>
        /// <param name="labelName">labelName.</param>
        /// <param name="labelID">labelID.</param>
        public PkgSummarySummaryLabelMappings(string resourceName = default(string), string resourceID = default(string), string resourceType = default(string), string labelName = default(string), string labelID = default(string))
        {
            this.ResourceName = resourceName;
            this.ResourceID = resourceID;
            this.ResourceType = resourceType;
            this.LabelName = labelName;
            this.LabelID = labelID;
        }

        /// <summary>
        /// Gets or Sets ResourceName
        /// </summary>
        [DataMember(Name="resourceName", EmitDefaultValue=false)]
        public string ResourceName { get; set; }

        /// <summary>
        /// Gets or Sets ResourceID
        /// </summary>
        [DataMember(Name="resourceID", EmitDefaultValue=false)]
        public string ResourceID { get; set; }

        /// <summary>
        /// Gets or Sets ResourceType
        /// </summary>
        [DataMember(Name="resourceType", EmitDefaultValue=false)]
        public string ResourceType { get; set; }

        /// <summary>
        /// Gets or Sets LabelName
        /// </summary>
        [DataMember(Name="labelName", EmitDefaultValue=false)]
        public string LabelName { get; set; }

        /// <summary>
        /// Gets or Sets LabelID
        /// </summary>
        [DataMember(Name="labelID", EmitDefaultValue=false)]
        public string LabelID { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class PkgSummarySummaryLabelMappings {\n");
            sb.Append("  ResourceName: ").Append(ResourceName).Append("\n");
            sb.Append("  ResourceID: ").Append(ResourceID).Append("\n");
            sb.Append("  ResourceType: ").Append(ResourceType).Append("\n");
            sb.Append("  LabelName: ").Append(LabelName).Append("\n");
            sb.Append("  LabelID: ").Append(LabelID).Append("\n");
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
            return this.Equals(input as PkgSummarySummaryLabelMappings);
        }

        /// <summary>
        /// Returns true if PkgSummarySummaryLabelMappings instances are equal
        /// </summary>
        /// <param name="input">Instance of PkgSummarySummaryLabelMappings to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PkgSummarySummaryLabelMappings input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.ResourceName == input.ResourceName ||
                    (this.ResourceName != null &&
                    this.ResourceName.Equals(input.ResourceName))
                ) && 
                (
                    this.ResourceID == input.ResourceID ||
                    (this.ResourceID != null &&
                    this.ResourceID.Equals(input.ResourceID))
                ) && 
                (
                    this.ResourceType == input.ResourceType ||
                    (this.ResourceType != null &&
                    this.ResourceType.Equals(input.ResourceType))
                ) && 
                (
                    this.LabelName == input.LabelName ||
                    (this.LabelName != null &&
                    this.LabelName.Equals(input.LabelName))
                ) && 
                (
                    this.LabelID == input.LabelID ||
                    (this.LabelID != null &&
                    this.LabelID.Equals(input.LabelID))
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
                if (this.ResourceName != null)
                    hashCode = hashCode * 59 + this.ResourceName.GetHashCode();
                if (this.ResourceID != null)
                    hashCode = hashCode * 59 + this.ResourceID.GetHashCode();
                if (this.ResourceType != null)
                    hashCode = hashCode * 59 + this.ResourceType.GetHashCode();
                if (this.LabelName != null)
                    hashCode = hashCode * 59 + this.LabelName.GetHashCode();
                if (this.LabelID != null)
                    hashCode = hashCode * 59 + this.LabelID.GetHashCode();
                return hashCode;
            }
        }

    }

}
