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
    /// TemplateSummarySummaryDashboards
    /// </summary>
    [DataContract]
    public partial class TemplateSummarySummaryDashboards :  IEquatable<TemplateSummarySummaryDashboards>
    {
        /// <summary>
        /// Gets or Sets Kind
        /// </summary>
        [DataMember(Name="kind", EmitDefaultValue=false)]
        public TemplateKind? Kind { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateSummarySummaryDashboards" /> class.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="orgID">orgID.</param>
        /// <param name="kind">kind.</param>
        /// <param name="templateMetaName">templateMetaName.</param>
        /// <param name="name">name.</param>
        /// <param name="description">description.</param>
        /// <param name="labelAssociations">labelAssociations.</param>
        /// <param name="charts">charts.</param>
        /// <param name="envReferences">envReferences.</param>
        public TemplateSummarySummaryDashboards(string id = default(string), string orgID = default(string), TemplateKind? kind = default(TemplateKind?), string templateMetaName = default(string), string name = default(string), string description = default(string), List<TemplateSummaryLabel> labelAssociations = default(List<TemplateSummaryLabel>), List<TemplateChart> charts = default(List<TemplateChart>), List<Object> envReferences = default(List<Object>))
        {
            this.Id = id;
            this.OrgID = orgID;
            this.Kind = kind;
            this.TemplateMetaName = templateMetaName;
            this.Name = name;
            this.Description = description;
            this.LabelAssociations = labelAssociations;
            this.Charts = charts;
            this.EnvReferences = envReferences;
        }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name="id", EmitDefaultValue=false)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets OrgID
        /// </summary>
        [DataMember(Name="orgID", EmitDefaultValue=false)]
        public string OrgID { get; set; }


        /// <summary>
        /// Gets or Sets TemplateMetaName
        /// </summary>
        [DataMember(Name="templateMetaName", EmitDefaultValue=false)]
        public string TemplateMetaName { get; set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name="name", EmitDefaultValue=false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets Description
        /// </summary>
        [DataMember(Name="description", EmitDefaultValue=false)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or Sets LabelAssociations
        /// </summary>
        [DataMember(Name="labelAssociations", EmitDefaultValue=false)]
        public List<TemplateSummaryLabel> LabelAssociations { get; set; }

        /// <summary>
        /// Gets or Sets Charts
        /// </summary>
        [DataMember(Name="charts", EmitDefaultValue=false)]
        public List<TemplateChart> Charts { get; set; }

        /// <summary>
        /// Gets or Sets EnvReferences
        /// </summary>
        [DataMember(Name="envReferences", EmitDefaultValue=false)]
        public List<Object> EnvReferences { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class TemplateSummarySummaryDashboards {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  OrgID: ").Append(OrgID).Append("\n");
            sb.Append("  Kind: ").Append(Kind).Append("\n");
            sb.Append("  TemplateMetaName: ").Append(TemplateMetaName).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  LabelAssociations: ").Append(LabelAssociations).Append("\n");
            sb.Append("  Charts: ").Append(Charts).Append("\n");
            sb.Append("  EnvReferences: ").Append(EnvReferences).Append("\n");
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
            return this.Equals(input as TemplateSummarySummaryDashboards);
        }

        /// <summary>
        /// Returns true if TemplateSummarySummaryDashboards instances are equal
        /// </summary>
        /// <param name="input">Instance of TemplateSummarySummaryDashboards to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TemplateSummarySummaryDashboards input)
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
                    this.OrgID == input.OrgID ||
                    (this.OrgID != null &&
                    this.OrgID.Equals(input.OrgID))
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
                    this.Name == input.Name ||
                    (this.Name != null &&
                    this.Name.Equals(input.Name))
                ) && 
                (
                    this.Description == input.Description ||
                    (this.Description != null &&
                    this.Description.Equals(input.Description))
                ) && 
                (
                    this.LabelAssociations == input.LabelAssociations ||
                    this.LabelAssociations != null &&
                    this.LabelAssociations.SequenceEqual(input.LabelAssociations)
                ) && 
                (
                    this.Charts == input.Charts ||
                    this.Charts != null &&
                    this.Charts.SequenceEqual(input.Charts)
                ) && 
                (
                    this.EnvReferences == input.EnvReferences ||
                    this.EnvReferences != null &&
                    this.EnvReferences.SequenceEqual(input.EnvReferences)
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
                if (this.OrgID != null)
                    hashCode = hashCode * 59 + this.OrgID.GetHashCode();
                if (this.Kind != null)
                    hashCode = hashCode * 59 + this.Kind.GetHashCode();
                if (this.TemplateMetaName != null)
                    hashCode = hashCode * 59 + this.TemplateMetaName.GetHashCode();
                if (this.Name != null)
                    hashCode = hashCode * 59 + this.Name.GetHashCode();
                if (this.Description != null)
                    hashCode = hashCode * 59 + this.Description.GetHashCode();
                if (this.LabelAssociations != null)
                    hashCode = hashCode * 59 + this.LabelAssociations.GetHashCode();
                if (this.Charts != null)
                    hashCode = hashCode * 59 + this.Charts.GetHashCode();
                if (this.EnvReferences != null)
                    hashCode = hashCode * 59 + this.EnvReferences.GetHashCode();
                return hashCode;
            }
        }

    }

}
