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
    /// PostStackRequest
    /// </summary>
    [DataContract]
    public partial class PostStackRequest :  IEquatable<PostStackRequest>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PostStackRequest" /> class.
        /// </summary>
        /// <param name="orgID">orgID.</param>
        /// <param name="name">name.</param>
        /// <param name="description">description.</param>
        /// <param name="urls">urls.</param>
        public PostStackRequest(string orgID = default(string), string name = default(string), string description = default(string), List<string> urls = default(List<string>))
        {
            this.OrgID = orgID;
            this.Name = name;
            this.Description = description;
            this.Urls = urls;
        }

        /// <summary>
        /// Gets or Sets OrgID
        /// </summary>
        [DataMember(Name="orgID", EmitDefaultValue=false)]
        public string OrgID { get; set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name="name", EmitDefaultValue=false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets Description
        /// </summary>
        [DataMember(Name="description", EmitDefaultValue=false)]
        public string Description { get; set; }

        /// <summary>
        /// Gets or Sets Urls
        /// </summary>
        [DataMember(Name="urls", EmitDefaultValue=false)]
        public List<string> Urls { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class PostStackRequest {\n");
            sb.Append("  OrgID: ").Append(OrgID).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Description: ").Append(Description).Append("\n");
            sb.Append("  Urls: ").Append(Urls).Append("\n");
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
            return this.Equals(input as PostStackRequest);
        }

        /// <summary>
        /// Returns true if PostStackRequest instances are equal
        /// </summary>
        /// <param name="input">Instance of PostStackRequest to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(PostStackRequest input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.OrgID == input.OrgID ||
                    (this.OrgID != null && this.OrgID.Equals(input.OrgID))
                ) && 
                (
                    this.Name == input.Name ||
                    (this.Name != null && this.Name.Equals(input.Name))
                ) && 
                (
                    this.Description == input.Description ||
                    (this.Description != null && this.Description.Equals(input.Description))
                ) && 
                (
                    this.Urls == input.Urls ||
                    this.Urls != null &&
                    this.Urls.SequenceEqual(input.Urls)
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
                
                if (this.OrgID != null)
                    hashCode = hashCode * 59 + this.OrgID.GetHashCode();
                if (this.Name != null)
                    hashCode = hashCode * 59 + this.Name.GetHashCode();
                if (this.Description != null)
                    hashCode = hashCode * 59 + this.Description.GetHashCode();
                if (this.Urls != null)
                    hashCode = hashCode * 59 + this.Urls.GetHashCode();
                return hashCode;
            }
        }

    }

}
