using System;
using System.Reflection;
using Remotion.Linq.Parsing.Structure;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace InfluxDB.Client.Linq.Internal.NodeTypes
{
    internal class InfluxDBNodeTypeProvider : INodeTypeProvider
    {
        private readonly MethodInfoBasedNodeTypeRegistry _methodInfoRegistry = new MethodInfoBasedNodeTypeRegistry();

        internal InfluxDBNodeTypeProvider()
        {
            _methodInfoRegistry.Register(TakeLastExpressionNode.GetSupportedMethods, typeof(TakeLastExpressionNode));
        }

        public bool IsRegistered(MethodInfo method)
        {
            return _methodInfoRegistry.IsRegistered(method);
        }

        public Type GetNodeType(MethodInfo method)
        {
            return _methodInfoRegistry.GetNodeType(method);
        }
    }
}