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
    /// BucketRetentionRules
    /// </summary>
    [DataContract]
    public partial class BucketRetentionRules : IEquatable<BucketRetentionRules>
    {
        /// <summary>
        /// Defines Type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TypeEnum
        {
            /// <summary>
            /// Enum Expire for value: expire
            /// </summary>
            [EnumMember(Value = "expire")] Expire = 1
        }

        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name = "type", EmitDefaultValue = false)]
        public TypeEnum Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="BucketRetentionRules" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected BucketRetentionRules()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="BucketRetentionRules" /> class.
        /// </summary>
        /// <param name="type">type (required) (default to TypeEnum.Expire).</param>
        /// <param name="everySeconds">Duration in seconds for how long data will be kept in the database. 0 means infinite. (required).</param>
        /// <param name="shardGroupDurationSeconds">Shard duration measured in seconds..</param>
        public BucketRetentionRules(TypeEnum type = TypeEnum.Expire, long? everySeconds = default,
            long? shardGroupDurationSeconds = default)
        {
            // to ensure "type" is required (not null)
            Type = type;
            // to ensure "everySeconds" is required (not null)
            if (everySeconds == null)
            {
                throw new InvalidDataException(
                    "everySeconds is a required property for BucketRetentionRules and cannot be null");
            }

            EverySeconds = everySeconds;
            ShardGroupDurationSeconds = shardGroupDurationSeconds;
        }


        /// <summary>
        /// Duration in seconds for how long data will be kept in the database. 0 means infinite.
        /// </summary>
        /// <value>Duration in seconds for how long data will be kept in the database. 0 means infinite.</value>
        [DataMember(Name = "everySeconds", EmitDefaultValue = false)]
        public long? EverySeconds { get; set; }

        /// <summary>
        /// Shard duration measured in seconds.
        /// </summary>
        /// <value>Shard duration measured in seconds.</value>
        [DataMember(Name = "shardGroupDurationSeconds", EmitDefaultValue = false)]
        public long? ShardGroupDurationSeconds { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class BucketRetentionRules {\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  EverySeconds: ").Append(EverySeconds).Append("\n");
            sb.Append("  ShardGroupDurationSeconds: ").Append(ShardGroupDurationSeconds).Append("\n");
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
            return Equals(input as BucketRetentionRules);
        }

        /// <summary>
        /// Returns true if BucketRetentionRules instances are equal
        /// </summary>
        /// <param name="input">Instance of BucketRetentionRules to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(BucketRetentionRules input)
        {
            if (input == null)
            {
                return false;
            }

            return
                (
                    Type == input.Type ||
                    Type.Equals(input.Type)
                ) &&
                (
                    EverySeconds == input.EverySeconds ||
                    EverySeconds != null && EverySeconds.Equals(input.EverySeconds)
                ) &&
                (
                    ShardGroupDurationSeconds == input.ShardGroupDurationSeconds ||
                    ShardGroupDurationSeconds != null &&
                    ShardGroupDurationSeconds.Equals(input.ShardGroupDurationSeconds)
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

                hashCode = hashCode * 59 + Type.GetHashCode();
                if (EverySeconds != null)
                {
                    hashCode = hashCode * 59 + EverySeconds.GetHashCode();
                }

                if (ShardGroupDurationSeconds != null)
                {
                    hashCode = hashCode * 59 + ShardGroupDurationSeconds.GetHashCode();
                }

                return hashCode;
            }
        }
    }
}