using System.Collections.Generic;
using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions.String
{
    internal class ContainsStr : IExpressionPart
    {
        private readonly IEnumerable<IExpressionPart> _value;
        private readonly IEnumerable<IExpressionPart> _substr;

        internal ContainsStr(IEnumerable<IExpressionPart> value, IEnumerable<IExpressionPart> substr)
        {
            _value = value;
            _substr = substr;
        }

        public void AppendFlux(StringBuilder builder)
        {
            builder.Append("strings.containsStr(v: ");
            foreach (var expressionPart in _value) expressionPart.AppendFlux(builder);
            builder.Append(", substr: ");
            foreach (var expressionPart in _substr) expressionPart.AppendFlux(builder);
            builder.Append(")");
        }
    }
}