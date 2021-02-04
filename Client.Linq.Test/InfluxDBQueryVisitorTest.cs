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
using Remotion.Linq.Parsing.ExpressionVisitors;
using Remotion.Linq.Parsing.Structure;
using Expression = System.Linq.Expressions.Expression;

namespace Client.Linq.Test
{
    [TestFixture]
    public class InfluxDBQueryVisitorTest : AbstractTest
    {
        private QueryApi _queryApi;

        [OneTimeSetUp]
        public void InitQueryApi()
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
                BuildQueryVisitor(MakeExpression(query, q => q.Max())));

            Assert.AreEqual("MaxResultOperator is not supported.", nse.Message);
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
        public void ResultOperatorAnd()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.SensorId == "my-id" && s.Value > 10
                select s;
            var visitor = BuildQueryVisitor(query.Expression);

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> filter(fn: (r) => ((r[\"sensor_id\"] == p3) and (r[\"data\"] > p4)))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
            var ast = visitor.BuildFluxAST();
            Assert.AreEqual("my-id", GetLiteral<StringLiteral>(ast, 2).Value);
            Assert.AreEqual(10, GetLiteral<FloatLiteral>(ast, 3).Value);
        }

        [Test]
        public void ResultOperatorOr()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.SensorId == "my-id" || s.Value > 10
                select s;
            var visitor = BuildQueryVisitor(query.Expression);

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> filter(fn: (r) => ((r[\"sensor_id\"] == p3) or (r[\"data\"] > p4)))";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
            var ast = visitor.BuildFluxAST();
            Assert.AreEqual("my-id", GetLiteral<StringLiteral>(ast, 2).Value);
            Assert.AreEqual(10, GetLiteral<FloatLiteral>(ast, 3).Value);
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
                    where s.Timestamp > month10 && s.Timestamp < month11
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
                    Assert.AreEqual(dateTime, GetLiteral<DateTimeLiteral>(ast, index).Value,
                        $"Expected DateTime: {dateTime} with index: {index} for Queryable expression: {queryable.Expression}");
                }
            }
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
            var visitor = BuildQueryVisitor(query.Expression);

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p3, stop: p4) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";

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
                    new Dictionary<int, string> {{3, "good"}}
                ),
                (
                    from s in InfluxDBQueryable<SensorCustom>.Queryable("my-bucket", "my-org", _queryApi,
                        memberResolver)
                    where s.Attributes.Any(a => "quality" == a.Name && "good" == a.Value)
                    select s,
                    "(r[\"attribute_quality\"] == p4)",
                    new Dictionary<int, string> {{3, "good"}}
                ),
                (
                    from s in InfluxDBQueryable<SensorCustom>.Queryable("my-bucket", "my-org", _queryApi,
                        memberResolver)
                    where s.Attributes.Any(a => a.Value == "good" && a.Name == "quality")
                    select s,
                    "(p3 == r[\"attribute_quality\"])",
                    new Dictionary<int, string> {{2, "good"}}
                ),
            };

            foreach (var (queryable, filter, assignments) in queries)
            {
                var visitor = BuildQueryVisitor(queryable.Expression, memberResolver);
                var ast = visitor.BuildFluxAST();

                var expected = "from(bucket: p1) " +
                               "|> range(start: p2) " +
                               "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                               $"|> filter(fn: (r) => {filter})";

                Assert.AreEqual(expected, visitor.BuildFluxQuery(),
                    $"Expected Filter: {filter}, Queryable expression: {queryable.Expression}");

                foreach (var (index, value) in assignments)
                {
                    Assert.AreEqual(value, GetLiteral<StringLiteral>(ast, index).Value,
                        $"Expected Literal: {value} with index: {index} for Queryable expression: {queryable.Expression}");
                }
            }
        }

        [Test]
        public void ResultOperatorCount()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                select s;
            var visitor = BuildQueryVisitor(MakeExpression(query, q => q.Count()));

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
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
            var visitor = BuildQueryVisitor(MakeExpression(query, q => q.LongCount()));

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> stateCount(fn: (r) => true, column: \"linq_result_column\") " +
                                    "|> last(column: \"linq_result_column\") " +
                                    "|> keep(columns: [\"linq_result_column\"])";

            Assert.AreEqual(expected, visitor.BuildFluxQuery());
        }

        [Test]
        public void UnaryExpressionConvert()
        {
            var query = from s in InfluxDBQueryable<Sensor>.Queryable("my-bucket", "my-org", _queryApi)
                where s.Deployment == Convert.ToString("d")
                select s;
            var visitor = BuildQueryVisitor(query.Expression);

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p2) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> filter(fn: (r) => (r[\"deployment\"] == p3))";

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
                new QueryGenerationContext(new QueryAggregator(), aggregator, new DefaultMemberNameResolver());
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

            const string expected = "from(bucket: p1) " +
                                    "|> range(start: p5, stop: p6) " +
                                    "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\") " +
                                    "|> filter(fn: (r) => (r[\"sensor_id\"] == p3) and (r[\"data\"] > p4)) " +
                                    "|> sort(columns: [p7], desc: p8) " +
                                    "|> limit(n: p9, offset: p10)";

            Assert.AreEqual(expected, ((InfluxDBQueryable<Sensor>) query).ToDebugQuery()._Query);
        }

        private InfluxDBQueryVisitor BuildQueryVisitor(Expression expression)
        {
            return BuildQueryVisitor(expression, new DefaultMemberNameResolver());
        }

        private InfluxDBQueryVisitor BuildQueryVisitor(Expression expression, IMemberNameResolver memberResolver)
        {
            var queryModel = QueryParser.CreateDefault().GetParsedQuery(expression);

            var visitor = new InfluxDBQueryVisitor("my-bucket", memberResolver);
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
            return GetLiteral<TLiteralType>(ast.Body[index]);
        }

        private TLiteralType GetLiteral<TLiteralType>(Statement statement) where TLiteralType : class
        {
            return (((OptionStatement) statement).Assignment as VariableAssignment)?.Init as TLiteralType;
        }
    }
}