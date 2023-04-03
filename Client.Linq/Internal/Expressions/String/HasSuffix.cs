using System.Collections.Generic;
using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions.String
{
    internal class HasSuffix : IExpressionPart
    {
        private readonly IEnumerable<IExpressionPart> _value;
        private readonly IEnumerable<IExpressionPart> _suffix;

        internal HasSuffix(IEnumerable<IExpressionPart> value, IEnumerable<IExpressionPart> suffix)
        {
            _value = value;
            _suffix = suffix;
        }

        public void AppendFlux(StringBuilder builder)
        {
            builder.Append("strings.hasSuffix(v: ");
            foreach (var expressionPart in _value) expressionPart.AppendFlux(builder);
            builder.Append(", suffix: ");
            foreach (var expressionPart in _suffix) expressionPart.AppendFlux(builder);
            builder.Append(")");
        }
    }
}