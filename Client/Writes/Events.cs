using System;
using System.Diagnostics;

namespace InfluxDB.Client.Writes
{
    public abstract class InfluxDBEventArgs : EventArgs
    {
        internal abstract void LogEvent();
    }

    public class WriteSuccessEvent : AbstractWriteEvent
    {
        public WriteSuccessEvent(string organization, string bucket, TimeUnit precision, string lineProtocol) :
            base(organization, bucket, precision, lineProtocol)
        {
        }

        internal override void LogEvent()
        {
            Trace.WriteLine("The data was successfully written to InfluxDB 2.0.");
        }
    }

    public class WriteErrorEvent : AbstractWriteEvent
    {
        /// <summary>
        /// The exception that was throw.
        /// </summary>
        public Exception Exception { get; }

        public WriteErrorEvent(string organization, string bucket, TimeUnit precision, string lineProtocol, Exception exception) :
            base(organization, bucket, precision, lineProtocol)
        {
            Exception = exception;
        }

        internal override void LogEvent()
        {
            Trace.TraceError($"The error occurred during writing of data: {Exception.Message}");
        }
    }

    public abstract class AbstractWriteEvent : InfluxDBEventArgs
    {
        /// <summary>
        /// The organization that was used for write data.
        /// </summary>
        public string Organization { get; }

        /// <summary>
        /// The bucket that was used for write data.
        /// </summary>
        public string Bucket { get; }

        /// <summary>
        /// The Precision that was used for write data.
        /// </summary>
        public TimeUnit Precision { get; }

        /// <summary>
        /// The Data that was written.
        /// </summary>
        public string LineProtocol { get; }

        internal AbstractWriteEvent(string organization, string bucket, TimeUnit precision, string lineProtocol)
        {
            Organization = organization;
            Bucket = bucket;
            Precision = precision;
            LineProtocol = lineProtocol;
        }
    }
}