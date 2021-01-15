using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Internal;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Linq;
using InfluxDB.Client.Linq.Internal;
using Moq;
using NUnit.Framework;
using Remotion.Linq.Parsing.ExpressionVisitors;
using Remotion.Linq.Parsing.Structure;
using Expression = System.Linq.Expressions.Expression;

namespace Client.Linq.Test
{
    [TestFixture]
    public class InfluxDBQueryVisitorTest
    {
        private QueryApi _queryApi;

        [OneTimeSetUp]
        public void SetUp()
        {
            var options = new InfluxDBClientOptions.Builder()
                .Url("http://localhost:8086")
                .AuthenticateToken("my-token")
                .Build();
            var queryService = new Mock<QueryService>("http://localhost:8086/api/v2");
            _queryApi = new Mock<QueryApi>(options, queryService.Object, new FluxResultMapper()).Object;
        }

        [Test]
        public void DefaultQuery()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;
            var visitor = BuildQueryVisitor(query.Expression);

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";
            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void DefaultAst()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;
            var visitor = BuildQueryVisitor(query.Expression);

            var ast = visitor.BuildFluxAST();

            Assert.NotNull(ast);
            Assert.NotNull(ast.Body);
            Assert.AreEqual(2, ast.Body.Count);

            var bucketAssignment = ((OptionStatement) ast.Body[0]).Assignment as VariableAssignment;
            Assert.AreEqual("p1", bucketAssignment?.Id.Name);
            Assert.AreEqual("my-bucket", (bucketAssignment?.Init as StringLiteral)?.Value);

            var rangeAssignment = ((OptionStatement) ast.Body[1]).Assignment as VariableAssignment;
            Assert.AreEqual("p2", rangeAssignment?.Id.Name);
            Assert.AreEqual("0", (rangeAssignment?.Init as IntegerLiteral)?.Value);
        }

        [Test]
        public void ResultOperatorNotSupported()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;

            var nse = Assert.Throws<NotSupportedException>(() =>
                BuildQueryVisitor(MakeExpression(query, q => q.Count())));

            Assert.AreEqual("CountResultOperator is not supported.", nse.Message);
        }

        [Test]
        public void ResultOperatorTake()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;
            var visitor = BuildQueryVisitor(MakeExpression(query, q => q.Take(10)));

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> limit(n: p3)";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorSkip()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;
            var visitor = BuildQueryVisitor(MakeExpression(query, q => q.Skip(5)));

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorTakeSkip()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;
            var visitor = BuildQueryVisitor(MakeExpression(query, q => q.Take(10).Skip(5)));

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> limit(n: p3, offset: p4)";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorWhereByEqual()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.SensorId == "id-1"
                select s;
            var visitor = BuildQueryVisitor(query.Expression);

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> filter(fn: (r) => (r[\"sensor_id\"] == p3))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorWhereNotEqual()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.SensorId != "id-1"
                select s;
            var visitor = BuildQueryVisitor(query.Expression);

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> filter(fn: (r) => (r[\"sensor_id\"] != p3))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorWhereLessThen()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Value < 10
                select s;
            var visitor = BuildQueryVisitor(query.Expression);

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> filter(fn: (r) => (r[\"data\"] < p3))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorWhereLessThanOrEqual()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Value <= 10
                select s;
            var visitor = BuildQueryVisitor(query.Expression);

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> filter(fn: (r) => (r[\"data\"] <= p3))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorGreaterThan()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Value > 10
                select s;
            var visitor = BuildQueryVisitor(query.Expression);

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> filter(fn: (r) => (r[\"data\"] > p3))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorGreaterThanOrEqual()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Value >= 10
                select s;
            var visitor = BuildQueryVisitor(query.Expression);

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> filter(fn: (r) => (r[\"data\"] >= p3))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void ResultOperatorByTimestamp()
        {
            var month10 = new DateTime(2020, 10, 15, 8, 20, 15, DateTimeKind.Utc);
            var month11 = new DateTime(2020, 11, 15, 8, 20, 15, DateTimeKind.Utc);

            var queries = new[]
            {
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp > month10
                    select s,
                    "range(start: p3)",
                    new Dictionary<int, DateTime> {{2, month10}}
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp < month10
                    select s,
                    "range(start: p2, stop: p3)",
                    new Dictionary<int, DateTime> {{2, month10}}
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp >= month10
                    select s,
                    "range(start: p3)",
                    new Dictionary<int, DateTime> {{2, month10}}
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp <= month10
                    select s,
                    "range(start: p2, stop: p3)",
                    new Dictionary<int, DateTime> {{2, month10}}
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp > month10
                    where s.Timestamp < month11
                    select s,
                    "range(start: p3, stop: p4)",
                    new Dictionary<int, DateTime> {{2, month10}, {3, month11}}
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp >= month10
                    where s.Timestamp <= month11
                    select s,
                    "range(start: p3, stop: p4)",
                    new Dictionary<int, DateTime> {{2, month10}, {3, month11}}
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp >= month10
                    where s.Timestamp < month11
                    select s,
                    "range(start: p3, stop: p4)",
                    new Dictionary<int, DateTime> {{2, month10}, {3, month11}}
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where s.Timestamp == month10
                    select s,
                    "range(start: p3, stop: p3)",
                    new Dictionary<int, DateTime> {{2, month10}}
                ),
                (
                    from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                    where month10 < s.Timestamp
                    where month11 > s.Timestamp
                    select s, "range(start: p3, stop: p4)",
                    new Dictionary<int, DateTime> {{2, month10}, {3, month11}}
                )
            };

            foreach (var (queryable, range, assignments) in queries)
            {
                var visitor = BuildQueryVisitor(queryable.Expression);
                var ast = visitor.BuildFluxAST();

                var expected = "from(bucket: p1) " +
                               $"|> {range} " +
                               "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";

                Assert.AreEqual(expected, visitor.BuildFluxQuery(),
                    $"Expected Range: {range}, Queryable expression: {queryable.Expression}");

                foreach (var (index, dateTime) in assignments)
                {
                    var rangeAssignment = ((OptionStatement) ast.Body[index]).Assignment as VariableAssignment;
                    Assert.AreEqual(dateTime, (rangeAssignment?.Init as DateTimeLiteral)?.Value,
                        $"Expected DateTime: {dateTime} with index: {index} for Queryable expression: {queryable.Expression}");
                }
            }
        }

        [Test]
        public void OrderBy()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                orderby s.Deployment
                select s;
            var visitor = BuildQueryVisitor(query.Expression);

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> sort(columns: [p3], desc: p4)";

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
            var visitor = BuildQueryVisitor(query.Expression);

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> sort(columns: [p3], desc: p4)";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());

            var ast = visitor.BuildFluxAST();
            Assert.AreEqual(true, GetLiteral<BooleanLiteral>(ast, 3).Value);
        }

        [Test]
        public void OrderByTime()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                orderby s.Timestamp
                select s;
            var visitor = BuildQueryVisitor(query.Expression);

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> sort(columns: [p3], desc: p4)";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());

            var ast = visitor.BuildFluxAST();
            Assert.AreEqual("_time", GetLiteral<StringLiteral>(ast, 2).Value);
        }

        private InfluxDBQueryVisitor BuildQueryVisitor(Expression expression)
        {
            var queryModel = QueryParser.CreateDefault().GetParsedQuery(expression);

            var visitor = new InfluxDBQueryVisitor("my-bucket", _queryApi);
            visitor.VisitQueryModel(queryModel);

            return visitor;
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
            return (((OptionStatement) ast.Body[index]).Assignment as VariableAssignment)?.Init as TLiteralType;
        }
    }
}