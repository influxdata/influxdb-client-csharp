using System;
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
            {
                namedVariable.IsTag = true;
            }
        }

        internal List<Statement> GetStatements()
        {
            return _variables.Select(variable =>
            {
                Expression literal;
                if (variable.IsTag)
                {
                    literal = CreateStringLiteral(variable);
                }
                else if (variable.Value is int i)
                {
                    literal = new IntegerLiteral("IntegerLiteral", Convert.ToString(i));
                }
                else if (variable.Value is long l)
                {
                    literal = new IntegerLiteral("IntegerLiteral", Convert.ToString(l));
                }
                else if (variable.Value is bool b)
                {
                    literal = new BooleanLiteral("BooleanLiteral", b);
                }
                else if (variable.Value is float f)
                {
                    literal = new FloatLiteral("FloatLiteral", Convert.ToDecimal(f));
                }
                else if (variable.Value is DateTime d)
                {
                    literal = new DateTimeLiteral("DateTimeLiteral", d);
                }
                else if (variable.Value is DateTimeOffset o)
                {
                    literal = new DateTimeLiteral("DateTimeLiteral", o.UtcDateTime);
                }
                else
                {
                    literal = CreateStringLiteral(variable);
                }

                var assignment = new VariableAssignment("VariableAssignment",
                    new Identifier("Identifier", variable.Name), literal);

                return new OptionStatement("OptionStatement", assignment) as Statement;
            }).ToList();
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