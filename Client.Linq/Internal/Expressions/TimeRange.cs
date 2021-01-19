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

        internal void AddRange(QueryAggregator queryAggregator)
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
                    queryAggregator.AddRangeStart(assignment);
                    queryAggregator.AddRangeStop(assignment);
                    break;

                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                    if (memberAtLeft)
                    {
                        queryAggregator.AddRangeStop(assignment);
                    }
                    else
                    {
                        queryAggregator.AddRangeStart(assignment);
                    }

                    break;

                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    if (memberAtLeft)
                    {
                        queryAggregator.AddRangeStart(assignment);
                    }
                    else
                    {
                        queryAggregator.AddRangeStop(assignment);
                    }

                    break;

                default:
                    Operator.NotSupported(Operator.Expression);
                    break;
            }
        }
    }
}