using System.Collections.Generic;
using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions.String
{
    internal class HasPrefix : IExpressionPart
    {
        private readonly IEnumerable<IExpressionPart> _value;
        private readonly IEnumerable<IExpressionPart> _prefix;

        internal HasPrefix(IEnumerable<IExpressionPart> value, IEnumerable<IExpressionPart> prefix)
        {
            _value = value;
            _prefix = prefix;
        }

        public void AppendFlux(StringBuilder builder)
        {
            builder.Append("strings.hasPrefix(v: ");
            foreach (var expressionPart in _value) expressionPart.AppendFlux(builder);
            builder.Append(", prefix: ");
            foreach (var expressionPart in _prefix) expressionPart.AppendFlux(builder);
            builder.Append(")");
        }
    }
}