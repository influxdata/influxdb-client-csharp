using System;
using InfluxDB.Client.Core.Flux.Internal;

namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryGenerationContext
    {
        internal readonly VariableAggregator Variables;
        internal readonly AttributesCache Attributes;
        
        internal Type ItemType;

        internal QueryGenerationContext()
        {
            Variables = new VariableAggregator();
            Attributes = new AttributesCache();
        }
    }
}