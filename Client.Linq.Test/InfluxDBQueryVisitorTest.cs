using System;
using System.Linq;
using System.Linq.Expressions;
using InfluxDB.Client;
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
            _queryApi = new Mock<QueryApi>(options, queryService.Object).Object;
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
            
            var nse = Assert.Throws<NotSupportedException>(() => BuildQueryVisitor(MakeExpression(query, q => q.Count())));
            
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

        private static InfluxDBQueryVisitor BuildQueryVisitor(Expression expression)
        {
            var queryModel = QueryParser.CreateDefault().GetParsedQuery(expression);

            var visitor = new InfluxDBQueryVisitor("my-bucket");
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
    }
}