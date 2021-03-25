using System;
using System.Linq.Expressions;
using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions
{
    internal class BinaryOperator : IExpressionPart
    {
        internal readonly BinaryExpression Expression;
        internal readonly Func<BinaryExpression, Expression> NotSupported;

        internal BinaryOperator(BinaryExpression expression, Func<BinaryExpression, Expression> notSupported)
        {
            Expression = expression;
            NotSupported = notSupported;
        }

        public void AppendFlux(StringBuilder builder)
        {
            builder.Append(" ");
            switch (Expression.NodeType)
            {
                case ExpressionType.Equal:
                    builder.Append("==");
                    break;

                case ExpressionType.NotEqual:
                    builder.Append("!=");
                    break;

                case ExpressionType.LessThan:
                    builder.Append("<");
                    break;

                case ExpressionType.LessThanOrEqual:
                    builder.Append("<=");
                    break;

                case ExpressionType.GreaterThan:
                    builder.Append(">");
                    break;

                case ExpressionType.GreaterThanOrEqual:
                    builder.Append(">=");
                    break;

                case ExpressionType.AndAlso:
                case ExpressionType.And:
                    builder.Append("and");
                    break;

                case ExpressionType.OrElse:
                case ExpressionType.Or:
                    builder.Append("or");
                    break;

                default:
                    NotSupported(Expression);
                    break;
            }

            builder.Append(" ");
        }
    }
}