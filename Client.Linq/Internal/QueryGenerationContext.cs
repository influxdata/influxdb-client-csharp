namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryGenerationContext
    {
        internal readonly QueryAggregator QueryAggregator;
        internal readonly QueryApi QueryApi;
        internal readonly VariableAggregator Variables;

        internal QueryGenerationContext(QueryAggregator queryAggregator, QueryApi queryApi)
        {
            QueryAggregator = queryAggregator;
            QueryApi = queryApi;
            Variables = new VariableAggregator();
        }
    }
}