using System.Runtime.Serialization;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// Source is an external Influx with time series data.
    /// </summary>
    public class Source
    {
        /// <summary>
        /// The unique ID of the source.
        /// </summary>
        [JsonProperty("id")]
        public string Id { get; set; }

        /// <summary>
        /// The organization ID that resource belongs to.
        /// </summary>
        [JsonProperty("orgID")]
        public string OrgId { get; set; }

        /// <summary>
        /// Specifies the default source for the application.
        /// </summary>
        [JsonProperty("default")]
        public bool DefaultSource { get; set; }

        /// <summary>
        /// The user-defined name for the source.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        /// Type specifies which kinds of source (enterprise vs oss vs 2.0). 
        /// </summary>
        [JsonProperty("type"), JsonConverter(typeof(StringEnumConverter))]
        public SourceType Type { get; set; }

        /// <summary>
        /// SourceType is a string for types of sources. 
        /// </summary>
        public enum SourceType
        {
            [EnumMember(Value = "v2")]
            V2SourceType,

            [EnumMember(Value = "v1")]
            V1SourceType,

            [EnumMember(Value = "self")]
            SelfSourceType
        }

        /// <summary>
        /// URL are the connections to the source. 
        /// </summary>
        [JsonProperty("url")]
        public string Url { get; set; }

        /// <summary>
        /// InsecureSkipVerify as true means any certificate presented by the source is accepted. 
        /// </summary>
        [JsonProperty("insecureSkipVerify")]
        public bool InsecureSkipVerify { get; set; }

        /// <summary>
        /// Telegraf is the db telegraf is written to. By default it is "telegraf". 
        /// </summary>
        [JsonProperty("telegraf")]
        public string Telegraf { get; set; }

        /// <summary>
        /// Token is the 2.0 authorization token associated with a source. 
        /// </summary>
        [JsonProperty("token")]
        public string Token { get; set; }

        //
        // V1SourceFields are the fields for connecting to a 1.0 source (oss or enterprise)
        //

        /// <summary>
        /// The username to connect to the source (V1SourceFields).
        /// </summary>
        [JsonProperty("username")]
        public string UserName { get; set; }

        /// <summary>
        /// Password is in CLEARTEXT (V1SourceFields).
        /// </summary>
        [JsonProperty("password")]
        public string Password { get; set; }

        /// <summary>
        /// The optional signing secret for Influx JWT authorization (V1SourceFields).
        /// </summary>
        [JsonProperty("sharedSecret")]
        public string SharedSecret { get; set; }

        /// <summary>
        /// The url for the meta node (V1SourceFields).
        /// </summary>
        [JsonProperty("metaUrl")]
        public string MetaUrl { get; set; }

        /// <summary>
        /// The default retention policy used in database queries to this source (V1SourceFields).
        /// </summary>
        [JsonProperty("defaultRP")]
        public string DefaultRp { get; set; }
    }
}