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
    /// Tasks
    /// </summary>
    [DataContract]
    public partial class Tasks :  IEquatable<Tasks>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Tasks" /> class.
        /// </summary>
        /// <param name="links">links.</param>
        /// <param name="tasks">tasks.</param>
        public Tasks(Links links = default(Links), List<TaskType> tasks = default(List<TaskType>))
        {
            this.Links = links;
            this._Tasks = tasks;
        }

        /// <summary>
        /// Gets or Sets Links
        /// </summary>
        [DataMember(Name="links", EmitDefaultValue=false)]
        public Links Links { get; set; }

        /// <summary>
        /// Gets or Sets _Tasks
        /// </summary>
        [DataMember(Name="tasks", EmitDefaultValue=false)]
        public List<TaskType> _Tasks { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Tasks {\n");
            sb.Append("  Links: ").Append(Links).Append("\n");
            sb.Append("  _Tasks: ").Append(_Tasks).Append("\n");
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
            return this.Equals(input as Tasks);
        }

        /// <summary>
        /// Returns true if Tasks instances are equal
        /// </summary>
        /// <param name="input">Instance of Tasks to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Tasks input)
        {
            if (input == null)
                return false;

            return 
                (
                    
                    (this.Links != null && this.Links.Equals(input.Links))
                ) && 
                (
                    this._Tasks == input._Tasks ||
                    this._Tasks != null &&
                    this._Tasks.SequenceEqual(input._Tasks)
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
                
                if (this.Links != null)
                    hashCode = hashCode * 59 + this.Links.GetHashCode();
                if (this._Tasks != null)
                    hashCode = hashCode * 59 + this._Tasks.GetHashCode();
                return hashCode;
            }
        }

    }

}
