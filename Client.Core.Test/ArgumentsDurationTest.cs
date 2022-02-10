using System;
using NUnit.Framework;

namespace InfluxDB.Client.Core.Test
{
    [TestFixture]
    public class ArgumentsDurationTest
    {
        [SetUp]
        public void SetUp()
        {
        }

        [Test]
        public void Literals()
        {
            Arguments.CheckDuration("1s", "duration");
            Arguments.CheckDuration("10d", "duration");
            Arguments.CheckDuration("1h15m", "duration");
            Arguments.CheckDuration("5w", "duration");
            Arguments.CheckDuration("1mo5d", "duration");
            Arguments.CheckDuration("-1mo5d", "duration");
            Arguments.CheckDuration("inf", "duration");
            Arguments.CheckDuration("-inf", "duration");
        }

        [Test]
        public void LiteralNull()
        {
            try
            {
                Arguments.CheckDuration(null, "duration");

                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.That(e.Message.Equals("Expecting a duration string for duration. But got: "));
            }
        }

        [Test]
        public void LiteralEmpty()
        {
            try
            {
                Arguments.CheckDuration("", "duration");

                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.That(e.Message.Equals("Expecting a duration string for duration. But got: "));
            }
        }

        [Test]
        public void LiteralNotDuration()
        {
            try
            {
                Arguments.CheckDuration("x", "duration");

                Assert.Fail();
            }
            catch (ArgumentException e)
            {
                Assert.That(e.Message.Equals("Expecting a duration string for duration. But got: x"));
            }
        }
    }
}