using System.Text;
using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// Organization
    /// </summary>
    public class Organization : AbstractHasLinks
    {
        [JsonProperty("id")]
        public string Id { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        public override string ToString()
        {
            return new StringBuilder(GetType().Name + "[")
                            .Append("id='" + Id + "'")
                            .Append(", name='" + Name + "'")
                            .Append(", links=" + LinksToString())
                            .Append("]").ToString();
        }
    }
}