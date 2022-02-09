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
    /// Telegrafs
    /// </summary>
    [DataContract]
    public partial class Telegrafs :  IEquatable<Telegrafs>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Telegrafs" /> class.
        /// </summary>
        /// <param name="configurations">configurations.</param>
        public Telegrafs(List<Telegraf> configurations = default(List<Telegraf>))
        {
            this.Configurations = configurations;
        }

        /// <summary>
        /// Gets or Sets Configurations
        /// </summary>
        [DataMember(Name="configurations", EmitDefaultValue=false)]
        public List<Telegraf> Configurations { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Telegrafs {\n");
            sb.Append("  Configurations: ").Append(Configurations).Append("\n");
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
            return this.Equals(input as Telegrafs);
        }

        /// <summary>
        /// Returns true if Telegrafs instances are equal
        /// </summary>
        /// <param name="input">Instance of Telegrafs to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Telegrafs input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Configurations == input.Configurations ||
                    this.Configurations != null &&
                    this.Configurations.SequenceEqual(input.Configurations)
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
                
                if (this.Configurations != null)
                    hashCode = hashCode * 59 + this.Configurations.GetHashCode();
                return hashCode;
            }
        }

    }

}
