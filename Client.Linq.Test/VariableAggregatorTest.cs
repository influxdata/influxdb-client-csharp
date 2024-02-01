using System;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Test;
using InfluxDB.Client.Linq.Internal;
using NodaTime;
using NodaTime.Text;
using NUnit.Framework;
using Duration = NodaTime.Duration;

namespace Client.Linq.Test
{
    [TestFixture]
    public class VariableAggregatorTest : AbstractTest
    {
        [Test]
        public void DurationLiteral()
        {
            var data = new (object, long)[]
            {
                (
                    TimeSpan.FromTicks(1),
                    100
                ),
                (
                    TimeSpan.FromMilliseconds(1),
                    1_000_000
                ),
                (
                    TimeSpan.FromMilliseconds(-1),
                    -1_000_000
                ),
                (
                    TimeSpan.FromDays(2 * 365),
                    63_072_000_000_000_000
                ),
                (
                    Duration.FromNanoseconds(3),
                    3
                ),
                (
                    Duration.FromHours(42) - Duration.FromMilliseconds(50),
                    151_199_950_000_000
                ),
                (
                    Period.FromNanoseconds(-5),
                    -5
                ),
                (
                    new PeriodBuilder
                    {
                        Weeks = 1, Hours = 2, Seconds = 30, Milliseconds = 3, Nanoseconds = 4
                    }.Build(),
                    612_030_003_000_004
                ),
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
                Assert.AreEqual("ns", duration.Unit);
            }
        }

        [Test]
        public void DateTimeLiteral()
        {
            var data = new (object, string)[]
            {
                (
                    new DateTime(2024, 01, 02, 03, 04, 05, 06, DateTimeKind.Utc),
                    "2024-01-02T03:04:05.006Z"
                ),
                (
                    new DateTime(2024, 01, 02, 03, 04, 05, DateTimeKind.Local).AddTicks(678_912_3),
                    new DateTime(2024, 01, 02, 03, 04, 05, DateTimeKind.Local).AddTicks(678_912_3)
                        .ToUniversalTime().ToString("O")
                ),
                (
                    new DateTimeOffset(2024, 01, 02, 03, 04, 05, 06, TimeSpan.FromHours(-5d)),
                    "2024-01-02T08:04:05.006Z"
                ),
                (
                    Instant.FromUtc(2024, 01, 02, 03, 04, 05).PlusNanoseconds(6),
                    "2024-01-02T03:04:05.000000006Z"
                ),
                (
                    new ZonedDateTime(
                        Instant.FromUtc(2024, 01, 02, 03, 04, 05).PlusNanoseconds(6_007_008),
                        DateTimeZone.ForOffset(Offset.FromHoursAndMinutes(-9, 30))),
                    "2024-01-02T03:04:05.006007008Z"
                ),
                (
                    OffsetDateTime.FromDateTimeOffset(new DateTimeOffset(2024, 01, 02, 05, 04, 05, TimeSpan.FromHours(2d)))
                        .PlusNanoseconds(678_912_345),
                    "2024-01-02T03:04:05.678912345Z"
                ),
                (
                    new OffsetDate(new LocalDate(2024, 01, 02), Offset.FromHours(8)),
                    "2024-01-01T16:00:00Z"
                ),
                (
                    new LocalDateTime(2024, 01, 02, 03, 04, 05, 06),
                    "2024-01-02T03:04:05.006Z"
                ),
                (
                    new LocalDate(2024, 01, 02),
                    "2024-01-02T00:00:00Z"
                )
            };

            foreach (var (dateTime, expected) in data)
            {
                var aggregator = new VariableAggregator();
                aggregator.AddNamedVariable(dateTime);

                var instant =
                    (((aggregator.GetStatements()[0] as OptionStatement)?.Assignment as VariableAssignment)?.Init as
                        DateTimeLiteral)?.ValueInstant;
                Assert.NotNull(instant);
                Assert.AreEqual(expected, InstantPattern.ExtendedIso.Format(instant.Value));
            }
        }
    }
}