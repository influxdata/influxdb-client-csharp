using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Linq.Internal.Expressions;
using Remotion.Linq;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Expression = System.Linq.Expressions.Expression;

namespace InfluxDB.Client.Linq.Internal
{
    internal class InfluxDBQueryVisitor : QueryModelVisitorBase
    {
        private readonly QueryGenerationContext _context;

        internal InfluxDBQueryVisitor(string bucket, IMemberNameResolver memberResolver) : 
            this(new QueryGenerationContext(new QueryAggregator(), new VariableAggregator(), memberResolver))
        {
            var bucketVariable = _context.Variables.AddNamedVariable(bucket);
            _context.QueryAggregator.AddBucket(bucketVariable);
            var rangeVariable = _context.Variables.AddNamedVariable(0);
            _context.QueryAggregator.AddRangeStart(rangeVariable, RangeExpressionType.Equal);
        }

        internal InfluxDBQueryVisitor(QueryGenerationContext context)
        {
            _context = context;
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
            return new File {Imports = null, Package = null, Body = _context.Variables.GetStatements()};
        }

        internal string BuildFluxQuery()
        {
            return _context.QueryAggregator.BuildFluxQuery();
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            base.VisitWhereClause (whereClause, queryModel, index);

            var expressions = GetExpressions(whereClause.Predicate, whereClause).ToList();
            
            // range
            foreach (var expressionPart in expressions.Where(it => it is TimeRange))
            {
                var timeRange = (TimeRange) expressionPart;
                timeRange.AddRange(_context.QueryAggregator, _context.Variables);
            }
            
            // filter
            var expressionParts = expressions.Where(it => !(it is TimeRange)).ToList();
            QueryExpressionTreeVisitor.NormalizeEmptyBinary(expressionParts);
            
            var filterPart = ConcatExpression(expressionParts);
            _context.QueryAggregator.AddFilter(filterPart);
        }

        public override void VisitResultOperator(ResultOperatorBase resultOperator, QueryModel queryModel, int index)
        {
            base.VisitResultOperator(resultOperator, queryModel, index);

            switch (resultOperator)
            {
                case TakeResultOperator takeResultOperator:
                    var takeVariable = GetFluxExpression(takeResultOperator.Count, resultOperator);
                    _context.QueryAggregator.AddLimitN(takeVariable);
                    break;

                case SkipResultOperator skipResultOperator:
                    var skipVariable = GetFluxExpression(skipResultOperator.Count, resultOperator);
                    _context.QueryAggregator.AddLimitOffset(skipVariable);
                    break;
                case AnyResultOperator _:
                    break;
                case LongCountResultOperator _:
                case CountResultOperator _:
                    _context.QueryAggregator.AddResultFunction(ResultFunction.Count);
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
                var orderPart = _context.Variables
                    .AddNamedVariable(GetFluxExpression(ordering.Expression, orderByClause));
                var desc = _context.Variables
                    .AddNamedVariable(ordering.OrderingDirection == OrderingDirection.Desc);
                _context.QueryAggregator.AddOrder(orderPart, desc);
            }
        }

        private string GetFluxExpression(Expression expression, object clause)
        {
            return ConcatExpression(GetExpressions(expression, clause));
        }

        private IEnumerable<IExpressionPart> GetExpressions(Expression expression, object clause)
        {
            return QueryExpressionTreeVisitor.GetFluxExpressions(expression, clause, _context);
        }

        private string ConcatExpression(IEnumerable<IExpressionPart> expressions)
        {
            return expressions.Aggregate(new StringBuilder(), (builder, part) =>
            {
                part.AppendFlux(builder);

                return builder;
            }).ToString();
        }
    }
}