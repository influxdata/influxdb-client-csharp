using System.Runtime.Serialization;
using System.Text;

namespace InfluxData.Platform.Client.Domain
{
/**
 * Task
 */
    public class Task
    {
        public string Id { get; set; }

        /**
         * A read-only description of the task.
         */
        public string Name { get; set; }

        /**
         * The {@link User} that owns this Task.
         */
        public User Owner { get; set; }

        /**
         * The ID of the organization that owns this Task.
         */
        public string OrganizationId { get; set; }

        /**
         * The current status of the task. When updated to 'disabled', cancels all queued jobs of this task.
         */
        public Status Status { get; set; }

        /**
         * The Flux script to run for this task.
         */
        public string Flux { get; set; }

        /**
         * A simple task repetition schedule (duration type); parsed from Flux.
         */
        public string Every { get; set; }

        /**
         * A task repetition schedule in the form '* * * * * *'; parsed from Flux.
         */
        public string Cron { get; set; }

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
                            .Append("]").ToString();
        }
    }
}