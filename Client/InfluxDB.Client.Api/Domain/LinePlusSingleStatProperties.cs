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
    /// LinePlusSingleStatProperties
    /// </summary>
    [DataContract]
    public partial class LinePlusSingleStatProperties : ViewProperties,  IEquatable<LinePlusSingleStatProperties>
    {
        /// <summary>
        /// Defines Type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TypeEnum
        {
            /// <summary>
            /// Enum LinePlusSingleStat for value: line-plus-single-stat
            /// </summary>
            [EnumMember(Value = "line-plus-single-stat")]
            LinePlusSingleStat = 1

        }

        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name="type", EmitDefaultValue=false)]
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
        [DataMember(Name="shape", EmitDefaultValue=false)]
        public ShapeEnum Shape { get; set; }
        /// <summary>
        /// Defines HoverDimension
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum HoverDimensionEnum
        {
            /// <summary>
            /// Enum Auto for value: auto
            /// </summary>
            [EnumMember(Value = "auto")]
            Auto = 1,

            /// <summary>
            /// Enum X for value: x
            /// </summary>
            [EnumMember(Value = "x")]
            X = 2,

            /// <summary>
            /// Enum Y for value: y
            /// </summary>
            [EnumMember(Value = "y")]
            Y = 3,

            /// <summary>
            /// Enum Xy for value: xy
            /// </summary>
            [EnumMember(Value = "xy")]
            Xy = 4

        }

        /// <summary>
        /// Gets or Sets HoverDimension
        /// </summary>
        [DataMember(Name="hoverDimension", EmitDefaultValue=false)]
        public HoverDimensionEnum? HoverDimension { get; set; }
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
        [DataMember(Name="position", EmitDefaultValue=false)]
        public PositionEnum Position { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="LinePlusSingleStatProperties" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected LinePlusSingleStatProperties() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="LinePlusSingleStatProperties" /> class.
        /// </summary>
        /// <param name="timeFormat">timeFormat.</param>
        /// <param name="type">type (required) (default to TypeEnum.LinePlusSingleStat).</param>
        /// <param name="queries">queries (required).</param>
        /// <param name="colors">Colors define color encoding of data into a visualization (required).</param>
        /// <param name="shape">shape (required) (default to ShapeEnum.ChronografV2).</param>
        /// <param name="note">note (required).</param>
        /// <param name="showNoteWhenEmpty">If true, will display note when empty (required).</param>
        /// <param name="axes">axes (required).</param>
        /// <param name="staticLegend">staticLegend.</param>
        /// <param name="xColumn">xColumn.</param>
        /// <param name="generateXAxisTicks">generateXAxisTicks.</param>
        /// <param name="xTotalTicks">xTotalTicks.</param>
        /// <param name="xTickStart">xTickStart.</param>
        /// <param name="xTickStep">xTickStep.</param>
        /// <param name="yColumn">yColumn.</param>
        /// <param name="generateYAxisTicks">generateYAxisTicks.</param>
        /// <param name="yTotalTicks">yTotalTicks.</param>
        /// <param name="yTickStart">yTickStart.</param>
        /// <param name="yTickStep">yTickStep.</param>
        /// <param name="shadeBelow">shadeBelow.</param>
        /// <param name="hoverDimension">hoverDimension.</param>
        /// <param name="position">position (required).</param>
        /// <param name="prefix">prefix (required).</param>
        /// <param name="suffix">suffix (required).</param>
        /// <param name="decimalPlaces">decimalPlaces (required).</param>
        /// <param name="legendColorizeRows">legendColorizeRows.</param>
        /// <param name="legendHide">legendHide.</param>
        /// <param name="legendOpacity">legendOpacity.</param>
        /// <param name="legendOrientationThreshold">legendOrientationThreshold.</param>
        public LinePlusSingleStatProperties(string timeFormat = default(string), TypeEnum type = TypeEnum.LinePlusSingleStat, List<DashboardQuery> queries = default(List<DashboardQuery>), List<DashboardColor> colors = default(List<DashboardColor>), ShapeEnum shape = ShapeEnum.ChronografV2, string note = default(string), bool? showNoteWhenEmpty = default(bool?), Axes axes = default(Axes), StaticLegend staticLegend = default(StaticLegend), string xColumn = default(string), List<string> generateXAxisTicks = default(List<string>), int? xTotalTicks = default(int?), float? xTickStart = default(float?), float? xTickStep = default(float?), string yColumn = default(string), List<string> generateYAxisTicks = default(List<string>), int? yTotalTicks = default(int?), float? yTickStart = default(float?), float? yTickStep = default(float?), bool? shadeBelow = default(bool?), HoverDimensionEnum? hoverDimension = default(HoverDimensionEnum?), PositionEnum position = default(PositionEnum), string prefix = default(string), string suffix = default(string), DecimalPlaces decimalPlaces = default(DecimalPlaces), bool? legendColorizeRows = default(bool?), bool? legendHide = default(bool?), float? legendOpacity = default(float?), int? legendOrientationThreshold = default(int?)) : base()
        {
            // to ensure "type" is required (not null)
            if (type == null)
            {
                throw new InvalidDataException("type is a required property for LinePlusSingleStatProperties and cannot be null");
            }
            else
            {
                this.Type = type;
            }
            // to ensure "queries" is required (not null)
            if (queries == null)
            {
                throw new InvalidDataException("queries is a required property for LinePlusSingleStatProperties and cannot be null");
            }
            else
            {
                this.Queries = queries;
            }
            // to ensure "colors" is required (not null)
            if (colors == null)
            {
                throw new InvalidDataException("colors is a required property for LinePlusSingleStatProperties and cannot be null");
            }
            else
            {
                this.Colors = colors;
            }
            // to ensure "shape" is required (not null)
            if (shape == null)
            {
                throw new InvalidDataException("shape is a required property for LinePlusSingleStatProperties and cannot be null");
            }
            else
            {
                this.Shape = shape;
            }
            // to ensure "note" is required (not null)
            if (note == null)
            {
                throw new InvalidDataException("note is a required property for LinePlusSingleStatProperties and cannot be null");
            }
            else
            {
                this.Note = note;
            }
            // to ensure "showNoteWhenEmpty" is required (not null)
            if (showNoteWhenEmpty == null)
            {
                throw new InvalidDataException("showNoteWhenEmpty is a required property for LinePlusSingleStatProperties and cannot be null");
            }
            else
            {
                this.ShowNoteWhenEmpty = showNoteWhenEmpty;
            }
            // to ensure "axes" is required (not null)
            if (axes == null)
            {
                throw new InvalidDataException("axes is a required property for LinePlusSingleStatProperties and cannot be null");
            }
            else
            {
                this.Axes = axes;
            }
            // to ensure "position" is required (not null)
            if (position == null)
            {
                throw new InvalidDataException("position is a required property for LinePlusSingleStatProperties and cannot be null");
            }
            else
            {
                this.Position = position;
            }
            // to ensure "prefix" is required (not null)
            if (prefix == null)
            {
                throw new InvalidDataException("prefix is a required property for LinePlusSingleStatProperties and cannot be null");
            }
            else
            {
                this.Prefix = prefix;
            }
            // to ensure "suffix" is required (not null)
            if (suffix == null)
            {
                throw new InvalidDataException("suffix is a required property for LinePlusSingleStatProperties and cannot be null");
            }
            else
            {
                this.Suffix = suffix;
            }
            // to ensure "decimalPlaces" is required (not null)
            if (decimalPlaces == null)
            {
                throw new InvalidDataException("decimalPlaces is a required property for LinePlusSingleStatProperties and cannot be null");
            }
            else
            {
                this.DecimalPlaces = decimalPlaces;
            }
            this.TimeFormat = timeFormat;
            this.StaticLegend = staticLegend;
            this.XColumn = xColumn;
            this.GenerateXAxisTicks = generateXAxisTicks;
            this.XTotalTicks = xTotalTicks;
            this.XTickStart = xTickStart;
            this.XTickStep = xTickStep;
            this.YColumn = yColumn;
            this.GenerateYAxisTicks = generateYAxisTicks;
            this.YTotalTicks = yTotalTicks;
            this.YTickStart = yTickStart;
            this.YTickStep = yTickStep;
            this.ShadeBelow = shadeBelow;
            this.HoverDimension = hoverDimension;
            this.LegendColorizeRows = legendColorizeRows;
            this.LegendHide = legendHide;
            this.LegendOpacity = legendOpacity;
            this.LegendOrientationThreshold = legendOrientationThreshold;
        }

        /// <summary>
        /// Gets or Sets TimeFormat
        /// </summary>
        [DataMember(Name="timeFormat", EmitDefaultValue=false)]
        public string TimeFormat { get; set; }


        /// <summary>
        /// Gets or Sets Queries
        /// </summary>
        [DataMember(Name="queries", EmitDefaultValue=false)]
        public List<DashboardQuery> Queries { get; set; }

        /// <summary>
        /// Colors define color encoding of data into a visualization
        /// </summary>
        /// <value>Colors define color encoding of data into a visualization</value>
        [DataMember(Name="colors", EmitDefaultValue=false)]
        public List<DashboardColor> Colors { get; set; }


        /// <summary>
        /// Gets or Sets Note
        /// </summary>
        [DataMember(Name="note", EmitDefaultValue=false)]
        public string Note { get; set; }

        /// <summary>
        /// If true, will display note when empty
        /// </summary>
        /// <value>If true, will display note when empty</value>
        [DataMember(Name="showNoteWhenEmpty", EmitDefaultValue=false)]
        public bool? ShowNoteWhenEmpty { get; set; }

        /// <summary>
        /// Gets or Sets Axes
        /// </summary>
        [DataMember(Name="axes", EmitDefaultValue=false)]
        public Axes Axes { get; set; }

        /// <summary>
        /// Gets or Sets StaticLegend
        /// </summary>
        [DataMember(Name="staticLegend", EmitDefaultValue=false)]
        public StaticLegend StaticLegend { get; set; }

        /// <summary>
        /// Gets or Sets XColumn
        /// </summary>
        [DataMember(Name="xColumn", EmitDefaultValue=false)]
        public string XColumn { get; set; }

        /// <summary>
        /// Gets or Sets GenerateXAxisTicks
        /// </summary>
        [DataMember(Name="generateXAxisTicks", EmitDefaultValue=false)]
        public List<string> GenerateXAxisTicks { get; set; }

        /// <summary>
        /// Gets or Sets XTotalTicks
        /// </summary>
        [DataMember(Name="xTotalTicks", EmitDefaultValue=false)]
        public int? XTotalTicks { get; set; }

        /// <summary>
        /// Gets or Sets XTickStart
        /// </summary>
        [DataMember(Name="xTickStart", EmitDefaultValue=false)]
        public float? XTickStart { get; set; }

        /// <summary>
        /// Gets or Sets XTickStep
        /// </summary>
        [DataMember(Name="xTickStep", EmitDefaultValue=false)]
        public float? XTickStep { get; set; }

        /// <summary>
        /// Gets or Sets YColumn
        /// </summary>
        [DataMember(Name="yColumn", EmitDefaultValue=false)]
        public string YColumn { get; set; }

        /// <summary>
        /// Gets or Sets GenerateYAxisTicks
        /// </summary>
        [DataMember(Name="generateYAxisTicks", EmitDefaultValue=false)]
        public List<string> GenerateYAxisTicks { get; set; }

        /// <summary>
        /// Gets or Sets YTotalTicks
        /// </summary>
        [DataMember(Name="yTotalTicks", EmitDefaultValue=false)]
        public int? YTotalTicks { get; set; }

        /// <summary>
        /// Gets or Sets YTickStart
        /// </summary>
        [DataMember(Name="yTickStart", EmitDefaultValue=false)]
        public float? YTickStart { get; set; }

        /// <summary>
        /// Gets or Sets YTickStep
        /// </summary>
        [DataMember(Name="yTickStep", EmitDefaultValue=false)]
        public float? YTickStep { get; set; }

        /// <summary>
        /// Gets or Sets ShadeBelow
        /// </summary>
        [DataMember(Name="shadeBelow", EmitDefaultValue=false)]
        public bool? ShadeBelow { get; set; }



        /// <summary>
        /// Gets or Sets Prefix
        /// </summary>
        [DataMember(Name="prefix", EmitDefaultValue=false)]
        public string Prefix { get; set; }

        /// <summary>
        /// Gets or Sets Suffix
        /// </summary>
        [DataMember(Name="suffix", EmitDefaultValue=false)]
        public string Suffix { get; set; }

        /// <summary>
        /// Gets or Sets DecimalPlaces
        /// </summary>
        [DataMember(Name="decimalPlaces", EmitDefaultValue=false)]
        public DecimalPlaces DecimalPlaces { get; set; }

        /// <summary>
        /// Gets or Sets LegendColorizeRows
        /// </summary>
        [DataMember(Name="legendColorizeRows", EmitDefaultValue=false)]
        public bool? LegendColorizeRows { get; set; }

        /// <summary>
        /// Gets or Sets LegendHide
        /// </summary>
        [DataMember(Name="legendHide", EmitDefaultValue=false)]
        public bool? LegendHide { get; set; }

        /// <summary>
        /// Gets or Sets LegendOpacity
        /// </summary>
        [DataMember(Name="legendOpacity", EmitDefaultValue=false)]
        public float? LegendOpacity { get; set; }

        /// <summary>
        /// Gets or Sets LegendOrientationThreshold
        /// </summary>
        [DataMember(Name="legendOrientationThreshold", EmitDefaultValue=false)]
        public int? LegendOrientationThreshold { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class LinePlusSingleStatProperties {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  TimeFormat: ").Append(TimeFormat).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Queries: ").Append(Queries).Append("\n");
            sb.Append("  Colors: ").Append(Colors).Append("\n");
            sb.Append("  Shape: ").Append(Shape).Append("\n");
            sb.Append("  Note: ").Append(Note).Append("\n");
            sb.Append("  ShowNoteWhenEmpty: ").Append(ShowNoteWhenEmpty).Append("\n");
            sb.Append("  Axes: ").Append(Axes).Append("\n");
            sb.Append("  StaticLegend: ").Append(StaticLegend).Append("\n");
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
            sb.Append("  ShadeBelow: ").Append(ShadeBelow).Append("\n");
            sb.Append("  HoverDimension: ").Append(HoverDimension).Append("\n");
            sb.Append("  Position: ").Append(Position).Append("\n");
            sb.Append("  Prefix: ").Append(Prefix).Append("\n");
            sb.Append("  Suffix: ").Append(Suffix).Append("\n");
            sb.Append("  DecimalPlaces: ").Append(DecimalPlaces).Append("\n");
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
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as LinePlusSingleStatProperties);
        }

        /// <summary>
        /// Returns true if LinePlusSingleStatProperties instances are equal
        /// </summary>
        /// <param name="input">Instance of LinePlusSingleStatProperties to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(LinePlusSingleStatProperties input)
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
                    (this.Type != null &&
                    this.Type.Equals(input.Type))
                ) && base.Equals(input) && 
                (
                    this.Queries == input.Queries ||
                    this.Queries != null &&
                    this.Queries.SequenceEqual(input.Queries)
                ) && base.Equals(input) && 
                (
                    this.Colors == input.Colors ||
                    this.Colors != null &&
                    this.Colors.SequenceEqual(input.Colors)
                ) && base.Equals(input) && 
                (
                    this.Shape == input.Shape ||
                    (this.Shape != null &&
                    this.Shape.Equals(input.Shape))
                ) && base.Equals(input) && 
                (
                    this.Note == input.Note ||
                    (this.Note != null &&
                    this.Note.Equals(input.Note))
                ) && base.Equals(input) && 
                (
                    this.ShowNoteWhenEmpty == input.ShowNoteWhenEmpty ||
                    (this.ShowNoteWhenEmpty != null &&
                    this.ShowNoteWhenEmpty.Equals(input.ShowNoteWhenEmpty))
                ) && base.Equals(input) && 
                (
                    
                    (this.Axes != null &&
                    this.Axes.Equals(input.Axes))
                ) && base.Equals(input) && 
                (
                    
                    (this.StaticLegend != null &&
                    this.StaticLegend.Equals(input.StaticLegend))
                ) && base.Equals(input) && 
                (
                    this.XColumn == input.XColumn ||
                    (this.XColumn != null &&
                    this.XColumn.Equals(input.XColumn))
                ) && base.Equals(input) && 
                (
                    this.GenerateXAxisTicks == input.GenerateXAxisTicks ||
                    this.GenerateXAxisTicks != null &&
                    this.GenerateXAxisTicks.SequenceEqual(input.GenerateXAxisTicks)
                ) && base.Equals(input) && 
                (
                    this.XTotalTicks == input.XTotalTicks ||
                    (this.XTotalTicks != null &&
                    this.XTotalTicks.Equals(input.XTotalTicks))
                ) && base.Equals(input) && 
                (
                    this.XTickStart == input.XTickStart ||
                    (this.XTickStart != null &&
                    this.XTickStart.Equals(input.XTickStart))
                ) && base.Equals(input) && 
                (
                    this.XTickStep == input.XTickStep ||
                    (this.XTickStep != null &&
                    this.XTickStep.Equals(input.XTickStep))
                ) && base.Equals(input) && 
                (
                    this.YColumn == input.YColumn ||
                    (this.YColumn != null &&
                    this.YColumn.Equals(input.YColumn))
                ) && base.Equals(input) && 
                (
                    this.GenerateYAxisTicks == input.GenerateYAxisTicks ||
                    this.GenerateYAxisTicks != null &&
                    this.GenerateYAxisTicks.SequenceEqual(input.GenerateYAxisTicks)
                ) && base.Equals(input) && 
                (
                    this.YTotalTicks == input.YTotalTicks ||
                    (this.YTotalTicks != null &&
                    this.YTotalTicks.Equals(input.YTotalTicks))
                ) && base.Equals(input) && 
                (
                    this.YTickStart == input.YTickStart ||
                    (this.YTickStart != null &&
                    this.YTickStart.Equals(input.YTickStart))
                ) && base.Equals(input) && 
                (
                    this.YTickStep == input.YTickStep ||
                    (this.YTickStep != null &&
                    this.YTickStep.Equals(input.YTickStep))
                ) && base.Equals(input) && 
                (
                    this.ShadeBelow == input.ShadeBelow ||
                    (this.ShadeBelow != null &&
                    this.ShadeBelow.Equals(input.ShadeBelow))
                ) && base.Equals(input) && 
                (
                    this.HoverDimension == input.HoverDimension ||
                    (this.HoverDimension != null &&
                    this.HoverDimension.Equals(input.HoverDimension))
                ) && base.Equals(input) && 
                (
                    this.Position == input.Position ||
                    (this.Position != null &&
                    this.Position.Equals(input.Position))
                ) && base.Equals(input) && 
                (
                    this.Prefix == input.Prefix ||
                    (this.Prefix != null &&
                    this.Prefix.Equals(input.Prefix))
                ) && base.Equals(input) && 
                (
                    this.Suffix == input.Suffix ||
                    (this.Suffix != null &&
                    this.Suffix.Equals(input.Suffix))
                ) && base.Equals(input) && 
                (
                    
                    (this.DecimalPlaces != null &&
                    this.DecimalPlaces.Equals(input.DecimalPlaces))
                ) && base.Equals(input) && 
                (
                    this.LegendColorizeRows == input.LegendColorizeRows ||
                    (this.LegendColorizeRows != null &&
                    this.LegendColorizeRows.Equals(input.LegendColorizeRows))
                ) && base.Equals(input) && 
                (
                    this.LegendHide == input.LegendHide ||
                    (this.LegendHide != null &&
                    this.LegendHide.Equals(input.LegendHide))
                ) && base.Equals(input) && 
                (
                    this.LegendOpacity == input.LegendOpacity ||
                    (this.LegendOpacity != null &&
                    this.LegendOpacity.Equals(input.LegendOpacity))
                ) && base.Equals(input) && 
                (
                    this.LegendOrientationThreshold == input.LegendOrientationThreshold ||
                    (this.LegendOrientationThreshold != null &&
                    this.LegendOrientationThreshold.Equals(input.LegendOrientationThreshold))
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
                if (this.Type != null)
                    hashCode = hashCode * 59 + this.Type.GetHashCode();
                if (this.Queries != null)
                    hashCode = hashCode * 59 + this.Queries.GetHashCode();
                if (this.Colors != null)
                    hashCode = hashCode * 59 + this.Colors.GetHashCode();
                if (this.Shape != null)
                    hashCode = hashCode * 59 + this.Shape.GetHashCode();
                if (this.Note != null)
                    hashCode = hashCode * 59 + this.Note.GetHashCode();
                if (this.ShowNoteWhenEmpty != null)
                    hashCode = hashCode * 59 + this.ShowNoteWhenEmpty.GetHashCode();
                if (this.Axes != null)
                    hashCode = hashCode * 59 + this.Axes.GetHashCode();
                if (this.StaticLegend != null)
                    hashCode = hashCode * 59 + this.StaticLegend.GetHashCode();
                if (this.XColumn != null)
                    hashCode = hashCode * 59 + this.XColumn.GetHashCode();
                if (this.GenerateXAxisTicks != null)
                    hashCode = hashCode * 59 + this.GenerateXAxisTicks.GetHashCode();
                if (this.XTotalTicks != null)
                    hashCode = hashCode * 59 + this.XTotalTicks.GetHashCode();
                if (this.XTickStart != null)
                    hashCode = hashCode * 59 + this.XTickStart.GetHashCode();
                if (this.XTickStep != null)
                    hashCode = hashCode * 59 + this.XTickStep.GetHashCode();
                if (this.YColumn != null)
                    hashCode = hashCode * 59 + this.YColumn.GetHashCode();
                if (this.GenerateYAxisTicks != null)
                    hashCode = hashCode * 59 + this.GenerateYAxisTicks.GetHashCode();
                if (this.YTotalTicks != null)
                    hashCode = hashCode * 59 + this.YTotalTicks.GetHashCode();
                if (this.YTickStart != null)
                    hashCode = hashCode * 59 + this.YTickStart.GetHashCode();
                if (this.YTickStep != null)
                    hashCode = hashCode * 59 + this.YTickStep.GetHashCode();
                if (this.ShadeBelow != null)
                    hashCode = hashCode * 59 + this.ShadeBelow.GetHashCode();
                if (this.HoverDimension != null)
                    hashCode = hashCode * 59 + this.HoverDimension.GetHashCode();
                if (this.Position != null)
                    hashCode = hashCode * 59 + this.Position.GetHashCode();
                if (this.Prefix != null)
                    hashCode = hashCode * 59 + this.Prefix.GetHashCode();
                if (this.Suffix != null)
                    hashCode = hashCode * 59 + this.Suffix.GetHashCode();
                if (this.DecimalPlaces != null)
                    hashCode = hashCode * 59 + this.DecimalPlaces.GetHashCode();
                if (this.LegendColorizeRows != null)
                    hashCode = hashCode * 59 + this.LegendColorizeRows.GetHashCode();
                if (this.LegendHide != null)
                    hashCode = hashCode * 59 + this.LegendHide.GetHashCode();
                if (this.LegendOpacity != null)
                    hashCode = hashCode * 59 + this.LegendOpacity.GetHashCode();
                if (this.LegendOrientationThreshold != null)
                    hashCode = hashCode * 59 + this.LegendOrientationThreshold.GetHashCode();
                return hashCode;
            }
        }

    }

}
