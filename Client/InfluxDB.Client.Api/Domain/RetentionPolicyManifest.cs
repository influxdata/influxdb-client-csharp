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
    /// RetentionPolicyManifest
    /// </summary>
    [DataContract]
    public partial class RetentionPolicyManifest : IEquatable<RetentionPolicyManifest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RetentionPolicyManifest" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected RetentionPolicyManifest()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RetentionPolicyManifest" /> class.
        /// </summary>
        /// <param name="name">name (required).</param>
        /// <param name="replicaN">replicaN (required).</param>
        /// <param name="duration">duration (required).</param>
        /// <param name="shardGroupDuration">shardGroupDuration (required).</param>
        /// <param name="shardGroups">shardGroups (required).</param>
        /// <param name="subscriptions">subscriptions (required).</param>
        public RetentionPolicyManifest(string name = default, int? replicaN = default, long? duration = default,
            long? shardGroupDuration = default, List<ShardGroupManifest> shardGroups = default,
            List<SubscriptionManifest> subscriptions = default)
        {
            // to ensure "name" is required (not null)
            if (name == null)
            {
                throw new InvalidDataException(
                    "name is a required property for RetentionPolicyManifest and cannot be null");
            }

            Name = name;
            // to ensure "replicaN" is required (not null)
            if (replicaN == null)
            {
                throw new InvalidDataException(
                    "replicaN is a required property for RetentionPolicyManifest and cannot be null");
            }

            ReplicaN = replicaN;
            // to ensure "duration" is required (not null)
            if (duration == null)
            {
                throw new InvalidDataException(
                    "duration is a required property for RetentionPolicyManifest and cannot be null");
            }

            Duration = duration;
            // to ensure "shardGroupDuration" is required (not null)
            if (shardGroupDuration == null)
            {
                throw new InvalidDataException(
                    "shardGroupDuration is a required property for RetentionPolicyManifest and cannot be null");
            }

            ShardGroupDuration = shardGroupDuration;
            // to ensure "shardGroups" is required (not null)
            if (shardGroups == null)
            {
                throw new InvalidDataException(
                    "shardGroups is a required property for RetentionPolicyManifest and cannot be null");
            }

            ShardGroups = shardGroups;
            // to ensure "subscriptions" is required (not null)
            if (subscriptions == null)
            {
                throw new InvalidDataException(
                    "subscriptions is a required property for RetentionPolicyManifest and cannot be null");
            }

            Subscriptions = subscriptions;
        }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets ReplicaN
        /// </summary>
        [DataMember(Name = "replicaN", EmitDefaultValue = false)]
        public int? ReplicaN { get; set; }

        /// <summary>
        /// Gets or Sets Duration
        /// </summary>
        [DataMember(Name = "duration", EmitDefaultValue = false)]
        public long? Duration { get; set; }

        /// <summary>
        /// Gets or Sets ShardGroupDuration
        /// </summary>
        [DataMember(Name = "shardGroupDuration", EmitDefaultValue = false)]
        public long? ShardGroupDuration { get; set; }

        /// <summary>
        /// Gets or Sets ShardGroups
        /// </summary>
        [DataMember(Name = "shardGroups", EmitDefaultValue = false)]
        public List<ShardGroupManifest> ShardGroups { get; set; }

        /// <summary>
        /// Gets or Sets Subscriptions
        /// </summary>
        [DataMember(Name = "subscriptions", EmitDefaultValue = false)]
        public List<SubscriptionManifest> Subscriptions { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class RetentionPolicyManifest {\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  ReplicaN: ").Append(ReplicaN).Append("\n");
            sb.Append("  Duration: ").Append(Duration).Append("\n");
            sb.Append("  ShardGroupDuration: ").Append(ShardGroupDuration).Append("\n");
            sb.Append("  ShardGroups: ").Append(ShardGroups).Append("\n");
            sb.Append("  Subscriptions: ").Append(Subscriptions).Append("\n");
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
            return Equals(input as RetentionPolicyManifest);
        }

        /// <summary>
        /// Returns true if RetentionPolicyManifest instances are equal
        /// </summary>
        /// <param name="input">Instance of RetentionPolicyManifest to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(RetentionPolicyManifest input)
        {
            if (input == null)
            {
                return false;
            }

            return
                (
                    Name == input.Name ||
                    Name != null && Name.Equals(input.Name)
                ) &&
                (
                    ReplicaN == input.ReplicaN ||
                    ReplicaN != null && ReplicaN.Equals(input.ReplicaN)
                ) &&
                (
                    Duration == input.Duration ||
                    Duration != null && Duration.Equals(input.Duration)
                ) &&
                (
                    ShardGroupDuration == input.ShardGroupDuration ||
                    ShardGroupDuration != null && ShardGroupDuration.Equals(input.ShardGroupDuration)
                ) &&
                (
                    ShardGroups == input.ShardGroups ||
                    ShardGroups != null &&
                    ShardGroups.SequenceEqual(input.ShardGroups)
                ) &&
                (
                    Subscriptions == input.Subscriptions ||
                    Subscriptions != null &&
                    Subscriptions.SequenceEqual(input.Subscriptions)
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

                if (Name != null)
                {
                    hashCode = hashCode * 59 + Name.GetHashCode();
                }

                if (ReplicaN != null)
                {
                    hashCode = hashCode * 59 + ReplicaN.GetHashCode();
                }

                if (Duration != null)
                {
                    hashCode = hashCode * 59 + Duration.GetHashCode();
                }

                if (ShardGroupDuration != null)
                {
                    hashCode = hashCode * 59 + ShardGroupDuration.GetHashCode();
                }

                if (ShardGroups != null)
                {
                    hashCode = hashCode * 59 + ShardGroups.GetHashCode();
                }

                if (Subscriptions != null)
                {
                    hashCode = hashCode * 59 + Subscriptions.GetHashCode();
                }

                return hashCode;
            }
        }
    }
}