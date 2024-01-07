using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using InfluxDB.Client.Api.Domain;

namespace InfluxDB.Client.Linq.Internal
{
    internal sealed class VariableAggregator
    {
        private readonly List<NamedVariable> _variables = new List<NamedVariable>();

        internal string AddNamedVariable(object value)
        {
            var variable = new NamedVariable
            {
                Value = value,
                Name = $"p{_variables.Count + 1}",
                IsTag = false
            };
            _variables.Add(variable);
            return variable.Name;
        }

        /// <summary>
        /// Mark variable with specified name as a Tag.
        /// </summary>
        /// <param name="variableName">variable name</param>
        internal void VariableIsTag(string variableName)
        {
            foreach (var namedVariable in _variables.Where(it => it.Name.Equals(variableName)))
                namedVariable.IsTag = true;
        }

        internal List<Statement> GetStatements()
        {
            return _variables.Select(variable =>
            {
                var literal = CreateExpression(variable);

                var assignment = new VariableAssignment("VariableAssignment",
                    new Identifier("Identifier", variable.Name), literal);

                return new OptionStatement("OptionStatement", assignment) as Statement;
            }).ToList();
        }

        private Expression CreateExpression(NamedVariable variable)
        {
            // Handle string here to avoid conflict with IEnumerable
            if (variable.IsTag || variable.Value is string)
            {
                return CreateStringLiteral(variable);
            }

            return variable.Value switch
            {
                bool b => new BooleanLiteral("BooleanLiteral", b),
                sbyte or short or int or long => new IntegerLiteral("IntegerLiteral", Convert.ToString(variable.Value)),
                byte or ushort or uint or ulong => new UnsignedIntegerLiteral("UnsignedIntegerLiteral", Convert.ToString(variable.Value)),
                float or double or decimal => new FloatLiteral("FloatLiteral", Convert.ToDecimal(variable.Value)),
                DateTime d => new DateTimeLiteral("DateTimeLiteral", d),
                DateTimeOffset o => new DateTimeLiteral("DateTimeLiteral", o.UtcDateTime),
                IEnumerable e => new ArrayExpression("ArrayExpression",
                    e.Cast<object>()
                        .Select(o => CreateExpression(new NamedVariable { Value = o, IsTag = variable.IsTag }))
                        .ToList()),
                TimeSpan timeSpan => new DurationLiteral("DurationLiteral",
                    [new Duration("Duration", (long)(1000.0 * timeSpan.TotalMilliseconds), "us")]),
                Expression e => e,
                _ => CreateStringLiteral(variable)
            };
        }

        private StringLiteral CreateStringLiteral(NamedVariable variable)
        {
            return new StringLiteral("StringLiteral", Convert.ToString(variable.Value));
        }
    }

    internal sealed class NamedVariable
    {
        internal string Name { get; set; }
        internal object Value { get; set; }
        internal bool IsTag { get; set; }
    }
}