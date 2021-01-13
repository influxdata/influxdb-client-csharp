using System;
using System.Linq.Expressions;
using System.Text;
using InfluxDB.Client.Core;
using Remotion.Linq.Parsing;

namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryExpressionTreeVisitor : ThrowingExpressionVisitor
    {
        private readonly QueryGenerationContext _context;
        private readonly StringBuilder _expression = new StringBuilder();

        private QueryExpressionTreeVisitor(QueryGenerationContext context)
        {
            Arguments.CheckNotNull(context, nameof(context));

            _context = context;
        }

        internal static string GetQueryExpression(Expression expression, QueryGenerationContext context)
        {
            var visitor = new QueryExpressionTreeVisitor(context);
            visitor.Visit(expression);
            return visitor.GetQueryExpression();
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            var namedVariable = _context.Variables.AddNamedVariable(expression.Value);
            _expression.Append(namedVariable);

            return expression;
        }

        private string GetQueryExpression()
        {
            return _expression.ToString();
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            var message = $"The expression '{unhandledItem}', type: '{typeof(T)}' is not supported.";

            return new NotSupportedException(message);
        }
    }
}