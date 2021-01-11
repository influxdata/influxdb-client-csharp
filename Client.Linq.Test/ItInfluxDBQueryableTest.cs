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

            var sensor1 = new Sensor
            {
                SensorId = "id-1", Deployment = "testing", Value = 15,
                Timestamp = new DateTime(2020, 10, 15, 8, 20, 15, DateTimeKind.Utc)
            };
            var sensor2 = new Sensor
            {
                SensorId = "id-2", Deployment = "production", Value = 28,
                Timestamp = new DateTime(2020, 11, 15, 8, 20, 15, DateTimeKind.Utc)
            };

            await _client
                .GetWriteApiAsync()
                .WriteMeasurementsAsync("my-bucket", "my-org", WritePrecision.S, sensor1, sensor2);
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

    [Measurement("sensor")]
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