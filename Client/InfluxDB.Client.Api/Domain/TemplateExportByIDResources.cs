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
    /// TemplateExportByIDResources
    /// </summary>
    [DataContract]
    public partial class TemplateExportByIDResources : IEquatable<TemplateExportByIDResources>
    {
        /// <summary>
        /// Gets or Sets Kind
        /// </summary>
        [DataMember(Name = "kind", EmitDefaultValue = false)]
        public TemplateKind Kind { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateExportByIDResources" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected TemplateExportByIDResources()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateExportByIDResources" /> class.
        /// </summary>
        /// <param name="id">id (required).</param>
        /// <param name="kind">kind (required).</param>
        /// <param name="name">if defined with id, name is used for resource exported by id. if defined independently, resources strictly matching name are exported.</param>
        public TemplateExportByIDResources(string id = default, TemplateKind kind = default, string name = default)
        {
            // to ensure "id" is required (not null)
            if (id == null)
            {
                throw new InvalidDataException(
                    "id is a required property for TemplateExportByIDResources and cannot be null");
            }

            Id = id;
            // to ensure "kind" is required (not null)
            Kind = kind;
            Name = name;
        }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; set; }


        /// <summary>
        /// if defined with id, name is used for resource exported by id. if defined independently, resources strictly matching name are exported
        /// </summary>
        /// <value>if defined with id, name is used for resource exported by id. if defined independently, resources strictly matching name are exported</value>
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class TemplateExportByIDResources {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Kind: ").Append(Kind).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
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
            return Equals(input as TemplateExportByIDResources);
        }

        /// <summary>
        /// Returns true if TemplateExportByIDResources instances are equal
        /// </summary>
        /// <param name="input">Instance of TemplateExportByIDResources to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TemplateExportByIDResources input)
        {
            if (input == null)
            {
                return false;
            }

            return
                (
                    Id == input.Id ||
                    Id != null && Id.Equals(input.Id)
                ) &&
                (
                    Kind == input.Kind ||
                    Kind.Equals(input.Kind)
                ) &&
                (
                    Name == input.Name ||
                    Name != null && Name.Equals(input.Name)
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
                var hashCode = 41;

                if (Id != null)
                {
                    hashCode = hashCode * 59 + Id.GetHashCode();
                }

                hashCode = hashCode * 59 + Kind.GetHashCode();
                if (Name != null)
                {
                    hashCode = hashCode * 59 + Name.GetHashCode();
                }

                return hashCode;
            }
        }
    }
}