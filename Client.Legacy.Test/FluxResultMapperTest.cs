using System;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Internal;
using NodaTime;
using NUnit.Framework;

namespace Client.Legacy.Test
{
    [TestFixture]
    public class FluxResultMapperTest
    {
        private FluxResultMapper _parser;

        [SetUp]
        public void SetUp()
        {
            _parser = new FluxResultMapper();
        }

        [Test]
        public void MapByColumnName()
        {
            var fluxRecord = new FluxRecord(0);
            fluxRecord.Values["host"] = "aws.north.1";
            fluxRecord.Values["region"] = "carolina";

            var poco = _parser.ToPoco<PocoDifferentNameProperty>(fluxRecord);
            
            Assert.AreEqual("aws.north.1", poco.Host);
            Assert.AreEqual("carolina", poco.DifferentName);
        }

        [Test]
        public void MapByColumnNameType()
        {
            var fluxRecord = new FluxRecord(0);
            fluxRecord.Values["host"] = "aws.north.1";
            fluxRecord.Values["region"] = "carolina";

            var poco = _parser.ToPoco(fluxRecord, typeof(PocoDifferentNameProperty)) as PocoDifferentNameProperty;

            Assert.AreEqual("aws.north.1", poco.Host);
            Assert.AreEqual("carolina", poco.DifferentName);
        }

        private class PocoDifferentNameProperty
        {
            [Column("host")] 
            public string Host { get; set; }

            [Column("region")]
            public string DifferentName { get; set; }
        }

        [Test]
        public void MapTimestampWithInstantTime()
        {
            var now = DateTime.UtcNow;

            var fluxRecord = new FluxRecord(0);
            fluxRecord.Values["tag"] = "production";
            fluxRecord.Values["min"] = 10.5;
            fluxRecord.Values["max"] = 20.0;
            fluxRecord.Values["avg"] = 18.0D;
            fluxRecord.Values["_time"] = Instant.FromDateTimeUtc(now);

            var poco = _parser.ToPoco<PointWithoutTimestampNameInstant>(fluxRecord);

            Assert.AreEqual("production", poco.Tag);
            Assert.AreEqual(10.5, poco.Minimum);
            Assert.AreEqual(20, poco.Maximum);
            Assert.AreEqual(18, poco.Average);
            Assert.AreEqual(Instant.FromDateTimeUtc(now), poco.Timestamp);
        }

        [Test]
        public void MapTimestampWithInstantTimeType()
        {
            var now = DateTime.UtcNow;

            var fluxRecord = new FluxRecord(0);
            fluxRecord.Values["tag"] = "production";
            fluxRecord.Values["min"] = 10.5;
            fluxRecord.Values["max"] = 20.0;
            fluxRecord.Values["avg"] = 18.0D;
            fluxRecord.Values["_time"] = Instant.FromDateTimeUtc(now);

            var poco = _parser.ToPoco(fluxRecord, typeof(PointWithoutTimestampNameInstant)) as PointWithoutTimestampNameInstant;

            Assert.AreEqual("production", poco.Tag);
            Assert.AreEqual(10.5, poco.Minimum);
            Assert.AreEqual(20, poco.Maximum);
            Assert.AreEqual(18, poco.Average);
            Assert.AreEqual(Instant.FromDateTimeUtc(now), poco.Timestamp);
        }

        private class PointWithoutTimestampNameInstant
        {
            [Column("tag", IsTag = true)] public string Tag { get; set; }
            [Column("min")] public double Minimum { get; set; }
            [Column("max")] public double Maximum { get; set; }
            [Column("avg")] public double Average { get; set; }
            [Column(IsTimestamp = true)] public Instant Timestamp { get; set; }
        }

        [Test]
        public void MapTimestampDifferentName()
        {
            var now = DateTime.UtcNow;

            var fluxRecord = new FluxRecord(0);
            fluxRecord.Values["tag"] = "production";
            fluxRecord.Values["min"] = 10.5;
            fluxRecord.Values["max"] = 20.0;
            fluxRecord.Values["avg"] = (Double)18;
            fluxRecord.Values["_time"] = Instant.FromDateTimeUtc(now);

            var poco = _parser.ToPoco<PointWithoutTimestampName>(fluxRecord);

            Assert.AreEqual("production", poco.Tag);
            Assert.AreEqual(10.5, poco.Minimum);
            Assert.AreEqual(20, poco.Maximum);
            Assert.AreEqual(18, poco.Average);
            Assert.AreEqual(now, poco.Timestamp);
        }

        [Test]
        public void MapTimestampDifferentNameType()
        {
            var now = DateTime.UtcNow;

            var fluxRecord = new FluxRecord(0);
            fluxRecord.Values["tag"] = "production";
            fluxRecord.Values["min"] = 10.5;
            fluxRecord.Values["max"] = 20.0;
            fluxRecord.Values["avg"] = (Double)18;
            fluxRecord.Values["_time"] = Instant.FromDateTimeUtc(now);

            var poco = _parser.ToPoco(fluxRecord, typeof(PointWithoutTimestampName)) as PointWithoutTimestampName;

            Assert.AreEqual("production", poco.Tag);
            Assert.AreEqual(10.5, poco.Minimum);
            Assert.AreEqual(20, poco.Maximum);
            Assert.AreEqual(18, poco.Average);
            Assert.AreEqual(now, poco.Timestamp);
        }

        private class PointWithoutTimestampName
        {
            [Column("tag", IsTag = true)] public string Tag { get; set; }
            [Column("min")] public double Minimum { get; set; }
            [Column("max")] public double Maximum { get; set; }
            [Column("avg")] public Double Average { get; set; }
            [Column(IsTimestamp = true)] public DateTime Timestamp { get; set; }
        }
    }
}