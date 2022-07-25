using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core.Flux.Internal;
using InfluxDB.Client.Core.Test;
using InfluxDB.Client.Linq;
using InfluxDB.Client.Linq.Internal;
using Moq;
using NUnit.Framework;
using Remotion.Linq;
using Remotion.Linq.Parsing.ExpressionVisitors;
using Expression = System.Linq.Expressions.Expression;

namespace Client.Linq.Test
{
    [TestFixture]
    public class InfluxDBQueryVisitorTest : AbstractTest
    {
        private const string FluxStart = "start_shifted = int(v: time(v: p2))\n\n" +
                                         "from(bucket: p1) " +
                                         "|> range(start: time(v: start_shifted)) " +
                                         "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                         "|> drop(columns: [\"_start\", \"_stop\", \"_measurement\"])";

        private QueryApiSync _queryApi;

        [OneTimeSetUp]
        public void InitQueryApi()
        {
            var options = new InfluxDBClientOptions.Builder()
                .Url("http://localhost:8086")
                .AuthenticateToken("my-token")
                .Build();
            var queryService = new Mock<QueryService>("http://localhost:8086/api/v2");
            _queryApi = new Mock<QueryApiSync>(options, queryService.Object, new FluxResultMapper()).Object;
        }

        [Test]
        public void DefaultQuery()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = FluxStart;
            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void DefaultAst()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;
            var visitor = BuildQueryVisitor(query);

            var ast = visitor.BuildFluxAST();

            Assert.NotNull(ast);
            Assert.NotNull(ast.Body);
            Assert.AreEqual(2, ast.Body.Count);

            var bucketAssignment = ((OptionStatement)ast.Body[0]).Assignment as VariableAssignment;
            Assert.AreEqual("p1", bucketAssignment?.Id.Name);
            Assert.AreEqual("my-bucket", (bucketAssignment?.Init as StringLiteral)?.Value);

            var rangeAssignment = ((OptionStatement)ast.Body[1]).Assignment as VariableAssignment;
            Assert.AreEqual("p2", rangeAssignment?.Id.Name);
            Assert.AreEqual("0", (rangeAssignment?.Init as IntegerLiteral)?.Value);
        }

        [Test]
        public void ResultOperatorNotSupported()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;

            var nse = Assert.Throws<NotSupportedException>(() =>
                BuildQueryVisitor(query, MakeExpression(query, q => q.Max())));

            Assert.AreEqual("MaxResultOperator is not supported.", nse.Message);
        }

        [Test]
        public void ResultOperatorTake()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;
            var visitor = BuildQueryVisitor(query, MakeExpression(query, q => q.Take(10)));

            const string expected = FluxStart + " " + "|> limit(n: p3)";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorTakeLast()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;

            var visitor = BuildQueryVisitor(query, MakeExpression(query, q => q.TakeLast(10)));

            var expected = FluxStart + " " + "|> tail(n: p3)";
            Assert.AreEqual(expected, visitor.BuildFluxQuery());

            visitor = BuildQueryVisitor(query, MakeExpression(query, q => q.TakeLast(10).Skip(5)));

            expected = FluxStart + " " + "|> tail(n: p3, offset: p4)";
            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorSkip()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;
            var visitor = BuildQueryVisitor(query, MakeExpression(query, q => q.Skip(5)));

            Assert.AreEqual(FluxStart, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorTakeSkip()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;
            var visitor = BuildQueryVisitor(query, MakeExpression(query, q => q.Take(10).Skip(5)));

            const string expected = FluxStart + " " + "|> limit(n: p3, offset: p4)";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorTakeSkipMultipleTimes()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;

            var queries = new[]
            {
                ((Expression<Func<IQueryable<Sensor>, IQueryable<Sensor>>> expression, string, Dictionary<int, string>))
                (
                    q => q.Take(10).Skip(5),
                    "|> limit(n: p3, offset: p4)",
                    new Dictionary<int, string> { { 2, "10" }, { 3, "5" } }
                ),
                (
                    q => q.Take(2).Take(65252),
                    "|> limit(n: p3) |> limit(n: p4)",
                    new Dictionary<int, string> { { 2, "2" }, { 3, "65252" } }
                ),
                (
                    q => q.Take(10).Skip(5).Take(3),
                    "|> limit(n: p3, offset: p4) |> limit(n: p5)",
                    new Dictionary<int, string> { { 2, "10" }, { 3, "5" }, { 4, "3" } }
                ),
                (
                    q => q.Take(10).Skip(5).Take(3).Skip(2),
                    "|> limit(n: p3, offset: p4) |> limit(n: p5, offset: p6)",
                    new Dictionary<int, string> { { 2, "10" }, { 3, "5" }, { 4, "3" }, { 5, "2" } }
                ),
                (
                    q => q.Take(10).Skip(5).Take(3).Skip(2).Take(663),
                    "|> limit(n: p3, offset: p4) |> limit(n: p5, offset: p6) |> limit(n: p7)",
                    new Dictionary<int, string> { { 2, "10" }, { 3, "5" }, { 4, "3" }, { 5, "2" }, { 6, "663" } }
                ),
                (
                    q => q.Take(10).Skip(5).Skip(2),
                    "|> limit(n: p3, offset: p5)",
                    new Dictionary<int, string> { { 2, "10" }, { 3, "5" }, { 4, "2" } }
                ),
                (
                    q => q.Take(10).Skip(5).Take(3).Skip(2).Skip(15).Take(663),
                    "|> limit(n: p3, offset: p4) |> limit(n: p5, offset: p7) |> limit(n: p8)",
                    new Dictionary<int, string>
                        { { 2, "10" }, { 3, "5" }, { 4, "3" }, { 5, "2" }, { 6, "15" }, { 7, "663" } }
                ),
                (
                    q => q.Skip(5).Take(10),
                    "|> limit(n: p4, offset: p3)",
                    new Dictionary<int, string> { { 2, "5" }, { 3, "10" } }
                )
            };

            foreach (var (expression, flux, assignments) in queries)
            {
                var queryable = MakeExpression(query, expression);
                var visitor = BuildQueryVisitor(query, queryable);
                var ast = visitor.BuildFluxAST();

                var expected = FluxStart + " " + flux;

                Assert.AreEqual(expected, visitor.BuildFluxQuery(),
                    $"Expected Flux: {flux} for expression {expression}.");

                foreach (var (index, value) in assignments)
                    Assert.AreEqual(value, GetLiteral<IntegerLiteral>(ast, index).Value,
                        $"Expected: {value} with index: {index} for Queryable expression: {queryable}");
            }
        }

