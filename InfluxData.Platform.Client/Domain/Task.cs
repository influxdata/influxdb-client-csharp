using System.Text;

namespace InfluxData.Platform.Client.Domain
{
    /// <summary>
    /// Task is a task.
    /// </summary>
    public class Task
    {
        public string Id { get; set; }

        /// <summary>
        /// A read-only description of the task.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The <see cref="User"/> that owns this Task.
        /// </summary>
        public User Owner { get; set; }

        /// <summary>
        /// The ID of the organization that owns this Task.
        /// </summary>
        public string OrganizationId { get; set; }

        /// <summary>
        /// The current status of the task. When updated to 'disabled', cancels all queued jobs of this task.
        /// </summary>
        public Status Status { get; set; }

        /// <summary>
        /// The Flux script to run for this task.
        /// </summary>
        public string Flux { get; set; }

        /// <summary>
        /// A simple task repetition schedule (duration type); parsed from Flux.
        /// </summary>
        public string Every { get; set; }

        /// <summary>
        /// A task repetition schedule in the form '* * * * * *'; parsed from Flux.
        /// </summary>
        public string Cron { get; set; }

        /// <summary>
        /// Duration to delay after the schedule, before executing the task; parsed from flux.
        /// </summary>
        public string Delay { get; set; }

        public override string ToString()
        {
            return new StringBuilder(GetType().Name + "[")
                .Append("id='" + Id + "'")
                .Append(", name='" + Name + "'")
                .Append(", owner=" + Owner)
                .Append(", organizationId='" + OrganizationId + "'")
                .Append(", status=" + Status)
                .Append(", flux='" + Flux + "'")
                .Append(", every='" + Every + "'")
                .Append(", cron='" + Cron + "'")
                .Append(", delay='" + Delay + "'")
                .Append("]").ToString();
        }
    }
}