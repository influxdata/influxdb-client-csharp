using System;
using System.Text;
using InfluxDB.Client.Core.Internal;
using Newtonsoft.Json;

namespace InfluxDB.Client.Domain
{
    /// <summary>
    ///     Task is a task.
    /// </summary>
    public class Task
    {
        /// <summary>
        ///     Timestamp of latest scheduled, completed run, RFC3339.
        /// </summary>
        [JsonProperty("latestCompleted")] public DateTime LatestCompleted;

        [JsonProperty("id")] public string Id { get; set; }

        /// <summary>
        ///     A read-only description of the task.
        /// </summary>
        [JsonProperty("name")]
        public string Name { get; set; }

        /// <summary>
        ///     The ID of the organization that owns this Task.
        /// </summary>
        [JsonProperty("orgID")]
        public string OrgId { get; set; }

        /// <summary>
        ///     The current status of the task. When updated to 'disabled', cancels all queued jobs of this task.
        /// </summary>
        [JsonProperty("status")]
        [JsonConverter(typeof(EnumConverter))]
        public Status? Status { get; set; }

        /// <summary>
        ///     The Flux script to run for this task.
        /// </summary>
        [JsonProperty("flux")]
        public string Flux { get; set; }

        /// <summary>
        ///     A simple task repetition schedule (duration type); parsed from Flux.
        /// </summary>
        [JsonProperty("every")]
        public string Every { get; set; }

        /// <summary>
        ///     A task repetition schedule in the form '* * * * * *'; parsed from Flux.
        /// </summary>
        [JsonProperty("cron")]
        public string Cron { get; set; }

        /// <summary>
        ///     Duration to delay after the schedule, before executing the task; parsed from flux.
        /// </summary>
        [JsonProperty("offset")]
        public string Offset { get; set; }

        [JsonProperty("createdAt")] public DateTime CreatedAt { get; set; }

        [JsonProperty("updatedAt")] public DateTime UpdatedAt { get; set; }

        public override string ToString()
        {
            return new StringBuilder(GetType().Name + "[")
                .Append("id='" + Id + "'")
                .Append(", name='" + Name + "'")
                .Append(", organizationId='" + OrgId + "'")
                .Append(", status=" + Status)
                .Append(", flux='" + Flux + "'")
                .Append(", every='" + Every + "'")
                .Append(", cron='" + Cron + "'")
                .Append(", delay='" + Offset + "'")
                .Append(", createdAt='" + CreatedAt + "'")
                .Append(", updatedAt='" + UpdatedAt + "'")
                .Append(", LatestCompleted='" + LatestCompleted + "'")
                .Append("]").ToString();
        }
    }
}