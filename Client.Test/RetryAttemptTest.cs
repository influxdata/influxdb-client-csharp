using System;
using System.Collections.Generic;
using System.Net;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Internal;
using NUnit.Framework;
using RestSharp;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class RetryAttemptTest
    {
        private readonly WriteOptions _default = WriteOptions.CreateNew().Build();

        [Test]
        public void ErrorType()
        {
            var retry = new RetryAttempt(new ArgumentException(""), 1, _default);
            Assert.IsFalse(retry.IsRetry());

            retry = new RetryAttempt(new HttpException("", 429), 1, _default);
            Assert.IsTrue(retry.IsRetry());

            retry = new RetryAttempt(new WebException("", WebExceptionStatus.Timeout), 1, _default);
            Assert.IsTrue(retry.IsRetry());

            retry = new RetryAttempt(new HttpException("", 0, new WebException("", WebExceptionStatus.Timeout)), 1,
                _default);
            Assert.IsTrue(retry.IsRetry());

            retry = new RetryAttempt(new WebException("", WebExceptionStatus.ProtocolError), 1, _default);
            Assert.IsFalse(retry.IsRetry());
        }

        [Test]
        public void RetryableHttpErrorCodes()
        {
            var retry = new RetryAttempt(new HttpException("", 428), 1, _default);
            Assert.IsFalse(retry.IsRetry());

            retry = new RetryAttempt(new HttpException("", 429), 1, _default);
            Assert.IsTrue(retry.IsRetry());

            retry = new RetryAttempt(new HttpException("", 504), 1, _default);
            Assert.IsTrue(retry.IsRetry());
        }

        [Test]
        public void MaxRetries()
        {
            var options = WriteOptions.CreateNew().MaxRetries(5).Build();

            var retry = new RetryAttempt(new HttpException("", 429), 1, _default);
            Assert.IsTrue(retry.IsRetry());

            retry = new RetryAttempt(new HttpException("", 429), 2, options);
            Assert.IsTrue(retry.IsRetry());

            retry = new RetryAttempt(new HttpException("", 429), 3, options);
            Assert.IsTrue(retry.IsRetry());

            retry = new RetryAttempt(new HttpException("", 429), 4, options);
            Assert.IsTrue(retry.IsRetry());

            retry = new RetryAttempt(new HttpException("", 429), 5, options);
            Assert.IsTrue(retry.IsRetry());

            retry = new RetryAttempt(new HttpException("", 429), 6, options);
            Assert.IsFalse(retry.IsRetry());
        }

        [Test]
        public void HeaderHasPriority()
        {
            var exception = CreateException();

            var retry = new RetryAttempt(exception, 1, _default);
            Assert.AreEqual(10_000, retry.GetRetryInterval());

            retry = new RetryAttempt(new HttpException("", 429), 1, _default);
            Assert.GreaterOrEqual(retry.GetRetryInterval(), 5_000);
            Assert.LessOrEqual(retry.GetRetryInterval(), 10_000);
        }

        [Test]
        public void ExponentialBase()
        {
            var options = WriteOptions.CreateNew()
                .RetryInterval(5_000)
                .ExponentialBase(5)
                .MaxRetries(4)
                .MaxRetryDelay(int.MaxValue)
                .Build();

            var retry = new RetryAttempt(new HttpException("", 429), 1, options);
            var retryInterval = retry.GetRetryInterval();
            Assert.GreaterOrEqual(retryInterval, 5_000);
            Assert.LessOrEqual(retryInterval, 25_000);
            Assert.IsTrue(retry.IsRetry());

            retry = new RetryAttempt(new HttpException("", 429), 2, options);
            retryInterval = retry.GetRetryInterval();
            Assert.GreaterOrEqual(retryInterval, 25_000);
            Assert.LessOrEqual(retryInterval, 125_000);
            Assert.IsTrue(retry.IsRetry());

            retry = new RetryAttempt(new HttpException("", 429), 3, options);
            retryInterval = retry.GetRetryInterval();
            Assert.GreaterOrEqual(retryInterval, 125_000);
            Assert.LessOrEqual(retryInterval, 625_000);
            Assert.IsTrue(retry.IsRetry());

            retry = new RetryAttempt(new HttpException("", 429), 4, options);
            retryInterval = retry.GetRetryInterval();
            Assert.GreaterOrEqual(retryInterval, 625_000);
            Assert.LessOrEqual(retryInterval, 3_125_000);
            Assert.IsTrue(retry.IsRetry());

            retry = new RetryAttempt(new HttpException("", 429), 5, options);
            retryInterval = retry.GetRetryInterval();
            Assert.GreaterOrEqual(retryInterval, 3_125_000);
            Assert.LessOrEqual(retryInterval, 15_625_000);
            Assert.IsFalse(retry.IsRetry());

            retry = new RetryAttempt(new HttpException("", 429), 6, options);
            retryInterval = retry.GetRetryInterval();
            Assert.GreaterOrEqual(retryInterval, 15_625_000);
            Assert.LessOrEqual(retryInterval, 78_125_000);
            Assert.IsFalse(retry.IsRetry());

            retry = new RetryAttempt(CreateException(3), 7, options);
            retryInterval = retry.GetRetryInterval();
            Assert.AreEqual(3_000, retryInterval);
        }

        [Test]
        public void MaxRetryDelay()
        {
            var options = WriteOptions.CreateNew()
                .RetryInterval(2_000)
                .ExponentialBase(2)
                .MaxRetries(10)
                .MaxRetryDelay(50_000)
                .Build();

            var retry = new RetryAttempt(new HttpException("", 429), 1, options);
            Assert.GreaterOrEqual(retry.GetRetryInterval(), 2_000);
            Assert.LessOrEqual(retry.GetRetryInterval(), 4_000);

            retry = new RetryAttempt(new HttpException("", 429), 2, options);
            Assert.GreaterOrEqual(retry.GetRetryInterval(), 4_000);
            Assert.LessOrEqual(retry.GetRetryInterval(), 8_000);

            retry = new RetryAttempt(new HttpException("", 429), 3, options);
            Assert.GreaterOrEqual(retry.GetRetryInterval(), 8_000);
            Assert.LessOrEqual(retry.GetRetryInterval(), 16_000);

            retry = new RetryAttempt(new HttpException("", 429), 4, options);
            Assert.GreaterOrEqual(retry.GetRetryInterval(), 16_000);
            Assert.LessOrEqual(retry.GetRetryInterval(), 32_000);

            retry = new RetryAttempt(new HttpException("", 429), 5, options);
            Assert.GreaterOrEqual(retry.GetRetryInterval(), 32_000);
            Assert.LessOrEqual(retry.GetRetryInterval(), 50_000);

            retry = new RetryAttempt(new HttpException("", 429), 6, options);
            Assert.LessOrEqual(retry.GetRetryInterval(), 50_000);
        }

        private HttpException CreateException(int retryAfter = 10)
        {
            var headers = new List<HeaderParameter> { new HeaderParameter("Retry-After", retryAfter.ToString()) };
            var exception = HttpException.Create("", headers, "", HttpStatusCode.TooManyRequests);
            return exception;
        }
    }
}