        [Test]
        public void ResultOperatorWhereByEqual()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Value == 5
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = FluxStart + " " + "|> filter(fn: (r) => (r[\"data\"] == p3))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorWhereNotEqual()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Value != 5
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = FluxStart + " " + "|> filter(fn: (r) => (r[\"data\"] != p3))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorWhereLessThen()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Value < 10
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = FluxStart + " " + "|> filter(fn: (r) => (r[\"data\"] < p3))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorWhereLessThanOrEqual()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Value <= 10
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = FluxStart + " " + "|> filter(fn: (r) => (r[\"data\"] <= p3))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorGreaterThan()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Value > 10
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = FluxStart + " " + "|> filter(fn: (r) => (r[\"data\"] > p3))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorGreaterThanOrEqual()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Value >= 10
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = FluxStart + " " + "|> filter(fn: (r) => (r[\"data\"] >= p3))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorAnd()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Value < 4 && s.Value > 10
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = FluxStart + " " +
                                    "|> filter(fn: (r) => (r[\"data\"] < p3) and (r[\"data\"] > p4))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
            var ast = visitor.BuildFluxAST();
            Assert.AreEqual("4", GetLiteral<IntegerLiteral>(ast, 2).Value);
            Assert.AreEqual("10", GetLiteral<IntegerLiteral>(ast, 3).Value);
        }

