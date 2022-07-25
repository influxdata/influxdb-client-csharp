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

            switch (variable.Value)
            {
                case int i:
                    return new IntegerLiteral("IntegerLiteral", Convert.ToString(i));
                case long l:
                    return new IntegerLiteral("IntegerLiteral", Convert.ToString(l));
                case bool b:
                    return new BooleanLiteral("BooleanLiteral", b);
                case float f:
                    return new FloatLiteral("FloatLiteral", Convert.ToDecimal(f));
                case DateTime d:
                    return new DateTimeLiteral("DateTimeLiteral", d);
                case DateTimeOffset o:
                    return new DateTimeLiteral("DateTimeLiteral", o.UtcDateTime);
                case IEnumerable e:
                {
                    var expressions =
                        e.Cast<object>()
                            .Select(o => new NamedVariable { Value = o, IsTag = variable.IsTag })
                            .Select(CreateExpression)
                            .ToList();
                    return new ArrayExpression("ArrayExpression", expressions);
                }
                case TimeSpan timeSpan:
                    var timeSpanTotalMilliseconds = 1000.0 * timeSpan.TotalMilliseconds;
                    var duration = new Duration("Duration", (long)timeSpanTotalMilliseconds, "us");
                    return new DurationLiteral("DurationLiteral", new List<Duration> { duration });
                case Expression e:
                    return e;
                default:
                    return CreateStringLiteral(variable);
            }
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