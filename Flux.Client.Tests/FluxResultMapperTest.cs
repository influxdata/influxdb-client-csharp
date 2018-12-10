using NUnit.Framework;
using Platform.Common.Flux.Domain;
using Platform.Common.Flux.Parser;
using Platform.Common.Platform;

namespace Flux.Client.Tests
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