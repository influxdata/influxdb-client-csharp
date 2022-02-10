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
    /// Defines an encoding of data value into color space.
    /// </summary>
    [DataContract]
    public partial class DashboardColor : IEquatable<DashboardColor>
    {
        /// <summary>
        /// Type is how the color is used.
        /// </summary>
        /// <value>Type is how the color is used.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TypeEnum
        {
            /// <summary>
            /// Enum Min for value: min
            /// </summary>
            [EnumMember(Value = "min")] Min = 1,

            /// <summary>
            /// Enum Max for value: max
            /// </summary>
            [EnumMember(Value = "max")] Max = 2,

            /// <summary>
            /// Enum Threshold for value: threshold
            /// </summary>
            [EnumMember(Value = "threshold")] Threshold = 3,

            /// <summary>
            /// Enum Scale for value: scale
            /// </summary>
            [EnumMember(Value = "scale")] Scale = 4,

            /// <summary>
            /// Enum Text for value: text
            /// </summary>
            [EnumMember(Value = "text")] Text = 5,

            /// <summary>
            /// Enum Background for value: background
            /// </summary>
            [EnumMember(Value = "background")] Background = 6
        }

        /// <summary>
        /// Type is how the color is used.
        /// </summary>
        /// <value>Type is how the color is used.</value>
        [DataMember(Name = "type", EmitDefaultValue = false)]
        public TypeEnum Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardColor" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected DashboardColor()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DashboardColor" /> class.
        /// </summary>
        /// <param name="id">The unique ID of the view color. (required).</param>
        /// <param name="type">Type is how the color is used. (required).</param>
        /// <param name="hex">The hex number of the color (required).</param>
        /// <param name="name">The user-facing name of the hex color. (required).</param>
        /// <param name="value">The data value mapped to this color. (required).</param>
        public DashboardColor(string id = default, TypeEnum type = default, string hex = default, string name = default,
            float? value = default)
        {
            // to ensure "id" is required (not null)
            if (id == null)
            {
                throw new InvalidDataException("id is a required property for DashboardColor and cannot be null");
            }

            Id = id;
            // to ensure "type" is required (not null)
            Type = type;
            // to ensure "hex" is required (not null)
            if (hex == null)
            {
                throw new InvalidDataException("hex is a required property for DashboardColor and cannot be null");
            }

            Hex = hex;
            // to ensure "name" is required (not null)
            if (name == null)
            {
                throw new InvalidDataException("name is a required property for DashboardColor and cannot be null");
            }

            Name = name;
            // to ensure "value" is required (not null)
            if (value == null)
            {
                throw new InvalidDataException("value is a required property for DashboardColor and cannot be null");
            }

            Value = value;
        }

        /// <summary>
        /// The unique ID of the view color.
        /// </summary>
        /// <value>The unique ID of the view color.</value>
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; set; }


        /// <summary>
        /// The hex number of the color
        /// </summary>
        /// <value>The hex number of the color</value>
        [DataMember(Name = "hex", EmitDefaultValue = false)]
        public string Hex { get; set; }

        /// <summary>
        /// The user-facing name of the hex color.
        /// </summary>
        /// <value>The user-facing name of the hex color.</value>
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// The data value mapped to this color.
        /// </summary>
        /// <value>The data value mapped to this color.</value>
        [DataMember(Name = "value", EmitDefaultValue = false)]
        public float? Value { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class DashboardColor {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Hex: ").Append(Hex).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Value: ").Append(Value).Append("\n");
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
            return Equals(input as DashboardColor);
        }

        /// <summary>
        /// Returns true if DashboardColor instances are equal
        /// </summary>
        /// <param name="input">Instance of DashboardColor to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(DashboardColor input)
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
                    Type == input.Type ||
                    Type.Equals(input.Type)
                ) &&
                (
                    Hex == input.Hex ||
                    Hex != null && Hex.Equals(input.Hex)
                ) &&
                (
                    Name == input.Name ||
                    Name != null && Name.Equals(input.Name)
                ) &&
                (
                    Value == input.Value ||
                    Value != null && Value.Equals(input.Value)
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

                hashCode = hashCode * 59 + Type.GetHashCode();
                if (Hex != null)
                {
                    hashCode = hashCode * 59 + Hex.GetHashCode();
                }

                if (Name != null)
                {
                    hashCode = hashCode * 59 + Name.GetHashCode();
                }

                if (Value != null)
                {
                    hashCode = hashCode * 59 + Value.GetHashCode();
                }

                return hashCode;
            }
        }
    }
}