using System;
using NUnit.Framework;

namespace InfluxDB.Client.Core.Test
{
    [TestFixture]
    public class ArgumentsTest
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void CheckNonNullString()
        {
            Arguments.CheckNotNull("valid", "property");
        }

        [Test]
        public void CheckNonNullStringNull()
        {
            try
            {
                Arguments.CheckNotNull(null, "property");

                Assert.Fail();
            }
            catch (NullReferenceException e)
            {
                Assert.That(e.Message.Equals("Expecting a not null reference for property"));
            }
        }

        [Test]
        public void CheckNonEmptyString()
        {
            Arguments.CheckNonEmptyString("valid", "property");
        }

        [Test]
        public void CheckNonEmptyStringNull()
        {
            try
            {
                Arguments.CheckNonEmptyString(null, "property");

                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.That(e.Message.Equals("Expecting a non-empty string for property"));
            }
        }

        [Test]
        public void CheckNonEmptyStringEmpty()
        {
            try
            {
                Arguments.CheckNonEmptyString("", "property");

                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.That(e.Message.Equals("Expecting a non-empty string for property"));
            }
        }

        [Test]
        public void CheckPositiveNumber()
        {
            Arguments.CheckPositiveNumber(10, "property");
        }

        [Test]
        public void CheckPositiveNumberZero()
        {
            try
            {
                Arguments.CheckPositiveNumber(0, "property");

                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.That(e.Message.Equals("Expecting a positive number for property"));
            }
        }

        [Test]
        public void CheckPositiveNumberNegative()
        {
            try
            {
                Arguments.CheckPositiveNumber(-12, "property");

                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.That(e.Message.Equals("Expecting a positive number for property"));
            }
        }

        [Test]
        public void CheckNotNegativeNumber()
        {
            Arguments.CheckNotNegativeNumber(0, "valid");
        }

        [Test]
        public void CheckNotNegativeNumberNegative()
        {
            try
            {
                Arguments.CheckPositiveNumber(-12, "property");

                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.That(e.Message.Equals("Expecting a positive number for property"));
            }
        }
    }
}