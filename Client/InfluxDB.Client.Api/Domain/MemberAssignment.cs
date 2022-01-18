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
    /// Object property assignment
    /// </summary>
    [DataContract(Name = "MemberAssignment")]
    public partial class MemberAssignment : Statement, IEquatable<MemberAssignment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MemberAssignment" /> class.
        /// </summary>
        /// <param name="type">Type of AST node.</param>
        /// <param name="member">member.</param>
        /// <param name="init">init.</param>
        public MemberAssignment(string type = default(string), MemberExpression member = default(MemberExpression), Expression init = default(Expression)) : base()
        {
            this.Type = type;
            this.Member = member;
            this.Init = init;
        }

        /// <summary>
        /// Type of AST node
        /// </summary>
        /// <value>Type of AST node</value>
        [DataMember(Name = "type", EmitDefaultValue = false)]
        public string Type { get; set; }

        /// <summary>
        /// Gets or Sets Member
        /// </summary>
        [DataMember(Name = "member", EmitDefaultValue = false)]
        public MemberExpression Member { get; set; }

        /// <summary>
        /// Gets or Sets Init
        /// </summary>
        [DataMember(Name = "init", EmitDefaultValue = false)]
        public Expression Init { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class MemberAssignment {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Member: ").Append(Member).Append("\n");
            sb.Append("  Init: ").Append(Init).Append("\n");
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
            return this.Equals(input as MemberAssignment);
        }

        /// <summary>
        /// Returns true if MemberAssignment instances are equal
        /// </summary>
        /// <param name="input">Instance of MemberAssignment to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(MemberAssignment input)
        {
            if (input == null)
                return false;

            return base.Equals(input) && 
                (
                    this.Type == input.Type ||
                    (this.Type != null &&
                    this.Type.Equals(input.Type))
                ) && base.Equals(input) && 
                (
                    this.Member == input.Member ||
                    (this.Member != null &&
                    this.Member.Equals(input.Member))
                ) && base.Equals(input) && 
                (
                    this.Init == input.Init ||
                    (this.Init != null &&
                    this.Init.Equals(input.Init))
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
                if (this.Type != null)
                    hashCode = hashCode * 59 + this.Type.GetHashCode();
                if (this.Member != null)
                    hashCode = hashCode * 59 + this.Member.GetHashCode();
                if (this.Init != null)
                    hashCode = hashCode * 59 + this.Init.GetHashCode();
                return hashCode;
            }
        }

    }

}
