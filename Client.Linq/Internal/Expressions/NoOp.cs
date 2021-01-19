using System;
using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions
{
    internal class NoOp: IExpressionPart
    {
        public void AppendFlux(StringBuilder builder)
        {
            throw new NotSupportedException();
        }
    }
}