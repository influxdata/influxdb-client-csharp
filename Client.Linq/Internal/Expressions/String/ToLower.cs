using System.Collections.Generic;
using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions.String
{
    internal class ToLower : IExpressionPart
    {
        private readonly IEnumerable<IExpressionPart> _value;

        internal ToLower(IEnumerable<IExpressionPart> value)
        {
            _value = value;
        }

        public void AppendFlux(StringBuilder builder)
        {
            builder.Append("strings.toLower(v: ");
            foreach (var expressionPart in _value) expressionPart.AppendFlux(builder);
            builder.Append(")");
        }
    }
}