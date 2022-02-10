using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions
{
    internal abstract class AbstractExpressionPart : IExpressionPart
    {
        private readonly string _expression;

        protected AbstractExpressionPart(string expression)
        {
            _expression = expression;
        }

        public void AppendFlux(StringBuilder builder)
        {
            builder.Append(_expression);
        }
    }
}