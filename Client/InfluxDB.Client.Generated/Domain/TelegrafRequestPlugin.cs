/* 
 * Influx API Service
 *
 * No description provided (generated by Openapi Generator https://github.com/openapitools/openapi-generator)
 *
 * OpenAPI spec version: 0.1.0
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
using System.ComponentModel.DataAnnotations;
using OpenAPIDateConverter = InfluxDB.Client.Generated.Client.OpenAPIDateConverter;

namespace InfluxDB.Client.Generated.Domain
{
    /// <summary>
    /// TelegrafRequestPlugin
    /// </summary>
    [DataContract]
    public partial class TelegrafRequestPlugin :  IEquatable<TelegrafRequestPlugin>
    {
        /// <summary>
        /// Defines type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TypeEnum
        {
            
            /// <summary>
            /// Enum Input for value: input
            /// </summary>
            [EnumMember(Value = "input")]
            Input = 1,
            
            /// <summary>
            /// Enum Output for value: output
            /// </summary>
            [EnumMember(Value = "output")]
            Output = 2
        }

        /// <summary>
        /// Gets or Sets type
        /// </summary>
        [DataMember(Name="type", EmitDefaultValue=false)]
        public TypeEnum? type { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="TelegrafRequestPlugin" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected TelegrafRequestPlugin() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TelegrafRequestPlugin" /> class.
        /// </summary>
        /// <param name="type">type.</param>
        public TelegrafRequestPlugin(TypeEnum? type = default(TypeEnum?))
        {
            this.type = type;
        }


        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class TelegrafRequestPlugin {\n");
            sb.Append("  type: ").Append(type).Append("\n");
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
            return this.Equals(input as TelegrafRequestPlugin);
        }

        /// <summary>
        /// Returns true if TelegrafRequestPlugin instances are equal
        /// </summary>
        /// <param name="input">Instance of TelegrafRequestPlugin to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TelegrafRequestPlugin input)
        {
            if (input == null)
                return false;

            return 
                (
                    
                    (this.type != null &&
                    this.type.Equals(input.type))
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
                if (this.type != null)
                    hashCode = hashCode * 59 + this.type.GetHashCode();
                return hashCode;
            }
        }

    }

}
