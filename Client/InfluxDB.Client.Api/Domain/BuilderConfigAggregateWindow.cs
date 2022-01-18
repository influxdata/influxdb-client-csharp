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
    /// BuilderConfigAggregateWindow
    /// </summary>
    [DataContract(Name = "BuilderConfig_aggregateWindow")]
    public partial class BuilderConfigAggregateWindow : IEquatable<BuilderConfigAggregateWindow>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BuilderConfigAggregateWindow" /> class.
        /// </summary>
        /// <param name="period">period.</param>
        /// <param name="fillValues">fillValues.</param>
        public BuilderConfigAggregateWindow(string period = default(string), bool fillValues = default(bool))
        {
            this.Period = period;
            this.FillValues = fillValues;
        }

        /// <summary>
        /// Gets or Sets Period
        /// </summary>
        [DataMember(Name = "period", EmitDefaultValue = false)]
        public string Period { get; set; }

        /// <summary>
        /// Gets or Sets FillValues
        /// </summary>
        [DataMember(Name = "fillValues", EmitDefaultValue = true)]
        public bool FillValues { get; set; }

        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class BuilderConfigAggregateWindow {\n");
            sb.Append("  Period: ").Append(Period).Append("\n");
            sb.Append("  FillValues: ").Append(FillValues).Append("\n");
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
            return this.Equals(input as BuilderConfigAggregateWindow);
        }

        /// <summary>
        /// Returns true if BuilderConfigAggregateWindow instances are equal
        /// </summary>
        /// <param name="input">Instance of BuilderConfigAggregateWindow to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(BuilderConfigAggregateWindow input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Period == input.Period ||
                    (this.Period != null &&
                    this.Period.Equals(input.Period))
                ) && 
                (
                    this.FillValues == input.FillValues ||
                    this.FillValues.Equals(input.FillValues)
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
                if (this.Period != null)
                    hashCode = hashCode * 59 + this.Period.GetHashCode();
                hashCode = hashCode * 59 + this.FillValues.GetHashCode();
                return hashCode;
            }
        }

    }

}
