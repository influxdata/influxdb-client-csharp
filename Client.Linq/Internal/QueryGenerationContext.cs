namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryGenerationContext
    {
        internal readonly VariableAggregator Variables;
        internal readonly QueryApi QueryApi;

        internal QueryGenerationContext(QueryApi queryApi)
        {
            QueryApi = queryApi;
            Variables = new VariableAggregator();
        }
    }
}