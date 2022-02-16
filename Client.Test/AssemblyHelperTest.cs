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
            Assert.AreEqual(4, Version.Parse(version).Major);
            Assert.GreaterOrEqual(Version.Parse(version).Minor, 0);
            Assert.AreEqual(0, Version.Parse(version).Build);
            Assert.AreEqual(0, Version.Parse(version).Revision);
        }
    }
}