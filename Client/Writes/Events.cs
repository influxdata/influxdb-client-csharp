using System;
using System.Diagnostics;
using InfluxDB.Client.Api.Domain;

namespace InfluxDB.Client.Writes
{
    public abstract class InfluxDBEventArgs : EventArgs
    {
        internal abstract void LogEvent();
    }

    public class WriteSuccessEvent : AbstractWriteEvent
    {
        public WriteSuccessEvent(string organization, string bucket, WritePrecision precision, string lineProtocol) :
            base(organization, bucket, precision, lineProtocol)
        {
        }

        internal override void LogEvent()
        {
            Trace.WriteLine("The data was successfully written to InfluxDB 2.");
        }
    }

    public class WriteErrorEvent : AbstractWriteEvent
    {
        /// <summary>
        /// The exception that was throw.
        /// </summary>
        public Exception Exception { get; }

        public WriteErrorEvent(string organization, string bucket, WritePrecision precision, string lineProtocol,
            Exception exception) :
            base(organization, bucket, precision, lineProtocol)
        {
            Exception = exception;
        }

        internal override void LogEvent()
        {
            Trace.TraceError($"The error occurred during writing of data: {Exception.Message}");
        }
    }

    /// <summary>
    /// The event is published when occurs a retriable write exception.
    /// </summary>
    public class WriteRetriableErrorEvent : AbstractWriteEvent
    {
        /// <summary>
        /// The exception that was throw.
        /// </summary>
        public Exception Exception { get; }

        /// <summary>
        /// The time to wait before retry unsuccessful write (milliseconds)
        /// </summary>
        public long RetryInterval { get; }

        public WriteRetriableErrorEvent(string organization, string bucket, WritePrecision precision,
            string lineProtocol, Exception exception, long retryInterval) : base(organization, bucket, precision,
            lineProtocol)
        {
            Exception = exception;
            RetryInterval = retryInterval;
        }

        internal override void LogEvent()
        {
            var message = "The retriable error occurred during writing of data. " +
                          $"Reason: '{Exception.Message}'. " +
                          $"Retry in: {(double)RetryInterval / 1000}s.";

            Trace.TraceWarning(message);
        }
    }

    /// <summary>
    /// Published when occurs a runtime exception in background batch processing.
    /// </summary>
    public class WriteRuntimeExceptionEvent : InfluxDBEventArgs
    {
        /// <summary>
        /// The Runtime Exception that was throw.
        /// </summary>
        public Exception Exception { get; }

        internal WriteRuntimeExceptionEvent(Exception exception)
        {
            Exception = exception;
        }

        internal override void LogEvent()
        {
            Trace.TraceError($"The unhandled exception occurs: {Exception}");
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
        public WritePrecision Precision { get; }

        /// <summary>
        /// The Data that was written.
        /// </summary>
        public string LineProtocol { get; }

        internal AbstractWriteEvent(string organization, string bucket, WritePrecision precision, string lineProtocol)
        {
            Organization = organization;
            Bucket = bucket;
            Precision = precision;
            LineProtocol = lineProtocol;
        }
    }
}