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
    /// TemplateApplyTemplate
    /// </summary>
    [DataContract]
    public partial class TemplateApplyTemplate :  IEquatable<TemplateApplyTemplate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateApplyTemplate" /> class.
        /// </summary>
        /// <param name="contentType">contentType.</param>
        /// <param name="sources">sources.</param>
        /// <param name="contents">contents.</param>
        public TemplateApplyTemplate(string contentType = default(string), List<string> sources = default(List<string>), List<Object> contents = default(List<Object>))
        {
            this.ContentType = contentType;
            this.Sources = sources;
            this.Contents = contents;
        }

        /// <summary>
        /// Gets or Sets ContentType
        /// </summary>
        [DataMember(Name="contentType", EmitDefaultValue=false)]
        public string ContentType { get; set; }

        /// <summary>
        /// Gets or Sets Sources
        /// </summary>
        [DataMember(Name="sources", EmitDefaultValue=false)]
        public List<string> Sources { get; set; }

        /// <summary>
        /// Gets or Sets Contents
        /// </summary>
        [DataMember(Name="contents", EmitDefaultValue=false)]
        public List<Object> Contents { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class TemplateApplyTemplate {\n");
            sb.Append("  ContentType: ").Append(ContentType).Append("\n");
            sb.Append("  Sources: ").Append(Sources).Append("\n");
            sb.Append("  Contents: ").Append(Contents).Append("\n");
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
            return this.Equals(input as TemplateApplyTemplate);
        }

        /// <summary>
        /// Returns true if TemplateApplyTemplate instances are equal
        /// </summary>
        /// <param name="input">Instance of TemplateApplyTemplate to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TemplateApplyTemplate input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.ContentType == input.ContentType ||
                    (this.ContentType != null && this.ContentType.Equals(input.ContentType))
                ) && 
                (
                    this.Sources == input.Sources ||
                    this.Sources != null &&
                    this.Sources.SequenceEqual(input.Sources)
                ) && 
                (
                    this.Contents == input.Contents ||
                    this.Contents != null &&
                    this.Contents.SequenceEqual(input.Contents)
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
                
                if (this.ContentType != null)
                    hashCode = hashCode * 59 + this.ContentType.GetHashCode();
                if (this.Sources != null)
                    hashCode = hashCode * 59 + this.Sources.GetHashCode();
                if (this.Contents != null)
                    hashCode = hashCode * 59 + this.Contents.GetHashCode();
                return hashCode;
            }
        }

    }

}
