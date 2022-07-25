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
    /// Variable
    /// </summary>
    [DataContract]
    public partial class Variable : IEquatable<Variable>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Variable" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected Variable()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Variable" /> class.
        /// </summary>
        /// <param name="links">links.</param>
        /// <param name="orgID">orgID (required).</param>
        /// <param name="name">name (required).</param>
        /// <param name="description">description.</param>
        /// <param name="selected">selected.</param>
        /// <param name="labels">labels.</param>
        /// <param name="arguments">arguments (required).</param>
        /// <param name="createdAt">createdAt.</param>
        /// <param name="updatedAt">updatedAt.</param>
        public Variable(VariableLinks links = default, string orgID = default, string name = default,
            string description = default, List<string> selected = default, List<Label> labels = default,
            VariableProperties arguments = default, DateTime? createdAt = default, DateTime? updatedAt = default)
        {
            // to ensure "orgID" is required (not null)
            if (orgID == null)
            {
                throw new InvalidDataException("orgID is a required property for Variable and cannot be null");
            }

            OrgID = orgID;
            // to ensure "name" is required (not null)
            if (name == null)
            {
                throw new InvalidDataException("name is a required property for Variable and cannot be null");
            }

            Name = name;
            // to ensure "arguments" is required (not null)
            if (arguments == null)
            {
                throw new InvalidDataException("arguments is a required property for Variable and cannot be null");
            }

            Arguments = arguments;
            Links = links;
            Description = description;
            Selected = selected;
            Labels = labels;
            CreatedAt = createdAt;
            UpdatedAt = updatedAt;
        }

        /// <summary>
        /// Gets or Sets Links
        /// </summary>
        [DataMember(Name = "links", EmitDefaultValue = false)]
        public VariableLinks Links { get; set; }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; private set; }

        /// <summary>
        /// Gets or Sets OrgID
        /// </summary>
        [DataMember(Name = "orgID", EmitDefaultValue = false)]
        public string OrgID { get; set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets Description
        /// </summary>
        [DataMember(Name = "description", EmitDefaultValue = false)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or Sets Selected
        /// </summary>
        [DataMember(Name = "selected", EmitDefaultValue = false)]
        public List<string> Selected { get; set; }

        /// <summary>
        /// Gets or Sets Labels
        /// </summary>
        [DataMember(Name = "labels", EmitDefaultValue = false)]
        public List<Label> Labels { get; set; }

        /// <summary>
        /// Gets or Sets Arguments
        /// </summary>
        [DataMember(Name = "arguments", EmitDefaultValue = false)]
        [JsonConverter(typeof(VariableArgumentsAdapter))]
        public VariableProperties Arguments { get; set; }

        /// <summary>
        /// Gets or Sets CreatedAt
        /// </summary>
        [DataMember(Name = "createdAt", EmitDefaultValue = false)]
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Gets or Sets UpdatedAt
        /// </summary>
        [DataMember(Name = "updatedAt", EmitDefaultValue = false)]
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Variable {\n");
            sb.Append("  Links: ").Append(Links).Append("\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  OrgID: ").Append(OrgID).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  Selected: ").Append(Selected).Append("\n");
            sb.Append("  Labels: ").Append(Labels).Append("\n");
            sb.Append("  Arguments: ").Append(Arguments).Append("\n");
            sb.Append("  CreatedAt: ").Append(CreatedAt).Append("\n");
            sb.Append("  UpdatedAt: ").Append(UpdatedAt).Append("\n");
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
            return Equals(input as Variable);
        }

        /// <summary>
        /// Returns true if Variable instances are equal
        /// </summary>
        /// <param name="input">Instance of Variable to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Variable input)
        {
            if (input == null)
            {
                return false;
            }

            return
                Links != null && Links.Equals(input.Links) &&
                (
                    Id == input.Id ||
                    Id != null && Id.Equals(input.Id)
                ) &&
                (
                    OrgID == input.OrgID ||
                    OrgID != null && OrgID.Equals(input.OrgID)
                ) &&
                (
                    Name == input.Name ||
                    Name != null && Name.Equals(input.Name)
                ) &&
                (
                    Description == input.Description ||
                    Description != null && Description.Equals(input.Description)
                ) &&
                (
                    Selected == input.Selected ||
                    Selected != null &&
                    Selected.SequenceEqual(input.Selected)
                ) &&
                (
                    Labels == input.Labels ||
                    Labels != null &&
                    Labels.SequenceEqual(input.Labels)
                ) && Arguments != null && Arguments.Equals(input.Arguments) && (
                    CreatedAt == input.CreatedAt ||
                    CreatedAt != null && CreatedAt.Equals(input.CreatedAt)
                ) && (
                    UpdatedAt == input.UpdatedAt ||
                    UpdatedAt != null && UpdatedAt.Equals(input.UpdatedAt)
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

                if (Links != null)
                {
                    hashCode = hashCode * 59 + Links.GetHashCode();
                }

                if (Id != null)
                {
                    hashCode = hashCode * 59 + Id.GetHashCode();
                }

                if (OrgID != null)
                {
                    hashCode = hashCode * 59 + OrgID.GetHashCode();
                }

                if (Name != null)
                {
                    hashCode = hashCode * 59 + Name.GetHashCode();
                }

                if (Description != null)
                {
                    hashCode = hashCode * 59 + Description.GetHashCode();
                }

                if (Selected != null)
                {
                    hashCode = hashCode * 59 + Selected.GetHashCode();
                }

                if (Labels != null)
                {
                    hashCode = hashCode * 59 + Labels.GetHashCode();
                }

                if (Arguments != null)
                {
                    hashCode = hashCode * 59 + Arguments.GetHashCode();
                }

                if (CreatedAt != null)
                {
                    hashCode = hashCode * 59 + CreatedAt.GetHashCode();
                }

                if (UpdatedAt != null)
                {
                    hashCode = hashCode * 59 + UpdatedAt.GetHashCode();
                }

                return hashCode;
            }
        }

        public class VariableArgumentsAdapter : JsonConverter
        {
            private static readonly Dictionary<string[], Type> Types =
                new Dictionary<string[], Type>(new Client.DiscriminatorComparer<string>())
                {
                    {new[] {"query"}, typeof(QueryVariableProperties)},
                    {new[] {"constant"}, typeof(ConstantVariableProperties)},
                    {new[] {"map"}, typeof(MapVariableProperties)}
                };

            public override bool CanConvert(Type objectType)
            {
                return false;
            }

            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
            {
                serializer.Serialize(writer, value);
            }

            public override object ReadJson(JsonReader reader, Type objectType, object existingValue,
                JsonSerializer serializer)
            {
                return Deserialize(reader, objectType, serializer);
            }

            private object Deserialize(JsonReader reader, Type objectType, JsonSerializer serializer)
            {
                switch (reader.TokenType)
                {
                    case JsonToken.StartObject:

                        var jObject = Newtonsoft.Json.Linq.JObject.Load(reader);

                        var discriminator = new[] {"type"}.Select(key => jObject[key].ToString()).ToArray();

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
                    list.Add(Deserialize(reader, elementType, serializer));

                return list;
            }
        }
    }
}