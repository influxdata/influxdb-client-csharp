using System;
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
    public class ItInfluxDBQueryableTest : AbstractTest
    {
        private InfluxDBClient _client;

        [SetUp]
        public new async Task SetUp()
        {
            _client = InfluxDBClientFactory.Create(GetInfluxDb2Url(), "my-token");
            _client.SetLogLevel(LogLevel.Body);

            // DateTime(2020, 10, 15, 8, 20, 15, DateTimeKind.Utc)
            const string sensor11 = "sensor,deployment=production,sensor_id=id-1 data=15i 1602750015";
            const string sensor21 = "sensor,deployment=production,sensor_id=id-2 data=15i 1602750015";
            // new DateTime(2020, 11, 15, 8, 20, 15, DateTimeKind.Utc)
            const string sensor12 = "sensor,deployment=production,sensor_id=id-1 data=28i 1605428415";
            const string sensor22 = "sensor,deployment=production,sensor_id=id-2 data=28i 1605428415";
            // new DateTime(2020, 11, 16, 8, 20, 15, DateTimeKind.Utc)
            const string sensor13 = "sensor,deployment=production,sensor_id=id-1 data=12i 1605514815";
            const string sensor23 = "sensor,deployment=production,sensor_id=id-2 data=12i 1605514815";
            // new DateTime(2020, 11, 17, 8, 20, 15, DateTimeKind.Utc)
            const string sensor14 = "sensor,deployment=production,sensor_id=id-1 data=89i 1605601215";
            const string sensor24 = "sensor,deployment=production,sensor_id=id-2 data=89i 1605601215";

            await _client
                .GetWriteApiAsync()
                .WriteRecordsAsync(new[]
                {
                    sensor11, sensor21, sensor12, sensor22, sensor13, sensor23, sensor14, sensor24
                }, WritePrecision.S, "my-bucket", "my-org");
        }

        [Test]
        public void QueryAll()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(8, sensors.Count);
        }

        [Test]
        public void QueryExample()
        {
            var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                    where s.SensorId == "id-1"
                    where s.Value > 12
                    where s.Timestamp > new DateTime(2019, 11, 16, 8, 20, 15, DateTimeKind.Utc)
                    where s.Timestamp < new DateTime(2021, 01, 10, 5, 10, 0, DateTimeKind.Utc)
                    orderby s.Timestamp
                    select s)
                .Take(2)
                .Skip(2);

            var sensors = query.ToList();

            Assert.AreEqual(1, sensors.Count);
        }

        [Test]
        public void QueryExampleCount()
        {
            var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                    where s.SensorId == "id-1"
                    where s.Value > 12
                    where s.Timestamp > new DateTime(2019, 11, 16, 8, 20, 15, DateTimeKind.Utc)
                    where s.Timestamp < new DateTime(2021, 01, 10, 5, 10, 0, DateTimeKind.Utc)
                    orderby s.Timestamp
                    select s)
                .Count();

            Assert.AreEqual(3, query);
        }

        [Test]
        public void QueryTake()
        {
            var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                select s).Take(2);

            var sensors = query.ToList();

            Assert.AreEqual(2 * 2, sensors.Count);
        }

        [Test]
        public void QueryTakeMultipleTimeSeries()
        {
            var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync(),
                    new QueryableOptimizerSettings { QueryMultipleTimeSeries = true })
                select s).Take(2);

            var sensors = query.ToList();

            Assert.AreEqual(2, sensors.Count);
        }

        [Test]
        public void QueryTakeSkip()
        {
            var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                select s).Take(2).Skip(3);

            var sensors = query.ToList();

            Assert.AreEqual(1 + 1, sensors.Count);
        }

        [Test]
        public void QueryWhereEqual()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where s.SensorId == "id-1"
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(4, sensors.Count);
            foreach (var sensor in sensors) Assert.AreEqual("id-1", sensor.SensorId);
        }

        [Test]
        public void QueryWhereNotEqual()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where s.SensorId != "id-1"
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(4, sensors.Count);
            foreach (var sensor in sensors) Assert.AreEqual("id-2", sensor.SensorId);
        }

        [Test]
        public void QueryLess()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where s.Value < 28
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(4, sensors.Count);
            foreach (var sensor in sensors) Assert.Less(sensor.Value, 28);
        }

        [Test]
        public void QueryLessThanOrEqual()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where s.Value <= 28
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(6, sensors.Count);
            foreach (var sensor in sensors) Assert.LessOrEqual(sensor.Value, 28);
        }

        [Test]
        public void QueryGreaterThan()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where s.Value > 28
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(2, sensors.Count);
            foreach (var sensor in sensors) Assert.Greater(sensor.Value, 28);
        }

        [Test]
        public void QueryGreaterThanOrEqual()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where s.Value >= 28
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(4, sensors.Count);
            foreach (var sensor in sensors) Assert.GreaterOrEqual(sensor.Value, 28);
        }

        [Test]
        public void QueryAnd()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where s.Value >= 28 && s.SensorId != "id-1"
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(2, sensors.Count);
            foreach (var sensor in sensors) Assert.GreaterOrEqual(sensor.Value, 28);
        }

        [Test]
        public void QueryOr()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where s.Value >= 28 || s.Value <= 12
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(6, sensors.Count);
        }

        [Test]
        public void QueryTimeRange()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where s.Timestamp > new DateTime(2020, 11, 16, 8, 20, 15, DateTimeKind.Utc)
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(2, sensors.Count);
            foreach (var sensor in sensors) Assert.GreaterOrEqual(sensor.Value, 89);
        }

        [Test]
        public void QueryTimeGreaterEqual()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where s.Timestamp >= new DateTime(2020, 11, 16, 8, 20, 15, DateTimeKind.Utc)
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(4, sensors.Count);
        }

        [Test]
        public void QueryTimeEqual()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where s.Timestamp == new DateTime(2020, 10, 15, 8, 20, 15, DateTimeKind.Utc)
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(2, sensors.Count);
            foreach (var sensor in sensors) Assert.GreaterOrEqual(sensor.Value, 15);
        }

        [Test]
        public void QueryWhereNothing()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where s.SensorId == "id-nothing"
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(0, sensors.Count);
        }

        [Test]
        public void QueryOrderBy()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                orderby s.Value
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(12, sensors.First().Value);
            Assert.AreEqual(89, sensors.Last().Value);
        }

        [Test]
        public void QueryOrderByTime()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                orderby s.Timestamp descending
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(new DateTime(2020, 11, 17, 8, 20, 15, DateTimeKind.Utc),
                sensors.First().Timestamp);
            Assert.AreEqual(new DateTime(2020, 10, 15, 8, 20, 15, DateTimeKind.Utc),
                sensors.Last().Timestamp);
        }

        [Test]
        public void QueryCount()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where s.SensorId == "id-1"
                select s;

            var sensors = query.Count();

            Assert.AreEqual(4, sensors);
        }

        [Test]
        public void QueryCountDifferentTimeSeries()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                select s;

            var sensors = query.LongCount();

            Assert.AreEqual(8, sensors);
        }

        [Test]
        public void QueryContainsField()
        {
            int[] values = { 15, 28 };

            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where values.Contains(s.Value)
                select s;

            var sensors = query.Count();

            Assert.AreEqual(4, sensors);
        }

        [Test]
        public void QueryContainsTag()
        {
            string[] deployment = { "production", "testing" };

            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where deployment.Contains(s.Deployment)
                select s;

            var sensors = query.Count();

            Assert.AreEqual(8, sensors);
        }

        [Test]
        public void SyncQueryConfiguration()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApi())
                select s;

            var ae = Assert.Throws<ArgumentException>(() => query.ToList());
            Assert.AreEqual("The 'QueryApiSync' has to be configured for sync queries.", ae.Message);
        }

        [Test]
        public void ASyncQueryConfiguration()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                select s;

            var ae = Assert.Throws<ArgumentException>(() => query.ToInfluxQueryable().GetAsyncEnumerator());
            Assert.AreEqual("The 'QueryApi' has to be configured for Async queries.", ae.Message);
        }

        [Test]
        public async Task ASyncQuery()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApi())
                select s;

            var sensors = await query
                .ToInfluxQueryable()
                .GetAsyncEnumerator()
                .ToListAsync();

            Assert.AreEqual(8, sensors.Count);
        }

        [Test]
        public async Task ASyncQueryFirst()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApi())
                select s;

            var sensor = await query
                .ToInfluxQueryable()
                .GetAsyncEnumerator()
                .FirstOrDefaultAsync();

            Assert.IsNotNull(sensor);
        }

        [Test]
        public void AggregateFunction()
        {
            var count = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                    where s.Timestamp > new DateTime(2019, 11, 16, 8, 20, 15, DateTimeKind.Utc)
                    where s.Timestamp < new DateTime(2021, 01, 10, 5, 10, 0, DateTimeKind.Utc)
                    orderby s.Timestamp
                    select s)
                .Count();

            Assert.AreEqual(8, count);
        }

        [Test]
        public async Task AggregateFunctionAsync()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApi())
                where s.Timestamp > new DateTime(2019, 11, 16, 8, 20, 15, DateTimeKind.Utc)
                where s.Timestamp < new DateTime(2021, 01, 10, 5, 10, 0, DateTimeKind.Utc)
                orderby s.Timestamp
                select s;

            var count = await query
                .ToInfluxQueryable()
                .GetAsyncEnumerator()
                .CountAsync();

            Assert.AreEqual(8, count);
        }

        [Test]
        public void QueryAggregateWindow()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync())
                where s.Timestamp.AggregateWindow(TimeSpan.FromDays(4), null, "mean")
                where s.Timestamp > new DateTime(2020, 11, 15, 0, 0, 0, DateTimeKind.Utc)
                where s.Timestamp < new DateTime(2020, 11, 18, 0, 0, 0, DateTimeKind.Utc)
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(2, sensors.Count);
            // (28 + 12 + 89) / 3 = 43
            Assert.AreEqual(43, sensors[0].Value);
            Assert.AreEqual(43, sensors.Last().Value);
        }

        [Test]
        public void RangeValue()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApiSync(),
                    new QueryableOptimizerSettings
                        { RangeStartValue = new DateTime(2020, 11, 17, 8, 20, 15, DateTimeKind.Utc) })
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(2, sensors.Count);
        }

        [TearDown]
        protected void After()
        {
            _client.Dispose();
        }
    }
}