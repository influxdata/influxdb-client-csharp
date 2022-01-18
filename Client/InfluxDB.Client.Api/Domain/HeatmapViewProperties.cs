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
    /// HeatmapViewProperties
    /// </summary>
    [DataContract(Name = "HeatmapViewProperties")]
    public partial class HeatmapViewProperties : ViewProperties, IEquatable<HeatmapViewProperties>
    {
        /// <summary>
        /// Defines Type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TypeEnum
        {
            /// <summary>
            /// Enum Heatmap for value: heatmap
            /// </summary>
            [EnumMember(Value = "heatmap")]
            Heatmap = 1

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
        /// Initializes a new instance of the <see cref="HeatmapViewProperties" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected HeatmapViewProperties() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="HeatmapViewProperties" /> class.
        /// </summary>
        /// <param name="timeFormat">timeFormat.</param>
        /// <param name="type">type (required).</param>
        /// <param name="queries">queries (required).</param>
        /// <param name="colors">Colors define color encoding of data into a visualization (required).</param>
        /// <param name="shape">shape (required).</param>
        /// <param name="note">note (required).</param>
        /// <param name="showNoteWhenEmpty">If true, will display note when empty (required).</param>
        /// <param name="xColumn">xColumn (required).</param>
        /// <param name="generateXAxisTicks">generateXAxisTicks.</param>
        /// <param name="xTotalTicks">xTotalTicks.</param>
        /// <param name="xTickStart">xTickStart.</param>
        /// <param name="xTickStep">xTickStep.</param>
        /// <param name="yColumn">yColumn (required).</param>
        /// <param name="generateYAxisTicks">generateYAxisTicks.</param>
        /// <param name="yTotalTicks">yTotalTicks.</param>
        /// <param name="yTickStart">yTickStart.</param>
        /// <param name="yTickStep">yTickStep.</param>
        /// <param name="xDomain">xDomain (required).</param>
        /// <param name="yDomain">yDomain (required).</param>
        /// <param name="xAxisLabel">xAxisLabel (required).</param>
        /// <param name="yAxisLabel">yAxisLabel (required).</param>
        /// <param name="xPrefix">xPrefix (required).</param>
        /// <param name="xSuffix">xSuffix (required).</param>
        /// <param name="yPrefix">yPrefix (required).</param>
        /// <param name="ySuffix">ySuffix (required).</param>
        /// <param name="binSize">binSize (required).</param>
        /// <param name="legendColorizeRows">legendColorizeRows.</param>
        /// <param name="legendHide">legendHide.</param>
        /// <param name="legendOpacity">legendOpacity.</param>
        /// <param name="legendOrientationThreshold">legendOrientationThreshold.</param>
        public HeatmapViewProperties(string timeFormat = default(string), TypeEnum type = default(TypeEnum), List<DashboardQuery> queries = default(List<DashboardQuery>), List<string> colors = default(List<string>), ShapeEnum shape = default(ShapeEnum), string note = default(string), bool showNoteWhenEmpty = default(bool), string xColumn = default(string), List<string> generateXAxisTicks = default(List<string>), int xTotalTicks = default(int), float xTickStart = default(float), float xTickStep = default(float), string yColumn = default(string), List<string> generateYAxisTicks = default(List<string>), int yTotalTicks = default(int), float yTickStart = default(float), float yTickStep = default(float), List<decimal> xDomain = default(List<decimal>), List<decimal> yDomain = default(List<decimal>), string xAxisLabel = default(string), string yAxisLabel = default(string), string xPrefix = default(string), string xSuffix = default(string), string yPrefix = default(string), string ySuffix = default(string), decimal binSize = default(decimal), bool legendColorizeRows = default(bool), bool legendHide = default(bool), float legendOpacity = default(float), int legendOrientationThreshold = default(int)) : base()
        {
            this.Type = type;
            // to ensure "queries" is required (not null)
            if (queries == null) {
                throw new ArgumentNullException("queries is a required property for HeatmapViewProperties and cannot be null");
            }
            this.Queries = queries;
            // to ensure "colors" is required (not null)
            if (colors == null) {
                throw new ArgumentNullException("colors is a required property for HeatmapViewProperties and cannot be null");
            }
            this.Colors = colors;
            this.Shape = shape;
            // to ensure "note" is required (not null)
            if (note == null) {
                throw new ArgumentNullException("note is a required property for HeatmapViewProperties and cannot be null");
            }
            this.Note = note;
            this.ShowNoteWhenEmpty = showNoteWhenEmpty;
            // to ensure "xColumn" is required (not null)
            if (xColumn == null) {
                throw new ArgumentNullException("xColumn is a required property for HeatmapViewProperties and cannot be null");
            }
            this.XColumn = xColumn;
            // to ensure "yColumn" is required (not null)
            if (yColumn == null) {
                throw new ArgumentNullException("yColumn is a required property for HeatmapViewProperties and cannot be null");
            }
            this.YColumn = yColumn;
            // to ensure "xDomain" is required (not null)
            if (xDomain == null) {
                throw new ArgumentNullException("xDomain is a required property for HeatmapViewProperties and cannot be null");
            }
            this.XDomain = xDomain;
            // to ensure "yDomain" is required (not null)
            if (yDomain == null) {
                throw new ArgumentNullException("yDomain is a required property for HeatmapViewProperties and cannot be null");
            }
            this.YDomain = yDomain;
            // to ensure "xAxisLabel" is required (not null)
            if (xAxisLabel == null) {
                throw new ArgumentNullException("xAxisLabel is a required property for HeatmapViewProperties and cannot be null");
            }
            this.XAxisLabel = xAxisLabel;
            // to ensure "yAxisLabel" is required (not null)
            if (yAxisLabel == null) {
                throw new ArgumentNullException("yAxisLabel is a required property for HeatmapViewProperties and cannot be null");
            }
            this.YAxisLabel = yAxisLabel;
            // to ensure "xPrefix" is required (not null)
            if (xPrefix == null) {
                throw new ArgumentNullException("xPrefix is a required property for HeatmapViewProperties and cannot be null");
            }
            this.XPrefix = xPrefix;
            // to ensure "xSuffix" is required (not null)
            if (xSuffix == null) {
                throw new ArgumentNullException("xSuffix is a required property for HeatmapViewProperties and cannot be null");
            }
            this.XSuffix = xSuffix;
            // to ensure "yPrefix" is required (not null)
            if (yPrefix == null) {
                throw new ArgumentNullException("yPrefix is a required property for HeatmapViewProperties and cannot be null");
            }
            this.YPrefix = yPrefix;
            // to ensure "ySuffix" is required (not null)
            if (ySuffix == null) {
                throw new ArgumentNullException("ySuffix is a required property for HeatmapViewProperties and cannot be null");
            }
            this.YSuffix = ySuffix;
            this.BinSize = binSize;
            this.TimeFormat = timeFormat;
            this.GenerateXAxisTicks = generateXAxisTicks;
            this.XTotalTicks = xTotalTicks;
            this.XTickStart = xTickStart;
            this.XTickStep = xTickStep;
            this.GenerateYAxisTicks = generateYAxisTicks;
            this.YTotalTicks = yTotalTicks;
            this.YTickStart = yTickStart;
            this.YTickStep = yTickStep;
            this.LegendColorizeRows = legendColorizeRows;
            this.LegendHide = legendHide;
            this.LegendOpacity = legendOpacity;
            this.LegendOrientationThreshold = legendOrientationThreshold;
        }

        /// <summary>
        /// Gets or Sets TimeFormat
        /// </summary>
        [DataMember(Name = "timeFormat", EmitDefaultValue = false)]
        public string TimeFormat { get; set; }

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
        public List<string> Colors { get; set; }

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
        /// Gets or Sets GenerateXAxisTicks
        /// </summary>
        [DataMember(Name = "generateXAxisTicks", EmitDefaultValue = false)]
        public List<string> GenerateXAxisTicks { get; set; }

        /// <summary>
        /// Gets or Sets XTotalTicks
        /// </summary>
        [DataMember(Name = "xTotalTicks", EmitDefaultValue = false)]
        public int XTotalTicks { get; set; }

        /// <summary>
        /// Gets or Sets XTickStart
        /// </summary>
        [DataMember(Name = "xTickStart", EmitDefaultValue = false)]
        public float XTickStart { get; set; }

        /// <summary>
        /// Gets or Sets XTickStep
        /// </summary>
        [DataMember(Name = "xTickStep", EmitDefaultValue = false)]
        public float XTickStep { get; set; }

        /// <summary>
        /// Gets or Sets YColumn
        /// </summary>
        [DataMember(Name = "yColumn", IsRequired = true, EmitDefaultValue = false)]
        public string YColumn { get; set; }

        /// <summary>
        /// Gets or Sets GenerateYAxisTicks
        /// </summary>
        [DataMember(Name = "generateYAxisTicks", EmitDefaultValue = false)]
        public List<string> GenerateYAxisTicks { get; set; }

        /// <summary>
        /// Gets or Sets YTotalTicks
        /// </summary>
        [DataMember(Name = "yTotalTicks", EmitDefaultValue = false)]
        public int YTotalTicks { get; set; }

        /// <summary>
        /// Gets or Sets YTickStart
        /// </summary>
        [DataMember(Name = "yTickStart", EmitDefaultValue = false)]
        public float YTickStart { get; set; }

        /// <summary>
        /// Gets or Sets YTickStep
        /// </summary>
        [DataMember(Name = "yTickStep", EmitDefaultValue = false)]
        public float YTickStep { get; set; }

        /// <summary>
        /// Gets or Sets XDomain
        /// </summary>
        [DataMember(Name = "xDomain", IsRequired = true, EmitDefaultValue = false)]
        public List<decimal> XDomain { get; set; }

        /// <summary>
        /// Gets or Sets YDomain
        /// </summary>
        [DataMember(Name = "yDomain", IsRequired = true, EmitDefaultValue = false)]
        public List<decimal> YDomain { get; set; }

        /// <summary>
        /// Gets or Sets XAxisLabel
        /// </summary>
        [DataMember(Name = "xAxisLabel", IsRequired = true, EmitDefaultValue = false)]
        public string XAxisLabel { get; set; }

        /// <summary>
        /// Gets or Sets YAxisLabel
        /// </summary>
        [DataMember(Name = "yAxisLabel", IsRequired = true, EmitDefaultValue = false)]
        public string YAxisLabel { get; set; }

        /// <summary>
        /// Gets or Sets XPrefix
        /// </summary>
        [DataMember(Name = "xPrefix", IsRequired = true, EmitDefaultValue = false)]
        public string XPrefix { get; set; }

        /// <summary>
        /// Gets or Sets XSuffix
        /// </summary>
        [DataMember(Name = "xSuffix", IsRequired = true, EmitDefaultValue = false)]
        public string XSuffix { get; set; }

        /// <summary>
        /// Gets or Sets YPrefix
        /// </summary>
        [DataMember(Name = "yPrefix", IsRequired = true, EmitDefaultValue = false)]
        public string YPrefix { get; set; }

        /// <summary>
        /// Gets or Sets YSuffix
        /// </summary>
        [DataMember(Name = "ySuffix", IsRequired = true, EmitDefaultValue = false)]
        public string YSuffix { get; set; }

        /// <summary>
        /// Gets or Sets BinSize
        /// </summary>
        [DataMember(Name = "binSize", IsRequired = true, EmitDefaultValue = false)]
        public decimal BinSize { get; set; }

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
            sb.Append("class HeatmapViewProperties {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  TimeFormat: ").Append(TimeFormat).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Queries: ").Append(Queries).Append("\n");
            sb.Append("  Colors: ").Append(Colors).Append("\n");
            sb.Append("  Shape: ").Append(Shape).Append("\n");
            sb.Append("  Note: ").Append(Note).Append("\n");
            sb.Append("  ShowNoteWhenEmpty: ").Append(ShowNoteWhenEmpty).Append("\n");
            sb.Append("  XColumn: ").Append(XColumn).Append("\n");
            sb.Append("  GenerateXAxisTicks: ").Append(GenerateXAxisTicks).Append("\n");
            sb.Append("  XTotalTicks: ").Append(XTotalTicks).Append("\n");
            sb.Append("  XTickStart: ").Append(XTickStart).Append("\n");
            sb.Append("  XTickStep: ").Append(XTickStep).Append("\n");
            sb.Append("  YColumn: ").Append(YColumn).Append("\n");
            sb.Append("  GenerateYAxisTicks: ").Append(GenerateYAxisTicks).Append("\n");
            sb.Append("  YTotalTicks: ").Append(YTotalTicks).Append("\n");
            sb.Append("  YTickStart: ").Append(YTickStart).Append("\n");
            sb.Append("  YTickStep: ").Append(YTickStep).Append("\n");
            sb.Append("  XDomain: ").Append(XDomain).Append("\n");
            sb.Append("  YDomain: ").Append(YDomain).Append("\n");
            sb.Append("  XAxisLabel: ").Append(XAxisLabel).Append("\n");
            sb.Append("  YAxisLabel: ").Append(YAxisLabel).Append("\n");
            sb.Append("  XPrefix: ").Append(XPrefix).Append("\n");
            sb.Append("  XSuffix: ").Append(XSuffix).Append("\n");
            sb.Append("  YPrefix: ").Append(YPrefix).Append("\n");
            sb.Append("  YSuffix: ").Append(YSuffix).Append("\n");
            sb.Append("  BinSize: ").Append(BinSize).Append("\n");
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
            return this.Equals(input as HeatmapViewProperties);
        }

        /// <summary>
        /// Returns true if HeatmapViewProperties instances are equal
        /// </summary>
        /// <param name="input">Instance of HeatmapViewProperties to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(HeatmapViewProperties input)
        {
            if (input == null)
                return false;

            return base.Equals(input) && 
                (
                    this.TimeFormat == input.TimeFormat ||
                    (this.TimeFormat != null &&
                    this.TimeFormat.Equals(input.TimeFormat))
                ) && base.Equals(input) && 
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
                    this.GenerateXAxisTicks == input.GenerateXAxisTicks ||
                    this.GenerateXAxisTicks != null &&
                    input.GenerateXAxisTicks != null &&
                    this.GenerateXAxisTicks.SequenceEqual(input.GenerateXAxisTicks)
                ) && base.Equals(input) && 
                (
                    this.XTotalTicks == input.XTotalTicks ||
                    this.XTotalTicks.Equals(input.XTotalTicks)
                ) && base.Equals(input) && 
                (
                    this.XTickStart == input.XTickStart ||
                    this.XTickStart.Equals(input.XTickStart)
                ) && base.Equals(input) && 
                (
                    this.XTickStep == input.XTickStep ||
                    this.XTickStep.Equals(input.XTickStep)
                ) && base.Equals(input) && 
                (
                    this.YColumn == input.YColumn ||
                    (this.YColumn != null &&
                    this.YColumn.Equals(input.YColumn))
                ) && base.Equals(input) && 
                (
                    this.GenerateYAxisTicks == input.GenerateYAxisTicks ||
                    this.GenerateYAxisTicks != null &&
                    input.GenerateYAxisTicks != null &&
                    this.GenerateYAxisTicks.SequenceEqual(input.GenerateYAxisTicks)
                ) && base.Equals(input) && 
                (
                    this.YTotalTicks == input.YTotalTicks ||
                    this.YTotalTicks.Equals(input.YTotalTicks)
                ) && base.Equals(input) && 
                (
                    this.YTickStart == input.YTickStart ||
                    this.YTickStart.Equals(input.YTickStart)
                ) && base.Equals(input) && 
                (
                    this.YTickStep == input.YTickStep ||
                    this.YTickStep.Equals(input.YTickStep)
                ) && base.Equals(input) && 
                (
                    this.XDomain == input.XDomain ||
                    this.XDomain != null &&
                    input.XDomain != null &&
                    this.XDomain.SequenceEqual(input.XDomain)
                ) && base.Equals(input) && 
                (
                    this.YDomain == input.YDomain ||
                    this.YDomain != null &&
                    input.YDomain != null &&
                    this.YDomain.SequenceEqual(input.YDomain)
                ) && base.Equals(input) && 
                (
                    this.XAxisLabel == input.XAxisLabel ||
                    (this.XAxisLabel != null &&
                    this.XAxisLabel.Equals(input.XAxisLabel))
                ) && base.Equals(input) && 
                (
                    this.YAxisLabel == input.YAxisLabel ||
                    (this.YAxisLabel != null &&
                    this.YAxisLabel.Equals(input.YAxisLabel))
                ) && base.Equals(input) && 
                (
                    this.XPrefix == input.XPrefix ||
                    (this.XPrefix != null &&
                    this.XPrefix.Equals(input.XPrefix))
                ) && base.Equals(input) && 
                (
                    this.XSuffix == input.XSuffix ||
                    (this.XSuffix != null &&
                    this.XSuffix.Equals(input.XSuffix))
                ) && base.Equals(input) && 
                (
                    this.YPrefix == input.YPrefix ||
                    (this.YPrefix != null &&
                    this.YPrefix.Equals(input.YPrefix))
                ) && base.Equals(input) && 
                (
                    this.YSuffix == input.YSuffix ||
                    (this.YSuffix != null &&
                    this.YSuffix.Equals(input.YSuffix))
                ) && base.Equals(input) && 
                (
                    this.BinSize == input.BinSize ||
                    this.BinSize.Equals(input.BinSize)
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
                if (this.TimeFormat != null)
                    hashCode = hashCode * 59 + this.TimeFormat.GetHashCode();
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
                if (this.GenerateXAxisTicks != null)
                    hashCode = hashCode * 59 + this.GenerateXAxisTicks.GetHashCode();
                hashCode = hashCode * 59 + this.XTotalTicks.GetHashCode();
                hashCode = hashCode * 59 + this.XTickStart.GetHashCode();
                hashCode = hashCode * 59 + this.XTickStep.GetHashCode();
                if (this.YColumn != null)
                    hashCode = hashCode * 59 + this.YColumn.GetHashCode();
                if (this.GenerateYAxisTicks != null)
                    hashCode = hashCode * 59 + this.GenerateYAxisTicks.GetHashCode();
                hashCode = hashCode * 59 + this.YTotalTicks.GetHashCode();
                hashCode = hashCode * 59 + this.YTickStart.GetHashCode();
                hashCode = hashCode * 59 + this.YTickStep.GetHashCode();
                if (this.XDomain != null)
                    hashCode = hashCode * 59 + this.XDomain.GetHashCode();
                if (this.YDomain != null)
                    hashCode = hashCode * 59 + this.YDomain.GetHashCode();
                if (this.XAxisLabel != null)
                    hashCode = hashCode * 59 + this.XAxisLabel.GetHashCode();
                if (this.YAxisLabel != null)
                    hashCode = hashCode * 59 + this.YAxisLabel.GetHashCode();
                if (this.XPrefix != null)
                    hashCode = hashCode * 59 + this.XPrefix.GetHashCode();
                if (this.XSuffix != null)
                    hashCode = hashCode * 59 + this.XSuffix.GetHashCode();
                if (this.YPrefix != null)
                    hashCode = hashCode * 59 + this.YPrefix.GetHashCode();
                if (this.YSuffix != null)
                    hashCode = hashCode * 59 + this.YSuffix.GetHashCode();
                hashCode = hashCode * 59 + this.BinSize.GetHashCode();
                hashCode = hashCode * 59 + this.LegendColorizeRows.GetHashCode();
                hashCode = hashCode * 59 + this.LegendHide.GetHashCode();
                hashCode = hashCode * 59 + this.LegendOpacity.GetHashCode();
                hashCode = hashCode * 59 + this.LegendOrientationThreshold.GetHashCode();
                return hashCode;
            }
        }

    }

}
