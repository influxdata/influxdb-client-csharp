using System.Reflection;
using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions
{
    internal class NamedField : IExpressionPart
    {
        private readonly MemberInfo _member;
        private readonly IMemberNameResolver _memberResolver;
        internal AssignmentValue Assignment;

        internal NamedField(MemberInfo member, IMemberNameResolver memberResolver)
        {
            _member = member;
            _memberResolver = memberResolver;
        }

        public void AppendFlux(StringBuilder builder)
        {
            var name = _memberResolver.GetNamedFieldName(_member, Assignment.Value);

            builder
                .Append("r[\"")
                .Append(name)
                .Append("\"]");
        }
    }
}