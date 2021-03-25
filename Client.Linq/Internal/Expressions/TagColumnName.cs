using System.Reflection;
using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions
{
    internal class TagColumnName : IExpressionPart
    {
        private readonly ColumnName _delegate;

        internal TagColumnName(MemberInfo member, IMemberNameResolver memberNameResolver)
        {
            _delegate = new ColumnName(member, memberNameResolver);
        }

        public void AppendFlux(StringBuilder builder)
        {
            builder.Append("r[\"");
            _delegate.AppendFlux(builder);
            builder.Append("\"]");
        }
    }
}