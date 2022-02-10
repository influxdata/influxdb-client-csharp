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

        private class PocoDifferentNameProperty
        {
            [Column("host")] public string Host { get; set; }

            [Column("region")] public string DifferentName { get; set; }
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
            fluxRecord.Values["avg"] = (double)18;
            fluxRecord.Values["_time"] = Instant.FromDateTimeUtc(now);

            var poco = _parser.ToPoco<PointWithoutTimestampName>(fluxRecord);

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
            [Column("avg")] public double Average { get; set; }
            [Column(IsTimestamp = true)] public DateTime Timestamp { get; set; }
        }

        [Test]
        public void ParseableProperty()
        {
            var expectedTag = "test";
            var expectedValue = Guid.NewGuid();
            var expectedTime = Instant.FromDateTimeUtc(DateTime.UtcNow);

            var record = new FluxRecord(0);
            record.Values["tag"] = expectedTag;
            record.Values["value"] = expectedValue.ToString();
            record.Values["_time"] = expectedTime;

            var poco = _parser.ToPoco<ParseablePoco>(record);

            Assert.AreEqual(expectedTag, poco.Tag);
            Assert.AreEqual(expectedValue, poco.Value);
            Assert.AreEqual(expectedTime, Instant.FromDateTimeUtc(poco.Timestamp));
        }

        [TestCase(null)]
        [TestCase("e11351a6-62ec-468b-8b64-e1414aca2c7d")]
        public void NullableParseableProperty(string guid)
        {
            var expectedTag = "test";
            var expectedValue = guid == null ? (Guid?)null : Guid.Parse(guid);
            var expectedTime = Instant.FromDateTimeUtc(DateTime.UtcNow);

            var record = new FluxRecord(0);
            record.Values["tag"] = expectedTag;
            record.Values["value"] = guid;
            record.Values["_time"] = expectedTime;

            var poco = _parser.ToPoco<NullableParseablePoco>(record);

            Assert.AreEqual(expectedTag, poco.Tag);
            Assert.AreEqual(expectedValue, poco.Value);
            Assert.AreEqual(expectedTime, Instant.FromDateTimeUtc(poco.Timestamp));
        }

        [Test]
        public void NullableTimestamp()
        {
            var expectedTime = Instant.FromDateTimeUtc(DateTime.UtcNow);

            var record = new FluxRecord(0)
            {
                Values =
                {
                    ["tag"] = "test",
                    ["value"] = 20D,
                    ["_time"] = expectedTime
                }
            };

            var poco = _parser.ToPoco<NullableTimestampPoco>(record);

            Assert.AreEqual("test", poco.Tag);
            Assert.AreEqual(20D, poco.Value);
            Assert.NotNull(poco.Time);
            Assert.AreEqual(expectedTime, Instant.FromDateTimeUtc(poco.Time.Value));
        }

        [Measurement("poco")]
        private class ParseablePoco
        {
            [Column("tag", IsTag = true)] public string Tag { get; set; }

            [Column("value")] public Guid Value { get; set; }

            [Column(IsTimestamp = true)] public DateTime Timestamp { get; set; }
        }

        [Measurement("poco")]
        private class NullableParseablePoco
        {
            [Column("tag", IsTag = true)] public string Tag { get; set; }

            [Column("value")] public Guid? Value { get; set; }

            [Column(IsTimestamp = true)] public DateTime Timestamp { get; set; }
        }

        [Measurement("poco")]
        private class NullableTimestampPoco
        {
            [Column("tag", IsTag = true)] public string Tag { get; set; }

            [Column("value")] public double Value { get; set; }

            [Column(IsTimestamp = true)] public DateTime? Time { get; set; }
        }
    }
}