/*
 * InfluxDB OSS API Service
 *
 * The InfluxDB v2 API provides a programmatic interface for all interactions with InfluxDB. Access the InfluxDB API using the `/api/v2/` endpoint. 
 *
 * The version of the OpenAPI document: 2.0.0
 * Generated by: https://github.com/openapitools/openapi-generator.git
 */


using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using FileParameter = InfluxDB.Client.Core.Api.FileParameter;
using OpenAPIDateConverter = InfluxDB.Client.Core.Api.OpenAPIDateConverter;

namespace InfluxDB.Client.Api.Domain
{
    /// <summary>
    /// SlackNotificationEndpoint
    /// </summary>
    [DataContract(Name = "SlackNotificationEndpoint")]
    public partial class SlackNotificationEndpoint : NotificationEndpoint, IEquatable<SlackNotificationEndpoint>
    {

        /// <summary>
        /// Gets or Sets Type
        /// </summary>
        [DataMember(Name = "type", IsRequired = true, EmitDefaultValue = false)]
        public NotificationEndpointType Type { get; set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="SlackNotificationEndpoint" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected SlackNotificationEndpoint() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="SlackNotificationEndpoint" /> class.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="orgID">orgID.</param>
        /// <param name="userID">userID.</param>
        /// <param name="description">An optional description of the notification endpoint..</param>
        /// <param name="name">name (required).</param>
        /// <param name="status">The status of the endpoint. (default to StatusEnum.Active).</param>
        /// <param name="labels">labels.</param>
        /// <param name="links">links.</param>
        /// <param name="type">type (required).</param>
        /// <param name="url">Specifies the URL of the Slack endpoint. Specify either &#x60;URL&#x60; or &#x60;Token&#x60;..</param>
        /// <param name="token">Specifies the API token string. Specify either &#x60;URL&#x60; or &#x60;Token&#x60;..</param>
        public SlackNotificationEndpoint(string id = default(string), string orgID = default(string), string userID = default(string), string description = default(string), string name = default(string), StatusEnum? status = StatusEnum.Active, List<Label> labels = default(List<Label>), NotificationEndpointBaseLinks links = default(NotificationEndpointBaseLinks), NotificationEndpointType type = default(NotificationEndpointType), string url = default(string), string token = default(string)) : base(id, orgID, userID, description, name, status, labels, links)
        {
            this.Type = type;
            this.Url = url;
            this.Token = token;
        }

        /// <summary>
        /// Specifies the URL of the Slack endpoint. Specify either &#x60;URL&#x60; or &#x60;Token&#x60;.
        /// </summary>
        /// <value>Specifies the URL of the Slack endpoint. Specify either &#x60;URL&#x60; or &#x60;Token&#x60;.</value>
        [DataMember(Name = "url", EmitDefaultValue = false)]
        public string Url { get; set; }

        /// <summary>
        /// Specifies the API token string. Specify either &#x60;URL&#x60; or &#x60;Token&#x60;.
        /// </summary>
        /// <value>Specifies the API token string. Specify either &#x60;URL&#x60; or &#x60;Token&#x60;.</value>
        [DataMember(Name = "token", EmitDefaultValue = false)]
        public string Token { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class SlackNotificationEndpoint {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
            sb.Append("  Type: ").Append(Type).Append("\n");
            sb.Append("  Url: ").Append(Url).Append("\n");
            sb.Append("  Token: ").Append(Token).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public override string ToJson()
        {
            return Newtonsoft.Json.JsonConvert.SerializeObject(this, Newtonsoft.Json.Formatting.Indented);
        }

        /// <summary>
        /// Returns true if objects are equal
        /// </summary>
        /// <param name="input">Object to be compared</param>
        /// <returns>Boolean</returns>
        public override bool Equals(object input)
        {
            return this.Equals(input as SlackNotificationEndpoint);
        }

        /// <summary>
        /// Returns true if SlackNotificationEndpoint instances are equal
        /// </summary>
        /// <param name="input">Instance of SlackNotificationEndpoint to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(SlackNotificationEndpoint input)
        {
            if (input == null)
                return false;

            return base.Equals(input) && 
                (
                    this.Type == input.Type ||
                    this.Type.Equals(input.Type)
                ) && base.Equals(input) && 
                (
                    this.Url == input.Url ||
                    (this.Url != null &&
                    this.Url.Equals(input.Url))
                ) && base.Equals(input) && 
                (
                    this.Token == input.Token ||
                    (this.Token != null &&
                    this.Token.Equals(input.Token))
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
                if (this.Url != null)
                    hashCode = hashCode * 59 + this.Url.GetHashCode();
                if (this.Token != null)
                    hashCode = hashCode * 59 + this.Token.GetHashCode();
                return hashCode;
            }
        }

    }

}
