using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core;
using InfluxDB.Client.Linq.Internal.Expressions;
using Remotion.Linq.Clauses;
using Remotion.Linq.Clauses.Expressions;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Parsing;
using BinaryExpression = System.Linq.Expressions.BinaryExpression;
using Expression = System.Linq.Expressions.Expression;
using MemberExpression = System.Linq.Expressions.MemberExpression;
using UnaryExpression = System.Linq.Expressions.UnaryExpression;

namespace InfluxDB.Client.Linq.Internal
{
    internal class QueryExpressionTreeVisitor : ThrowingExpressionVisitor
    {
        private readonly object _clause;

        private readonly QueryGenerationContext _context;
        private readonly List<IExpressionPart> _expressionParts = new List<IExpressionPart>();

        private QueryExpressionTreeVisitor(object clause, QueryGenerationContext context)
        {
            Arguments.CheckNotNull(clause, nameof(clause));
            Arguments.CheckNotNull(context, nameof(context));

            _clause = clause;
            _context = context;
        }

        internal static IEnumerable<IExpressionPart> GetFluxExpressions(Expression expression, object clause,
            QueryGenerationContext context)
        {
            Arguments.CheckNotNull(expression, nameof(expression));
            Arguments.CheckNotNull(clause, nameof(clause));
            Arguments.CheckNotNull(context, nameof(context));

            var visitor = new QueryExpressionTreeVisitor(clause, context);
            visitor.Visit(expression);
            return visitor.GetFluxExpressions();
        }

        protected override Expression VisitConstant(ConstantExpression expression)
        {
            var value = expression.Value;
            var assignmentValue = new AssignmentValue(value, _context.Variables.AddNamedVariable(value));
            _expressionParts.Add(assignmentValue);

            return expression;
        }

        protected override Expression VisitSubQuery(SubQueryExpression subQuery)
        {
            if (subQuery.QueryModel.ResultOperators.All(p => p is AnyResultOperator) ||
                subQuery.QueryModel.ResultOperators.All(p => p is ContainsResultOperator))
            {
                var query = new QueryAggregator();

                var modelVisitor = new InfluxDBQueryVisitor(_context.Clone(query));
                modelVisitor.VisitQueryModel(subQuery.QueryModel);

                _context.QueryAggregator.AddSubQueries(query);

                return subQuery;
            }

            return base.VisitSubQuery(subQuery);
        }

        protected override Expression VisitBinary(BinaryExpression expression)
        {
            _expressionParts.Add(new LeftParenthesis());
            Visit(expression.Left);

            switch (expression.NodeType)
            {
                case ExpressionType.Equal:
                case ExpressionType.NotEqual:
                case ExpressionType.LessThan:
                case ExpressionType.LessThanOrEqual:
                case ExpressionType.GreaterThan:
                case ExpressionType.GreaterThanOrEqual:
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                case ExpressionType.OrElse:
                case ExpressionType.Or:
                    _expressionParts.Add(new BinaryOperator(expression, exp => base.VisitBinary(exp)));
                    break;

                default:
                    base.VisitBinary(expression);
                    break;
            }

            Visit(expression.Right);
            _expressionParts.Add(new RightParenthesis());

            return expression;
        }

        protected override Expression VisitMember(MemberExpression expression)
        {
            if (_clause is WhereClause || _clause is MainFromClause)
            {
                switch (_context.MemberResolver.ResolveMemberType(expression.Member))
                {
                    case MemberType.Measurement:
                        _expressionParts.Add(new MeasurementColumnName(expression.Member, _context.MemberResolver));
                        break;
                    case MemberType.Timestamp:
                        _expressionParts.Add(new TimeColumnName(expression.Member, _context.MemberResolver));
                        break;
                    case MemberType.Tag:
                        _expressionParts.Add(new TagColumnName(expression.Member, _context.MemberResolver));
                        break;
                    case MemberType.NamedField:
                        _expressionParts.Add(new NamedField(expression.Member, _context.MemberResolver));
                        break;
                    case MemberType.NamedFieldValue:
                        _expressionParts.Add(new NamedFieldValue());
                        break;
                    default:
                        _expressionParts.Add(new RecordColumnName(expression.Member, _context.MemberResolver));
                        break;
                }
            }
            else
            {
                _expressionParts.Add(new ColumnName(expression.Member, _context.MemberResolver));
            }

            return expression;
        }

        protected override Expression VisitUnary(UnaryExpression expression)
        {
            if (expression.NodeType == ExpressionType.Convert)
            {
                return Visit(expression.Operand);
            }

            return base.VisitUnary(expression);
        }

