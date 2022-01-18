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
    /// CellWithViewProperties
    /// </summary>
    [DataContract(Name = "CellWithViewProperties")]
    public partial class CellWithViewProperties : IEquatable<CellWithViewProperties>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CellWithViewProperties" /> class.
        /// </summary>
        /// <param name="id">id.</param>
        /// <param name="links">links.</param>
        /// <param name="x">x.</param>
        /// <param name="y">y.</param>
        /// <param name="w">w.</param>
        /// <param name="h">h.</param>
        /// <param name="viewID">The reference to a view from the views API..</param>
        /// <param name="name">name.</param>
        /// <param name="properties">properties.</param>
        public CellWithViewProperties(string id = default(string), CellLinks links = default(CellLinks), int x = default(int), int y = default(int), int w = default(int), int h = default(int), string viewID = default(string), string name = default(string), ViewProperties properties = default(ViewProperties))
        {
            this.Id = id;
            this.Links = links;
            this.X = x;
            this.Y = y;
            this.W = w;
            this.H = h;
            this.ViewID = viewID;
            this.Name = name;
            this.Properties = properties;
        }

        /// <summary>
        /// Gets or Sets Id
        /// </summary>
        [DataMember(Name = "id", EmitDefaultValue = false)]
        public string Id { get; set; }

        /// <summary>
        /// Gets or Sets Links
        /// </summary>
        [DataMember(Name = "links", EmitDefaultValue = false)]
        public CellLinks Links { get; set; }

        /// <summary>
        /// Gets or Sets X
        /// </summary>
        [DataMember(Name = "x", EmitDefaultValue = false)]
        public int X { get; set; }

        /// <summary>
        /// Gets or Sets Y
        /// </summary>
        [DataMember(Name = "y", EmitDefaultValue = false)]
        public int Y { get; set; }

        /// <summary>
        /// Gets or Sets W
        /// </summary>
        [DataMember(Name = "w", EmitDefaultValue = false)]
        public int W { get; set; }

        /// <summary>
        /// Gets or Sets H
        /// </summary>
        [DataMember(Name = "h", EmitDefaultValue = false)]
        public int H { get; set; }

        /// <summary>
        /// The reference to a view from the views API.
        /// </summary>
        /// <value>The reference to a view from the views API.</value>
        [DataMember(Name = "viewID", EmitDefaultValue = false)]
        public string ViewID { get; set; }

        /// <summary>
        /// Gets or Sets Name
        /// </summary>
        [DataMember(Name = "name", EmitDefaultValue = false)]
        public string Name { get; set; }

        /// <summary>
        /// Gets or Sets Properties
        /// </summary>
        [DataMember(Name = "properties", EmitDefaultValue = false)]
        public ViewProperties Properties { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class CellWithViewProperties {\n");
            sb.Append("  Id: ").Append(Id).Append("\n");
            sb.Append("  Links: ").Append(Links).Append("\n");
            sb.Append("  X: ").Append(X).Append("\n");
            sb.Append("  Y: ").Append(Y).Append("\n");
            sb.Append("  W: ").Append(W).Append("\n");
            sb.Append("  H: ").Append(H).Append("\n");
            sb.Append("  ViewID: ").Append(ViewID).Append("\n");
            sb.Append("  Name: ").Append(Name).Append("\n");
            sb.Append("  Properties: ").Append(Properties).Append("\n");
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
            return this.Equals(input as CellWithViewProperties);
        }

        /// <summary>
        /// Returns true if CellWithViewProperties instances are equal
        /// </summary>
        /// <param name="input">Instance of CellWithViewProperties to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(CellWithViewProperties input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Id == input.Id ||
                    (this.Id != null &&
                    this.Id.Equals(input.Id))
                ) && 
                (
                    this.Links == input.Links ||
                    (this.Links != null &&
                    this.Links.Equals(input.Links))
                ) && 
                (
                    this.X == input.X ||
                    this.X.Equals(input.X)
                ) && 
                (
                    this.Y == input.Y ||
                    this.Y.Equals(input.Y)
                ) && 
                (
                    this.W == input.W ||
                    this.W.Equals(input.W)
                ) && 
                (
                    this.H == input.H ||
                    this.H.Equals(input.H)
                ) && 
                (
                    this.ViewID == input.ViewID ||
                    (this.ViewID != null &&
                    this.ViewID.Equals(input.ViewID))
                ) && 
                (
                    this.Name == input.Name ||
                    (this.Name != null &&
                    this.Name.Equals(input.Name))
                ) && 
                (
                    this.Properties == input.Properties ||
                    (this.Properties != null &&
                    this.Properties.Equals(input.Properties))
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
                if (this.Id != null)
                    hashCode = hashCode * 59 + this.Id.GetHashCode();
                if (this.Links != null)
                    hashCode = hashCode * 59 + this.Links.GetHashCode();
                hashCode = hashCode * 59 + this.X.GetHashCode();
                hashCode = hashCode * 59 + this.Y.GetHashCode();
                hashCode = hashCode * 59 + this.W.GetHashCode();
                hashCode = hashCode * 59 + this.H.GetHashCode();
                if (this.ViewID != null)
                    hashCode = hashCode * 59 + this.ViewID.GetHashCode();
                if (this.Name != null)
                    hashCode = hashCode * 59 + this.Name.GetHashCode();
                if (this.Properties != null)
                    hashCode = hashCode * 59 + this.Properties.GetHashCode();
                return hashCode;
            }
        }

    }

}
