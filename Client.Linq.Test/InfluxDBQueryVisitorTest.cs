using InfluxDB.Client.Linq;
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

            const string query = "from(bucket: \"my-bucket\") " +
                                 "|> range(start: 0) " +
                                 "|> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";
            Assert.AreEqual(query, visitor.BuildFluxQuery());
        }

        [Test]
        public void DefaultAst()
        {
            var visitor = new InfluxDBQueryVisitor("my-bucket");

            var ast = visitor.BuildFluxAST();
            
            Assert.NotNull(ast);
            Assert.Null(ast.Body);
        }
    }
}