        [Test]
        public void ResultOperatorOr()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Value < 5 || s.Value > 10
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = FluxStart + " " +
                                    "|> filter(fn: (r) => (r[\"data\"] < p3) or (r[\"data\"] > p4))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
            var ast = visitor.BuildFluxAST();
            Assert.AreEqual("5", GetLiteral<IntegerLiteral>(ast, 2).Value);
            Assert.AreEqual("10", GetLiteral<IntegerLiteral>(ast, 3).Value);
        }

        [Test]
        public void ResultOperatorByTimestamp()
        {
            var month10 = new DateTime(2020, 10, 15, 8, 20, 15, DateTimeKind.Utc);
            var month11 = new DateTime(2020, 11, 15, 8, 20, 15, DateTimeKind.Utc);

            var defaultStart = DateTime.UtcNow.AddHours(-15);
            var defaultStop = DateTime.UtcNow.AddHours(15);

            var queries = new[]
            {
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp > month10
                    select s,
                    "start_shifted = int(v: time(v: p3)) + 1\n\n",
                    "range(start: time(v: start_shifted))",
                    new Dictionary<int, DateTime> { { 2, month10 } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp < month10
                    select s,
                    "start_shifted = int(v: time(v: p2))\n" +
                    "stop_shifted = int(v: time(v: p3))\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))",
                    new Dictionary<int, DateTime> { { 2, month10 } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp >= month10
                    select s,
                    "start_shifted = int(v: time(v: p3))\n\n",
                    "range(start: time(v: start_shifted))",
                    new Dictionary<int, DateTime> { { 2, month10 } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp <= month10
                    select s,
                    "start_shifted = int(v: time(v: p2))\n" +
                    "stop_shifted = int(v: time(v: p3)) + 1\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))",
                    new Dictionary<int, DateTime> { { 2, month10 } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp > month10
                    where s.Timestamp < month11
                    select s,
                    "start_shifted = int(v: time(v: p3)) + 1\n" +
                    "stop_shifted = int(v: time(v: p4))\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))",
                    new Dictionary<int, DateTime> { { 2, month10 }, { 3, month11 } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp > month10 && s.Timestamp < month11
                    select s,
                    "start_shifted = int(v: time(v: p3)) + 1\n" +
                    "stop_shifted = int(v: time(v: p4))\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))",
                    new Dictionary<int, DateTime> { { 2, month10 }, { 3, month11 } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp >= month10
                    where s.Timestamp <= month11
                    select s,
                    "start_shifted = int(v: time(v: p3))\n" +
                    "stop_shifted = int(v: time(v: p4)) + 1\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))",
                    new Dictionary<int, DateTime> { { 2, month10 }, { 3, month11 } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp >= month10
                    where s.Timestamp < month11
                    select s,
                    "start_shifted = int(v: time(v: p3))\n" +
                    "stop_shifted = int(v: time(v: p4))\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))",
                    new Dictionary<int, DateTime> { { 2, month10 }, { 3, month11 } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp == month10
                    select s,
                    "start_shifted = int(v: time(v: p3))\n" +
                    "stop_shifted = int(v: time(v: p3)) + 1\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))",
                    new Dictionary<int, DateTime> { { 2, month10 } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where month10 < s.Timestamp
                    where month11 > s.Timestamp
                    select s,
                    "start_shifted = int(v: time(v: p3)) + 1\n" +
                    "stop_shifted = int(v: time(v: p4))\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))",
                    new Dictionary<int, DateTime> { { 2, month10 }, { 3, month11 } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi,
                        new QueryableOptimizerSettings
                        {
                            RangeStartValue = defaultStart
                        })
                    select s,
                    "start_shifted = int(v: time(v: p2))\n\n",
                    "range(start: time(v: start_shifted))",
                    new Dictionary<int, DateTime> { { 1, defaultStart } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi,
                        new QueryableOptimizerSettings
                        {
                            RangeStopValue = defaultStop
                        })
                    select s,
                    "start_shifted = int(v: time(v: p2))\n" +
                    "stop_shifted = int(v: time(v: p3))\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))",
                    new Dictionary<int, DateTime> { { 2, defaultStop } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi,
                        new QueryableOptimizerSettings
                        {
                            RangeStartValue = defaultStart,
                            RangeStopValue = defaultStop
                        })
                    select s,
                    "start_shifted = int(v: time(v: p2))\n" +
                    "stop_shifted = int(v: time(v: p3))\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))",
                    new Dictionary<int, DateTime> { { 1, defaultStart }, { 2, defaultStop } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi,
                        new QueryableOptimizerSettings
                        {
                            RangeStartValue = defaultStart,
                            RangeStopValue = defaultStop
                        })
                    where s.Timestamp >= month10
                    select s,
                    "start_shifted = int(v: time(v: p4))\n" +
                    "stop_shifted = int(v: time(v: p3))\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))",
                    new Dictionary<int, DateTime> { { 3, month10 }, { 2, defaultStop } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi,
                        new QueryableOptimizerSettings
                        {
                            RangeStartValue = defaultStart,
                            RangeStopValue = defaultStop
                        })
                    where s.Timestamp >= month10
                    where s.Timestamp < month11
                    select s,
                    "start_shifted = int(v: time(v: p4))\n" +
                    "stop_shifted = int(v: time(v: p5))\n\n",
                    "range(start: time(v: start_shifted), stop: time(v: stop_shifted))",
                    new Dictionary<int, DateTime> { { 3, month10 }, { 4, month11 } }
                )
            };

            foreach (var (queryable, shift, range, assignments) in queries)
            {
                var visitor = BuildQueryVisitor(queryable);
                var ast = visitor.BuildFluxAST();

                var expected = shift +
                               "from(bucket: p1) " +
                               $"|> {range} " +
                               "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                               "|> drop(columns: [\"_start\", \"_stop\", \"_measurement\"])";

                Assert.AreEqual(expected, visitor.BuildFluxQuery(),
                    $"Expected Range: {range}, Shift: {shift}, Queryable expression: {queryable.Expression}");

                foreach (var (index, dateTime) in assignments)
                    Assert.AreEqual(dateTime, GetLiteral<DateTimeLiteral>(ast, index).Value,
                        $"Expected DateTime: {dateTime} with index: {index} for Queryable expression: {queryable.Expression}");
            }
        }

        [Test]
        public void ResultOperatorByMeasurement()
        {
            var settings = new QueryableOptimizerSettings
            {
                DropMeasurementColumn = false
            };

            var query = from s in InfluxDBQueryable<SensorWithCustomMeasurement>.Queryable("my-bucket", "my-org",
                    _queryApi, settings)
                where s.Value > 10
                where s.Measurement == "my-measurement"
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = "start_shifted = int(v: time(v: p2))\n\nfrom(bucket: p1) " +
                                    "|> range(start: time(v: start_shifted)) " +
                                    "|> filter(fn: (r) => (r[\"_measurement\"] == p4)) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> drop(columns: [\"_start\", \"_stop\"]) " +
                                    "|> filter(fn: (r) => (r[\"data\"] > p3))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());

            var ast = visitor.BuildFluxAST();

            var measurementAssignment = ((OptionStatement)ast.Body[3]).Assignment as VariableAssignment;
            Assert.AreEqual("p4", measurementAssignment?.Id.Name);
            Assert.AreEqual("my-measurement", (measurementAssignment?.Init as StringLiteral)?.Value);
        }

        [Test]
        public void TimestampAsDateTimeOffset()
        {
            var start = new DateTimeOffset(2020, 10, 15, 8, 20, 15,
                new TimeSpan(0, 0, 0));
            var stop = new DateTimeOffset(2020, 11, 15, 8, 20, 15,
                new TimeSpan(0, 0, 0));

            var query = from s in InfluxDBQueryable<SensorDateTimeOffset>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Timestamp >= start
                where s.Timestamp <= stop
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = "start_shifted = int(v: time(v: p3))\n" +
                                    "stop_shifted = int(v: time(v: p4)) + 1\n\n" +
                                    "from(bucket: p1) " +
                                    "|> range(start: time(v: start_shifted), stop: time(v: stop_shifted)) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> drop(columns: [\"_start\", \"_stop\", \"_measurement\"])";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
            var ast = visitor.BuildFluxAST();
            Assert.AreEqual(start.UtcDateTime, GetLiteral<DateTimeLiteral>(ast, 2).Value);
            Assert.AreEqual(stop.UtcDateTime, GetLiteral<DateTimeLiteral>(ast, 3).Value);
        }

        [Test]
        public void ResultOperatorAny()
        {
            var memberResolver = new MemberNameResolver();

            var queries = new[]
            {
                (
                    from s in InfluxDBQueryable<SensorCustom>.Queryable("my-bucket", "my-org", _queryApi,
                        memberResolver)
                    where s.Attributes.Any(a => a.Name == "quality" && a.Value == "good")
                    select s,
                    "(r[\"attribute_quality\"] == p4)",
                    new Dictionary<int, string> { { 3, "good" } }
                ),
                (
                    from s in InfluxDBQueryable<SensorCustom>.Queryable("my-bucket", "my-org", _queryApi,
                        memberResolver)
                    where s.Attributes.Any(a => "quality" == a.Name && "good" == a.Value)
                    select s,
                    "(r[\"attribute_quality\"] == p4)",
                    new Dictionary<int, string> { { 3, "good" } }
                ),
                (
                    from s in InfluxDBQueryable<SensorCustom>.Queryable("my-bucket", "my-org", _queryApi,
                        memberResolver)
                    where s.Attributes.Any(a => a.Value == "good" && a.Name == "quality")
                    select s,
                    "(p3 == r[\"attribute_quality\"])",
                    new Dictionary<int, string> { { 2, "good" } }
                )
            };

            foreach (var (queryable, filter, assignments) in queries)
            {
                var visitor = BuildQueryVisitor(queryable);
                var ast = visitor.BuildFluxAST();

                var expected = FluxStart + " " +
                               $"|> filter(fn: (r) => {filter})";

                Assert.AreEqual(expected, visitor.BuildFluxQuery(),
                    $"Expected Filter: {filter}, Queryable expression: {queryable.Expression}");

                foreach (var (index, value) in assignments)
                    Assert.AreEqual(value, GetLiteral<StringLiteral>(ast, index).Value,
                        $"Expected Literal: {value} with index: {index} for Queryable expression: {queryable.Expression}");
            }
        }

        [Test]
        public void ResultOperatorCount()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;
            var visitor = BuildQueryVisitor(query, MakeExpression(query, q => q.Count()));

            const string expected = FluxStart + " " +
                                    "|> stateCount(fn: (r) => true, column: \"linq_result_column\") " +
                                    "|> last(column: \"linq_result_column\") " +
                                    "|> keep(columns: [\"linq_result_column\"])";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorLongCount()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;
            var visitor = BuildQueryVisitor(query, MakeExpression(query, q => q.LongCount()));

            const string expected = FluxStart + " " +
                                    "|> stateCount(fn: (r) => true, column: \"linq_result_column\") " +
                                    "|> last(column: \"linq_result_column\") " +
                                    "|> keep(columns: [\"linq_result_column\"])";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorContainsField()
        {
            int[] values = { 15, 28 };

            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where values.Contains(s.Value)
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = "start_shifted = int(v: time(v: p2))\n\nfrom(bucket: p1) " +
                                    "|> range(start: time(v: start_shifted)) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> drop(columns: [\"_start\", \"_stop\", \"_measurement\"]) " +
                                    "|> filter(fn: (r) => contains(value: r[\"data\"], set: p3))";

            var actual = visitor.BuildFluxQuery();
            Assert.AreEqual(expected, actual);

            var ast = visitor.BuildFluxAST();

            var arrayAssignment = ((OptionStatement)ast.Body[2]).Assignment as VariableAssignment;
            var arrayAssignmentValues =
                (arrayAssignment.Init as ArrayExpression).Elements
                .Cast<IntegerLiteral>()
                .Select(i => i.Value)
                .Select(int.Parse)
                .ToArray();

            Assert.AreEqual("p3", arrayAssignment.Id.Name);
            Assert.AreEqual(values, arrayAssignmentValues);
        }

        [Test]
        public void ResultOperatorContainsTag()
        {
            string[] deployment = { "production", "testing" };

            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where deployment.Contains(s.Deployment)
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = "start_shifted = int(v: time(v: p2))\n\nfrom(bucket: p1) " +
                                    "|> range(start: time(v: start_shifted)) " +
                                    "|> filter(fn: (r) => contains(value: r[\"deployment\"], set: p3)) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> drop(columns: [\"_start\", \"_stop\", \"_measurement\"])";

            var actual = visitor.BuildFluxQuery();
            Assert.AreEqual(expected, actual);

            var ast = visitor.BuildFluxAST();

            var arrayAssignment = ((OptionStatement)ast.Body[2]).Assignment as VariableAssignment;
            var arrayAssignmentValues =
                (arrayAssignment.Init as ArrayExpression).Elements
                .Cast<StringLiteral>()
                .Select(i => i.Value)
                .ToArray();

            Assert.AreEqual("p3", arrayAssignment.Id.Name);
            Assert.AreEqual(deployment, arrayAssignmentValues);
        }

        [Test]
        public void UnaryExpressionConvert()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Deployment == Convert.ToString("d")
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = "start_shifted = int(v: time(v: p2))\n\n" +
                                    "from(bucket: p1) " +
                                    "|> range(start: time(v: start_shifted)) " +
                                    "|> filter(fn: (r) => (r[\"deployment\"] == p3)) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> drop(columns: [\"_start\", \"_stop\", \"_measurement\"])";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());

            Expression equalExpr = Expression.Equal(
                Expression.PropertyOrField(
                    Expression.Constant(new Sensor()),
                    "deployment"
                ),
                Expression.Convert(Expression.Constant("production"), typeof(object))
            );

            var aggregator = new VariableAggregator();
            var context =
                new QueryGenerationContext(
                    new QueryAggregator(),
                    aggregator,
                    new DefaultMemberNameResolver(),
                    new QueryableOptimizerSettings());
            var flux = QueryExpressionTreeVisitor.GetFluxExpressions(equalExpr, equalExpr, context).Aggregate(
                new StringBuilder(), (builder, part) =>
                {
                    part.AppendFlux(builder);

                    return builder;
                }).ToString();

            Assert.AreEqual(1, aggregator.GetStatements().Count);
            Assert.AreEqual("production", GetLiteral<StringLiteral>(aggregator.GetStatements()[0]).Value);
            Assert.AreEqual("(deployment == p1)", flux);
        }

        private class MemberNameResolver : IMemberNameResolver
        {
            public MemberType ResolveMemberType(MemberInfo memberInfo)
            {
                if (memberInfo.DeclaringType == typeof(SensorAttribute))
                {
                    return memberInfo.Name switch
                    {
                        "Name" => MemberType.NamedField,
                        "Value" => MemberType.NamedFieldValue,
                        _ => MemberType.Field
                    };
                }

                return memberInfo.Name switch
                {
                    "Time" => MemberType.Timestamp,
                    "Id" => MemberType.Tag,
                    _ => MemberType.Field
                };
            }

            public string GetColumnName(MemberInfo memberInfo)
            {
                return memberInfo.Name;
            }

            public string GetNamedFieldName(MemberInfo memberInfo, object value)
            {
                return "attribute_" + Convert.ToString(value);
            }
        }

        [Test]
        public void OrderBy()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                orderby s.Deployment
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = FluxStart + " " + "|> sort(columns: [p3], desc: p4)";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
            var ast = visitor.BuildFluxAST();
            Assert.AreEqual("deployment", GetLiteral<StringLiteral>(ast, 2).Value);
            Assert.AreEqual(false, GetLiteral<BooleanLiteral>(ast, 3).Value);
        }

        [Test]
        public void OrderByDescending()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                orderby s.Deployment descending
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = FluxStart + " " + "|> sort(columns: [p3], desc: p4)";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());

            var ast = visitor.BuildFluxAST();
            Assert.AreEqual(true, GetLiteral<BooleanLiteral>(ast, 3).Value);
        }

        [Test]
        public void OrderByTime()
        {
            var queries = new[]
            {
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    orderby s.Timestamp
                    select s,
                    "",
                    new Dictionary<int, object> { { 2, "_time" }, { 3, false } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    orderby s.Timestamp descending
                    select s,
                    " |> sort(columns: [p3], desc: p4)",
                    new Dictionary<int, object> { { 2, "_time" }, { 3, true } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi,
                        new QueryableOptimizerSettings { QueryMultipleTimeSeries = true })
                    orderby s.Timestamp
                    select s,
                    " |> group() |> sort(columns: [p3], desc: p4)",
                    new Dictionary<int, object> { { 2, "_time" }, { 3, false } }
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi,
                        new QueryableOptimizerSettings { QueryMultipleTimeSeries = true })
                    orderby s.Timestamp descending
                    select s,
                    " |> group() |> sort(columns: [p3], desc: p4)",
                    new Dictionary<int, object> { { 2, "_time" }, { 3, true } }
                )
            };

            foreach (var (queryable, sort, assignments) in queries)
            {
                var visitor = BuildQueryVisitor(queryable);
                var ast = visitor.BuildFluxAST();

                var expected = FluxStart + sort;

                Assert.AreEqual(expected, visitor.BuildFluxQuery(),
                    $"Expected Sort: {sort}, Queryable expression: {queryable.Expression}");

                foreach (var (index, value) in assignments)
                {
                    var message =
                        $"Expected Literal: {value} with index: {index} for Queryable expression: {queryable.Expression}";

                    switch (value)
                    {
                        case string _:
                            Assert.AreEqual(value, GetLiteral<StringLiteral>(ast, index).Value, message);
                            break;
                        case bool _:
                            Assert.AreEqual(value, GetLiteral<BooleanLiteral>(ast, index).Value, message);
                            break;
                    }
                }
            }
        }

        [Test]
        public void ToDebugQuery()
        {
            var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.SensorId == "id-1"
                    where s.Value > 12
                    where s.Timestamp > new DateTime(2019, 11, 16, 8, 20, 15, DateTimeKind.Utc)
                    where s.Timestamp < new DateTime(2021, 01, 10, 5, 10, 0, DateTimeKind.Utc)
                    orderby s.Timestamp
                    select s)
                .Take(2)
                .Skip(2);

            const string expected = "start_shifted = int(v: time(v: p5)) + 1\n" +
                                    "stop_shifted = int(v: time(v: p6))\n\n" +
                                    "from(bucket: p1) " +
                                    "|> range(start: time(v: start_shifted), stop: time(v: stop_shifted)) " +
                                    "|> filter(fn: (r) => (r[\"sensor_id\"] == p3)) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> drop(columns: [\"_start\", \"_stop\", \"_measurement\"]) " +
                                    "|> filter(fn: (r) => (r[\"data\"] > p4)) " +
                                    "|> limit(n: p9, offset: p10)";

            Assert.AreEqual(expected, ((InfluxDBQueryable<Sensor>)query).ToDebugQuery()._Query);
        }

        [Test]
        public void ToDebugQueryMultipleTimeSeries()
        {
            var settings = new QueryableOptimizerSettings { QueryMultipleTimeSeries = true };
            var query = (from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi, settings)
                    where s.SensorId == "id-1"
                    where s.Value > 12
                    where s.Timestamp > new DateTime(2019, 11, 16, 8, 20, 15, DateTimeKind.Utc)
                    where s.Timestamp < new DateTime(2021, 01, 10, 5, 10, 0, DateTimeKind.Utc)
                    orderby s.Timestamp
                    select s)
                .Take(2)
                .Skip(2);

            const string expected = "start_shifted = int(v: time(v: p5)) + 1\n" +
                                    "stop_shifted = int(v: time(v: p6))\n\n" +
                                    "from(bucket: p1) " +
                                    "|> range(start: time(v: start_shifted), stop: time(v: stop_shifted)) " +
                                    "|> filter(fn: (r) => (r[\"sensor_id\"] == p3)) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> drop(columns: [\"_start\", \"_stop\", \"_measurement\"]) " +
                                    "|> group() " +
                                    "|> filter(fn: (r) => (r[\"data\"] > p4)) " +
                                    "|> sort(columns: [p7], desc: p8) " +
                                    "|> limit(n: p9, offset: p10)";

            Assert.AreEqual(expected, ((InfluxDBQueryable<Sensor>)query).ToDebugQuery()._Query);
        }

        [Test]
        public void TagIsNotDefinedAsString()
        {
            var fromDateTime = new DateTime(2020, 10, 15, 8, 20, 15, DateTimeKind.Utc);
            var queries = new[]
            {
                (
                    from s in InfluxDBQueryable<TagIsNotDefinedAsString>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp >= fromDateTime
                    where s.Id == 123456
                    select s,
                    "(r[\"Id\"] == p4)"
                ),
                (
                    from s in InfluxDBQueryable<TagIsNotDefinedAsString>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp >= fromDateTime
                    where 123456 == s.Id
                    select s,
                    "(p4 == r[\"Id\"])"
                )
            };

            foreach (var (queryable, expression) in queries)
            {
                var visitor = BuildQueryVisitor(queryable);

                var expected = "start_shifted = int(v: time(v: p3))\n\n" +
                               "from(bucket: p1) " +
                               "|> range(start: time(v: start_shifted)) " +
                               $"|> filter(fn: (r) => {expression}) " +
                               "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                               "|> drop(columns: [\"_start\", \"_stop\", \"_measurement\"])";

                Assert.AreEqual(expected, visitor.BuildFluxQuery());
                var ast = visitor.BuildFluxAST();
                Assert.AreEqual(fromDateTime, GetLiteral<DateTimeLiteral>(ast, 2).Value);
                Assert.AreEqual("123456", GetLiteral<StringLiteral>(ast, 3).Value);
            }
        }

        [Test]
        public void FilterByLong()
        {
            var query = from s in InfluxDBQueryable<DataEntityWithLong>.Queryable("my-bucket", "my-org", _queryApi)
                where s.EndWithTicks <= 637656739543829486
                select s;
            var visitor = BuildQueryVisitor(query);

            Assert.AreEqual(FluxStart + " |> filter(fn: (r) => (r[\"EndWithTicks\"] <= p3))", visitor.BuildFluxQuery());

            var ast = visitor.BuildFluxAST();

            Assert.NotNull(ast);
            Assert.NotNull(ast.Body);
            Assert.AreEqual(3, ast.Body.Count);

            var bucketAssignment = ((OptionStatement)ast.Body[0]).Assignment as VariableAssignment;
            Assert.AreEqual("p1", bucketAssignment?.Id.Name);
            Assert.AreEqual("my-bucket", (bucketAssignment?.Init as StringLiteral)?.Value);

            var rangeAssignment = ((OptionStatement)ast.Body[1]).Assignment as VariableAssignment;
            Assert.AreEqual("p2", rangeAssignment?.Id.Name);
            Assert.AreEqual("0", (rangeAssignment?.Init as IntegerLiteral)?.Value);

            var endWithTicksAssignment = ((OptionStatement)ast.Body[2]).Assignment as VariableAssignment;
            Assert.AreEqual("p3", endWithTicksAssignment?.Id.Name);
            Assert.AreEqual("637656739543829486", (endWithTicksAssignment?.Init as IntegerLiteral)?.Value);
        }

        [Test]
        public void AggregateWindow()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Timestamp.AggregateWindow(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(40), "mean")
                where s.Value == 5
                select s;
            var visitor = BuildQueryVisitor(query);

            StringAssert.Contains("aggregateWindow(every: p3, period: p4, fn: p5)", visitor.BuildFluxQuery());

            var ast = visitor.BuildFluxAST();

            Assert.NotNull(ast);
            Assert.NotNull(ast.Body);
            Assert.AreEqual(6, ast.Body.Count);

            var everyAssignment = ((OptionStatement)ast.Body[2]).Assignment as VariableAssignment;
            Assert.AreEqual("p3", everyAssignment?.Id.Name);
            Assert.AreEqual(20000000, (everyAssignment.Init as DurationLiteral)?.Values[0].Magnitude);
            Assert.AreEqual("us", (everyAssignment.Init as DurationLiteral)?.Values[0].Unit);

            var periodAssignment = ((OptionStatement)ast.Body[3]).Assignment as VariableAssignment;
            Assert.AreEqual("p4", periodAssignment?.Id.Name);
            Assert.AreEqual(40000000, (periodAssignment.Init as DurationLiteral)?.Values[0].Magnitude);
            Assert.AreEqual("us", (periodAssignment.Init as DurationLiteral)?.Values[0].Unit);

            var fnAssignment = ((OptionStatement)ast.Body[4]).Assignment as VariableAssignment;
            Assert.AreEqual("p5", fnAssignment?.Id.Name);
            Assert.AreEqual("mean", (fnAssignment.Init as Identifier)?.Name);
        }

        [Test]
        public void AggregateWindowFluxQuery()
        {
            var queries = new[]
            {
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp.AggregateWindow(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(40), "mean")
                    select s,
                    "aggregateWindow(every: p3, period: p4, fn: p5)",
                    ""
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp.AggregateWindow(TimeSpan.FromSeconds(20), null, "mean")
                    select s,
                    "aggregateWindow(every: p3, fn: p4)",
                    ""
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp.AggregateWindow(TimeSpan.FromSeconds(20), null, "mean")
                    where s.Value == 5
                    select s,
                    "aggregateWindow(every: p3, fn: p4)",
                    " |> filter(fn: (r) => (r[\"data\"] == p5))"
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Value == 5
                    where s.Timestamp.AggregateWindow(TimeSpan.FromSeconds(20), null, "mean")
                    select s,
                    "aggregateWindow(every: p4, fn: p5)",
                    " |> filter(fn: (r) => (r[\"data\"] == p3))"
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Deployment == "prod"
                    where s.Value == 5
                    where s.Timestamp.AggregateWindow(TimeSpan.FromSeconds(20), null, "mean")
                    select s,
                    "filter(fn: (r) => (r[\"deployment\"] == p3)) |> aggregateWindow(every: p5, fn: p6)",
                    " |> filter(fn: (r) => (r[\"data\"] == p4))"
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Deployment == "prod" && s.Value == 5 &&
                          s.Timestamp.AggregateWindow(TimeSpan.FromSeconds(20), null, "mean")
                    select s,
                    "filter(fn: (r) => (r[\"deployment\"] == p3)) |> aggregateWindow(every: p5, fn: p6)",
                    " |> filter(fn: (r) => (r[\"data\"] == p4))"
                )
            };

            foreach (var (queryable, expected, filter) in queries)
            {
                var visitor = BuildQueryVisitor(queryable);

                var flux =
                    "start_shifted = int(v: time(v: p2))\n\nfrom(bucket: p1) |> range(start: time(v: start_shifted)) |> " +
                    expected +
                    " |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") |> drop(columns: [\"_start\", \"_stop\", \"_measurement\"])" +
                    filter;
                Assert.AreEqual(flux, visitor.BuildFluxQuery());
            }
        }

        [Test]
        public void AggregateWindowOnlyForTimestamp()
        {
            var query = from s in InfluxDBQueryable<SensorDateTimeAsField>.Queryable("my-bucket", "my-org", _queryApi)
                where s.DateTimeField.AggregateWindow(TimeSpan.FromSeconds(20), TimeSpan.FromSeconds(40), "mean")
                where s.Value == 5
                select s;

            var nse = Assert.Throws<NotSupportedException>(() => BuildQueryVisitor(query));
            Assert.AreEqual(
                "AggregateWindow() has to be used only for Timestamp member, e.g. [Column(IsTimestamp = true)].",
                nse?.Message);
        }

        [Test]
        public void AlignFieldsWithPivot()
        {
            var queries = new[]
            {
                (
                    true,
                    FluxStart
                ),
                (
                    false,
                    "start_shifted = int(v: time(v: p2))\n\n" +
                    "from(bucket: p1) " +
                    "|> range(start: time(v: start_shifted)) " +
                    "|> drop(columns: [\"_start\", \"_stop\", \"_measurement\"])"
                )
            };

            foreach (var (alignFieldsWithPivot, expected) in queries)
            {
                var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi,
                        new QueryableOptimizerSettings { AlignFieldsWithPivot = alignFieldsWithPivot })
                    select s;
                var visitor = BuildQueryVisitor(query);

                Assert.AreEqual(expected, visitor.BuildFluxQuery());
            }
        }

        [Test]
        public void FilterByTimeAndTagWithAnds()
        {
            var start = new DateTime(2019, 11, 16, 8, 20, 15, DateTimeKind.Utc);
            var stop = new DateTime(2021, 01, 10, 5, 10, 0, DateTimeKind.Utc);

            var query = from s in InfluxDBQueryable<SensorDateTimeOffset>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Timestamp >= start && s.Timestamp < stop && s.SensorId == "id-1"
                select s;
            var visitor = BuildQueryVisitor(query);

            const string expected = "start_shifted = int(v: time(v: p3))\n" +
                                    "stop_shifted = int(v: time(v: p4))\n\n" +
                                    "from(bucket: p1) " +
                                    "|> range(start: time(v: start_shifted), stop: time(v: stop_shifted)) " +
                                    "|> filter(fn: (r) => (r[\"sensor_id\"] == p5)) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> drop(columns: [\"_start\", \"_stop\", \"_measurement\"])";

            Console.WriteLine(visitor.BuildFluxQuery());

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void AlignLimitFunctionBeforePivot()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi,
                    new QueryableOptimizerSettings { AlignLimitFunctionAfterPivot = false })
                select s;

            var visitor = BuildQueryVisitor(query, MakeExpression(query, q => q.TakeLast(10)));
            var expected = "start_shifted = int(v: time(v: p2))\n\n" +
                           "from(bucket: p1) " +
                           "|> range(start: time(v: start_shifted)) " +
                           "|> tail(n: p3) " +
                           "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                           "|> drop(columns: [\"_start\", \"_stop\", \"_measurement\"])";
            Assert.AreEqual(expected, visitor.BuildFluxQuery());

            visitor = BuildQueryVisitor(query, MakeExpression(query, q => q.Take(10)));
            expected = "start_shifted = int(v: time(v: p2))\n\n" +
                       "from(bucket: p1) " +
                       "|> range(start: time(v: start_shifted)) " +
                       "|> limit(n: p3) " +
                       "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                       "|> drop(columns: [\"_start\", \"_stop\", \"_measurement\"])";
            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        private InfluxDBQueryVisitor BuildQueryVisitor(IQueryable queryable, Expression expression = null)
        {
            var queryExecutor = (InfluxDBQueryExecutor)((DefaultQueryProvider)queryable.Provider).Executor;
            var queryModel = InfluxDBQueryable<Sensor>.CreateQueryParser()
                .GetParsedQuery(expression ?? queryable.Expression);
            return queryExecutor.QueryVisitor(queryModel);
        }

        private Expression MakeExpression<TSource, TResult>(IQueryable<TSource> queryable,
            Expression<Func<IQueryable<TSource>, TResult>> func)
        {
            var makeExpression = ReplacingExpressionVisitor
                .Replace(func.Parameters[0], queryable.Expression, func.Body);

            return makeExpression;
        }

        private TLiteralType GetLiteral<TLiteralType>(File ast, int index) where TLiteralType : class
        {
            return GetLiteral<TLiteralType>(ast.Body[index]);
        }

        private TLiteralType GetLiteral<TLiteralType>(Statement statement) where TLiteralType : class
        {
            return (((OptionStatement)statement).Assignment as VariableAssignment)?.Init as TLiteralType;
        }
    }
}