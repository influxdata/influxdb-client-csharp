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
    /// Defines WritePrecision
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum WritePrecision
    {
        /// <summary>
        /// Enum Ms for value: ms
        /// </summary>
        [EnumMember(Value = "ms")]
        Ms = 1,

        /// <summary>
        /// Enum S for value: s
        /// </summary>
        [EnumMember(Value = "s")]
        S = 2,

        /// <summary>
        /// Enum Us for value: us
        /// </summary>
        [EnumMember(Value = "us")]
        Us = 3,

        /// <summary>
        /// Enum Ns for value: ns
        /// </summary>
        [EnumMember(Value = "ns")]
        Ns = 4

    }

}
