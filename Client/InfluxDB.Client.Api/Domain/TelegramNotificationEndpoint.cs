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
    /// TelegramNotificationEndpoint
    /// </summary>
    [DataContract]
    public partial class TelegramNotificationEndpoint : NotificationEndpoint,  IEquatable<TelegramNotificationEndpoint>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="TelegramNotificationEndpoint" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected TelegramNotificationEndpoint() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="TelegramNotificationEndpoint" /> class.
        /// </summary>
        /// <param name="token">Specifies the Telegram bot token. See https://core.telegram.org/bots#creating-a-new-bot . (required).</param>
        /// <param name="channel">ID of the telegram channel, a chat_id in https://core.telegram.org/bots/api#sendmessage . (required).</param>
        public TelegramNotificationEndpoint(string token = default(string), string channel = default(string), string id = default(string), string orgID = default(string), string userID = default(string), string description = default(string), string name = default(string), StatusEnum? status = StatusEnum.Active, List<Label> labels = default(List<Label>), NotificationEndpointBaseLinks links = default(NotificationEndpointBaseLinks), NotificationEndpointType type = NotificationEndpointType.Telegram) : base(id, orgID, userID, description, name, status, labels, links, type)
        {
            // to ensure "token" is required (not null)
            if (token == null)
            {
                throw new InvalidDataException("token is a required property for TelegramNotificationEndpoint and cannot be null");
            }
            this.Token = token;
            // to ensure "channel" is required (not null)
            if (channel == null)
            {
                throw new InvalidDataException("channel is a required property for TelegramNotificationEndpoint and cannot be null");
            }
            this.Channel = channel;
        }

        /// <summary>
        /// Specifies the Telegram bot token. See https://core.telegram.org/bots#creating-a-new-bot .
        /// </summary>
        /// <value>Specifies the Telegram bot token. See https://core.telegram.org/bots#creating-a-new-bot .</value>
        [DataMember(Name="token", EmitDefaultValue=false)]
        public string Token { get; set; }

        /// <summary>
        /// ID of the telegram channel, a chat_id in https://core.telegram.org/bots/api#sendmessage .
        /// </summary>
        /// <value>ID of the telegram channel, a chat_id in https://core.telegram.org/bots/api#sendmessage .</value>
        [DataMember(Name="channel", EmitDefaultValue=false)]
        public string Channel { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class TelegramNotificationEndpoint {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  Token: ").Append(Token).Append("\n");
            sb.Append("  Channel: ").Append(Channel).Append("\n");
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
            return this.Equals(input as TelegramNotificationEndpoint);
        }

        /// <summary>
        /// Returns true if TelegramNotificationEndpoint instances are equal
        /// </summary>
        /// <param name="input">Instance of TelegramNotificationEndpoint to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(TelegramNotificationEndpoint input)
        {
            if (input == null)
                return false;

            return base.Equals(input) && 
                (
                    this.Token == input.Token ||
                    (this.Token != null && this.Token.Equals(input.Token))
                ) && base.Equals(input) && 
                (
                    this.Channel == input.Channel ||
                    (this.Channel != null && this.Channel.Equals(input.Channel))
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
                
                if (this.Token != null)
                    hashCode = hashCode * 59 + this.Token.GetHashCode();
                if (this.Channel != null)
                    hashCode = hashCode * 59 + this.Channel.GetHashCode();
                return hashCode;
            }
        }

    }

}
