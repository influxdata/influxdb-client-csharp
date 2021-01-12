using System;
using System.Collections.Generic;
using InfluxDB.Client.Api.Domain;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;

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

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            base.VisitResultOperator(resultOperator, queryModel, index);

            switch (resultOperator)
            {
                case TakeResultOperator takeResultOperator:
                    var takeVariable = _variables.AddNamedVariable(takeResultOperator.GetConstantCount());
                    _query.AddLimitN(takeVariable);
                    break;

                case SkipResultOperator skipResultOperator:
                    var skipVariable = _variables.AddNamedVariable(skipResultOperator.GetConstantCount());
                    _query.AddLimitOffset(skipVariable);
                    break;
                default:
                    throw new NotSupportedException($"{resultOperator.GetType().Name} is not supported.");
            }
        }
    }
}