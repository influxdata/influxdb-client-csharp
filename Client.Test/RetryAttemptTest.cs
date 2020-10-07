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

            retry = new RetryAttempt(new HttpException("", 0, new WebException("", WebExceptionStatus.Timeout)), 1, _default);
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
            Assert.AreEqual(5_000, retry.GetRetryInterval());
        }

        [Test]
        public void ExponentialBase()
        {
            var options = WriteOptions.CreateNew()
                .RetryInterval(5_000)
                .ExponentialBase(5)
                .MaxRetryDelay(int.MaxValue)
                .Build();

            var retry = new RetryAttempt(new HttpException("", 429), 1, options);
            Assert.AreEqual(5_000, retry.GetRetryInterval());

            retry = new RetryAttempt(new HttpException("", 429), 2, options);
            Assert.AreEqual(25_000, retry.GetRetryInterval());

            retry = new RetryAttempt(new HttpException("", 429), 3, options);
            Assert.AreEqual(125_000, retry.GetRetryInterval());

            retry = new RetryAttempt(new HttpException("", 429), 4, options);
            Assert.AreEqual(625_000, retry.GetRetryInterval());

            retry = new RetryAttempt(new HttpException("", 429), 5, options);
            Assert.AreEqual(3_125_000, retry.GetRetryInterval());

            retry = new RetryAttempt(new HttpException("", 429), 6, options);
            Assert.AreEqual(15_625_000, retry.GetRetryInterval());

            retry = new RetryAttempt(CreateException(3), 7, options);
            Assert.AreEqual(3_000, retry.GetRetryInterval());
        }

        [Test]
        public void MaxRetryDelay()
        {
            var options = WriteOptions.CreateNew()
                .RetryInterval(2_000)
                .ExponentialBase(2)
                .MaxRetryDelay(50_000)
                .Build();

            var retry = new RetryAttempt(new HttpException("", 429), 1, options);
            Assert.AreEqual(2_000, retry.GetRetryInterval());

            retry = new RetryAttempt(new HttpException("", 429), 2, options);
            Assert.AreEqual(4_000, retry.GetRetryInterval());

            retry = new RetryAttempt(new HttpException("", 429), 3, options);
            Assert.AreEqual(8_000, retry.GetRetryInterval());

            retry = new RetryAttempt(new HttpException("", 429), 4, options);
            Assert.AreEqual(16_000, retry.GetRetryInterval());

            retry = new RetryAttempt(new HttpException("", 429), 5, options);
            Assert.AreEqual(32_000, retry.GetRetryInterval());

            retry = new RetryAttempt(new HttpException("", 429), 6, options);
            Assert.AreEqual(50_000, retry.GetRetryInterval());
        }

        private HttpException CreateException(int retryAfter = 10)
        {
            var headers = new List<HttpHeader> {new HttpHeader {Name = "Retry-After", Value = retryAfter.ToString()}};
            var exception = HttpException.Create("", headers, "", HttpStatusCode.TooManyRequests);
            return exception;
        }
    }
}