using System;
using System.Diagnostics;
using System.Globalization;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Write;
using NUnit.Framework;

namespace Platform.Client.Tests
{
    [TestFixture]
    public class PointTest
    {
        [Test]
        public void MeasurementEscape()
        {
            Point point = Point.Measurement("h2 o")
                .Tag("location", "europe")
                .Tag("", "warn")
                .Field("level", 2);

            Assert.AreEqual("h2\\ o,location=europe level=2i", point.ToLineProtocol());

            point = Point.Measurement("h2=o")
                .Tag("location", "europe")
                .Tag("", "warn")
                .Field("level", 2);

            Assert.AreEqual("h2\\=o,location=europe level=2i", point.ToLineProtocol());

            point = Point.Measurement("h2,o")
                .Tag("location", "europe")
                .Tag("", "warn")
                .Field("level", 2);

            Assert.AreEqual("h2\\,o,location=europe level=2i", point.ToLineProtocol());
        }

        [Test]
        public void TagEmptyKey()
        {
            Point point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("", "warn")
                .Field("level", 2);

            Assert.AreEqual("h2o,location=europe level=2i", point.ToLineProtocol());
        }

        [Test]
        public void TagEmptyValue()
        {
            Point point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("log", "")
                .Field("level", 2);

            Assert.AreEqual("h2o,location=europe level=2i", point.ToLineProtocol());
        }

        [Test]
        public void OverrideTagField()
        {
            Point point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("location", "europe2")
                .Field("level", 2)
                .Field("level", 3);

            Assert.AreEqual("h2o,location=europe2 level=3i", point.ToLineProtocol());
        }

        [Test]
        public void FieldTypes()
        {
            Point point = Point.Measurement("h2o").Tag("location", "europe")
                .Field("long", 1L)
                .Field("double", 250.69D)
                .Field("float", 35.0F)
                .Field("integer", 7)
                .Field("short", (short) 8)
                .Field("byte", (byte) 9)
                .Field("ulong", (ulong) 10)
                .Field("uint", (uint) 11)
                .Field("sbyte", (sbyte) 12)
                .Field("ushort", (ushort) 13)
                .Field("point", 13.3)
                .Field("decimal", (decimal) 25.6)
                .Field("boolean", false)
                .Field("string", "string value");

            String expected =
                "h2o,location=europe boolean=false,byte=9i,decimal=25.6,double=250.69,float=35,integer=7i,long=1i," +
                "point=13.3,sbyte=12i,short=8i,string=\"string value\",uint=11i,ulong=10i,ushort=13i";

            Assert.AreEqual(expected, point.ToLineProtocol());
        }

        [Test]
        public void FieldNullValue()
        {
            Point point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Field("warning", null);

            Assert.AreEqual("h2o,location=europe level=2i", point.ToLineProtocol());
        }

        [Test]
        public void FieldEscape()
        {
            Point point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", "string esc\\ape value");

            Assert.AreEqual("h2o,location=europe level=\"string esc\\\\ape value\"", point.ToLineProtocol());

            point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", "string esc\"ape value");

            Assert.AreEqual("h2o,location=europe level=\"string esc\\\"ape value\"", point.ToLineProtocol());
        }

        [Test]
        public void Time()
        {
            Point point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(123L, TimeUnit.Seconds);

            Assert.AreEqual("h2o,location=europe level=2i 123", point.ToLineProtocol());
        }

        [Test]
        public void TimePrecisionDefault()
        {
            Point point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2);

            Assert.AreEqual(TimeUnit.Nanos, point.Precision);
        }

        [Test]
        public void TimeSpanFormatting()
        {
            Point point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(TimeSpan.FromDays(1), TimeUnit.Nanos);

            Assert.AreEqual("h2o,location=europe level=2i 86400000000000", point.ToLineProtocol());

            point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(TimeSpan.FromHours(356), TimeUnit.Micros);

            Assert.AreEqual("h2o,location=europe level=2i 1281600000000", point.ToLineProtocol());

            point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(TimeSpan.FromSeconds(156), TimeUnit.Millis);

            Assert.AreEqual("h2o,location=europe level=2i 156000", point.ToLineProtocol());

            point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(TimeSpan.FromSeconds(123), TimeUnit.Seconds);

            Assert.AreEqual("h2o,location=europe level=2i 123", point.ToLineProtocol());
        }

        [Test]
        public void DateTimeFormatting()
        {
            DateTime dateTime = new DateTime(2015, 10, 15, 8, 20, 15, DateTimeKind.Utc);

            Point point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(dateTime, TimeUnit.Millis);

            Assert.AreEqual("h2o,location=europe level=2i 1444897215000", point.ToLineProtocol());
            
            dateTime = new DateTime(2015, 10, 15, 8, 20, 15, 750, DateTimeKind.Utc);

            point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", false)
                .Timestamp(dateTime, TimeUnit.Seconds);
            
            Assert.AreEqual("h2o,location=europe level=false 1444897215", point.ToLineProtocol());
            
            point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", true)
                .Timestamp(DateTime.UtcNow, TimeUnit.Seconds);

            var lineProtocol = point.ToLineProtocol();
            Assert.IsFalse(lineProtocol.Contains("."));
            
            point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", true)
                .Timestamp(DateTime.UtcNow, TimeUnit.Nanos);

            lineProtocol = point.ToLineProtocol();
            Assert.IsFalse(lineProtocol.Contains("."));
        }

        [Test]
        public void DateTimeUtc()
        {
            DateTime dateTime = new DateTime(2015, 10, 15, 8, 20, 15);

            Point point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2);

            Assert.Throws<ArgumentException>(() => point.Timestamp(dateTime, TimeUnit.Millis));
        }

        [Test]
        public void DateTimeOffsetFormatting()
        {
            DateTimeOffset offset = DateTimeOffset.FromUnixTimeSeconds(15678);

            Point point = Point.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(offset, TimeUnit.Nanos);

            Assert.AreEqual("h2o,location=europe level=2i 15678000000000", point.ToLineProtocol());
        }
    }
}