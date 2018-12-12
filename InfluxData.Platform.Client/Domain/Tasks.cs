using System.Collections.Generic;
using Newtonsoft.Json;

namespace InfluxData.Platform.Client.Domain
{
    public class Tasks
    {
        [JsonProperty("tasks")]
        public List<Task> TaskList { get; set; } = new List<Task>();
    }
}