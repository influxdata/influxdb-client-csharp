using System;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using NodaTime.Text;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class PointDataTest
    {
        [Test]
        public void TagEmptyTagValue()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("log", "to_delete")
                .Tag("log", "")
                .Field("level", 2);

            Assert.AreEqual("h2o,location=europe level=2i", point.ToLineProtocol());
        }

        [Test]
        public void TagEscapingKeyAndValue()
        {
            var point = PointData.Measurement("h\n2\ro\t_data")
                .Tag("new\nline", "new\nline")
                .Tag("carriage\rreturn", "carriage\rreturn")
                .Tag("t\tab", "t\tab")
                .Field("level", 2);

            Assert.AreEqual(
                "h\\n2\\ro\\t_data,carriage\\rreturn=carriage\\rreturn,new\\nline=new\\nline,t\\tab=t\\tab level=2i",
                point.ToLineProtocol());
        }

        [Test]
        public void EqualSignEscaping()
        {
            var point = PointData.Measurement("h=2o")
                .Tag("l=ocation", "e=urope")
                .Field("l=evel", 2);

            Assert.AreEqual("h=2o,l\\=ocation=e\\=urope l\\=evel=2i", point.ToLineProtocol());
        }

        [Test]
        public void Immutability()
        {
            var point = PointData.Measurement("h2 o")
                .Tag("location", "europe");

            var point1 = point
                .Tag("TAG", "VALX")
                .Field("level", 2);

            var point2 = point
                .Tag("TAG", "VALX")
                .Field("level", 2);

            var point3 = point
                .Tag("TAG", "VALY")
                .Field("level", 2);

            Assert.AreEqual(point1, point2);
            Assert.AreNotEqual(point, point1);
            Assert.False(ReferenceEquals(point1, point2));
            Assert.AreNotEqual(point3, point1);
        }

        [Test]
        public void MeasurementEscape()
        {
            var point = PointData.Measurement("h2 o")
                .Tag("location", "europe")
                .Tag("", "warn")
                .Field("level", 2);

            Assert.AreEqual("h2\\ o,location=europe level=2i", point.ToLineProtocol());

            point = PointData.Measurement("h2=o")
                .Tag("location", "europe")
                .Tag("", "warn")
                .Field("level", 2);

            Assert.AreEqual("h2=o,location=europe level=2i", point.ToLineProtocol());

            point = PointData.Measurement("h2,o")
                .Tag("location", "europe")
                .Tag("", "warn")
                .Field("level", 2);

            Assert.AreEqual("h2\\,o,location=europe level=2i", point.ToLineProtocol());
        }

        [Test]
        public void TagEmptyKey()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("", "warn")
                .Field("level", 2);

            Assert.AreEqual("h2o,location=europe level=2i", point.ToLineProtocol());
        }

        [Test]
        public void TagEmptyValue()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("log", "")
                .Field("level", 2);

            Assert.AreEqual("h2o,location=europe level=2i", point.ToLineProtocol());
        }

        [Test]
        public void OverrideTagField()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("location", "europe2")
                .Field("level", 2)
                .Field("level", 3);

            Assert.AreEqual("h2o,location=europe2 level=3i", point.ToLineProtocol());
        }

        [Test]
        public void FieldTypes()
        {
            var point = PointData.Measurement("h2o").Tag("location", "europe")
                .Field("long", 1L)
                .Field("double", 250.69D)
                .Field("float", 35.0F)
                .Field("integer", 7)
                .Field("short", (short)8)
                .Field("byte", (byte)9)
                .Field("ulong", (ulong)10)
                .Field("uint", (uint)11)
                .Field("sbyte", (sbyte)12)
                .Field("ushort", (ushort)13)
                .Field("point", 13.3)
                .Field("decimal", (decimal)25.6)
                .Field("boolean", false)
                .Field("string", "string value");

            var expected =
                "h2o,location=europe boolean=false,byte=9i,decimal=25.6,double=250.69,float=35,integer=7i,long=1i," +
                "point=13.3,sbyte=12i,short=8i,string=\"string value\",uint=11u,ulong=10u,ushort=13u";

            Assert.AreEqual(expected, point.ToLineProtocol());
        }

        [Test]
        public void FieldNullValue()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Field("warning", null);

            Assert.AreEqual("h2o,location=europe level=2i", point.ToLineProtocol());
        }

        [Test]
        public void FieldEscape()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", "string esc\\ape value");

            Assert.AreEqual("h2o,location=europe level=\"string esc\\\\ape value\"", point.ToLineProtocol());

            point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", "string esc\"ape value");

            Assert.AreEqual("h2o,location=europe level=\"string esc\\\"ape value\"", point.ToLineProtocol());
        }

        [Test]
        public void Time()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(123L, WritePrecision.S);

            Assert.AreEqual("h2o,location=europe level=2i 123", point.ToLineProtocol());
        }

        [Test]
        public void TimePrecisionDefault()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2);

            Assert.AreEqual(WritePrecision.Ns, point.Precision);
        }

        [Test]
        public void TimeSpanFormatting()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(TimeSpan.FromDays(1), WritePrecision.Ns);

            Assert.AreEqual("h2o,location=europe level=2i 86400000000000", point.ToLineProtocol());

            point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(TimeSpan.FromHours(356), WritePrecision.Us);

            Assert.AreEqual("h2o,location=europe level=2i 1281600000000", point.ToLineProtocol());

            point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(TimeSpan.FromSeconds(156), WritePrecision.Ms);

            Assert.AreEqual("h2o,location=europe level=2i 156000", point.ToLineProtocol());

            point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(TimeSpan.FromSeconds(123), WritePrecision.S);

            Assert.AreEqual("h2o,location=europe level=2i 123", point.ToLineProtocol());
        }

        [Test]
        public void DateTimeFormatting()
        {
            var dateTime = new DateTime(2015, 10, 15, 8, 20, 15, DateTimeKind.Utc);

            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(dateTime, WritePrecision.Ms);

            Assert.AreEqual("h2o,location=europe level=2i 1444897215000", point.ToLineProtocol());

            dateTime = new DateTime(2015, 10, 15, 8, 20, 15, 750, DateTimeKind.Utc);

            point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", false)
                .Timestamp(dateTime, WritePrecision.S);

            Assert.AreEqual("h2o,location=europe level=false 1444897215", point.ToLineProtocol());

            point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", true)
                .Timestamp(DateTime.UtcNow, WritePrecision.S);

            var lineProtocol = point.ToLineProtocol();
            Assert.IsFalse(lineProtocol.Contains("."));

            point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", true)
                .Timestamp(DateTime.UtcNow, WritePrecision.Ns);

            lineProtocol = point.ToLineProtocol();
            Assert.IsFalse(lineProtocol.Contains("."));
        }

        [Test]
        public void DateTimeUtc()
        {
            var dateTime = new DateTime(2015, 10, 15, 8, 20, 15);

            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2);

            Assert.Throws<ArgumentException>(() => point.Timestamp(dateTime, WritePrecision.Ms));
        }

        [Test]
        public void DateTimeOffsetFormatting()
        {
            var offset = DateTimeOffset.FromUnixTimeSeconds(15678);

            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(offset, WritePrecision.Ns);

            Assert.AreEqual("h2o,location=europe level=2i 15678000000000", point.ToLineProtocol());
        }

        [Test]
        public void InstantFormatting()
        {
            var instant = InstantPattern.ExtendedIso.Parse("1970-01-01T00:00:45.999999999Z").Value;

            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(instant, WritePrecision.S);

            Assert.AreEqual("h2o,location=europe level=2i 45", point.ToLineProtocol());
        }

        [Test]
        public void DefaultTags()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2);

            var defaults = new PointSettings().AddDefaultTag("expensive", "true");

            Assert.AreEqual("h2o,expensive=true,location=europe level=2i", point.ToLineProtocol(defaults));
            Assert.AreEqual("h2o,location=europe level=2i", point.ToLineProtocol());
        }

        [Test]
        public void DefaultTagsOverride()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("expensive", "")
                .Field("level", 2);

            var defaults = new PointSettings().AddDefaultTag("expensive", "true");

            var actual = point.ToLineProtocol(defaults);
            Assert.AreEqual("h2o,expensive=true,location=europe level=2i", actual);
        }

        [Test]
        public void DefaultTagsOverrideNull()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("expensive", null)
                .Field("level", 2);

            var defaults = new PointSettings().AddDefaultTag("expensive", "true");

            Assert.AreEqual("h2o,expensive=true,location=europe level=2i", point.ToLineProtocol(defaults));
        }

        [Test]
        public void DefaultTagsNotOverride()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("expensive", "false")
                .Field("level", 2);

            var defaults = new PointSettings().AddDefaultTag("expensive", "true");

            var lineProtocol = point.ToLineProtocol(defaults);
            Assert.AreEqual("h2o,expensive=false,location=europe level=2i", lineProtocol);
        }

        [Test]
        public void DefaultTagsSorted()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2);

            var defaults = new PointSettings()
                .AddDefaultTag("a-expensive", "true")
                .AddDefaultTag("z-expensive", "false");

            Assert.AreEqual("h2o,a-expensive=true,location=europe,z-expensive=false level=2i",
                point.ToLineProtocol(defaults));
        }

        [Test]
        public void HasFields()
        {
            Assert.IsFalse(PointData.Measurement("h2o").HasFields());
            Assert.IsFalse(PointData.Measurement("h2o").Tag("location", "europe").HasFields());
            Assert.IsTrue(PointData.Measurement("h2o").Field("level", "2").HasFields());
            Assert.IsTrue(PointData.Measurement("h2o").Tag("location", "europe").Field("level", "2").HasFields());
        }

        [Test]
        public void InfinityValues()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("double-infinity-positive", double.PositiveInfinity)
                .Field("double-infinity-negative", double.NegativeInfinity)
                .Field("double-nan", double.NaN)
                .Field("flout-infinity-positive", float.PositiveInfinity)
                .Field("flout-infinity-negative", float.NegativeInfinity)
                .Field("flout-nan", float.NaN)
                .Field("level", 2);

            Assert.AreEqual("h2o,location=europe level=2i", point.ToLineProtocol());
        }

        [Test]
        public void OnlyInfinityValues()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("double-infinity-positive", double.PositiveInfinity)
                .Field("double-infinity-negative", double.NegativeInfinity)
                .Field("double-nan", double.NaN)
                .Field("flout-infinity-positive", float.PositiveInfinity)
                .Field("flout-infinity-negative", float.NegativeInfinity)
                .Field("flout-nan", float.NaN);

            Assert.AreEqual("", point.ToLineProtocol());
        }

        [Test]
        public void UseGenericObjectAsFieldValue()
        {
            var point = PointData.Measurement("h2o")
                .Tag("location", "europe")
                .Field("custom-object", new GenericObject { Value1 = "test", Value2 = 10 });

            Assert.AreEqual("h2o,location=europe custom-object=\"test-10\"", point.ToLineProtocol());
        }
    }

    internal class GenericObject
    {
        internal string Value1 { get; set; }
        internal int Value2 { get; set; }

        public override string ToString()
        {
            return $"{Value1}-{Value2}";
        }
    }
}