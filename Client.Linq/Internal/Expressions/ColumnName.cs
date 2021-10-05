using System.Reflection;
using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions
{
    internal class ColumnName : IExpressionPart
    {
        private readonly MemberInfo _member;
        private readonly IMemberNameResolver _memberResolver;

        internal ColumnName(MemberInfo member, IMemberNameResolver memberResolver)
        {
            _member = member;
            _memberResolver = memberResolver;
        }

        public void AppendFlux(StringBuilder builder)
        {
            switch (_memberResolver.ResolveMemberType(_member))
            {
                case MemberType.Measurement:
                    builder.Append("_measurement");
                    break;
                case MemberType.Timestamp:
                    builder.Append("_time");
                    break;
                default:
                    builder.Append(_memberResolver.GetColumnName(_member));
                    break;
            }
        }
    }
}