        protected override Expression VisitMethodCall(MethodCallExpression expression)
        {
            if (expression.Method.Name.Equals("AggregateWindow"))
            {
                var member = (MemberExpression)expression.Arguments[0];
                if (_context.MemberResolver.ResolveMemberType(member.Member) != MemberType.Timestamp)
                {
                    throw new NotSupportedException(
                        "AggregateWindow() has to be used only for Timestamp member, e.g. [Column(IsTimestamp = true)].");
                }

                //
                // every
                //
                var every = (TimeSpan)((ConstantExpression)expression.Arguments[1]).Value;
                Arguments.CheckNotNull(every, "every");
                var everyVariable = _context.Variables.AddNamedVariable(every);

                //
                // period
                //
                string periodVariable = null;
                var period = ((ConstantExpression)expression.Arguments[2]).Value as TimeSpan?;
                if (period.HasValue)
                {
                    Arguments.CheckNotNull(period, "period");
                    periodVariable = _context.Variables.AddNamedVariable(period);
                }

                //
                // fn
                //
                var fn = ((ConstantExpression)expression.Arguments[3]).Value as string;
                Arguments.CheckNonEmptyString(fn, "fn");
                var fnVariable = _context.Variables.AddNamedVariable(new Identifier("Identifier", "mean"));

                _context.QueryAggregator.AddAggregateWindow(everyVariable, periodVariable, fnVariable);

                return expression;
            }

            return base.VisitMethodCall(expression);
        }

        protected override Exception CreateUnhandledItemException<T>(T unhandledItem, string visitMethod)
        {
            var message = $"The expression '{unhandledItem}', type: '{typeof(T)}' is not supported.";

            return new NotSupportedException(message);
        }

        private IEnumerable<IExpressionPart> GetFluxExpressions()
        {
            NormalizeNamedField();
            NormalizeNamedFieldValue();
            return _expressionParts;
        }

        private void NormalizeNamedField()
        {
            var index = _expressionParts
                .FindIndex(it => it is NamedField nf && nf.Assignment == null);

            if (index == -1)
            {
                return;
            }

            var namedField = (NamedField)_expressionParts[index];
            // Constant on right: a.Name == "quality"
            if (_expressionParts[index + 1] is BinaryOperator)
            {
                namedField.Assignment = _expressionParts[index + 2] as AssignmentValue;

                _expressionParts.RemoveAt(index + 3);
                _expressionParts.RemoveAt(index + 2);
                _expressionParts.RemoveAt(index + 1);
                _expressionParts.RemoveAt(index - 1);
            }
            // Constant on left: "quality" == a.Name
            else
            {
                namedField.Assignment = _expressionParts[index - 2] as AssignmentValue;

                _expressionParts.RemoveAt(index + 1);
                _expressionParts.RemoveAt(index - 1);
                _expressionParts.RemoveAt(index - 2);
                _expressionParts.RemoveAt(index - 3);
            }

            NormalizeNamedField();
        }

        private void NormalizeNamedFieldValue()
        {
            var index = _expressionParts
                .FindIndex(it => it is NamedFieldValue nf && nf.Assignment == null);

            if (index == -1)
            {
                return;
            }

            var namedField = (NamedFieldValue)_expressionParts[index];
            // Constant on right: a.Value == "good"
            if (_expressionParts[index + 1] is BinaryOperator)
            {
                namedField.Operator = _expressionParts[index + 1] as BinaryOperator;
                namedField.Assignment = _expressionParts[index + 2] as AssignmentValue;
                // == "good" ) and NamedField
                var i = index + 5;
                namedField.Left = _expressionParts.Count > i && _expressionParts[i] is NamedField;

                _expressionParts.RemoveAt(index + 3);
                _expressionParts.RemoveAt(index + 2);
                _expressionParts.RemoveAt(index + 1);
                _expressionParts.RemoveAt(index - 1);
            }
            // Constant on left: "good" == a.Value
            else
            {
                namedField.Operator = _expressionParts[index - 1] as BinaryOperator;
                namedField.Assignment = _expressionParts[index - 2] as AssignmentValue;

                // ) and NamedField
                var i = index + 3;
                namedField.Left = _expressionParts.Count > i && _expressionParts[i] is NamedField;

                _expressionParts.RemoveAt(index + 1);
                _expressionParts.RemoveAt(index - 1);
                _expressionParts.RemoveAt(index - 2);
                _expressionParts.RemoveAt(index - 3);
            }

            // remove trailing operator: r["attribute_quality"] and  == p4
            index = _expressionParts.IndexOf(namedField);
            if (namedField.Left)
            {
                _expressionParts.RemoveAt(index + 1);
            }
            else
            {
                _expressionParts.RemoveAt(index - 1);
            }

            NormalizeNamedFieldValue();
        }

