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
    /// Query influx using the Flux language
    /// </summary>
    [DataContract]
    public partial class Query :  IEquatable<Query>
    {
        /// <summary>
        /// The type of query. Must be \&quot;flux\&quot;.
        /// </summary>
        /// <value>The type of query. Must be \&quot;flux\&quot;.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TypeEnum
        {
            /// <summary>
            /// Enum Flux for value: flux
            /// </summary>
            [EnumMember(Value = "flux")]
            Flux = 1

        }

        /// <summary>
        /// The type of query. Must be \&quot;flux\&quot;.
        /// </summary>
        /// <value>The type of query. Must be \&quot;flux\&quot;.</value>
        [DataMember(Name="type", EmitDefaultValue=false)]
        public TypeEnum? Type { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Query" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected Query() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="Query" /> class.
        /// </summary>
        /// <param name="_extern">_extern.</param>
        /// <param name="query">Query script to execute. (required).</param>
        /// <param name="type">The type of query. Must be \&quot;flux\&quot;..</param>
        /// <param name="_params">Enumeration of key/value pairs that respresent parameters to be injected into query (can only specify either this field or extern and not both).</param>
        /// <param name="dialect">dialect.</param>
        /// <param name="now">Specifies the time that should be reported as \&quot;now\&quot; in the query. Default is the server&#39;s now time..</param>
        public Query(File _extern = default(File), string query = default(string), TypeEnum? type = default(TypeEnum?), Dictionary<string, Object> _params = default(Dictionary<string, Object>), Dialect dialect = default(Dialect), DateTime? now = default(DateTime?))
        {
            // to ensure "query" is required (not null)
            if (query == null)
            {
                throw new InvalidDataException("query is a required property for Query and cannot be null");
            }
            else
            {
                this._Query = query;
            }
            this.Extern = _extern;
            this.Type = type;
            this.Params = _params;
            this.Dialect = dialect;
            this.Now = now;
        }

        /// <summary>
        /// Gets or Sets Extern
        /// </summary>
        [DataMember(Name="extern", EmitDefaultValue=false)]
        public File Extern { get; set; }

        /// <summary>
        /// Query script to execute.
        /// </summary>
        /// <value>Query script to execute.</value>
        [DataMember(Name="query", EmitDefaultValue=false)]
        public string _Query { get; set; }


        /// <summary>
        /// Enumeration of key/value pairs that respresent parameters to be injected into query (can only specify either this field or extern and not both)
        /// </summary>
        /// <value>Enumeration of key/value pairs that respresent parameters to be injected into query (can only specify either this field or extern and not both)</value>
        [DataMember(Name="params", EmitDefaultValue=false)]
        public Dictionary<string, Object> Params { get; set; }

        /// <summary>
        /// Gets or Sets Dialect
        /// </summary>
        [DataMember(Name="dialect", EmitDefaultValue=false)]
        public Dialect Dialect { get; set; }

        /// <summary>
        /// Specifies the time that should be reported as \&quot;now\&quot; in the query. Default is the server&#39;s now time.
        /// </summary>
        /// <value>Specifies the time that should be reported as \&quot;now\&quot; in the query. Default is the server&#39;s now time.</value>
        [DataMember(Name="now", EmitDefaultValue=false)]
        public DateTime? Now { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Query {\n");
            sb.Append("  Extern: ").Append(Extern).Append("\n");
            sb.Append("  _Query: ").Append(_Query).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Params: ").Append(Params).Append("\n");
            sb.Append("  Dialect: ").Append(Dialect).Append("\n");
            sb.Append("  Now: ").Append(Now).Append("\n");
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
            return this.Equals(input as Query);
        }

        /// <summary>
        /// Returns true if Query instances are equal
        /// </summary>
        /// <param name="input">Instance of Query to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Query input)
        {
            if (input == null)
                return false;

            return 
                (
                    
                    (this.Extern != null &&
                    this.Extern.Equals(input.Extern))
                ) && 
                (
                    this._Query == input._Query ||
                    (this._Query != null &&
                    this._Query.Equals(input._Query))
                ) && 
                (
                    this.Type == input.Type ||
                    (this.Type != null &&
                    this.Type.Equals(input.Type))
                ) && 
                (
                    this.Params == input.Params ||
                    this.Params != null &&
                    this.Params.SequenceEqual(input.Params)
                ) && 
                (
                    
                    (this.Dialect != null &&
                    this.Dialect.Equals(input.Dialect))
                ) && 
                (
                    this.Now == input.Now ||
                    (this.Now != null &&
                    this.Now.Equals(input.Now))
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
                if (this.Extern != null)
                    hashCode = hashCode * 59 + this.Extern.GetHashCode();
                if (this._Query != null)
                    hashCode = hashCode * 59 + this._Query.GetHashCode();
                if (this.Type != null)
                    hashCode = hashCode * 59 + this.Type.GetHashCode();
                if (this.Params != null)
                    hashCode = hashCode * 59 + this.Params.GetHashCode();
                if (this.Dialect != null)
                    hashCode = hashCode * 59 + this.Dialect.GetHashCode();
                if (this.Now != null)
                    hashCode = hashCode * 59 + this.Now.GetHashCode();
                return hashCode;
            }
        }

    }

}
