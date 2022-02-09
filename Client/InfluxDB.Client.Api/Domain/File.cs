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
    /// Represents a source from a single file
    /// </summary>
    [DataContract]
    public partial class File :  IEquatable<File>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="File" /> class.
        /// </summary>
        /// <param name="type">Type of AST node.</param>
        /// <param name="name">The name of the file..</param>
        /// <param name="package">package.</param>
        /// <param name="imports">A list of package imports.</param>
        /// <param name="body">List of Flux statements.</param>
        public File(string type = default(string), string name = default(string), PackageClause package = default(PackageClause), List<ImportDeclaration> imports = default(List<ImportDeclaration>), List<Statement> body = default(List<Statement>))
        {
            this.Type = type;
            this.Name = name;
            this.Package = package;
            this.Imports = imports;
            this.Body = body;
        }

        /// <summary>
        /// Type of AST node
        /// </summary>
        /// <value>Type of AST node</value>
        [DataMember(Name="type", EmitDefaultValue=false)]
        public string Type { get; set; }

        /// <summary>
        /// The name of the file.
        /// </summary>
        /// <value>The name of the file.</value>
        [DataMember(Name="name", EmitDefaultValue=false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets Package
        /// </summary>
        [DataMember(Name="package", EmitDefaultValue=false)]
        public PackageClause Package { get; set; }

        /// <summary>
        /// A list of package imports
        /// </summary>
        /// <value>A list of package imports</value>
        [DataMember(Name="imports", EmitDefaultValue=false)]
        public List<ImportDeclaration> Imports { get; set; }

        /// <summary>
        /// List of Flux statements
        /// </summary>
        /// <value>List of Flux statements</value>
        [DataMember(Name="body", EmitDefaultValue=false)]
        [JsonConverter(typeof(FileBodyAdapter))]
        public List<Statement> Body { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class File {\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Package: ").Append(Package).Append("\n");
            sb.Append("  Imports: ").Append(Imports).Append("\n");
            sb.Append("  Body: ").Append(Body).Append("\n");
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
            return this.Equals(input as File);
        }

        /// <summary>
        /// Returns true if File instances are equal
        /// </summary>
        /// <param name="input">Instance of File to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(File input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Type == input.Type ||
                    (this.Type != null && this.Type.Equals(input.Type))
                ) && 
                (
                    this.Name == input.Name ||
                    (this.Name != null && this.Name.Equals(input.Name))
                ) && 
                (
                    
                    (this.Package != null && this.Package.Equals(input.Package))
                ) && 
                (
                    this.Imports == input.Imports ||
                    this.Imports != null &&
                    this.Imports.SequenceEqual(input.Imports)
                ) && 
                (
                    this.Body == input.Body ||
                    this.Body != null &&
                    this.Body.SequenceEqual(input.Body)
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
                
                if (this.Type != null)
                    hashCode = hashCode * 59 + this.Type.GetHashCode();
                if (this.Name != null)
                    hashCode = hashCode * 59 + this.Name.GetHashCode();
                if (this.Package != null)
                    hashCode = hashCode * 59 + this.Package.GetHashCode();
                if (this.Imports != null)
                    hashCode = hashCode * 59 + this.Imports.GetHashCode();
                if (this.Body != null)
                    hashCode = hashCode * 59 + this.Body.GetHashCode();
                return hashCode;
            }
        }

    public class FileBodyAdapter : JsonConverter
    {
        private static readonly Dictionary<string[], Type> Types = new Dictionary<string[], Type>(new Client.DiscriminatorComparer<string>())
        {
            {new []{ "BadStatement" }, typeof(BadStatement)},
            {new []{ "VariableAssignment" }, typeof(VariableAssignment)},
            {new []{ "MemberAssignment" }, typeof(MemberAssignment)},
            {new []{ "ExpressionStatement" }, typeof(ExpressionStatement)},
            {new []{ "ReturnStatement" }, typeof(ReturnStatement)},
            {new []{ "OptionStatement" }, typeof(OptionStatement)},
            {new []{ "BuiltinStatement" }, typeof(BuiltinStatement)},
            {new []{ "TestStatement" }, typeof(TestStatement)},
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

                    var discriminator = new []{ "type" }.Select(key => jObject[key].ToString()).ToArray();

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
