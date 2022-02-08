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
    /// Defines XYGeom
    /// </summary>
    
    [JsonConverter(typeof(StringEnumConverter))]
    
    public enum XYGeom
    {
        /// <summary>
        /// Enum Line for value: line
        /// </summary>
        [EnumMember(Value = "line")]
        Line = 1,

        /// <summary>
        /// Enum Step for value: step
        /// </summary>
        [EnumMember(Value = "step")]
        Step = 2,

        /// <summary>
        /// Enum Stacked for value: stacked
        /// </summary>
        [EnumMember(Value = "stacked")]
        Stacked = 3,

        /// <summary>
        /// Enum Bar for value: bar
        /// </summary>
        [EnumMember(Value = "bar")]
        Bar = 4,

        /// <summary>
        /// Enum MonotoneX for value: monotoneX
        /// </summary>
        [EnumMember(Value = "monotoneX")]
        MonotoneX = 5

    }

}
