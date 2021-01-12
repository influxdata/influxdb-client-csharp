using System.Collections.Generic;
using System.Linq;
using InfluxDB.Client.Api.Domain;
using Remotion.Linq;

namespace InfluxDB.Client.Linq.Internal
{
    internal class InfluxDBQueryVisitor : QueryModelVisitorBase
    {
        private readonly QueryAggregator _query;
        private readonly VariableAggregator _variables;

        public InfluxDBQueryVisitor(string bucket)
        {
            _variables = new VariableAggregator();
            var bucketVariable = _variables.AddNamedVariable(bucket);
            var rangeVariable = _variables.AddNamedVariable(0);

            _query = new QueryAggregator(bucketVariable, rangeVariable);
        }

        public Query GenerateQuery()
        {
            var query = new Query(BuildFluxAST(), BuildFluxQuery())
            {
                Dialect = new Dialect
                {
                    Header = true,
                    Delimiter = ",",
                    CommentPrefix = "#",
                    Annotations = new List<Dialect.AnnotationsEnum>
                    {
                        Dialect.AnnotationsEnum.Datatype,
                        Dialect.AnnotationsEnum.Group,
                        Dialect.AnnotationsEnum.Default
                    }
                }
            };

            return query;
        }

        public File BuildFluxAST()
        {
            return new File {Imports = null, Package = null, Body = _variables.GetStatements()};
        }

        public string BuildFluxQuery()
        {
            return _query.BuildFluxQuery();
        }
    }
}