using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using InfluxDB.Client.Core;
using InfluxDB.Client.Linq.Internal.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;

namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryExpressionTreeVisitor : ThrowingExpressionVisitor
    {
        private readonly object _clause;

        private readonly QueryGenerationContext _context;
        private readonly List<IExpressionPart> _expressionParts = new List<IExpressionPart>();

        private QueryExpressionTreeVisitor(object clause, QueryGenerationContext context)
        {
            Arguments.CheckNotNull(clause, nameof(clause));
            Arguments.CheckNotNull(context, nameof(context));

            _clause = clause;
            _context = context;
            _expressionParts.Add(new NoOp());
        }

        internal static IEnumerable<IExpressionPart> GetFluxExpressions(Expression expression, object clause,
            QueryGenerationContext context)
        {
            Arguments.CheckNotNull(expression, nameof(expression));
            Arguments.CheckNotNull(clause, nameof(clause));
            Arguments.CheckNotNull(context, nameof(context));

            var visitor = new QueryExpressionTreeVisitor(clause, context);
            visitor.Visit(expression);
            return visitor.GetFluxExpressions();
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            var value = expression.Value;
            var assignmentValue = new AssignmentValue(value, _context.Variables.AddNamedVariable(value));
            _expressionParts.Add(assignmentValue);

            return expression;
        }

        protected override Expression VisitSubQuery(SubQueryExpression subQuery)
        {
            if (subQuery.QueryModel.ResultOperators.All(p => p is AnyResultOperator))
            {
                var query = new QueryAggregator();

                var modelVisitor = new InfluxDBQueryVisitor(_context.Clone(query));
                modelVisitor.VisitQueryModel(subQuery.QueryModel);

                _context.QueryAggregator.AddSubQueries(query);

                return subQuery;
            }

            return base.VisitSubQuery(subQuery);
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            _expressionParts.Add(new LeftParenthesis());
            Visit(expression.Left);

            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                case ExpressionType.OrElse:
                case ExpressionType.Or:
                    _expressionParts.Add(new BinaryOperator(expression, exp => base.VisitBinary(exp)));
                    break;

                default:
                    base.VisitBinary(expression);
                    break;
            }

            Visit(expression.Right);
            _expressionParts.Add(new RightParenthesis());

            return expression;
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            if (_clause is WhereClause)
            {
                switch (_context.MemberResolver.ResolveMemberType(expression.Member))
                {
                    case MemberType.Timestamp:
                        _expressionParts.Add(new TimeRange());
                        break;
                    case MemberType.NamedField:
                        _expressionParts.Add(new NamedField(expression.Member, _context.MemberResolver));
                        break;
                    case MemberType.NamedFieldValue:
                        _expressionParts.Add(new NamedFieldValue());
                        break;
                    default:
                        _expressionParts.Add(new RecordColumnName(expression.Member, _context.MemberResolver));
                        break;
                }
            }
            else
            {
                _expressionParts.Add(new ColumnName(expression.Member, _context.MemberResolver));
            }

            return expression;
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Convert)
            {
                return Visit(expression.Operand);
            }

            return base.VisitUnary(expression);
        }
        
        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            var message = $"The expression '{unhandledItem}', type: '{typeof(T)}' is not supported.";

            return new NotSupportedException(message);
        }

        private IEnumerable<IExpressionPart> GetFluxExpressions()
        {
            _expressionParts.RemoveAll(it => it is NoOp);
            NormalizeTimeRange();
            NormalizeNamedField();
            NormalizeNamedFieldValue();
            return _expressionParts;
        }

        private void NormalizeTimeRange()
        {
            var index = _expressionParts
                .FindIndex(it => it is TimeRange tr && tr.Left == null && tr.Right == null);

            if (index == -1)
            {
                return;
            }

            var timeRange = (TimeRange) _expressionParts[index];
            // TimeRange on left: 'where s.Timestamp > month11'
            if (_expressionParts[index + 1] is BinaryOperator)
            {
                timeRange.Operator = _expressionParts[index + 1] as BinaryOperator;
                timeRange.Right = _expressionParts[index + 2];

                _expressionParts.RemoveAt(index + 3);
                _expressionParts.RemoveAt(index + 2);
                _expressionParts.RemoveAt(index + 1);
                _expressionParts.RemoveAt(index - 1);
            }
            // TimeRange on right: 'where month11 > s.Timestamp'
            else
            {
                timeRange.Operator = _expressionParts[index - 1] as BinaryOperator;
                timeRange.Left = _expressionParts[index - 2];

                _expressionParts.RemoveAt(index + 1);
                _expressionParts.RemoveAt(index - 1);
                _expressionParts.RemoveAt(index - 2);
                _expressionParts.RemoveAt(index - 3);
            }

            NormalizeTimeRange();
        }
        
        private void NormalizeNamedField()
        {
            var index = _expressionParts
                .FindIndex(it => it is NamedField nf && nf.Assignment == null);

            if (index == -1)
            {
                return;
            }

            var namedField = (NamedField) _expressionParts[index];
            // Constant on right: a.Name == "quality"
            if (_expressionParts[index + 1] is BinaryOperator)
            {
                namedField.Assignment = _expressionParts[index + 2] as AssignmentValue;

                _expressionParts.RemoveAt(index + 3);
                _expressionParts.RemoveAt(index + 2);
                _expressionParts.RemoveAt(index + 1);
                _expressionParts.RemoveAt(index - 1);
            }
            // Constant on left: "quality" == a.Name
            else
            {
                namedField.Assignment = _expressionParts[index - 2] as AssignmentValue;

                _expressionParts.RemoveAt(index + 1);
                _expressionParts.RemoveAt(index - 1);
                _expressionParts.RemoveAt(index - 2);
                _expressionParts.RemoveAt(index - 3);
            }

            NormalizeNamedField();
        }
        
        private void NormalizeNamedFieldValue()
        {
            var index = _expressionParts
                .FindIndex(it => it is NamedFieldValue nf && nf.Assignment == null);

            if (index == -1)
            {
                return;
            }

            var namedField = (NamedFieldValue) _expressionParts[index];
            // Constant on right: a.Value == "good"
            if (_expressionParts[index + 1] is BinaryOperator)
            {
                namedField.Operator = _expressionParts[index + 1] as BinaryOperator;
                namedField.Assignment = _expressionParts[index + 2] as AssignmentValue;
                // == "good" ) and NamedField
                var i = index + 5;
                namedField.Left = _expressionParts.Count > i && _expressionParts[i] is NamedField;

                _expressionParts.RemoveAt(index + 3);
                _expressionParts.RemoveAt(index + 2);
                _expressionParts.RemoveAt(index + 1);
                _expressionParts.RemoveAt(index - 1);
            }
            // Constant on left: "good" == a.Value
            else
            {
                namedField.Operator = _expressionParts[index - 1] as BinaryOperator;
                namedField.Assignment = _expressionParts[index - 2] as AssignmentValue;

                // ) and NamedField
                var i = index + 3;
                namedField.Left = _expressionParts.Count > i && _expressionParts[i] is NamedField;
                
                _expressionParts.RemoveAt(index + 1);
                _expressionParts.RemoveAt(index - 1);
                _expressionParts.RemoveAt(index - 2);
                _expressionParts.RemoveAt(index - 3);
            }
            
            // remove trailing operator: r["attribute_quality"] and  == p4
            index = _expressionParts.IndexOf(namedField);
            if (namedField.Left)
            {
                _expressionParts.RemoveAt(index + 1);
            }
            else
            {
                _expressionParts.RemoveAt(index - 1);
            }

            NormalizeNamedFieldValue();
        }

        /// <summary>
        /// Remove "( and )"
        /// </summary>
        internal static void NormalizeEmptyBinary(List<IExpressionPart> parts)
        {
            var index = parts.FindIndex(it => it is BinaryOperator);

            if (index != -1 && parts[index - 1] is LeftParenthesis && parts[index + 1] is RightParenthesis)
            {

                parts.RemoveAt(index + 1);
                parts.RemoveAt(index);
                parts.RemoveAt(index - 1);
                
                NormalizeEmptyBinary(parts);
            }
        }
    }
}