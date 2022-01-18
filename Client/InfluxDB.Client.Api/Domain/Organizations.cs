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
    /// Organizations
    /// </summary>
    [DataContract(Name = "Organizations")]
    public partial class Organizations : IEquatable<Organizations>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Organizations" /> class.
        /// </summary>
        /// <param name="links">links.</param>
        /// <param name="orgs">orgs.</param>
        public Organizations(Links links = default(Links), List<Organization> orgs = default(List<Organization>))
        {
            this.Links = links;
            this.Orgs = orgs;
        }

        /// <summary>
        /// Gets or Sets Links
        /// </summary>
        [DataMember(Name = "links", EmitDefaultValue = false)]
        public Links Links { get; set; }

        /// <summary>
        /// Gets or Sets Orgs
        /// </summary>
        [DataMember(Name = "orgs", EmitDefaultValue = false)]
        public List<Organization> Orgs { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Organizations {\n");
            sb.Append("  Links: ").Append(Links).Append("\n");
            sb.Append("  Orgs: ").Append(Orgs).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

        /// <summary>
        /// Returns the JSON string presentation of the object
        /// </summary>
        /// <returns>JSON string presentation of the object</returns>
        public virtual string ToJson()
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
            return this.Equals(input as Organizations);
        }

        /// <summary>
        /// Returns true if Organizations instances are equal
        /// </summary>
        /// <param name="input">Instance of Organizations to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Organizations input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Links == input.Links ||
                    (this.Links != null &&
                    this.Links.Equals(input.Links))
                ) && 
                (
                    this.Orgs == input.Orgs ||
                    this.Orgs != null &&
                    input.Orgs != null &&
                    this.Orgs.SequenceEqual(input.Orgs)
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
                if (this.Orgs != null)
                    hashCode = hashCode * 59 + this.Orgs.GetHashCode();
                return hashCode;
            }
        }

    }

}
