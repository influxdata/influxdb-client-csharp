using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Linq.Internal;
using NUnit.Framework;

namespace Client.Linq.Test
{
    [TestFixture]
    public class InfluxDBQueryVisitorTest
    {
        [Test]
        public void DefaultQuery()
        {
            var visitor = new InfluxDBQueryVisitor("my-bucket");

            const string query = "from(bucket: p1) " +
                                 "|> range(start: p2) " +
                                 "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";
            Assert.AreEqual(query, visitor.BuildFluxQuery());
        }

        [Test]
        public void DefaultAst()
        {
            var visitor = new InfluxDBQueryVisitor("my-bucket");

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
    }
}