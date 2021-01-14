namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryGenerationContext
    {
        internal readonly VariableAggregator Variables;
        
        internal QueryApi QueryApi;
        // internal Type ItemType;

        internal QueryGenerationContext()
        {
            Variables = new VariableAggregator();
        }
    }
}