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
    /// QueryVariableProperties
    /// </summary>
    [DataContract(Name = "QueryVariableProperties")]
    public partial class QueryVariableProperties : VariableProperties, IEquatable<QueryVariableProperties>
    {
        /// <summary>
        /// Defines Type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TypeEnum
        {
            /// <summary>
            /// Enum Query for value: query
            /// </summary>
            [EnumMember(Value = "query")]
            Query = 1

        }


        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name = "type", EmitDefaultValue = false)]
        public TypeEnum? Type { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="QueryVariableProperties" /> class.
        /// </summary>
        /// <param name="type">type.</param>
        /// <param name="values">values.</param>
        public QueryVariableProperties(TypeEnum? type = default(TypeEnum?), QueryVariablePropertiesValues values = default(QueryVariablePropertiesValues)) : base()
        {
            this.Type = type;
            this.Values = values;
        }

        /// <summary>
        /// Gets or Sets Values
        /// </summary>
        [DataMember(Name = "values", EmitDefaultValue = false)]
        public QueryVariablePropertiesValues Values { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class QueryVariableProperties {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Values: ").Append(Values).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public override string ToJson()
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
            return this.Equals(input as QueryVariableProperties);
        }

        /// <summary>
        /// Returns true if QueryVariableProperties instances are equal
        /// </summary>
        /// <param name="input">Instance of QueryVariableProperties to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(QueryVariableProperties input)
        {
            if (input == null)
                return false;

            return base.Equals(input) && 
                (
                    this.Type == input.Type ||
                    this.Type.Equals(input.Type)
                ) && base.Equals(input) && 
                (
                    this.Values == input.Values ||
                    (this.Values != null &&
                    this.Values.Equals(input.Values))
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
                int hashCode = base.GetHashCode();
                hashCode = hashCode * 59 + this.Type.GetHashCode();
                if (this.Values != null)
                    hashCode = hashCode * 59 + this.Values.GetHashCode();
                return hashCode;
            }
        }

    }

}
