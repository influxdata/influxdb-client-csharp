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
    /// Contains the AST for the supplied Flux query
    /// </summary>
    [DataContract]
    public partial class ASTResponse :  IEquatable<ASTResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ASTResponse" /> class.
        /// </summary>
        /// <param name="ast">ast.</param>
        public ASTResponse(Package ast = default(Package))
        {
            this.Ast = ast;
        }

        /// <summary>
        /// Gets or Sets Ast
        /// </summary>
        [DataMember(Name="ast", EmitDefaultValue=false)]
        public Package Ast { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class ASTResponse {\n");
            sb.Append("  Ast: ").Append(Ast).Append("\n");
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
            return this.Equals(input as ASTResponse);
        }

        /// <summary>
        /// Returns true if ASTResponse instances are equal
        /// </summary>
        /// <param name="input">Instance of ASTResponse to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(ASTResponse input)
        {
            if (input == null)
                return false;

            return 
                (
                    
                    (this.Ast != null &&
                    this.Ast.Equals(input.Ast))
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
                if (this.Ast != null)
                    hashCode = hashCode * 59 + this.Ast.GetHashCode();
                return hashCode;
            }
        }

    }

}
