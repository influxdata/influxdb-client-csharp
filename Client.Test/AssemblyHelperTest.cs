using System;
using InfluxDB.Client.Core.Internal;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class AssemblyHelperTest
    {
        [Test]
        public void GetAssemblyVersion()
        {
            var version = AssemblyHelper.GetVersion(typeof(InfluxDBClient));
            Assert.AreEqual(1, Version.Parse(version).Major);
            Assert.Greater(Version.Parse(version).Minor, 5);
            Assert.AreEqual(0, Version.Parse(version).Build);
            Assert.AreEqual(0, Version.Parse(version).Revision);
        }
    }
}