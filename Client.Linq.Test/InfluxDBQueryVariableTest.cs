using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Test;
using InfluxDB.Client.Linq;
using NUnit.Framework;

namespace Client.Linq.Test
{
    [TestFixture]
    public class InfluxDbQueryVariableTest : AbstractTest
    {
        private InfluxDBClient _client;
        private DateTime _dateTime;

        [SetUp]
        public new Task SetUp()
        {
            _client = InfluxDBClientFactory.Create(GetInfluxDb2Url(), "my-token");
            _client.SetLogLevel(LogLevel.Body);
            return Task.CompletedTask;
        }

        [OneTimeSetUp]
        public async Task OneTimeSetUp()
        {
            _client = InfluxDBClientFactory.Create(GetInfluxDb2Url(), "my-token");
            _client.SetLogLevel(LogLevel.Body);

            _dateTime = DateTime.UtcNow;

            var orgId = (await _client.GetOrganizationsApi().FindOrganizationsAsync(org: "my-org")).First().Id;
            var bucket = await _client.GetBucketsApi().CreateBucketAsync("test-bucket", orgId);

            var point1 = new InfluxPoint()
            {
                IntValue = -1000, ShortValue = -1000,
                LongValue = -1000, DoubleValue = -1000, FloatValue = -1000, Timestamp = _dateTime
            };
            var point2 = new InfluxPoint()
            {
                IntValue = -450, ShortValue = -450, LongValue = -450,
                DoubleValue = -450, FloatValue = -450, Timestamp = _dateTime.AddSeconds(-1)
            };
            var point3 = new InfluxPoint()
            {
                IntValue = -80, ShortValue = -80, LongValue = -80,
                DoubleValue = -80, FloatValue = -80, Timestamp = _dateTime.AddSeconds(-2)
            };
            var point4 = new InfluxPoint()
            {
                IntValue = 65, ShortValue = 65, LongValue = 65,
                DoubleValue = 65, FloatValue = 65, Timestamp = _dateTime.AddSeconds(-3)
            };
            var point5 = new InfluxPoint()
            {
                IntValue = 190, ShortValue = 190, LongValue = 190,
                DoubleValue = 190, FloatValue = 190, Timestamp = _dateTime.AddSeconds(-4)
            };
            var point6 = new InfluxPoint()
            {
                IntValue = 350, ShortValue = 350, LongValue = 350,
                DoubleValue = 350, FloatValue = 350, Timestamp = _dateTime.AddSeconds(-5)
            };
            var point7 = new InfluxPoint()
            {
                IntValue = 500, ShortValue = 500, LongValue = 500,
                DoubleValue = 500, FloatValue = 500, Timestamp = _dateTime.AddSeconds(-6)
            };
            var point8 = new InfluxPoint()
            {
                IntValue = 750, ShortValue = 750, LongValue = 750,
                DoubleValue = 750, FloatValue = 750, Timestamp = _dateTime.AddSeconds(-7)
            };
            var point9 = new InfluxPoint()
            {
                IntValue = 1100, ShortValue = 1100, LongValue = 1100,
                DoubleValue = 1100, FloatValue = 1100, Timestamp = _dateTime.AddSeconds(-8)
            };
            var point10 = new InfluxPoint()
            {
                IntValue = 2535, ShortValue = 2535, LongValue = 2535,
                DoubleValue = 2535, FloatValue = 2535, Timestamp = _dateTime.AddSeconds(-9)
            };

            await _client
                .GetWriteApiAsync().WriteMeasurementsAsync(new[]
                {
                    point1, point2, point3, point4, point5, point6,
                    point7, point8, point9, point10
                }, WritePrecision.Ns, "test-bucket", "my-org");
        }

        [Test]
        public void QueryAll()
        {
            var query = from s in InfluxDBQueryable<InfluxPoint>
                    .Queryable("test-bucket", "my-org", _client.GetQueryApiSync())
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(10, sensors.Count);
        }

        [Test]
        public void QueryWhereInt()
        {
            var query = from s in InfluxDBQueryable<InfluxPoint>
                    .Queryable("test-bucket", "my-org", _client.GetQueryApiSync())
                where s.IntValue > -200
                select s;

            var points = query.ToList();

            Assert.AreEqual(8, points.Count);
            foreach (var point in points) Assert.Greater(point.IntValue, -200);
        }

        [Test]
        public void QueryWhereShort()
        {
            var query = from s in InfluxDBQueryable<InfluxPoint>
                    .Queryable("test-bucket", "my-org", _client.GetQueryApiSync())
                where s.ShortValue >= 500
                select s;

            var points = query.ToList();

            Assert.AreEqual(4, points.Count);
            foreach (var point in points) Assert.GreaterOrEqual(point.ShortValue, 500);
        }

        [Test]
        public void QueryWhereLong()
        {
            var query = from s in InfluxDBQueryable<InfluxPoint>
                    .Queryable("test-bucket", "my-org", _client.GetQueryApiSync())
                where s.LongValue < 200
                select s;

            var points = query.ToList();

            Assert.AreEqual(5, points.Count);
            foreach (var point in points) Assert.Less(point.LongValue, 200);
        }

        [Test]
        public void QueryWhereDouble()
        {
            var query = from s in InfluxDBQueryable<InfluxPoint>
                    .Queryable("test-bucket", "my-org", _client.GetQueryApiSync())
                where s.DoubleValue <= 0
                select s;

            var points = query.ToList();

            Assert.AreEqual(3, points.Count);
            foreach (var point in points) Assert.LessOrEqual(point.DoubleValue, 0);
        }

        [Test]
        public void QueryWhereFloat()
        {
            var query = from s in InfluxDBQueryable<InfluxPoint>
                    .Queryable("test-bucket", "my-org", _client.GetQueryApiSync())
                where s.FloatValue < 100
                select s;

            var points = query.ToList();

            Assert.AreEqual(4, points.Count);
            foreach (var point in points) Assert.Less(point.FloatValue, 100);
        }

        [TearDown]
        protected void After()
        {
            _client.Dispose();
        }
    }
}