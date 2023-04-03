using System.Collections.Generic;
using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions.String
{
    internal class ToString : IExpressionPart
    {
        private readonly IEnumerable<IExpressionPart> _value;

        internal ToString(IEnumerable<IExpressionPart> value)
        {
            _value = value;
        }

        public void AppendFlux(StringBuilder builder)
        {
            builder.Append("string(v: ");
            foreach (var expressionPart in _value) expressionPart.AppendFlux(builder);
            builder.Append(")");
        }
    }
}