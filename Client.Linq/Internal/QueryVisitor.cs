using System;
using System.Collections.Generic;
using InfluxDB.Client.Api.Domain;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Expression = System.Linq.Expressions.Expression;

namespace InfluxDB.Client.Linq.Internal
{
    internal class InfluxDBQueryVisitor : QueryModelVisitorBase
    {
        private readonly QueryAggregator _query;
        private readonly QueryGenerationContext _generationContext;

        internal InfluxDBQueryVisitor(string bucket, QueryApi queryApi)
        {
            _generationContext = new QueryGenerationContext(queryApi);
            var bucketVariable = _generationContext.Variables.AddNamedVariable(bucket);
            var rangeVariable = _generationContext.Variables.AddNamedVariable(0);

            _query = new QueryAggregator(bucketVariable, rangeVariable);
        }

        internal Query GenerateQuery()
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

        internal File BuildFluxAST()
        {
            return new File {Imports = null, Package = null, Body = _generationContext.Variables.GetStatements()};
        }

        internal string BuildFluxQuery()
        {
            return _query.BuildFluxQuery();
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            base.VisitWhereClause (whereClause, queryModel, index);

            var filterPart = GetFluxExpression(whereClause.Predicate, whereClause);
            _query.AddFilter(filterPart);
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            base.VisitResultOperator(resultOperator, queryModel, index);

            switch (resultOperator)
            {
                case TakeResultOperator takeResultOperator:
                    var takeVariable = GetFluxExpression(takeResultOperator.Count, resultOperator);
                    _query.AddLimitN(takeVariable);
                    break;

                case SkipResultOperator skipResultOperator:
                    var skipVariable = GetFluxExpression(skipResultOperator.Count, resultOperator);
                    _query.AddLimitOffset(skipVariable);
                    break;
                default:
                    throw new NotSupportedException($"{resultOperator.GetType().Name} is not supported.");
            }
        }

        public override void VisitOrderByClause(OrderByClause orderByClause, QueryModel queryModel, int index)
        {
            base.VisitOrderByClause(orderByClause, queryModel, index);

            foreach (var ordering in orderByClause.Orderings)
            {
                var orderPart = _generationContext.Variables
                    .AddNamedVariable(GetFluxExpression(ordering.Expression, orderByClause));
                var desc = _generationContext.Variables
                    .AddNamedVariable(ordering.OrderingDirection == OrderingDirection.Desc);
                _query.AddOrder(orderPart, desc);
            }
        }

        private string GetFluxExpression(Expression expression, object clause)
        {
            return QueryExpressionTreeVisitor.GetFluxExpression(expression, clause, _generationContext);
        }
    }
}