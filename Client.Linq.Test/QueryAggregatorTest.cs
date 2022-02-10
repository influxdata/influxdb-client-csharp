using InfluxDB.Client.Core.Test;
using InfluxDB.Client.Linq;
using InfluxDB.Client.Linq.Internal;
using NUnit.Framework;

namespace Client.Linq.Test
{
    [TestFixture]
    public class QueryAggregatorTest : AbstractTest
    {
        private QueryAggregator _aggregator;

        [SetUp]
        public void CreateAggregator()
        {
            _aggregator = new QueryAggregator();
        }

        [Test]
        public void Range()
        {
            var ranges = new[]
            {
                (
                    "p1",
                    RangeExpressionType.GreaterThanOrEqual,
                    "p2",
                    RangeExpressionType.LessThan,
                    "start_shifted = int(v: time(v: p1))\n" +
                    "stop_shifted = int(v: time(v: p2))\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))"
                ),
                (
                    "p1",
                    RangeExpressionType.GreaterThan,
                    "p2",
                    RangeExpressionType.LessThan,
                    "start_shifted = int(v: time(v: p1)) + 1\n" +
                    "stop_shifted = int(v: time(v: p2))\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))"
                ),
                (
                    "p1",
                    RangeExpressionType.GreaterThan,
                    "p2",
                    RangeExpressionType.LessThanOrEqual,
                    "start_shifted = int(v: time(v: p1)) + 1\n" +
                    "stop_shifted = int(v: time(v: p2)) + 1\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))"
                ),
                (
                    "p1",
                    RangeExpressionType.Equal,
                    "p2",
                    RangeExpressionType.Equal,
                    "start_shifted = int(v: time(v: p1))\n" +
                    "stop_shifted = int(v: time(v: p2)) + 1\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))"
                ),
                (
                    "p1",
                    RangeExpressionType.GreaterThan,
                    null,
                    RangeExpressionType.Equal,
                    "start_shifted = int(v: time(v: p1)) + 1\n\n",
                    "range(start: time(v: start_shifted))"
                )
            };

            foreach (var (startAssignment, startExpression, stopAssignment, stopExpression, shift, range) in ranges)
            {
                _aggregator.AddBucket("p1");
                _aggregator.AddRangeStart(startAssignment, startExpression);
                _aggregator.AddRangeStop(stopAssignment, stopExpression);

                var expected = shift +
                               "from(bucket: p1) " +
                               $"|> {range} " +
                               "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                               "|> drop(columns: [\"_start\", \"_stop\", \"_measurement\"])";

                var settings = new QueryableOptimizerSettings();
                Assert.AreEqual(expected, _aggregator.BuildFluxQuery(settings),
                    $"Expected Range: {range}, Shift: {shift}");
            }
        }
    }
}