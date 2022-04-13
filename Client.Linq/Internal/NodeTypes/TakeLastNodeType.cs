using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using InfluxDB.Client.Core;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace InfluxDB.Client.Linq.Internal.NodeTypes
{
    internal class TakeLastExpressionNode : ResultOperatorExpressionNodeBase
    {
        private readonly Expression _count;

        internal static readonly IEnumerable<MethodInfo> GetSupportedMethods =
            new ReadOnlyCollection<MethodInfo>(typeof(Enumerable).GetRuntimeMethods()
                    .Concat(typeof(Queryable).GetRuntimeMethods()).ToList())
                .Where(mi => mi.Name == "TakeLast");

        public TakeLastExpressionNode(MethodCallExpressionParseInfo parseInfo, Expression count)
            : base(parseInfo, null, null)
        {
            _count = count;
        }

        public override Expression Resolve(
            ParameterExpression inputParameter,
            Expression expressionToBeResolved,
            ClauseGenerationContext clauseGenerationContext)
        {
            Arguments.CheckNotNull(inputParameter, nameof(inputParameter));
            Arguments.CheckNotNull(expressionToBeResolved, nameof(expressionToBeResolved));
            return Source.Resolve(inputParameter, expressionToBeResolved, clauseGenerationContext);
        }

        protected override ResultOperatorBase CreateResultOperator(ClauseGenerationContext clauseGenerationContext)
        {
            return new TakeLastResultOperator(_count);
        }
    }

    internal class TakeLastResultOperator : SequenceTypePreservingResultOperatorBase
    {
        private Expression _count;

        internal TakeLastResultOperator(Expression count)
        {
            Arguments.CheckNotNull(count, nameof(count));
            Count = count;
        }

        public Expression Count
        {
            get => _count;
            private set
            {
                Arguments.CheckNotNull(value, nameof(value));
                _count = ReferenceEquals(value.Type, typeof(int))
                    ? value
                    : throw new ArgumentException(string.Format(
                        "The value expression returns '{0}', an expression returning 'System.Int32' was expected.",
                        new object[]
                        {
                            value.Type
                        }), nameof(value));
            }
        }

        public override ResultOperatorBase Clone(CloneContext cloneContext)
        {
            return new TakeResultOperator(Count);
        }

        public override StreamedSequence ExecuteInMemory<T>(StreamedSequence input)
        {
            return new StreamedSequence(
                input.GetTypedSequence<T>().Take(GetConstantCount()).AsQueryable(),
                GetOutputDataInfo(input.DataInfo));
        }

        public override void TransformExpressions(Func<Expression, Expression> transformation)
        {
            Arguments.CheckNotNull(transformation, nameof(transformation));
            Count = transformation(Count);
        }

        public override string ToString()
        {
            return $"TakeLast({Count})";
        }

        private int GetConstantCount()
        {
            return GetConstantValueFromExpression<int>("count", Count);
        }
    }
}