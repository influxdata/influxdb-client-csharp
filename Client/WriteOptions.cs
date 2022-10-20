using System.Reactive.Concurrency;
using InfluxDB.Client.Core;

namespace InfluxDB.Client
{
    /// <summary>
    /// WriteOptions are used to configure writes the data point into InfluxDB 2.x.
    ///
    /// <para>
    ///The default setting use the batching configured to (consistent with Telegraf):
    /// <list>
    /// <item><term>batchSize</term><description>1000</description></item>
    /// <item><term>flushInterval</term><description>1000 ms</description></item>
    /// <item><term>retryInterval</term><description>5000 ms</description></item>
    /// <item><term>jitterInterval</term><description>0</description></item>
    /// </list>
    /// </para>
    ///
    /// </summary>
    public class WriteOptions
    {
        private const int DefaultBatchSize = 1000;
        private const int DefaultFlushInterval = 1000;
        private const int DefaultJitterInterval = 0;
        private const int DefaultRetryInterval = 5000;
        private const int DefaultMaxRetries = 5;
        private const int DefaultMaxRetryDelay = 125_000;
        private const int DefaultExponentialBase = 2;

        private int _batchSize;
        private int _flushInterval;
        private int _jitterInterval;
        private int _retryInterval;
        private int _maxRetries;
        private int _maxRetryDelay;
        private int _exponentialBase;
        private IScheduler _writeScheduler;

        /// <summary>
        /// The number of data point to collect in batch.
        /// </summary>
        /// <seealso cref="Builder.BatchSize(int)"/>
        public int BatchSize
        {
            get => _batchSize;
            set
            {
                Arguments.CheckPositiveNumber(value, "batchSize");
                _batchSize = value;
            }
        }

        /// <summary>
        /// The time to wait at most (milliseconds).
        /// </summary>
        /// <seealso cref="Builder.FlushInterval(int)"/>
        public int FlushInterval
        {
            get => _flushInterval;
            set
            {
                Arguments.CheckPositiveNumber(value, "flushInterval");
                _flushInterval = value;
            }
        }

        /// <summary>
        /// The batch flush jitter interval value (milliseconds).
        /// </summary>
        /// <seealso cref="Builder.JitterInterval(int)"/>
        public int JitterInterval
        {
            get => _jitterInterval;
            set
            {
                Arguments.CheckNotNegativeNumber(value, "jitterInterval");
                _jitterInterval = value;
            }
        }

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
        public int RetryInterval
        {
            get => _retryInterval;
            set
            {
                Arguments.CheckPositiveNumber(value, "retryInterval");
                _retryInterval = value;
            }
        }

        /// <summary>
        /// The number of max retries when write fails.
        /// </summary>
        /// <seealso cref="Builder.MaxRetries(int)"/>
        public int MaxRetries
        {
            get => _maxRetries;
            set
            {
                Arguments.CheckPositiveNumber(value, "MaxRetries");
                _maxRetries = value;
            }
        }

        /// <summary>
        /// The maximum delay between each retry attempt in milliseconds.
        /// </summary>
        /// <seealso cref="Builder.MaxRetryDelay(int)"/>
        public int MaxRetryDelay
        {
            get => _maxRetryDelay;
            set
            {
                Arguments.CheckPositiveNumber(value, "MaxRetryDelay");
                _maxRetryDelay = value;
            }
        }

        /// <summary>
        /// The base for the exponential retry delay.
        /// </summary>
        /// <seealso cref="Builder.ExponentialBase(int)"/>
        public int ExponentialBase
        {
            get => _exponentialBase;
            set
            {
                Arguments.CheckPositiveNumber(value, "ExponentialBase");
                _exponentialBase = value;
            }
        }

        /// <summary>
        /// Set the scheduler which is used for write data points.
        /// </summary>
        /// <seealso cref="Builder.WriteScheduler(IScheduler)"/>
        public IScheduler WriteScheduler
        {
            get => _writeScheduler;
            set
            {
                Arguments.CheckNotNull(value, "Write scheduler");
                _writeScheduler = value;
            }
        }

        /// <summary>
        /// Create an instance of WriteOptions.
        /// <para>
        /// WriteOptions properties and their default values:
        /// <list type="bullet">
        /// <item>BatchSize:        1000</item>
        /// <item>FlushInterval:    1000(ms)</item>
        /// <item>JitterInterval:   0</item>
        /// <item>RetryInterval:    5000(ms)</item>
        /// <item>MaxRetries:       5</item>
        /// <item>MaxRetryDelay:    125_000</item>
        /// <item>ExponentialBase:  2</item>
        /// </list>
        /// </para>
        /// </summary>
        public WriteOptions()
        {
            _batchSize = DefaultBatchSize;
            _flushInterval = DefaultFlushInterval;
            _jitterInterval = DefaultJitterInterval;
            _retryInterval = DefaultRetryInterval;
            _maxRetries = DefaultMaxRetries;
            _maxRetryDelay = DefaultMaxRetryDelay;
            _exponentialBase = DefaultExponentialBase;
            _writeScheduler = ThreadPoolScheduler.Instance;
        }

        private WriteOptions(Builder builder)
        {
            Arguments.CheckNotNull(builder, "builder");

            BatchSize = builder.BatchSizeBuilder;
            FlushInterval = builder.FlushIntervalBuilder;
            JitterInterval = builder.JitterIntervalBuilder;
            RetryInterval = builder.RetryIntervalBuilder;
            MaxRetries = builder.MaxRetriesBuilder;
            MaxRetryDelay = builder.MaxRetryDelayBuilder;
            ExponentialBase = builder.ExponentialBaseBuilder;
            WriteScheduler = builder.WriteSchedulerBuilder;
        }

        /// <summary>
        /// Create a <see cref="WriteOptions"/> builder.
        /// </summary>
        /// <returns>builder</returns>
        public static Builder CreateNew()
        {
            return new Builder();
        }

        public sealed class Builder
        {
            internal int BatchSizeBuilder = DefaultBatchSize;
            internal int FlushIntervalBuilder = DefaultFlushInterval;
            internal int JitterIntervalBuilder = DefaultJitterInterval;
            internal int RetryIntervalBuilder = DefaultRetryInterval;
            internal int MaxRetriesBuilder = DefaultMaxRetries;
            internal int MaxRetryDelayBuilder = DefaultMaxRetryDelay;
            internal int ExponentialBaseBuilder = DefaultExponentialBase;
            internal IScheduler WriteSchedulerBuilder = ThreadPoolScheduler.Instance;

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
            /// The number of max retries when write fails.
            /// </summary>
            /// <param name="count">number of max retries</param>
            /// <returns>this</returns>
            public Builder MaxRetries(int count)
            {
                Arguments.CheckPositiveNumber(count, "MaxRetries");
                MaxRetriesBuilder = count;
                return this;
            }

            /// <summary>
            /// The maximum delay between each retry attempt in milliseconds.
            /// </summary>
            /// <param name="milliseconds">maximum delay</param>
            /// <returns>this</returns>
            public Builder MaxRetryDelay(int milliseconds)
            {
                Arguments.CheckPositiveNumber(milliseconds, "MaxRetryDelay");
                MaxRetryDelayBuilder = milliseconds;
                return this;
            }

            /// <summary>
            /// The base for the exponential retry delay.
            /// </summary>
            /// <param name="exponentialBase">exponential base</param>
            /// <returns>this</returns>
            public Builder ExponentialBase(int exponentialBase)
            {
                Arguments.CheckPositiveNumber(exponentialBase, "ExponentialBase");
                ExponentialBaseBuilder = exponentialBase;
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