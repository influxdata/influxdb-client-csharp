using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using InfluxDB.Client.Core;
using Remotion.Linq.Clauses;
using Remotion.Linq.Parsing;

namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryExpressionTreeVisitor : ThrowingExpressionVisitor
    {
        private readonly object _clause;
        private readonly QueryGenerationContext _context;
        private readonly StringBuilder _fluxExpression = new StringBuilder();

        private QueryExpressionTreeVisitor(object clause, QueryGenerationContext context)
        {
            Arguments.CheckNotNull(clause, nameof(clause));
            Arguments.CheckNotNull(context, nameof(context));

            _clause = clause;
            _context = context;
        }

        internal static string GetFluxExpression(Expression expression, object clause, QueryGenerationContext context)
        {
            Arguments.CheckNotNull(expression, nameof(expression));
            Arguments.CheckNotNull(clause, nameof(clause));
            Arguments.CheckNotNull(context, nameof(context));

            var visitor = new QueryExpressionTreeVisitor(clause, context);
            visitor.Visit(expression);
            return visitor.GetFluxExpression();
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            var namedVariable = _context.Variables.AddNamedVariable(expression.Value);
            _fluxExpression.Append(namedVariable);

            return expression;
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            if (VisitRangeBinaryExpression(expression))
            {
                return expression;
            }

            _fluxExpression.Append("(");
            Visit(expression.Left);

            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    _fluxExpression.Append(" == ");
                    break;

                case ExpressionType.NotEqual:
                    _fluxExpression.Append(" != ");
                    break;

                case ExpressionType.LessThan:
                    _fluxExpression.Append(" < ");
                    break;

                case ExpressionType.LessThanOrEqual:
                    _fluxExpression.Append(" <= ");
                    break;

                case ExpressionType.GreaterThan:
                    _fluxExpression.Append(" > ");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    _fluxExpression.Append(" >= ");
                    break;

                default:
                    base.VisitBinary(expression);
                    break;
            }

            Visit(expression.Right);
            _fluxExpression.Append(")");

            return expression;
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            var mapper = _context.QueryApi.GetFluxResultMapper();
            var propertyInfo = expression.Member as PropertyInfo;

            var columnName = mapper.GetColumnName(propertyInfo);
            if (mapper.IsTimestamp(propertyInfo))
            {
                columnName = "_time";
            }

            if (_clause is WhereClause)
            {
                _fluxExpression
                    .Append("r[\"")
                    .Append(columnName)
                    .Append("\"]");
            }
            else
            {
                _fluxExpression.Append(columnName);
            }

            return expression;
        }

        private string GetFluxExpression()
        {
            return _fluxExpression.ToString();
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            var message = $"The expression '{unhandledItem}', type: '{typeof(T)}' is not supported.";

            return new NotSupportedException(message);
        }

        private bool VisitRangeBinaryExpression(BinaryExpression expression)
        {
            Expression assignmentExpression = null;
            var memberAtLeft = true;

            // Left is Timestamp property
            if (expression.Left is MemberExpression lm)
            {
                var propertyInfo = lm.Member as PropertyInfo;
                if (_context.QueryApi.GetFluxResultMapper().IsTimestamp(propertyInfo))
                {
                    assignmentExpression = expression.Right;
                }
            }
            // Right is Timestamp property
            else if (expression.Right is MemberExpression rm)
            {
                var propertyInfo = rm.Member as PropertyInfo;
                if (_context.QueryApi.GetFluxResultMapper().IsTimestamp(propertyInfo))
                {
                    assignmentExpression = expression.Left;
                    memberAtLeft = false;
                }
            }

            if (assignmentExpression == null)
            {
                return false;
            }

            var assignment = GetFluxExpression(assignmentExpression, _clause, _context);
            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    _context.QueryAggregator.AddRangeStart(assignment);
                    _context.QueryAggregator.AddRangeStop(assignment);
                    break;

                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    if (memberAtLeft)
                    {
                        _context.QueryAggregator.AddRangeStop(assignment);
                    }
                    else
                    {
                        _context.QueryAggregator.AddRangeStart(assignment);
                    }

                    break;

                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    if (memberAtLeft)
                    {
                        _context.QueryAggregator.AddRangeStart(assignment);
                    }
                    else
                    {
                        _context.QueryAggregator.AddRangeStop(assignment);
                    }

                    break;

                default:
                    base.VisitBinary(expression);
                    break;
            }

            return true;
        }
    }
}