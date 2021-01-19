using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions
{
    internal class AssignmentValue: IExpressionPart
    {
        internal readonly object Value;
        private readonly string _assignment;
        internal AssignmentValue(object value, string assignment)
        {
            Value = value;
            _assignment = assignment;
        }

        public void AppendFlux(StringBuilder builder)
        {
            builder.Append(_assignment);
        }
    }
}