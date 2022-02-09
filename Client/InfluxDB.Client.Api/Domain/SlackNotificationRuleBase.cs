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
    /// SlackNotificationRuleBase
    /// </summary>
    [DataContract]
    public partial class SlackNotificationRuleBase : NotificationRule,  IEquatable<SlackNotificationRuleBase>
    {
        /// <summary>
        /// Defines Type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TypeEnum
        {
            /// <summary>
            /// Enum Slack for value: slack
            /// </summary>
            [EnumMember(Value = "slack")]
            Slack = 1

        }

        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name="type", EmitDefaultValue=false)]
        public TypeEnum Type { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="SlackNotificationRuleBase" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected SlackNotificationRuleBase() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SlackNotificationRuleBase" /> class.
        /// </summary>
        /// <param name="type">type (required) (default to TypeEnum.Slack).</param>
        /// <param name="channel">channel.</param>
        /// <param name="messageTemplate">messageTemplate (required).</param>
        public SlackNotificationRuleBase(TypeEnum type = TypeEnum.Slack, string channel = default(string), string messageTemplate = default(string), string endpointID = default(string), string orgID = default(string), string taskID = default(string), TaskStatusType status = default(TaskStatusType), string name = default(string), string sleepUntil = default(string), string every = default(string), string offset = default(string), string runbookLink = default(string), int? limitEvery = default(int?), int? limit = default(int?), List<TagRule> tagRules = default(List<TagRule>), string description = default(string), List<StatusRule> statusRules = default(List<StatusRule>), List<Label> labels = default(List<Label>), NotificationRuleBaseLinks links = default(NotificationRuleBaseLinks)) : base(endpointID, orgID, taskID, status, name, sleepUntil, every, offset, runbookLink, limitEvery, limit, tagRules, description, statusRules, labels, links)
        {
            // to ensure "type" is required (not null)
            this.Type = type;
            // to ensure "messageTemplate" is required (not null)
            if (messageTemplate == null)
            {
                throw new InvalidDataException("messageTemplate is a required property for SlackNotificationRuleBase and cannot be null");
            }
            this.MessageTemplate = messageTemplate;
            this.Channel = channel;
        }


        /// <summary>
        /// Gets or Sets Channel
        /// </summary>
        [DataMember(Name="channel", EmitDefaultValue=false)]
        public string Channel { get; set; }

        /// <summary>
        /// Gets or Sets MessageTemplate
        /// </summary>
        [DataMember(Name="messageTemplate", EmitDefaultValue=false)]
        public string MessageTemplate { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class SlackNotificationRuleBase {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Channel: ").Append(Channel).Append("\n");
            sb.Append("  MessageTemplate: ").Append(MessageTemplate).Append("\n");
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
            return this.Equals(input as SlackNotificationRuleBase);
        }

        /// <summary>
        /// Returns true if SlackNotificationRuleBase instances are equal
        /// </summary>
        /// <param name="input">Instance of SlackNotificationRuleBase to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(SlackNotificationRuleBase input)
        {
            if (input == null)
                return false;

            return base.Equals(input) && 
                (
                    this.Type == input.Type ||
                    this.Type.Equals(input.Type)
                ) && base.Equals(input) && 
                (
                    this.Channel == input.Channel ||
                    (this.Channel != null && this.Channel.Equals(input.Channel))
                ) && base.Equals(input) && 
                (
                    this.MessageTemplate == input.MessageTemplate ||
                    (this.MessageTemplate != null && this.MessageTemplate.Equals(input.MessageTemplate))
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
                if (this.Channel != null)
                    hashCode = hashCode * 59 + this.Channel.GetHashCode();
                if (this.MessageTemplate != null)
                    hashCode = hashCode * 59 + this.MessageTemplate.GetHashCode();
                return hashCode;
            }
        }

    }

}
