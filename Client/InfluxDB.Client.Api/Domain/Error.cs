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
    /// Error
    /// </summary>
    [DataContract(Name = "Error")]
    public partial class Error : IEquatable<Error>
    {
        /// <summary>
        /// code is the machine-readable error code.
        /// </summary>
        /// <value>code is the machine-readable error code.</value>
        [JsonConverter(typeof(StringEnumConverter))]
        public enum CodeEnum
        {
            /// <summary>
            /// Enum InternalError for value: internal error
            /// </summary>
            [EnumMember(Value = "internal error")]
            InternalError = 1,

            /// <summary>
            /// Enum NotFound for value: not found
            /// </summary>
            [EnumMember(Value = "not found")]
            NotFound = 2,

            /// <summary>
            /// Enum Conflict for value: conflict
            /// </summary>
            [EnumMember(Value = "conflict")]
            Conflict = 3,

            /// <summary>
            /// Enum Invalid for value: invalid
            /// </summary>
            [EnumMember(Value = "invalid")]
            Invalid = 4,

            /// <summary>
            /// Enum UnprocessableEntity for value: unprocessable entity
            /// </summary>
            [EnumMember(Value = "unprocessable entity")]
            UnprocessableEntity = 5,

            /// <summary>
            /// Enum EmptyValue for value: empty value
            /// </summary>
            [EnumMember(Value = "empty value")]
            EmptyValue = 6,

            /// <summary>
            /// Enum Unavailable for value: unavailable
            /// </summary>
            [EnumMember(Value = "unavailable")]
            Unavailable = 7,

            /// <summary>
            /// Enum Forbidden for value: forbidden
            /// </summary>
            [EnumMember(Value = "forbidden")]
            Forbidden = 8,

            /// <summary>
            /// Enum TooManyRequests for value: too many requests
            /// </summary>
            [EnumMember(Value = "too many requests")]
            TooManyRequests = 9,

            /// <summary>
            /// Enum Unauthorized for value: unauthorized
            /// </summary>
            [EnumMember(Value = "unauthorized")]
            Unauthorized = 10,

            /// <summary>
            /// Enum MethodNotAllowed for value: method not allowed
            /// </summary>
            [EnumMember(Value = "method not allowed")]
            MethodNotAllowed = 11,

            /// <summary>
            /// Enum RequestTooLarge for value: request too large
            /// </summary>
            [EnumMember(Value = "request too large")]
            RequestTooLarge = 12,

            /// <summary>
            /// Enum UnsupportedMediaType for value: unsupported media type
            /// </summary>
            [EnumMember(Value = "unsupported media type")]
            UnsupportedMediaType = 13

        }


        /// <summary>
        /// code is the machine-readable error code.
        /// </summary>
        /// <value>code is the machine-readable error code.</value>
        [DataMember(Name = "code", IsRequired = true, EmitDefaultValue = false)]
        public CodeEnum Code { get; set; }

        /// <summary>
        /// Returns false as Code should not be serialized given that it's read-only.
        /// </summary>
        /// <returns>false (boolean)</returns>
        public bool ShouldSerializeCode()
        {
            return false;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Error" /> class.
        /// </summary>
        [JsonConstructorAttribute]
        public Error()
        {
        }

        /// <summary>
        /// message is a human-readable message.
        /// </summary>
        /// <value>message is a human-readable message.</value>
        [DataMember(Name = "message", IsRequired = true, EmitDefaultValue = false)]
        public string Message { get; private set; }

        /// <summary>
        /// Returns false as Message should not be serialized given that it's read-only.
        /// </summary>
        /// <returns>false (boolean)</returns>
        public bool ShouldSerializeMessage()
        {
            return false;
        }
        /// <summary>
        /// op describes the logical code operation during error. Useful for debugging.
        /// </summary>
        /// <value>op describes the logical code operation during error. Useful for debugging.</value>
        [DataMember(Name = "op", EmitDefaultValue = false)]
        public string Op { get; private set; }

        /// <summary>
        /// Returns false as Op should not be serialized given that it's read-only.
        /// </summary>
        /// <returns>false (boolean)</returns>
        public bool ShouldSerializeOp()
        {
            return false;
        }
        /// <summary>
        /// err is a stack of errors that occurred during processing of the request. Useful for debugging.
        /// </summary>
        /// <value>err is a stack of errors that occurred during processing of the request. Useful for debugging.</value>
        [DataMember(Name = "err", EmitDefaultValue = false)]
        public string Err { get; private set; }

        /// <summary>
        /// Returns false as Err should not be serialized given that it's read-only.
        /// </summary>
        /// <returns>false (boolean)</returns>
        public bool ShouldSerializeErr()
        {
            return false;
        }
        /// <summary>
        /// Returns the string presentation of the object
        /// </summary>
        /// <returns>String presentation of the object</returns>
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Error {\n");
            sb.Append("  Code: ").Append(Code).Append("\n");
            sb.Append("  Message: ").Append(Message).Append("\n");
            sb.Append("  Op: ").Append(Op).Append("\n");
            sb.Append("  Err: ").Append(Err).Append("\n");
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
            return this.Equals(input as Error);
        }

        /// <summary>
        /// Returns true if Error instances are equal
        /// </summary>
        /// <param name="input">Instance of Error to be compared</param>
        /// <returns>Boolean</returns>
        public bool Equals(Error input)
        {
            if (input == null)
                return false;

            return 
                (
                    this.Code == input.Code ||
                    this.Code.Equals(input.Code)
                ) && 
                (
                    this.Message == input.Message ||
                    (this.Message != null &&
                    this.Message.Equals(input.Message))
                ) && 
                (
                    this.Op == input.Op ||
                    (this.Op != null &&
                    this.Op.Equals(input.Op))
                ) && 
                (
                    this.Err == input.Err ||
                    (this.Err != null &&
                    this.Err.Equals(input.Err))
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
                hashCode = hashCode * 59 + this.Code.GetHashCode();
                if (this.Message != null)
                    hashCode = hashCode * 59 + this.Message.GetHashCode();
                if (this.Op != null)
                    hashCode = hashCode * 59 + this.Op.GetHashCode();
                if (this.Err != null)
                    hashCode = hashCode * 59 + this.Err.GetHashCode();
                return hashCode;
            }
        }

    }

}
