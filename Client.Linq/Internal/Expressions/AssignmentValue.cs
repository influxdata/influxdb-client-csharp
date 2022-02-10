using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions
{
    internal class AssignmentValue : IExpressionPart
    {
        internal readonly object Value;
        internal readonly string Assignment;

        internal AssignmentValue(object value, string assignment)
        {
            Value = value;
            Assignment = assignment;
        }

        public void AppendFlux(StringBuilder builder)
        {
            builder.Append(Assignment);
        }
    }
}