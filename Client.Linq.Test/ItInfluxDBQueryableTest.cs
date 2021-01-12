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
            const string sensor1 = "sensor,deployment=production,sensor_id=id-1 data=15 1602750015";
            // new DateTime(2020, 11, 15, 8, 20, 15, DateTimeKind.Utc)
            const string sensor2 = "sensor,deployment=production,sensor_id=id-1 data=28 1605428415";
            // new DateTime(2020, 11, 16, 8, 20, 15, DateTimeKind.Utc)
            const string sensor3 = "sensor,deployment=production,sensor_id=id-1 data=12 1605514815";
            // new DateTime(2020, 11, 17, 8, 20, 15, DateTimeKind.Utc)
            const string sensor4 = "sensor,deployment=production,sensor_id=id-1 data=89 1605601215";

            await _client
                .GetWriteApiAsync()
                .WriteRecordsAsync("my-bucket", "my-org", WritePrecision.S, 
                    sensor1, sensor2, sensor3, sensor4);
        }

        [Test]
        public void QueryAll()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApi())
                select s;

            var sensors = query.ToList();

            Assert.AreEqual(4, sensors.Count);
        }

        [Test]
        public void QueryTake()
        {
            var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApi())
                select s).Take(2);

            var sensors = query.ToList();

            Assert.AreEqual(2, sensors.Count);
        }

        [Test]
        public void QueryTakeSkip()
        {
            var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _client.GetQueryApi())
                select s).Take(2).Skip(3);

            var sensors = query.ToList();

            Assert.AreEqual(1, sensors.Count);
        }

        [TearDown]
        protected void After()
        {
            _client.Dispose();
        }
    }
}