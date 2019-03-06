using System.Reactive.Concurrency;
using InfluxDB.Client.Core;

namespace InfluxDB.Client
{
    /// <summary>
    /// WriteOptions are used to configure writes the data point into InfluxDB 2.0.
    ///
    /// <para>
    ///The default setting use the batching configured to (consistent with Telegraf):
    /// <list>
    /// <item><term>batchSize</term><description>1000</description></item>
    /// <item><term>flushInterval</term><description>1000 ms</description></item>
    /// <item><term>retryInterval</term><description>1000 ms</description></item>
    /// <item><term>jitterInterval</term><description>0</description></item>
    /// <item><term>bufferLimit</term><description>10 000</description></item>
    /// </list>
    /// </para>
    ///
    /// </summary>
    public class WriteOptions
    {
        private const int DefaultBatchSize = 1000;
        private const int DefaultFlushInterval = 1000;
        private const int DefaultJitterInterval = 0;
        private const int DefaultRetryInterval = 1000;

        /// <summary>
        /// The number of data point to collect in batch.
        /// </summary>
        /// <seealso cref="Builder.BatchSize(int)"/>
        internal int BatchSize { get; }

        /// <summary>
        /// The time to wait at most (milliseconds).
        /// </summary>
        /// <seealso cref="Builder.FlushInterval(int)"/>
        internal int FlushInterval { get; }

        /// <summary>
        /// The batch flush jitter interval value (milliseconds).
        /// </summary>
        /// <seealso cref="Builder.JitterInterval(int)"/>
        internal int JitterInterval { get; }

        /// <summary>
        /// The time to wait before retry unsuccessful write (milliseconds).
        /// <para>
        /// The retry interval is used when the InfluxDB server does not specify "Retry-After" header.
        /// </para>
        /// <para>
        /// Retry-After: A non-negative decimal integer indicating the seconds to delay after the response is received.
        /// </para>
        /// </summary>
        /// <seealso cref="Builder.RetryInterval(int)"/>
        public int RetryInterval { get; }

        /// <summary>
        /// Set the scheduler which is used for write data points.
        /// </summary>
        /// <seealso cref="Builder.WriteScheduler(IScheduler)"/>
        internal IScheduler WriteScheduler { get; }

        private WriteOptions(Builder builder)
        {
            Arguments.CheckNotNull(builder, "builder");

            BatchSize = builder.BatchSizeBuilder;
            FlushInterval = builder.FlushIntervalBuilder;
            JitterInterval = builder.JitterIntervalBuilder;
            RetryInterval = builder.RetryIntervalBuilder;
            WriteScheduler = builder.WriteSchedulerBuilder;
        }
        
        /// <summary>
        /// Create a <see cref="WriteOptions"/> builder.
        /// </summary>
        /// <returns>builder</returns>
        public static Builder CreateNew() {
            return new Builder();
        }
        
        public sealed class Builder
        {
            internal int BatchSizeBuilder = DefaultBatchSize;
            internal int FlushIntervalBuilder = DefaultFlushInterval;
            internal int JitterIntervalBuilder = DefaultJitterInterval;
            internal int RetryIntervalBuilder = DefaultRetryInterval;
            internal IScheduler WriteSchedulerBuilder = NewThreadScheduler.Default;

            /// <summary>
            /// Set the number of data point to collect in batch.
            /// </summary>
            /// <param name="batchSize">the number of data point to collect in batch</param>
            /// <returns>this</returns>
            public Builder BatchSize(int batchSize)
            {
                Arguments.CheckPositiveNumber(batchSize, "batchSize");
                BatchSizeBuilder = batchSize;
                return this;
            }

            /// <summary>
            /// Set the time to wait at most (milliseconds).
            /// </summary>
            /// <param name="milliseconds">the time to wait at most (milliseconds).</param>
            /// <returns>this</returns>
            public Builder FlushInterval(int milliseconds)
            {
                Arguments.CheckPositiveNumber(milliseconds, "flushInterval");
                FlushIntervalBuilder = milliseconds;
                return this;
            }

            /// <summary>
            /// Jitters the batch flush interval by a random amount.
            /// This is primarily to avoid large write spikes for users running a large number of client instances.
            /// ie, a jitter of 5s and flush duration 10s means flushes will happen every 10-15s.
            /// </summary>
            /// <param name="milliseconds">Jitter interval in milliseconds</param>
            /// <returns>this</returns>
            public Builder JitterInterval(int milliseconds)
            {
                Arguments.CheckNotNegativeNumber(milliseconds, "jitterInterval");
                JitterIntervalBuilder = milliseconds;
                return this;
            }

            /// <summary>
            /// Set the the time to wait before retry unsuccessful write (milliseconds).
            /// <para>
            /// The retry interval is used when the InfluxDB server does not specify "Retry-After" header.
            /// </para>
            /// <para>
            /// Retry-After: A non-negative decimal integer indicating the seconds to delay after the response is received.
            /// </para>
            /// </summary>
            /// <param name="milliseconds">the time to wait before retry unsuccessful write</param>
            /// <returns>this</returns>
            public Builder RetryInterval(int milliseconds)
            {
                Arguments.CheckPositiveNumber(milliseconds, "retryInterval");
                RetryIntervalBuilder = milliseconds;
                return this;
            }

            /// <summary>
            /// Set the scheduler which is used for write data points. It is useful for disabling batch writes or
            /// for tuning the performance. Default value is <see cref="Scheduler.CurrentThread"/>
            /// </summary>
            /// <param name="writeScheduler"></param>
            /// <returns></returns>
            public Builder WriteScheduler(IScheduler writeScheduler)
            {
                Arguments.CheckNotNull(writeScheduler, "Write scheduler");

                WriteSchedulerBuilder = writeScheduler;
                return this;
            }

            /// <summary>
            ///  Build an instance of WriteOptions.
            /// </summary>
            /// <returns><see cref="WriteOptions"/></returns>
            public WriteOptions Build()
            {
                return new WriteOptions(this);
            }
        }
    }
}