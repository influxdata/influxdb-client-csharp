namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryGenerationContext
    {
        internal readonly QueryAggregator QueryAggregator;
        internal readonly IMemberNameResolver MemberResolver;
        internal readonly VariableAggregator Variables;

        internal QueryGenerationContext(QueryAggregator queryAggregator, VariableAggregator variableAggregator,
            IMemberNameResolver memberResolver)
        {
            QueryAggregator = queryAggregator;
            Variables = variableAggregator;
            MemberResolver = memberResolver;
        }

        internal QueryGenerationContext Clone(QueryAggregator queryAggregator)
        {
            return new QueryGenerationContext(queryAggregator, Variables, MemberResolver);
        }
    }
}