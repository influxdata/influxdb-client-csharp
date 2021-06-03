using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Net;
using InfluxDB.Client.Core.Exceptions;

namespace InfluxDB.Client.Internal
{
    /// <summary>
    /// RetryConfiguration.
    /// </summary>
    internal class RetryAttempt
    {
        private static readonly ReadOnlyCollection<WebExceptionStatus> RetryableStatuses =
            new ReadOnlyCollection<WebExceptionStatus>(
                new[]
                {
                    WebExceptionStatus.ConnectFailure,
                    WebExceptionStatus.NameResolutionFailure,
                    WebExceptionStatus.ProxyNameResolutionFailure,
                    WebExceptionStatus.SendFailure,
                    WebExceptionStatus.PipelineFailure,
                    WebExceptionStatus.ConnectionClosed,
                    WebExceptionStatus.KeepAliveFailure,
                    WebExceptionStatus.UnknownError,
                    WebExceptionStatus.ReceiveFailure,
                    WebExceptionStatus.RequestCanceled,
                    WebExceptionStatus.Timeout,
                });

        internal Exception Error { get; }
        private readonly int _count;
        private readonly WriteOptions _writeOptions;
        private readonly Random _random = new Random();

        internal RetryAttempt(Exception error, int count, WriteOptions writeOptions)
        {
            Error = error;
            _count = count;
            _writeOptions = writeOptions;
        }

        /// <summary>
        /// Is this request retryable?
        /// </summary>
        /// <returns>true if its retryable otherwise false</returns>
        internal bool IsRetry()
        {
            //
            // Max retries exceeded.
            //
            if (_count > _writeOptions.MaxRetries)
            {
                Trace.TraceWarning($"Max write retries exceeded. Response: '{Error.Message}'.");

                return false;
            }

            if (Error is HttpException httpException && httpException.Status > 0)
            {
                //
                // Retry HTTP error codes >= 429
                //
                return httpException.Status >= 429;
            }

            var webException = GetWebException(Error);

            if (webException != null)
            {
                if (RetryableStatuses.Contains(webException.Status))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Get current retry interval.
        /// </summary>
        /// <returns>retry interval to sleep</returns>
        internal long GetRetryInterval()
        {
            // from header
            if (Error is HttpException httpException && httpException.RetryAfter.HasValue)
            {
                return httpException.RetryAfter.Value * 1000 + JitterDelay(_writeOptions);
            }

            // from configuration
            var rangeStart = _writeOptions.RetryInterval;
            var rangeStop = _writeOptions.RetryInterval * _writeOptions.ExponentialBase;

            var i = 1;
            while (i < _count)
            {
                i++;
                rangeStart = rangeStop;
                rangeStop = rangeStop * _writeOptions.ExponentialBase;
                if (rangeStop > _writeOptions.MaxRetryDelay)
                {
                    break;
                }
            }

            if (rangeStop > _writeOptions.MaxRetryDelay)
            {
                rangeStop = _writeOptions.MaxRetryDelay;
            }

            var retryInterval = (long) (rangeStart + (rangeStop - rangeStart) * _random.NextDouble());

            Trace.WriteLine("The InfluxDB does not specify \"Retry-After\". " +
                            $"Use the default retryInterval: {retryInterval}");

            return retryInterval;
        }

        internal static int JitterDelay(WriteOptions writeOptions)
        {
            return (int) (new Random().NextDouble() * writeOptions.JitterInterval);
        }

        private WebException GetWebException(Exception exception)
        {
            switch (exception)
            {
                case null:
                    return null;
                case WebException webException:
                    return webException;
                default:
                    return GetWebException(exception.InnerException);
            }
        }
    }
}