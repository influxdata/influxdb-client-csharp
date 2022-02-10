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
    /// SimpleTableViewProperties
    /// </summary>
    [DataContract]
    public partial class SimpleTableViewProperties : ViewProperties, IEquatable<SimpleTableViewProperties>
    {
        /// <summary>
        /// Defines Type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TypeEnum
        {
            /// <summary>
            /// Enum SimpleTable for value: simple-table
            /// </summary>
            [EnumMember(Value = "simple-table")] SimpleTable = 1
        }

        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name = "type", EmitDefaultValue = false)]
        public TypeEnum Type { get; set; }

        /// <summary>
        /// Defines Shape
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum ShapeEnum
        {
            /// <summary>
            /// Enum ChronografV2 for value: chronograf-v2
            /// </summary>
            [EnumMember(Value = "chronograf-v2")] ChronografV2 = 1
        }

        /// <summary>
        /// Gets or Sets Shape
        /// </summary>
        [DataMember(Name = "shape", EmitDefaultValue = false)]
        public ShapeEnum Shape { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTableViewProperties" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected SimpleTableViewProperties()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleTableViewProperties" /> class.
        /// </summary>
        /// <param name="type">type (required) (default to TypeEnum.SimpleTable).</param>
        /// <param name="showAll">showAll (required).</param>
        /// <param name="queries">queries (required).</param>
        /// <param name="shape">shape (required) (default to ShapeEnum.ChronografV2).</param>
        /// <param name="note">note (required).</param>
        /// <param name="showNoteWhenEmpty">If true, will display note when empty (required).</param>
        public SimpleTableViewProperties(TypeEnum type = TypeEnum.SimpleTable, bool? showAll = default,
            List<DashboardQuery> queries = default, ShapeEnum shape = ShapeEnum.ChronografV2, string note = default,
            bool? showNoteWhenEmpty = default) : base()
        {
            // to ensure "type" is required (not null)
            Type = type;
            // to ensure "showAll" is required (not null)
            if (showAll == null)
            {
                throw new InvalidDataException(
                    "showAll is a required property for SimpleTableViewProperties and cannot be null");
            }

            ShowAll = showAll;
            // to ensure "queries" is required (not null)
            if (queries == null)
            {
                throw new InvalidDataException(
                    "queries is a required property for SimpleTableViewProperties and cannot be null");
            }

            Queries = queries;
            // to ensure "shape" is required (not null)
            Shape = shape;
            // to ensure "note" is required (not null)
            if (note == null)
            {
                throw new InvalidDataException(
                    "note is a required property for SimpleTableViewProperties and cannot be null");
            }

            Note = note;
            // to ensure "showNoteWhenEmpty" is required (not null)
            if (showNoteWhenEmpty == null)
            {
                throw new InvalidDataException(
                    "showNoteWhenEmpty is a required property for SimpleTableViewProperties and cannot be null");
            }

            ShowNoteWhenEmpty = showNoteWhenEmpty;
        }


        /// <summary>
        /// Gets or Sets ShowAll
        /// </summary>
        [DataMember(Name = "showAll", EmitDefaultValue = false)]
        public bool? ShowAll { get; set; }

        /// <summary>
        /// Gets or Sets Queries
        /// </summary>
        [DataMember(Name = "queries", EmitDefaultValue = false)]
        public List<DashboardQuery> Queries { get; set; }


        /// <summary>
        /// Gets or Sets Note
        /// </summary>
        [DataMember(Name = "note", EmitDefaultValue = false)]
        public string Note { get; set; }

        /// <summary>
        /// If true, will display note when empty
        /// </summary>
        /// <value>If true, will display note when empty</value>
        [DataMember(Name = "showNoteWhenEmpty", EmitDefaultValue = false)]
        public bool? ShowNoteWhenEmpty { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class SimpleTableViewProperties {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  ShowAll: ").Append(ShowAll).Append("\n");
            sb.Append("  Queries: ").Append(Queries).Append("\n");
            sb.Append("  Shape: ").Append(Shape).Append("\n");
            sb.Append("  Note: ").Append(Note).Append("\n");
            sb.Append("  ShowNoteWhenEmpty: ").Append(ShowNoteWhenEmpty).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public override string ToJson()
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
            return Equals(input as SimpleTableViewProperties);
        }

        /// <summary>
        /// Returns true if SimpleTableViewProperties instances are equal
        /// </summary>
        /// <param name="input">Instance of SimpleTableViewProperties to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(SimpleTableViewProperties input)
        {
            if (input == null)
            {
                return false;
            }

            return base.Equals(input) &&
                   (
                       Type == input.Type ||
                       Type.Equals(input.Type)
                   ) && base.Equals(input) &&
                   (
                       ShowAll == input.ShowAll ||
                       ShowAll != null && ShowAll.Equals(input.ShowAll)
                   ) && base.Equals(input) &&
                   (
                       Queries == input.Queries ||
                       Queries != null &&
                       Queries.SequenceEqual(input.Queries)
                   ) && base.Equals(input) &&
                   (
                       Shape == input.Shape ||
                       Shape.Equals(input.Shape)
                   ) && base.Equals(input) &&
                   (
                       Note == input.Note ||
                       Note != null && Note.Equals(input.Note)
                   ) && base.Equals(input) &&
                   (
                       ShowNoteWhenEmpty == input.ShowNoteWhenEmpty ||
                       ShowNoteWhenEmpty != null && ShowNoteWhenEmpty.Equals(input.ShowNoteWhenEmpty)
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
                var hashCode = base.GetHashCode();

                hashCode = hashCode * 59 + Type.GetHashCode();
                if (ShowAll != null)
                {
                    hashCode = hashCode * 59 + ShowAll.GetHashCode();
                }

                if (Queries != null)
                {
                    hashCode = hashCode * 59 + Queries.GetHashCode();
                }

                hashCode = hashCode * 59 + Shape.GetHashCode();
                if (Note != null)
                {
                    hashCode = hashCode * 59 + Note.GetHashCode();
                }

                if (ShowNoteWhenEmpty != null)
                {
                    hashCode = hashCode * 59 + ShowNoteWhenEmpty.GetHashCode();
                }

                return hashCode;
            }
        }
    }
}