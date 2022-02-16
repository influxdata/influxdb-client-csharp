using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Internal;
using NodaTime.Text;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class MeasurementMapperTest
    {
        private MeasurementMapper _mapper;

        [SetUp]
        public void SetUp()
        {
            _mapper = new MeasurementMapper();
        }

        [Test]
        public void Precision()
        {
            var timeStamp = InstantPattern.ExtendedIso.Parse("1970-01-01T00:00:10.999999999Z").Value;

            var poco = new Poco
            {
                Tag = "value",
                Value = "val",
                Timestamp = timeStamp
            };

            Assert.AreEqual("poco,tag=value value=\"val\" 10",
                _mapper.ToPoint(poco, WritePrecision.S).ToLineProtocol());
            Assert.AreEqual("poco,tag=value value=\"val\" 10999",
                _mapper.ToPoint(poco, WritePrecision.Ms).ToLineProtocol());
            Assert.AreEqual("poco,tag=value value=\"val\" 10999999",
                _mapper.ToPoint(poco, WritePrecision.Us).ToLineProtocol());
            Assert.AreEqual("poco,tag=value value=\"val\" 10999999999",
                _mapper.ToPoint(poco, WritePrecision.Ns).ToLineProtocol());
        }

        [Test]
        public void ColumnWithoutName()
        {
            var poco = new Poco
            {
                Tag = "tag val",
                Value = 15.444,
                ValueWithoutDefaultName = 20,
                ValueWithEmptyName = 25d,
                Timestamp = TimeSpan.FromDays(10)
            };

            var lineProtocol = _mapper.ToPoint(poco, WritePrecision.S).ToLineProtocol();

            Assert.AreEqual("poco,tag=tag\\ val value=15.444,ValueWithEmptyName=25,ValueWithoutDefaultName=20i 864000",
                lineProtocol);
        }

        [Test]
        public void DefaultToString()
        {
            var poco = new Poco
            {
                Tag = "value",
                Value = new MyClass()
            };

            var lineProtocol = _mapper.ToPoint(poco, WritePrecision.S).ToLineProtocol();

            Assert.AreEqual("poco,tag=value value=\"to-string\"", lineProtocol);
        }

        [Test]
        public void HeavyLoad()
        {
            var measurements = new List<Poco>();

            for (var i = 0; i < 500_000; i++)
                measurements.Add(new Poco
                    { Value = i, Tag = "Europe", Timestamp = DateTime.UnixEpoch.Add(TimeSpan.FromSeconds(i)) });

            var stopWatch = new Stopwatch();
            stopWatch.Start();

            var enumerable = measurements.Select(it => _mapper.ToPoint(it, WritePrecision.S).ToLineProtocol());
            var _ = string.Join("\n", enumerable);

            var ts = stopWatch.Elapsed;
            var elapsedTime = $"{ts.Hours:00}:{ts.Minutes:00}:{ts.Seconds:00}.{ts.Milliseconds / 10:00}";

            Assert.LessOrEqual(ts.Seconds, 10, $"Elapsed time: {elapsedTime}");
        }

        [Test]
        public void MeasurementProperty()
        {
            var poco = new MeasurementPropertyPoco
            {
                Measurement = "poco",
                Tag = "tag val",
                Value = 15.444,
                ValueWithoutDefaultName = 20,
                ValueWithEmptyName = 25d,
                Timestamp = TimeSpan.FromDays(10)
            };

            var lineProtocol = _mapper.ToPoint(poco, WritePrecision.S).ToLineProtocol();

            Assert.AreEqual("poco,tag=tag\\ val value=15.444,ValueWithEmptyName=25,ValueWithoutDefaultName=20i 864000",
                lineProtocol);
        }

        [Test]
        public void MeasurementPropertyValidation()
        {
            var poco = new BadMeasurementAttributesPoco
            {
                Measurement = "poco"
            };

            Assert.Throws<InvalidOperationException>(() => _mapper.ToPoint(poco, WritePrecision.S));
        }

        private class MyClass
        {
            public override string ToString()
            {
                return "to-string";
            }
        }

        [Measurement("poco")]
        private class Poco
        {
            [Column("tag", IsTag = true)] public string Tag { get; set; }

            [Column("value")] public object Value { get; set; }

            [Column] public int? ValueWithoutDefaultName { get; set; }

            [Column("")] public double? ValueWithEmptyName { get; set; }

            [Column(IsTimestamp = true)] public object Timestamp { get; set; }
        }

        private class MeasurementPropertyPoco
        {
            [Column(IsMeasurement = true)] public string Measurement { get; set; }

            [Column("tag", IsTag = true)] public string Tag { get; set; }

            [Column("value")] public object Value { get; set; }

            [Column] public int? ValueWithoutDefaultName { get; set; }

            [Column("")] public double? ValueWithEmptyName { get; set; }

            [Column(IsTimestamp = true)] public object Timestamp { get; set; }
        }

        [Measurement("poco")]
        private class BadMeasurementAttributesPoco
        {
            [Column(IsMeasurement = true)] public string Measurement { get; set; }
        }
    }
}