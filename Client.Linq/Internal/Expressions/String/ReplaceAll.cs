using System.Collections.Generic;
using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions.String
{
    internal class ReplaceAll : IExpressionPart
    {
        private readonly IEnumerable<IExpressionPart> _value;
        private readonly IEnumerable<IExpressionPart> _substring;
        private readonly IEnumerable<IExpressionPart> _replacement;

        internal ReplaceAll(IEnumerable<IExpressionPart> value, IEnumerable<IExpressionPart> substring,
            IEnumerable<IExpressionPart> replacement)
        {
            _value = value;
            _substring = substring;
            _replacement = replacement;
        }

        public void AppendFlux(StringBuilder builder)
        {
            builder.Append("strings.replaceAll(v: ");
            foreach (var expressionPart in _value) expressionPart.AppendFlux(builder);
            builder.Append(", t: ");
            foreach (var expressionPart in _substring) expressionPart.AppendFlux(builder);
            builder.Append(", u: ");
            foreach (var expressionPart in _replacement) expressionPart.AppendFlux(builder);
            builder.Append(")");
        }
    }
}