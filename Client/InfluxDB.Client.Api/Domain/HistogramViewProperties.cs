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
    /// HistogramViewProperties
    /// </summary>
    [DataContract(Name = "HistogramViewProperties")]
    public partial class HistogramViewProperties : ViewProperties, IEquatable<HistogramViewProperties>
    {
        /// <summary>
        /// Defines Type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TypeEnum
        {
            /// <summary>
            /// Enum Histogram for value: histogram
            /// </summary>
            [EnumMember(Value = "histogram")]
            Histogram = 1

        }


        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name = "type", IsRequired = true, EmitDefaultValue = false)]
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
            [EnumMember(Value = "chronograf-v2")]
            ChronografV2 = 1

        }


        /// <summary>
        /// Gets or Sets Shape
        /// </summary>
        [DataMember(Name = "shape", IsRequired = true, EmitDefaultValue = false)]
        public ShapeEnum Shape { get; set; }
        /// <summary>
        /// Defines Position
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum PositionEnum
        {
            /// <summary>
            /// Enum Overlaid for value: overlaid
            /// </summary>
            [EnumMember(Value = "overlaid")]
            Overlaid = 1,

            /// <summary>
            /// Enum Stacked for value: stacked
            /// </summary>
            [EnumMember(Value = "stacked")]
            Stacked = 2

        }


        /// <summary>
        /// Gets or Sets Position
        /// </summary>
        [DataMember(Name = "position", IsRequired = true, EmitDefaultValue = false)]
        public PositionEnum Position { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="HistogramViewProperties" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected HistogramViewProperties() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="HistogramViewProperties" /> class.
        /// </summary>
        /// <param name="type">type (required).</param>
        /// <param name="queries">queries (required).</param>
        /// <param name="colors">Colors define color encoding of data into a visualization (required).</param>
        /// <param name="shape">shape (required).</param>
        /// <param name="note">note (required).</param>
        /// <param name="showNoteWhenEmpty">If true, will display note when empty (required).</param>
        /// <param name="xColumn">xColumn (required).</param>
        /// <param name="fillColumns">fillColumns (required).</param>
        /// <param name="xDomain">xDomain (required).</param>
        /// <param name="xAxisLabel">xAxisLabel (required).</param>
        /// <param name="position">position (required).</param>
        /// <param name="binCount">binCount (required).</param>
        /// <param name="legendColorizeRows">legendColorizeRows.</param>
        /// <param name="legendHide">legendHide.</param>
        /// <param name="legendOpacity">legendOpacity.</param>
        /// <param name="legendOrientationThreshold">legendOrientationThreshold.</param>
        public HistogramViewProperties(TypeEnum type = default(TypeEnum), List<DashboardQuery> queries = default(List<DashboardQuery>), List<DashboardColor> colors = default(List<DashboardColor>), ShapeEnum shape = default(ShapeEnum), string note = default(string), bool showNoteWhenEmpty = default(bool), string xColumn = default(string), List<string> fillColumns = default(List<string>), List<float> xDomain = default(List<float>), string xAxisLabel = default(string), PositionEnum position = default(PositionEnum), int binCount = default(int), bool legendColorizeRows = default(bool), bool legendHide = default(bool), float legendOpacity = default(float), int legendOrientationThreshold = default(int)) : base()
        {
            this.Type = type;
            // to ensure "queries" is required (not null)
            if (queries == null) {
                throw new ArgumentNullException("queries is a required property for HistogramViewProperties and cannot be null");
            }
            this.Queries = queries;
            // to ensure "colors" is required (not null)
            if (colors == null) {
                throw new ArgumentNullException("colors is a required property for HistogramViewProperties and cannot be null");
            }
            this.Colors = colors;
            this.Shape = shape;
            // to ensure "note" is required (not null)
            if (note == null) {
                throw new ArgumentNullException("note is a required property for HistogramViewProperties and cannot be null");
            }
            this.Note = note;
            this.ShowNoteWhenEmpty = showNoteWhenEmpty;
            // to ensure "xColumn" is required (not null)
            if (xColumn == null) {
                throw new ArgumentNullException("xColumn is a required property for HistogramViewProperties and cannot be null");
            }
            this.XColumn = xColumn;
            // to ensure "fillColumns" is required (not null)
            if (fillColumns == null) {
                throw new ArgumentNullException("fillColumns is a required property for HistogramViewProperties and cannot be null");
            }
            this.FillColumns = fillColumns;
            // to ensure "xDomain" is required (not null)
            if (xDomain == null) {
                throw new ArgumentNullException("xDomain is a required property for HistogramViewProperties and cannot be null");
            }
            this.XDomain = xDomain;
            // to ensure "xAxisLabel" is required (not null)
            if (xAxisLabel == null) {
                throw new ArgumentNullException("xAxisLabel is a required property for HistogramViewProperties and cannot be null");
            }
            this.XAxisLabel = xAxisLabel;
            this.Position = position;
            this.BinCount = binCount;
            this.LegendColorizeRows = legendColorizeRows;
            this.LegendHide = legendHide;
            this.LegendOpacity = legendOpacity;
            this.LegendOrientationThreshold = legendOrientationThreshold;
        }

        /// <summary>
        /// Gets or Sets Queries
        /// </summary>
        [DataMember(Name = "queries", IsRequired = true, EmitDefaultValue = false)]
        public List<DashboardQuery> Queries { get; set; }

        /// <summary>
        /// Colors define color encoding of data into a visualization
        /// </summary>
        /// <value>Colors define color encoding of data into a visualization</value>
        [DataMember(Name = "colors", IsRequired = true, EmitDefaultValue = false)]
        public List<DashboardColor> Colors { get; set; }

        /// <summary>
        /// Gets or Sets Note
        /// </summary>
        [DataMember(Name = "note", IsRequired = true, EmitDefaultValue = false)]
        public string Note { get; set; }

        /// <summary>
        /// If true, will display note when empty
        /// </summary>
        /// <value>If true, will display note when empty</value>
        [DataMember(Name = "showNoteWhenEmpty", IsRequired = true, EmitDefaultValue = true)]
        public bool ShowNoteWhenEmpty { get; set; }

        /// <summary>
        /// Gets or Sets XColumn
        /// </summary>
        [DataMember(Name = "xColumn", IsRequired = true, EmitDefaultValue = false)]
        public string XColumn { get; set; }

        /// <summary>
        /// Gets or Sets FillColumns
        /// </summary>
        [DataMember(Name = "fillColumns", IsRequired = true, EmitDefaultValue = false)]
        public List<string> FillColumns { get; set; }

        /// <summary>
        /// Gets or Sets XDomain
        /// </summary>
        [DataMember(Name = "xDomain", IsRequired = true, EmitDefaultValue = false)]
        public List<float> XDomain { get; set; }

        /// <summary>
        /// Gets or Sets XAxisLabel
        /// </summary>
        [DataMember(Name = "xAxisLabel", IsRequired = true, EmitDefaultValue = false)]
        public string XAxisLabel { get; set; }

        /// <summary>
        /// Gets or Sets BinCount
        /// </summary>
        [DataMember(Name = "binCount", IsRequired = true, EmitDefaultValue = false)]
        public int BinCount { get; set; }

        /// <summary>
        /// Gets or Sets LegendColorizeRows
        /// </summary>
        [DataMember(Name = "legendColorizeRows", EmitDefaultValue = true)]
        public bool LegendColorizeRows { get; set; }

        /// <summary>
        /// Gets or Sets LegendHide
        /// </summary>
        [DataMember(Name = "legendHide", EmitDefaultValue = true)]
        public bool LegendHide { get; set; }

        /// <summary>
        /// Gets or Sets LegendOpacity
        /// </summary>
        [DataMember(Name = "legendOpacity", EmitDefaultValue = false)]
        public float LegendOpacity { get; set; }

        /// <summary>
        /// Gets or Sets LegendOrientationThreshold
        /// </summary>
        [DataMember(Name = "legendOrientationThreshold", EmitDefaultValue = false)]
        public int LegendOrientationThreshold { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class HistogramViewProperties {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Queries: ").Append(Queries).Append("\n");
            sb.Append("  Colors: ").Append(Colors).Append("\n");
            sb.Append("  Shape: ").Append(Shape).Append("\n");
            sb.Append("  Note: ").Append(Note).Append("\n");
            sb.Append("  ShowNoteWhenEmpty: ").Append(ShowNoteWhenEmpty).Append("\n");
            sb.Append("  XColumn: ").Append(XColumn).Append("\n");
            sb.Append("  FillColumns: ").Append(FillColumns).Append("\n");
            sb.Append("  XDomain: ").Append(XDomain).Append("\n");
            sb.Append("  XAxisLabel: ").Append(XAxisLabel).Append("\n");
            sb.Append("  Position: ").Append(Position).Append("\n");
            sb.Append("  BinCount: ").Append(BinCount).Append("\n");
            sb.Append("  LegendColorizeRows: ").Append(LegendColorizeRows).Append("\n");
            sb.Append("  LegendHide: ").Append(LegendHide).Append("\n");
            sb.Append("  LegendOpacity: ").Append(LegendOpacity).Append("\n");
            sb.Append("  LegendOrientationThreshold: ").Append(LegendOrientationThreshold).Append("\n");
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
            return this.Equals(input as HistogramViewProperties);
        }

        /// <summary>
        /// Returns true if HistogramViewProperties instances are equal
        /// </summary>
        /// <param name="input">Instance of HistogramViewProperties to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(HistogramViewProperties input)
        {
            if (input == null)
                return false;

            return base.Equals(input) && 
                (
                    this.Type == input.Type ||
                    this.Type.Equals(input.Type)
                ) && base.Equals(input) && 
                (
                    this.Queries == input.Queries ||
                    this.Queries != null &&
                    input.Queries != null &&
                    this.Queries.SequenceEqual(input.Queries)
                ) && base.Equals(input) && 
                (
                    this.Colors == input.Colors ||
                    this.Colors != null &&
                    input.Colors != null &&
                    this.Colors.SequenceEqual(input.Colors)
                ) && base.Equals(input) && 
                (
                    this.Shape == input.Shape ||
                    this.Shape.Equals(input.Shape)
                ) && base.Equals(input) && 
                (
                    this.Note == input.Note ||
                    (this.Note != null &&
                    this.Note.Equals(input.Note))
                ) && base.Equals(input) && 
                (
                    this.ShowNoteWhenEmpty == input.ShowNoteWhenEmpty ||
                    this.ShowNoteWhenEmpty.Equals(input.ShowNoteWhenEmpty)
                ) && base.Equals(input) && 
                (
                    this.XColumn == input.XColumn ||
                    (this.XColumn != null &&
                    this.XColumn.Equals(input.XColumn))
                ) && base.Equals(input) && 
                (
                    this.FillColumns == input.FillColumns ||
                    this.FillColumns != null &&
                    input.FillColumns != null &&
                    this.FillColumns.SequenceEqual(input.FillColumns)
                ) && base.Equals(input) && 
                (
                    this.XDomain == input.XDomain ||
                    this.XDomain != null &&
                    input.XDomain != null &&
                    this.XDomain.SequenceEqual(input.XDomain)
                ) && base.Equals(input) && 
                (
                    this.XAxisLabel == input.XAxisLabel ||
                    (this.XAxisLabel != null &&
                    this.XAxisLabel.Equals(input.XAxisLabel))
                ) && base.Equals(input) && 
                (
                    this.Position == input.Position ||
                    this.Position.Equals(input.Position)
                ) && base.Equals(input) && 
                (
                    this.BinCount == input.BinCount ||
                    this.BinCount.Equals(input.BinCount)
                ) && base.Equals(input) && 
                (
                    this.LegendColorizeRows == input.LegendColorizeRows ||
                    this.LegendColorizeRows.Equals(input.LegendColorizeRows)
                ) && base.Equals(input) && 
                (
                    this.LegendHide == input.LegendHide ||
                    this.LegendHide.Equals(input.LegendHide)
                ) && base.Equals(input) && 
                (
                    this.LegendOpacity == input.LegendOpacity ||
                    this.LegendOpacity.Equals(input.LegendOpacity)
                ) && base.Equals(input) && 
                (
                    this.LegendOrientationThreshold == input.LegendOrientationThreshold ||
                    this.LegendOrientationThreshold.Equals(input.LegendOrientationThreshold)
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
                if (this.Queries != null)
                    hashCode = hashCode * 59 + this.Queries.GetHashCode();
                if (this.Colors != null)
                    hashCode = hashCode * 59 + this.Colors.GetHashCode();
                hashCode = hashCode * 59 + this.Shape.GetHashCode();
                if (this.Note != null)
                    hashCode = hashCode * 59 + this.Note.GetHashCode();
                hashCode = hashCode * 59 + this.ShowNoteWhenEmpty.GetHashCode();
                if (this.XColumn != null)
                    hashCode = hashCode * 59 + this.XColumn.GetHashCode();
                if (this.FillColumns != null)
                    hashCode = hashCode * 59 + this.FillColumns.GetHashCode();
                if (this.XDomain != null)
                    hashCode = hashCode * 59 + this.XDomain.GetHashCode();
                if (this.XAxisLabel != null)
                    hashCode = hashCode * 59 + this.XAxisLabel.GetHashCode();
                hashCode = hashCode * 59 + this.Position.GetHashCode();
                hashCode = hashCode * 59 + this.BinCount.GetHashCode();
                hashCode = hashCode * 59 + this.LegendColorizeRows.GetHashCode();
                hashCode = hashCode * 59 + this.LegendHide.GetHashCode();
                hashCode = hashCode * 59 + this.LegendOpacity.GetHashCode();
                hashCode = hashCode * 59 + this.LegendOrientationThreshold.GetHashCode();
                return hashCode;
            }
        }

    }

}
