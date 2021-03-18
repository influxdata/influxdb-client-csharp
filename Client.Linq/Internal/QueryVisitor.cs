using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
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
            _context.QueryAggregator.AddRangeStart(rangeVariable, RangeExpressionType.GreaterThanOrEqual);
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
            return new File {Imports = new List<ImportDeclaration>(), Package = null, Body = _context.Variables.GetStatements()};
        }

        internal string BuildFluxQuery()
        {
            return _context.QueryAggregator.BuildFluxQuery();
        }

        public override void VisitWhereClause(WhereClause whereClause, QueryModel queryModel, int index)
        {
            base.VisitWhereClause (whereClause, queryModel, index);

            var expressions = GetExpressions(whereClause.Predicate, whereClause).ToList();

            var rangeFilter = new List<IExpressionPart>();
            var tagFilter = new List<IExpressionPart>();
            var fieldFilter = new List<IExpressionPart>();
            
            // Map LINQ filter expresion to right place: range, tag filtering, field filtering
            foreach (var expression in expressions)
            {
                switch (expression)
                {
                    // Range
                    case TimeColumnName _:
                        rangeFilter.Add(expression);
                        break;
                    // Tag
                    case TagColumnName _:
                        tagFilter.Add(expression);
                        break;
                    // Field
                    case RecordColumnName _:
                        fieldFilter.Add(expression);
                        break;
                    case NamedField _:
                        fieldFilter.Add(expression);
                        break;
                    case NamedFieldValue _:
                        fieldFilter.Add(expression);
                        break;
                    // Other expressions: binary operator, parenthesis
                    default:
                        rangeFilter.Add(expression);
                        tagFilter.Add(expression);
                        fieldFilter.Add(expression);
                        break;
                }
            }
            
            QueryExpressionTreeVisitor.NormalizeExpressions(rangeFilter);
            QueryExpressionTreeVisitor.NormalizeExpressions(tagFilter);
            QueryExpressionTreeVisitor.NormalizeExpressions(fieldFilter);
            
            Debug.WriteLine("--- normalized LINQ expressions: ---");
            Debug.WriteLine($"range: {ConcatExpression(rangeFilter)}");
            Debug.WriteLine($"tag: {ConcatExpression(tagFilter)}");
            Debug.WriteLine($"field: {ConcatExpression(fieldFilter)}");
            
            // filter by time
            AddFilterByRange(rangeFilter);

            // filter by tags
            _context.QueryAggregator.AddFilterByTags(ConcatExpression(tagFilter));
            
            // filter by fields
            _context.QueryAggregator.AddFilterByFields(ConcatExpression(fieldFilter));
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
        
                private void AddFilterByRange(List<IExpressionPart> rangeFilter)
        {
            var rangeBinaryIndexes = Enumerable.Range(0, rangeFilter.Count)
                .Where(i => rangeFilter[i] is BinaryOperator)
                .ToList();

            foreach (var rangeBinaryIndex in rangeBinaryIndexes)
            {
                var assignmentValueOnLeft = false;
                // assigned property for filter by timestamp
                var assignmentBuilder = new StringBuilder();
                // Timestamp on left: 'where s.Timestamp > month11'
                if (rangeFilter[rangeBinaryIndex - 1] is TimeColumnName)
                {
                    rangeFilter[rangeBinaryIndex + 1].AppendFlux(assignmentBuilder);
                }

                // Timestamp on right: 'where month11 > s.Timestamp'
                if (rangeFilter[rangeBinaryIndex + 1] is TimeColumnName)
                {
                    assignmentValueOnLeft = true;
                    rangeFilter[rangeBinaryIndex - 1].AppendFlux(assignmentBuilder);
                }

                var assignment = assignmentBuilder.ToString();
                if (assignment.Length > 0)
                {
                    var binaryOperator = (BinaryOperator) rangeFilter[rangeBinaryIndex];
                    switch (binaryOperator.Expression.NodeType)
                    {
                        case ExpressionType.Equal:
                            _context.QueryAggregator.AddRangeStart(assignment, RangeExpressionType.Equal);
                            _context.QueryAggregator.AddRangeStop(assignment, RangeExpressionType.Equal);
                            break;

                        case ExpressionType.LessThan:
                        case ExpressionType.LessThanOrEqual:

                            // assignment value is on left
                            // 'where month11 < s.Timestamp'
                            if (assignmentValueOnLeft)
                            {
                                // => 'where s.Timestamp > month11'
                                var lessExpression = binaryOperator.Expression.NodeType == ExpressionType.LessThan
                                    ? RangeExpressionType.GreaterThan
                                    : RangeExpressionType.GreaterThanOrEqual;

                                _context.QueryAggregator.AddRangeStart(assignment, lessExpression);
                            }
                            else
                            {
                                // => 'where s.Timestamp < month11'
                                var lessExpression = binaryOperator.Expression.NodeType == ExpressionType.LessThan
                                    ? RangeExpressionType.LessThan
                                    : RangeExpressionType.LessThanOrEqual;

                                _context.QueryAggregator.AddRangeStop(assignment, lessExpression);
                            }

                            break;

                        case ExpressionType.GreaterThan:
                        case ExpressionType.GreaterThanOrEqual:

                            // assignment value is on left
                            // 'where month11 > s.Timestamp'
                            if (assignmentValueOnLeft)
                            {
                                // => 'where s.Timestamp < month11'
                                var greaterExpression = binaryOperator.Expression.NodeType == ExpressionType.GreaterThan
                                    ? RangeExpressionType.LessThan
                                    : RangeExpressionType.LessThanOrEqual;

                                _context.QueryAggregator.AddRangeStop(assignment, greaterExpression);
                            }
                            else
                            {
                                // => 'where s.Timestamp > month11'
                                var greaterExpression = binaryOperator.Expression.NodeType == ExpressionType.GreaterThan
                                    ? RangeExpressionType.GreaterThan
                                    : RangeExpressionType.GreaterThanOrEqual;

                                _context.QueryAggregator.AddRangeStart(assignment, greaterExpression);
                            }

                            break;

                        default:
                            binaryOperator.NotSupported(binaryOperator.Expression);
                            break;
                    }
                }
            }
        }
    }
}