using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions
{
    internal class NamedFieldValue : IExpressionPart
    {
        internal BinaryOperator Operator;
        internal bool Left;
        internal AssignmentValue Assignment;

        public void AppendFlux(StringBuilder builder)
        {
            if (Left)
            {
                Assignment.AppendFlux(builder);
                Operator.AppendFlux(builder);
            }
            else
            {
                Operator.AppendFlux(builder);
                Assignment.AppendFlux(builder);
            }
        }
    }
}