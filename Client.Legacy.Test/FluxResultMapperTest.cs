using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Internal;
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
            [Column("host")] 
            public string Host { get; set; }

            [Column("region")]
            public string DifferentName { get; set; }
        }
    }
}