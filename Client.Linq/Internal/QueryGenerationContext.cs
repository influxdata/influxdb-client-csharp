namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryGenerationContext
    {
        internal readonly QueryAggregator QueryAggregator;
        internal readonly IMemberNameResolver MemberResolver;
        internal readonly VariableAggregator Variables;
        internal readonly QueryableOptimizerSettings QueryableOptimizerSettings;

        internal QueryGenerationContext(
            QueryAggregator queryAggregator,
            VariableAggregator variableAggregator,
            IMemberNameResolver memberResolver,
            QueryableOptimizerSettings queryableOptimizerSettings)
        {
            QueryAggregator = queryAggregator;
            Variables = variableAggregator;
            MemberResolver = memberResolver;
            QueryableOptimizerSettings = queryableOptimizerSettings;
        }

        internal QueryGenerationContext Clone(QueryAggregator queryAggregator)
        {
            return new QueryGenerationContext(queryAggregator, Variables, MemberResolver, QueryableOptimizerSettings);
        }
    }
}