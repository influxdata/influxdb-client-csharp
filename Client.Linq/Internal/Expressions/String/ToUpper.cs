using System.Collections.Generic;
using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions.String
{
    internal class ToUpper : IExpressionPart
    {
        private readonly IEnumerable<IExpressionPart> _value;

        internal ToUpper(IEnumerable<IExpressionPart> value)
        {
            _value = value;
        }

        public void AppendFlux(StringBuilder builder)
        {
            builder.Append("strings.toUpper(v: ");
            foreach (var expressionPart in _value) expressionPart.AppendFlux(builder);
            builder.Append(")");
        }
    }
}