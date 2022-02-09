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
    /// DBRPUpdate
    /// </summary>
    [DataContract]
    public partial class DBRPUpdate :  IEquatable<DBRPUpdate>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DBRPUpdate" /> class.
        /// </summary>
        /// <param name="retentionPolicy">InfluxDB v1 retention policy.</param>
        /// <param name="_default">_default.</param>
        public DBRPUpdate(string retentionPolicy = default(string), bool? _default = default(bool?))
        {
            this.RetentionPolicy = retentionPolicy;
            this.Default = _default;
        }

        /// <summary>
        /// InfluxDB v1 retention policy
        /// </summary>
        /// <value>InfluxDB v1 retention policy</value>
        [DataMember(Name="retention_policy", EmitDefaultValue=false)]
        public string RetentionPolicy { get; set; }

        /// <summary>
        /// Gets or Sets Default
        /// </summary>
        [DataMember(Name="default", EmitDefaultValue=false)]
        public bool? Default { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class DBRPUpdate {\n");
            sb.Append("  RetentionPolicy: ").Append(RetentionPolicy).Append("\n");
            sb.Append("  Default: ").Append(Default).Append("\n");
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
            return this.Equals(input as DBRPUpdate);
        }

        /// <summary>
        /// Returns true if DBRPUpdate instances are equal
        /// </summary>
        /// <param name="input">Instance of DBRPUpdate to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(DBRPUpdate input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.RetentionPolicy == input.RetentionPolicy ||
                    (this.RetentionPolicy != null && this.RetentionPolicy.Equals(input.RetentionPolicy))
                ) && 
                (
                    this.Default == input.Default ||
                    (this.Default != null && this.Default.Equals(input.Default))
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
                
                if (this.RetentionPolicy != null)
                    hashCode = hashCode * 59 + this.RetentionPolicy.GetHashCode();
                if (this.Default != null)
                    hashCode = hashCode * 59 + this.Default.GetHashCode();
                return hashCode;
            }
        }

    }

}
