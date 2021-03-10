using System.Linq.Expressions;
using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions
{
    internal class TimeRange : IExpressionPart
    {
        internal IExpressionPart Left;
        internal IExpressionPart Right;
        internal BinaryOperator Operator;

        public void AppendFlux(StringBuilder builder)
        {
            builder.Append(Left);
            builder.Append(Operator);
            builder.Append(Right);
        }

        internal void AddRange(QueryAggregator queryAggregator, VariableAggregator variableAggregator)
        {
            bool memberAtLeft;
            var builder = new StringBuilder();

            // requirement value is on left
            if (Left != null)
            {
                memberAtLeft = false;
                Left.AppendFlux(builder);
            }
            // requirement value is on right
            else
            {
                memberAtLeft = true;
                Right.AppendFlux(builder);
            }

            var assignment = builder.ToString();


            switch (Operator.Expression.NodeType)
            {
                case ExpressionType.Equal:
                    queryAggregator.AddRangeStart(assignment, RangeExpressionType.Equal);
                    queryAggregator.AddRangeStop(assignment, RangeExpressionType.Equal);
                    break;

                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    var lessExpression = Operator.Expression.NodeType == ExpressionType.LessThan ? 
                        RangeExpressionType.LessThan : RangeExpressionType.LessThanOrEqual;
                    if (memberAtLeft)
                    {
                        queryAggregator.AddRangeStop(assignment, lessExpression);
                    }
                    else
                    {
                        queryAggregator.AddRangeStart(assignment, lessExpression);
                    }

                    break;

                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    var greaterExpression = Operator.Expression.NodeType == ExpressionType.GreaterThan ? 
                        RangeExpressionType.GreaterThan : RangeExpressionType.GreaterThanOrEqual;
                    if (memberAtLeft)
                    {
                        queryAggregator.AddRangeStart(assignment, greaterExpression);
                    }
                    else
                    {
                        queryAggregator.AddRangeStop(assignment, greaterExpression);
                    }

                    break;

                default:
                    Operator.NotSupported(Operator.Expression);
                    break;
            }
        }
    }
}