using System;
using System.Linq;
using System.Linq.Expressions;
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
            const string sensor1 = "sensor,deployment=testing,sensor_id=id-1 data=15 1602750015";
            // new DateTime(2020, 11, 15, 8, 20, 15, DateTimeKind.Utc)
            const string sensor2 = "sensor,deployment=production,sensor_id=id-2 data=28 1605428415";

            await _client
                .GetWriteApiAsync()
                .WriteRecordsAsync("my-bucket", "my-org", WritePrecision.S, sensor1, sensor2);
        }

        [Test]
        public void QueryAll()
        {
            var query = from s in new InfluxDBQueryable<Sensor>(_client.GetQueryApi(), "my-org", "my-bucket") 
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

    class Sensor
    {
        [Column("sensor_id", IsTag = true)] public string SensorId { get; set; }

        /// <summary>
        /// "production" or "testing"
        /// </summary>
        [Column("deployment", IsTag = true)]
        public string Deployment { get; set; }

        /// <summary>
        /// Value measured by sensor
        /// </summary>
        [Column("data")]
        public float Value { get; set; }

        [Column(IsTimestamp = true)] public DateTime Timestamp { get; set; }
    }
}