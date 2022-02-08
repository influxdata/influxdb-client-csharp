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
    /// PatchDashboardRequest
    /// </summary>
    [DataContract]
    public partial class PatchDashboardRequest :  IEquatable<PatchDashboardRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PatchDashboardRequest" /> class.
        /// </summary>
        /// <param name="name">optional, when provided will replace the name.</param>
        /// <param name="description">optional, when provided will replace the description.</param>
        /// <param name="cells">cells.</param>
        public PatchDashboardRequest(string name = default(string), string description = default(string), CellWithViewProperties cells = default(CellWithViewProperties))
        {
            this.Name = name;
            this.Description = description;
            this.Cells = cells;
        }

        /// <summary>
        /// optional, when provided will replace the name
        /// </summary>
        /// <value>optional, when provided will replace the name</value>
        [DataMember(Name="name", EmitDefaultValue=false)]
        public string Name { get; set; }

        /// <summary>
        /// optional, when provided will replace the description
        /// </summary>
        /// <value>optional, when provided will replace the description</value>
        [DataMember(Name="description", EmitDefaultValue=false)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or Sets Cells
        /// </summary>
        [DataMember(Name="cells", EmitDefaultValue=false)]
        public CellWithViewProperties Cells { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class PatchDashboardRequest {\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  Cells: ").Append(Cells).Append("\n");
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
            return this.Equals(input as PatchDashboardRequest);
        }

        /// <summary>
        /// Returns true if PatchDashboardRequest instances are equal
        /// </summary>
        /// <param name="input">Instance of PatchDashboardRequest to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PatchDashboardRequest input)
        {
            if (input == null)
                return false;

            return 
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
                    
                    (this.Cells != null &&
                    this.Cells.Equals(input.Cells))
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
                if (this.Name != null)
                    hashCode = hashCode * 59 + this.Name.GetHashCode();
                if (this.Description != null)
                    hashCode = hashCode * 59 + this.Description.GetHashCode();
                if (this.Cells != null)
                    hashCode = hashCode * 59 + this.Cells.GetHashCode();
                return hashCode;
            }
        }

    }

}
