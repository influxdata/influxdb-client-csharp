namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryGenerationContext
    {
        internal VariableAggregator Variables { get; }

        internal QueryGenerationContext()
        {
            Variables = new VariableAggregator();
        }
    }
}