        /// <summary>
        /// Mark variables that are use to filter by tag by tag as tag.
        /// </summary>
        internal static void NormalizeTagsAssignments(List<IExpressionPart> parts, QueryGenerationContext context)
        {
            var indexes = Enumerable.Range(0, parts.Count)
                .Where(i => parts[i] is BinaryOperator)
                .ToList();

            foreach (var index in indexes)
            {
                // "sensorId == 123456"
                if (index >= 1 && parts[index - 1] is TagColumnName && parts[index + 1] is AssignmentValue)
                {
                    var assignmentValue = (AssignmentValue)parts[index + 1];
                    context.Variables.VariableIsTag(assignmentValue.Assignment);
                }

                // "123456 == sensorId"
                if (index >= 1 && parts[index - 1] is AssignmentValue && parts[index + 1] is TagColumnName)
                {
                    var assignmentValue = (AssignmentValue)parts[index - 1];
                    context.Variables.VariableIsTag(assignmentValue.Assignment);
                }
            }
        }

        /// <summary>
        /// Normalize generated expression.
        /// </summary>
        internal static void NormalizeExpressions(List<IExpressionPart> parts)
        {
            // Binary Expressions
            {
                var indexes = Enumerable.Range(0, parts.Count)
                    .Where(i => parts[i] is BinaryOperator)
                    .ToList();

                foreach (var index in indexes)
                {
                    // "( and )"
                    if (index >= 1 && parts[index - 1] is LeftParenthesis && parts[index + 1] is RightParenthesis)
                    {
                        parts.RemoveAt(index + 1);
                        parts.RemoveAt(index);
                        parts.RemoveAt(index - 1);

                        NormalizeExpressions(parts);
                        return;
                    }

                    // "( timestamp > )"
                    if (index >= 2 && parts[index - 2] is LeftParenthesis && parts[index + 1] is RightParenthesis)
                    {
                        parts.RemoveAt(index + 1);
                        parts.RemoveAt(index);
                        parts.RemoveAt(index - 1);
                        parts.RemoveAt(index - 2);

                        NormalizeExpressions(parts);
                        return;
                    }

                    // "( < timestamp )"  
                    if (index >= 1 && parts[index - 1] is LeftParenthesis && parts[index + 2] is RightParenthesis)
                    {
                        parts.RemoveAt(index + 2);
                        parts.RemoveAt(index + 1);
                        parts.RemoveAt(index);
                        parts.RemoveAt(index - 1);

                        NormalizeExpressions(parts);
                        return;
                    }

                    // "( or (r["sensor_id"] != p4))"
                    if (index >= 1 && parts[index - 1] is LeftParenthesis && parts[index + 1] is LeftParenthesis)
                    {
                        parts.RemoveAt(index);

                        NormalizeExpressions(parts);
                        return;
                    }

                    // "(r["sensor_id"] != p4) or )"
                    if (index >= 1 && parts[index - 1] is RightParenthesis && parts[index + 1] is RightParenthesis)
                    {
                        parts.RemoveAt(index);

                        NormalizeExpressions(parts);
                        return;
                    }
                }
            }

            // Parenthesis
            {
                var indexes = Enumerable.Range(0, parts.Count)
                    .Where(i => parts[i] is LeftParenthesis)
                    .ToList();

                foreach (var index in indexes)
                {
                    // ()
                    if (parts.Count > index + 1 && parts[index + 1] is RightParenthesis)
                    {
                        parts.RemoveAt(index + 1);
                        parts.RemoveAt(index);

                        NormalizeExpressions(parts);
                        return;
                    }

                    // (
                    if (parts.Count == 1 && parts[index] is LeftParenthesis)
                    {
                        parts.RemoveAt(index);

                        NormalizeExpressions(parts);
                        return;
                    }
                }

                // (( ))
                if (parts.Count >= 4 && parts[0] is LeftParenthesis && parts[1] is LeftParenthesis &&
                    parts[parts.Count - 2] is RightParenthesis && parts[parts.Count - 1] is RightParenthesis)
                {
                    parts.RemoveAt(parts.Count - 1);
                    parts.RemoveAt(0);

                    NormalizeExpressions(parts);
                }
            }
        }
    }
}