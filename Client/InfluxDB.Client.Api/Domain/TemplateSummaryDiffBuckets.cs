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
    /// TemplateSummaryDiffBuckets
    /// </summary>
    [DataContract]
    public partial class TemplateSummaryDiffBuckets :  IEquatable<TemplateSummaryDiffBuckets>
    {
        /// <summary>
        /// Gets or Sets Kind
        /// </summary>
        [DataMember(Name="kind", EmitDefaultValue=false)]
        public TemplateKind? Kind { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateSummaryDiffBuckets" /> class.
        /// </summary>
        /// <param name="kind">kind.</param>
        /// <param name="stateStatus">stateStatus.</param>
        /// <param name="id">id.</param>
        /// <param name="templateMetaName">templateMetaName.</param>
        /// <param name="_new">_new.</param>
        /// <param name="old">old.</param>
        public TemplateSummaryDiffBuckets(TemplateKind? kind = default(TemplateKind?), string stateStatus = default(string), string id = default(string), string templateMetaName = default(string), TemplateSummaryDiffBucketsNewOld _new = default(TemplateSummaryDiffBucketsNewOld), TemplateSummaryDiffBucketsNewOld old = default(TemplateSummaryDiffBucketsNewOld))
        {
            this.Kind = kind;
            this.StateStatus = stateStatus;
            this.Id = id;
            this.TemplateMetaName = templateMetaName;
            this.New = _new;
            this.Old = old;
        }


        /// <summary>
        /// Gets or Sets StateStatus
        /// </summary>
        [DataMember(Name="stateStatus", EmitDefaultValue=false)]
        public string StateStatus { get; set; }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name="id", EmitDefaultValue=false)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets TemplateMetaName
        /// </summary>
        [DataMember(Name="templateMetaName", EmitDefaultValue=false)]
        public string TemplateMetaName { get; set; }

        /// <summary>
        /// Gets or Sets New
        /// </summary>
        [DataMember(Name="new", EmitDefaultValue=false)]
        public TemplateSummaryDiffBucketsNewOld New { get; set; }

        /// <summary>
        /// Gets or Sets Old
        /// </summary>
        [DataMember(Name="old", EmitDefaultValue=false)]
        public TemplateSummaryDiffBucketsNewOld Old { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class TemplateSummaryDiffBuckets {\n");
            sb.Append("  Kind: ").Append(Kind).Append("\n");
            sb.Append("  StateStatus: ").Append(StateStatus).Append("\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  TemplateMetaName: ").Append(TemplateMetaName).Append("\n");
            sb.Append("  New: ").Append(New).Append("\n");
            sb.Append("  Old: ").Append(Old).Append("\n");
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
            return this.Equals(input as TemplateSummaryDiffBuckets);
        }

        /// <summary>
        /// Returns true if TemplateSummaryDiffBuckets instances are equal
        /// </summary>
        /// <param name="input">Instance of TemplateSummaryDiffBuckets to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TemplateSummaryDiffBuckets input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Kind == input.Kind ||
                    (this.Kind != null &&
                    this.Kind.Equals(input.Kind))
                ) && 
                (
                    this.StateStatus == input.StateStatus ||
                    (this.StateStatus != null &&
                    this.StateStatus.Equals(input.StateStatus))
                ) && 
                (
                    this.Id == input.Id ||
                    (this.Id != null &&
                    this.Id.Equals(input.Id))
                ) && 
                (
                    this.TemplateMetaName == input.TemplateMetaName ||
                    (this.TemplateMetaName != null &&
                    this.TemplateMetaName.Equals(input.TemplateMetaName))
                ) && 
                (
                    
                    (this.New != null &&
                    this.New.Equals(input.New))
                ) && 
                (
                    
                    (this.Old != null &&
                    this.Old.Equals(input.Old))
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
                if (this.Kind != null)
                    hashCode = hashCode * 59 + this.Kind.GetHashCode();
                if (this.StateStatus != null)
                    hashCode = hashCode * 59 + this.StateStatus.GetHashCode();
                if (this.Id != null)
                    hashCode = hashCode * 59 + this.Id.GetHashCode();
                if (this.TemplateMetaName != null)
                    hashCode = hashCode * 59 + this.TemplateMetaName.GetHashCode();
                if (this.New != null)
                    hashCode = hashCode * 59 + this.New.GetHashCode();
                if (this.Old != null)
                    hashCode = hashCode * 59 + this.Old.GetHashCode();
                return hashCode;
            }
        }

    }

}