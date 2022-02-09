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
    /// TemplateChart
    /// </summary>
    [DataContract]
    public partial class TemplateChart :  IEquatable<TemplateChart>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TemplateChart" /> class.
        /// </summary>
        /// <param name="xPos">xPos.</param>
        /// <param name="yPos">yPos.</param>
        /// <param name="height">height.</param>
        /// <param name="width">width.</param>
        /// <param name="properties">properties.</param>
        public TemplateChart(int? xPos = default(int?), int? yPos = default(int?), int? height = default(int?), int? width = default(int?), ViewProperties properties = default(ViewProperties))
        {
            this.XPos = xPos;
            this.YPos = yPos;
            this.Height = height;
            this.Width = width;
            this.Properties = properties;
        }

        /// <summary>
        /// Gets or Sets XPos
        /// </summary>
        [DataMember(Name="xPos", EmitDefaultValue=false)]
        public int? XPos { get; set; }

        /// <summary>
        /// Gets or Sets YPos
        /// </summary>
        [DataMember(Name="yPos", EmitDefaultValue=false)]
        public int? YPos { get; set; }

        /// <summary>
        /// Gets or Sets Height
        /// </summary>
        [DataMember(Name="height", EmitDefaultValue=false)]
        public int? Height { get; set; }

        /// <summary>
        /// Gets or Sets Width
        /// </summary>
        [DataMember(Name="width", EmitDefaultValue=false)]
        public int? Width { get; set; }

        /// <summary>
        /// Gets or Sets Properties
        /// </summary>
        [DataMember(Name="properties", EmitDefaultValue=false)]
        [JsonConverter(typeof(TemplateChartPropertiesAdapter))]
        public ViewProperties Properties { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class TemplateChart {\n");
            sb.Append("  XPos: ").Append(XPos).Append("\n");
            sb.Append("  YPos: ").Append(YPos).Append("\n");
            sb.Append("  Height: ").Append(Height).Append("\n");
            sb.Append("  Width: ").Append(Width).Append("\n");
            sb.Append("  Properties: ").Append(Properties).Append("\n");
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
            return this.Equals(input as TemplateChart);
        }

        /// <summary>
        /// Returns true if TemplateChart instances are equal
        /// </summary>
        /// <param name="input">Instance of TemplateChart to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TemplateChart input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.XPos == input.XPos ||
                    (this.XPos != null && this.XPos.Equals(input.XPos))
                ) && 
                (
                    this.YPos == input.YPos ||
                    (this.YPos != null && this.YPos.Equals(input.YPos))
                ) && 
                (
                    this.Height == input.Height ||
                    (this.Height != null && this.Height.Equals(input.Height))
                ) && 
                (
                    this.Width == input.Width ||
                    (this.Width != null && this.Width.Equals(input.Width))
                ) && 
                (
                    
                    (this.Properties != null && this.Properties.Equals(input.Properties))
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
                
                if (this.XPos != null)
                    hashCode = hashCode * 59 + this.XPos.GetHashCode();
                if (this.YPos != null)
                    hashCode = hashCode * 59 + this.YPos.GetHashCode();
                if (this.Height != null)
                    hashCode = hashCode * 59 + this.Height.GetHashCode();
                if (this.Width != null)
                    hashCode = hashCode * 59 + this.Width.GetHashCode();
                if (this.Properties != null)
                    hashCode = hashCode * 59 + this.Properties.GetHashCode();
                return hashCode;
            }
        }

    public class TemplateChartPropertiesAdapter : JsonConverter
    {
        private static readonly Dictionary<string[], Type> Types = new Dictionary<string[], Type>(new Client.DiscriminatorComparer<string>())
        {
            {new []{ "LinePlusSingleStatProperties", "line-plus-single-stat", "chronograf-v2" }, typeof(LinePlusSingleStatProperties)},
            {new []{ "XYViewProperties", "xy", "chronograf-v2" }, typeof(XYViewProperties)},
            {new []{ "single-stat", "chronograf-v2" }, typeof(SingleStatViewProperties)},
            {new []{ "histogram", "chronograf-v2" }, typeof(HistogramViewProperties)},
            {new []{ "gauge", "chronograf-v2" }, typeof(GaugeViewProperties)},
            {new []{ "table", "chronograf-v2" }, typeof(TableViewProperties)},
            {new []{ "simple-table", "chronograf-v2" }, typeof(SimpleTableViewProperties)},
            {new []{ "markdown", "chronograf-v2" }, typeof(MarkdownViewProperties)},
            {new []{ "check", "chronograf-v2" }, typeof(CheckViewProperties)},
            {new []{ "ScatterViewProperties", "scatter", "chronograf-v2" }, typeof(ScatterViewProperties)},
            {new []{ "HeatmapViewProperties", "heatmap", "chronograf-v2" }, typeof(HeatmapViewProperties)},
            {new []{ "MosaicViewProperties", "mosaic", "chronograf-v2" }, typeof(MosaicViewProperties)},
            {new []{ "BandViewProperties", "band", "chronograf-v2" }, typeof(BandViewProperties)},
        };

        public override bool CanConvert(Type objectType)
        {
            return false;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return Deserialize(reader, objectType, serializer);
        }

        private object Deserialize(JsonReader reader, Type objectType, JsonSerializer serializer)
        {
            switch (reader.TokenType)
            {
                case JsonToken.StartObject:

                    var jObject = Newtonsoft.Json.Linq.JObject.Load(reader);

                    var discriminator = new []{ "timeFormat", "type", "shape" }.Select(key => jObject[key].ToString()).ToArray();

                    Types.TryGetValue(discriminator, out var type);

                    return serializer.Deserialize(jObject.CreateReader(), type);

                case JsonToken.StartArray:
                    return DeserializeArray(reader, objectType, serializer);

                default:
                    return serializer.Deserialize(reader, objectType);
            }
        }

        private IList DeserializeArray(JsonReader reader, Type targetType, JsonSerializer serializer)
        {
            var elementType = targetType.GenericTypeArguments.FirstOrDefault();

            var list = (IList) Activator.CreateInstance(targetType);
            while (reader.Read() && reader.TokenType != JsonToken.EndArray)
            {
                list.Add(Deserialize(reader, elementType, serializer));
            }

            return list;
        }
    }
    }

}
