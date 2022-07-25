using System;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Writes;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class PointDataBuilderTest
    {
        [Test]
        public void BuilderValuesToPoint()
        {
            var builder = PointData.Builder.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("log", "some_log")
                .Field("level", 2);

            var point = builder.ToPointData();
            Assert.AreEqual("h2o,location=europe,log=some_log level=2i", point.ToLineProtocol());
        }

        [Test]
        public void TagEmptyRemovesTagValue()
        {
            var builder = PointData.Builder.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("log", "to_delete")
                .Tag("log", "")
                .Field("level", 2);

            var point = builder.ToPointData();
            Assert.AreEqual("h2o,location=europe level=2i", point.ToLineProtocol());
        }

        [Test]
        public void ReplaceTagValue()
        {
            var builder = PointData.Builder.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("log", "old_log")
                .Tag("log", "new_log")
                .Field("level", 2);

            var point = builder.ToPointData();
            Assert.AreEqual("h2o,location=europe,log=new_log level=2i", point.ToLineProtocol());
        }

        [Test]
        public void ReplaceTagValueInNewPoint()
        {
            var builder = PointData.Builder.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("log", "old_log")
                .Field("level", 2);

            var point = builder.ToPointData();

            builder.Tag("log", "new_log");

            var anotherPoint = builder.ToPointData();

            Assert.AreEqual("h2o,location=europe,log=old_log level=2i", point.ToLineProtocol());
            Assert.AreEqual("h2o,location=europe,log=new_log level=2i", anotherPoint.ToLineProtocol());
        }

        [Test]
        public void TagEmptyKey()
        {
            var builder = PointData.Builder.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("", "warn")
                .Field("level", 2);

            var point = builder.ToPointData();

            Assert.AreEqual("h2o,location=europe level=2i", point.ToLineProtocol());
        }

        [Test]
        public void TagEmptyValue()
        {
            var builder = PointData.Builder.Measurement("h2o")
                .Tag("location", "europe")
                .Tag("log", "")
                .Field("level", 2);

            var point = builder.ToPointData();

            Assert.AreEqual("h2o,location=europe level=2i", point.ToLineProtocol());
        }

        [Test]
        public void ReplaceFieldValue()
        {
            var builder = PointData.Builder.Measurement("h2o")
                .Tag("location", "europe2")
                .Field("level", 2)
                .Field("level", 3);

            var point = builder.ToPointData();

            Assert.AreEqual("h2o,location=europe2 level=3i", point.ToLineProtocol());
        }

        [Test]
        public void MultipleFields()
        {
            var builder = PointData.Builder.Measurement("h2o")
                .Tag("location", "europe2")
                .Field("levelA", 2)
                .Field("levelB", 3);

            var point = builder.ToPointData();

            Assert.AreEqual("h2o,location=europe2 levelA=2i,levelB=3i", point.ToLineProtocol());
        }

        [Test]
        public void FieldNullValue()
        {
            var builder = PointData.Builder.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Field("warning", null);

            var point = builder.ToPointData();

            Assert.AreEqual("h2o,location=europe level=2i", point.ToLineProtocol());
        }

        [Test]
        public void Time()
        {
            var builder = PointData.Builder.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2)
                .Timestamp(123L, WritePrecision.S);

            var point = builder.ToPointData();

            Assert.AreEqual("h2o,location=europe level=2i 123", point.ToLineProtocol());
        }

        [Test]
        public void DateTimeMustBeUtc()
        {
            var dateTime = new DateTime(2015, 10, 15, 8, 20, 15);

            var builder = PointData.Builder.Measurement("h2o")
                .Tag("location", "europe")
                .Field("level", 2);

            Assert.Throws<ArgumentException>(() => builder.Timestamp(dateTime, WritePrecision.Ms));
        }

        [Test]
        public void HasFields()
        {
            Assert.IsFalse(PointData.Builder.Measurement("h2o").HasFields());
            Assert.IsFalse(PointData.Builder.Measurement("h2o").Tag("location", "europe").HasFields());
            Assert.IsTrue(PointData.Builder.Measurement("h2o").Field("level", "2").HasFields());
            Assert.IsTrue(
                PointData.Builder.Measurement("h2o").Tag("location", "europe").Field("level", "2").HasFields());
        }

        [Test]
        public void UseGenericObjectAsFieldValue()
        {
            var point = PointData.Builder.Measurement("h2o")
                .Tag("location", "europe")
                .Field("custom-object", new GenericObject { Value1 = "test", Value2 = 10 });

            Assert.AreEqual("h2o,location=europe custom-object=\"test-10\"", point.ToPointData().ToLineProtocol());
        }
    }
}