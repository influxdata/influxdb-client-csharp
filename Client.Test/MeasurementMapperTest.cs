using System;
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
                Timestamp =  timeStamp
            };
            
            Assert.AreEqual("poco,tag=value value=\"val\" 10", _mapper.ToPoint(poco, TimeUnit.Seconds).ToLineProtocol());
            Assert.AreEqual("poco,tag=value value=\"val\" 10999", _mapper.ToPoint(poco, TimeUnit.Millis).ToLineProtocol());
            Assert.AreEqual("poco,tag=value value=\"val\" 10999999", _mapper.ToPoint(poco, TimeUnit.Micros).ToLineProtocol());
            Assert.AreEqual("poco,tag=value value=\"val\" 10999999999", _mapper.ToPoint(poco, TimeUnit.Nanos).ToLineProtocol());
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

            var lineProtocol = _mapper.ToPoint(poco, TimeUnit.Seconds).ToLineProtocol();
            
            Assert.AreEqual("poco,tag=tag\\ val ValueWithEmptyName=25,ValueWithoutDefaultName=20i,value=15.444 864000", lineProtocol);
        }
        
        [Test]
        public void DefaultToString()
        {
            var poco = new Poco
            {
                Tag = "value",
                Value = new MyClass()
            };

            var lineProtocol = _mapper.ToPoint(poco, TimeUnit.Seconds).ToLineProtocol();
            
            Assert.AreEqual("poco,tag=value value=\"to-string\"", lineProtocol);
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
            [Column("tag", IsTag = true)] 
            public string Tag { get; set; }

            [Column("value")]
            public Object Value { get; set; }

            [Column]
            public int? ValueWithoutDefaultName { get; set; }

            [Column("")]
            public Double? ValueWithEmptyName { get; set; }
 
            [Column(IsTimestamp = true)]
            public Object Timestamp { get; set; }
        }
    }
}