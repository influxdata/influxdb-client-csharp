using System;
using NodaTime;

namespace Platform.Client.Domain
{
    public class Run
    {
        public string Id { get; set; }

        public string TaskId { get; set; }

        public RunStatus Status { get; set; }

        public Instant ScheduledFor { get; set; }
        public Instant StartedAt { get; set; }
        public Instant FinishedAt { get; set; }
        public Instant RequestedAt { get; set; }

        public string Log { get; set; }
    }
}