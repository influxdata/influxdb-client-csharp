using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// Label is a tag set on a resource, typically used for filtering on a UI.
    /// </summary>
    public class Label
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }
        
        [JsonProperty("properties")]
        public Dictionary<string, string> Properties { get; set; } = new Dictionary<string, string>();
    }
}