using System;
using System.Configuration;
using InfluxDB.Client.Writes;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class PointSettingsTest
    {
        private PointSettings _pointSettings;

        [SetUp]
        public void SetUp()
        {
            _pointSettings = new PointSettings();
        }

        [Test]
        public void DefaultTagsEmpty()
        {
            var defaultTags = _pointSettings.GetDefaultTags();

            Assert.IsEmpty(defaultTags);
        }

        [Test]
        public void DefaultTagsValues()
        {
            Environment.SetEnvironmentVariable("datacenter", "LA");
            ConfigurationManager.AppSettings["mine-sensor.version"] = "1.23a";

            _pointSettings
                .AddDefaultTag("id", "132-987-655")
                .AddDefaultTag("customer", "California Miner")
                .AddDefaultTag("env-variable", "${env.datacenter}")
                .AddDefaultTag("sensor-version", "${mine-sensor.version}");

            var defaultTags = _pointSettings.GetDefaultTags();

            Assert.AreEqual(4, defaultTags.Count);
            Assert.AreEqual("132-987-655", defaultTags["id"]);
            Assert.AreEqual("California Miner", defaultTags["customer"]);
            Assert.AreEqual("LA", defaultTags["env-variable"]);
            Assert.AreEqual("1.23a", defaultTags["sensor-version"]);
        }

        [Test]
        public void DefaultTagsWithoutValues()
        {
            _pointSettings
                .AddDefaultTag("id", "132-987-655")
                .AddDefaultTag("customer", "California Miner")
                .AddDefaultTag("hostname", "${env.hostname-not-defined}")
                .AddDefaultTag("sensor-version", "${version-not-defined}");

            var defaultTags = _pointSettings.GetDefaultTags();

            Assert.AreEqual(2, defaultTags.Count);
            Assert.AreEqual("132-987-655", defaultTags["id"]);
            Assert.AreEqual("California Miner", defaultTags["customer"]);
        }

        [Test]
        public void DefaultTagsExpressionNull()
        {
            _pointSettings
                .AddDefaultTag("id", "132-987-655")
                .AddDefaultTag("customer", "California Miner")
                .AddDefaultTag("env-variable", null);

            var defaultTags = _pointSettings.GetDefaultTags();

            Assert.AreEqual(2, defaultTags.Count);
            Assert.AreEqual("132-987-655", defaultTags["id"]);
            Assert.AreEqual("California Miner", defaultTags["customer"]);
        }
    }
}