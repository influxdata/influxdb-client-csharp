using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using InfluxDB.Client.Core;
using Remotion.Linq.Parsing;

namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryExpressionTreeVisitor : ThrowingExpressionVisitor
    {
        private readonly QueryGenerationContext _context;
        private readonly StringBuilder _fluxExpression = new StringBuilder();

        private QueryExpressionTreeVisitor(QueryGenerationContext context)
        {
            Arguments.CheckNotNull(context, nameof(context));

            _context = context;
        }

        internal static string GetFluxExpression(Expression expression, QueryGenerationContext context)
        {
            Arguments.CheckNotNull(expression, nameof(expression));
            Arguments.CheckNotNull(context, nameof(context));
            
            var visitor = new QueryExpressionTreeVisitor(context);
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
            _fluxExpression.Append ("(");
            Visit(expression.Left);
            
            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                    _fluxExpression.Append (" == ");
                    break;
                
                default:
                    base.VisitBinary(expression);
                    break;
            }

            Visit(expression.Right);
            _fluxExpression.Append (")");
            
            return expression;
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            var propertyInfo = expression.Member as PropertyInfo;
            var columnName = _context.Attributes.GetColumnName(propertyInfo);

            _fluxExpression
                .Append("r[\"")
                .Append(columnName)
                .Append("\"]");
            
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
    }
}