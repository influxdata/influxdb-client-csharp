/*
 * InfluxDB OSS API Service
 *
 * The InfluxDB v2 API provides a programmatic interface for all interactions with InfluxDB. Access the InfluxDB API using the `/api/v2/` endpoint. 
 *
 * The version of the OpenAPI document: 2.0.0
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using FileParameter = InfluxDB.Client.Core.Api.FileParameter;
using OpenAPIDateConverter = InfluxDB.Client.Core.Api.OpenAPIDateConverter;

namespace InfluxDB.Client.Api.Domain
{
    /// <summary>
    /// LabelMapping
    /// </summary>
    [DataContract(Name = "LabelMapping")]
    public partial class LabelMapping : IEquatable<LabelMapping>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LabelMapping" /> class.
        /// </summary>
        /// <param name="labelID">labelID.</param>
        public LabelMapping(string labelID = default(string))
        {
            this.LabelID = labelID;
        }

        /// <summary>
        /// Gets or Sets LabelID
        /// </summary>
        [DataMember(Name = "labelID", EmitDefaultValue = false)]
        public string LabelID { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class LabelMapping {\n");
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
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as LabelMapping);
        }

        /// <summary>
        /// Returns true if LabelMapping instances are equal
        /// </summary>
        /// <param name="input">Instance of LabelMapping to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(LabelMapping input)
        {
            if (input == null)
                return false;

            return 
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
                if (this.LabelID != null)
                    hashCode = hashCode * 59 + this.LabelID.GetHashCode();
                return hashCode;
            }
        }

    }

}
