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
    /// NotificationRuleBaseLinks
    /// </summary>
    [DataContract]
    public partial class NotificationRuleBaseLinks :  IEquatable<NotificationRuleBaseLinks>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NotificationRuleBaseLinks" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        public NotificationRuleBaseLinks()
        {
        }

        /// <summary>
        /// URI of resource.
        /// </summary>
        /// <value>URI of resource.</value>
        [DataMember(Name="self", EmitDefaultValue=false)]
        public string Self { get; private set; }

        /// <summary>
        /// URI of resource.
        /// </summary>
        /// <value>URI of resource.</value>
        [DataMember(Name="labels", EmitDefaultValue=false)]
        public string Labels { get; private set; }

        /// <summary>
        /// URI of resource.
        /// </summary>
        /// <value>URI of resource.</value>
        [DataMember(Name="members", EmitDefaultValue=false)]
        public string Members { get; private set; }

        /// <summary>
        /// URI of resource.
        /// </summary>
        /// <value>URI of resource.</value>
        [DataMember(Name="owners", EmitDefaultValue=false)]
        public string Owners { get; private set; }

        /// <summary>
        /// URI of resource.
        /// </summary>
        /// <value>URI of resource.</value>
        [DataMember(Name="query", EmitDefaultValue=false)]
        public string Query { get; private set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class NotificationRuleBaseLinks {\n");
            sb.Append("  Self: ").Append(Self).Append("\n");
            sb.Append("  Labels: ").Append(Labels).Append("\n");
            sb.Append("  Members: ").Append(Members).Append("\n");
            sb.Append("  Owners: ").Append(Owners).Append("\n");
            sb.Append("  Query: ").Append(Query).Append("\n");
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
            return this.Equals(input as NotificationRuleBaseLinks);
        }

        /// <summary>
        /// Returns true if NotificationRuleBaseLinks instances are equal
        /// </summary>
        /// <param name="input">Instance of NotificationRuleBaseLinks to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(NotificationRuleBaseLinks input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Self == input.Self ||
                    (this.Self != null && this.Self.Equals(input.Self))
                ) && 
                (
                    this.Labels == input.Labels ||
                    (this.Labels != null && this.Labels.Equals(input.Labels))
                ) && 
                (
                    this.Members == input.Members ||
                    (this.Members != null && this.Members.Equals(input.Members))
                ) && 
                (
                    this.Owners == input.Owners ||
                    (this.Owners != null && this.Owners.Equals(input.Owners))
                ) && 
                (
                    this.Query == input.Query ||
                    (this.Query != null && this.Query.Equals(input.Query))
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
                
                if (this.Self != null)
                    hashCode = hashCode * 59 + this.Self.GetHashCode();
                if (this.Labels != null)
                    hashCode = hashCode * 59 + this.Labels.GetHashCode();
                if (this.Members != null)
                    hashCode = hashCode * 59 + this.Members.GetHashCode();
                if (this.Owners != null)
                    hashCode = hashCode * 59 + this.Owners.GetHashCode();
                if (this.Query != null)
                    hashCode = hashCode * 59 + this.Query.GetHashCode();
                return hashCode;
            }
        }

    }

}
