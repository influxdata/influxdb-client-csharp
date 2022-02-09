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
    /// CellLinks
    /// </summary>
    [DataContract]
    public partial class CellLinks :  IEquatable<CellLinks>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CellLinks" /> class.
        /// </summary>
        /// <param name="self">self.</param>
        /// <param name="view">view.</param>
        public CellLinks(string self = default(string), string view = default(string))
        {
            this.Self = self;
            this.View = view;
        }

        /// <summary>
        /// Gets or Sets Self
        /// </summary>
        [DataMember(Name="self", EmitDefaultValue=false)]
        public string Self { get; set; }

        /// <summary>
        /// Gets or Sets View
        /// </summary>
        [DataMember(Name="view", EmitDefaultValue=false)]
        public string View { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class CellLinks {\n");
            sb.Append("  Self: ").Append(Self).Append("\n");
            sb.Append("  View: ").Append(View).Append("\n");
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
            return this.Equals(input as CellLinks);
        }

        /// <summary>
        /// Returns true if CellLinks instances are equal
        /// </summary>
        /// <param name="input">Instance of CellLinks to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(CellLinks input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Self == input.Self ||
                    (this.Self != null && this.Self.Equals(input.Self))
                ) && 
                (
                    this.View == input.View ||
                    (this.View != null && this.View.Equals(input.View))
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
                
                if (this.Self != null)
                    hashCode = hashCode * 59 + this.Self.GetHashCode();
                if (this.View != null)
                    hashCode = hashCode * 59 + this.View.GetHashCode();
                return hashCode;
            }
        }

    }

}
