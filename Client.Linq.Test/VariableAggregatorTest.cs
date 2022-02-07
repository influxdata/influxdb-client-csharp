using System;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Test;
using InfluxDB.Client.Linq.Internal;
using NUnit.Framework;

namespace Client.Linq.Test
{
    [TestFixture]
    public class VariableAggregatorTest : AbstractTest
    {
        [Test]
        public void TimeStamp()
        {
            var data = new[]
            {
                (
                    TimeSpan.FromMilliseconds(1),
                    1000
                ),
                (
                    TimeSpan.FromMilliseconds(-1),
                    -1000
                ),
                (
                    TimeSpan.FromDays(2 * 365),
                    63072000000000
                )
            };

            foreach (var (timeSpan, expected) in data)
            {
                var aggregator = new VariableAggregator();
                aggregator.AddNamedVariable(timeSpan);

                var duration =
                    (((aggregator.GetStatements()[0] as OptionStatement)?.Assignment as VariableAssignment)?.Init as
                        DurationLiteral)?.Values[0];
                Assert.NotNull(duration);
                Assert.AreEqual(expected, duration.Magnitude);
            }
        }
    }
}