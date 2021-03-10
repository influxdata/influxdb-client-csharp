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
            var builder = new StringBuilder();

            // assignment value is on left
            if (Left != null)
            {
                Left.AppendFlux(builder);
            }
            // assignment value is on right
            else
            {
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
                    
                    // assignment value is on left
                    // 'where month11 < s.Timestamp'
                    if (Left != null)
                    {
                        // => 'where s.Timestamp > month11'
                        var lessExpression = Operator.Expression.NodeType == ExpressionType.LessThan
                            ? RangeExpressionType.GreaterThan
                            : RangeExpressionType.GreaterThanOrEqual;

                        queryAggregator.AddRangeStart(assignment, lessExpression);
                    }
                    else
                    {
                        // => 'where s.Timestamp < month11'
                        var lessExpression = Operator.Expression.NodeType == ExpressionType.LessThan
                            ? RangeExpressionType.LessThan
                            : RangeExpressionType.LessThanOrEqual;

                        queryAggregator.AddRangeStop(assignment, lessExpression);
                    }

                    break;

                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                    
                    // assignment value is on left
                    // 'where month11 > s.Timestamp'
                    if (Left != null)
                    {
                        // => 'where s.Timestamp < month11'
                        var greaterExpression = Operator.Expression.NodeType == ExpressionType.GreaterThan
                            ? RangeExpressionType.LessThan
                            : RangeExpressionType.LessThanOrEqual;
                        
                        queryAggregator.AddRangeStop(assignment, greaterExpression);
                    }
                    else
                    {
                        // => 'where s.Timestamp > month11'
                        var greaterExpression = Operator.Expression.NodeType == ExpressionType.GreaterThan
                            ? RangeExpressionType.GreaterThan
                            : RangeExpressionType.GreaterThanOrEqual;
                        
                        queryAggregator.AddRangeStart(assignment, greaterExpression);
                    }

                    break;

                default:
                    Operator.NotSupported(Operator.Expression);
                    break;
            }
        }
    }
}