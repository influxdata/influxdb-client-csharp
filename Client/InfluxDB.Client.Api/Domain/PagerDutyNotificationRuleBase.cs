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
    /// PagerDutyNotificationRuleBase
    /// </summary>
    [DataContract]
    public partial class PagerDutyNotificationRuleBase : NotificationRule, IEquatable<PagerDutyNotificationRuleBase>
    {
        /// <summary>
        /// Defines Type
        /// </summary>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum TypeEnum
        {
            /// <summary>
            /// Enum Pagerduty for value: pagerduty
            /// </summary>
            [EnumMember(Value = "pagerduty")] Pagerduty = 1
        }

        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name = "type", EmitDefaultValue = false)]
        public TypeEnum Type { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagerDutyNotificationRuleBase" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected PagerDutyNotificationRuleBase()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PagerDutyNotificationRuleBase" /> class.
        /// </summary>
        /// <param name="type">type (required) (default to TypeEnum.Pagerduty).</param>
        /// <param name="messageTemplate">messageTemplate (required).</param>
        public PagerDutyNotificationRuleBase(TypeEnum type = TypeEnum.Pagerduty, string messageTemplate = default,
            string endpointID = default, string orgID = default, string taskID = default,
            TaskStatusType status = default, string name = default, string sleepUntil = default, string every = default,
            string offset = default, string runbookLink = default, int? limitEvery = default, int? limit = default,
            List<TagRule> tagRules = default, string description = default, List<StatusRule> statusRules = default,
            List<Label> labels = default, NotificationRuleBaseLinks links = default) : base(endpointID, orgID, taskID,
            status, name, sleepUntil, every, offset, runbookLink, limitEvery, limit, tagRules, description, statusRules,
            labels, links)
        {
            // to ensure "type" is required (not null)
            Type = type;
            // to ensure "messageTemplate" is required (not null)
            if (messageTemplate == null)
            {
                throw new InvalidDataException(
                    "messageTemplate is a required property for PagerDutyNotificationRuleBase and cannot be null");
            }

            MessageTemplate = messageTemplate;
        }


        /// <summary>
        /// Gets or Sets MessageTemplate
        /// </summary>
        [DataMember(Name = "messageTemplate", EmitDefaultValue = false)]
        public string MessageTemplate { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class PagerDutyNotificationRuleBase {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
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
            return Equals(input as PagerDutyNotificationRuleBase);
        }

        /// <summary>
        /// Returns true if PagerDutyNotificationRuleBase instances are equal
        /// </summary>
        /// <param name="input">Instance of PagerDutyNotificationRuleBase to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PagerDutyNotificationRuleBase input)
        {
            if (input == null)
            {
                return false;
            }

            return base.Equals(input) &&
                   (
                       Type == input.Type ||
                       Type.Equals(input.Type)
                   ) && base.Equals(input) &&
                   (
                       MessageTemplate == input.MessageTemplate ||
                       MessageTemplate != null && MessageTemplate.Equals(input.MessageTemplate)
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
                var hashCode = base.GetHashCode();

                hashCode = hashCode * 59 + Type.GetHashCode();
                if (MessageTemplate != null)
                {
                    hashCode = hashCode * 59 + MessageTemplate.GetHashCode();
                }

                return hashCode;
            }
        }
    }
}