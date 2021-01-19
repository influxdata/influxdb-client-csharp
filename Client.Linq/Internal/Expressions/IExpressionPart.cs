using System.Text;

namespace InfluxDB.Client.Linq.Internal.Expressions
{
    internal interface IExpressionPart
    {
        /// <summary>
        /// Append Flux Query to buiilder
        /// </summary>
        /// <param name="builder">Flux query builder</param>
        void AppendFlux(StringBuilder builder);
    }
}