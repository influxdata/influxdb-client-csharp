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
using JsonSubTypes;
using FileParameter = InfluxDB.Client.Core.Api.FileParameter;
using OpenAPIDateConverter = InfluxDB.Client.Core.Api.OpenAPIDateConverter;

namespace InfluxDB.Client.Api.Domain
{
    /// <summary>
    /// CheckDiscriminator
    /// </summary>
    [DataContract(Name = "CheckDiscriminator")]
    [JsonConverter(typeof(JsonSubtypes), "Type")]
    [JsonSubtypes.KnownSubType(typeof(CustomCheck), "custom")]
    [JsonSubtypes.KnownSubType(typeof(ThresholdCheck), "threshold")]
    [JsonSubtypes.KnownSubType(typeof(DeadmanCheck), "deadman")]
    public partial class CheckDiscriminator : CheckBase, IEquatable<CheckDiscriminator>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckDiscriminator" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        protected CheckDiscriminator() { }
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckDiscriminator" /> class.
        /// </summary>
        /// <param name="name">name (required).</param>
        /// <param name="orgID">The ID of the organization that owns this check. (required).</param>
        /// <param name="taskID">The ID of the task associated with this check..</param>
        /// <param name="query">query (required).</param>
        /// <param name="status">status.</param>
        /// <param name="description">An optional description of the check..</param>
        /// <param name="labels">labels.</param>
        /// <param name="links">links.</param>
        public CheckDiscriminator(string name = default(string), string orgID = default(string), string taskID = default(string), DashboardQuery query = default(DashboardQuery), TaskStatusType? status = default(TaskStatusType?), string description = default(string), List<Label> labels = default(List<Label>), CheckBaseLinks links = default(CheckBaseLinks)) : base(name, orgID, taskID, query, status, description, labels, links)
        {
        }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class CheckDiscriminator {\n");
            sb.Append("  ").Append(base.ToString().Replace("\n", "\n  ")).Append("\n");
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
            return this.Equals(input as CheckDiscriminator);
        }

        /// <summary>
        /// Returns true if CheckDiscriminator instances are equal
        /// </summary>
        /// <param name="input">Instance of CheckDiscriminator to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(CheckDiscriminator input)
        {
            if (input == null)
                return false;

            return base.Equals(input);
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
                return hashCode;
            }
        }

    }

}
