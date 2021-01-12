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
                Name = $"p{_variables.Count + 1}"
            };
            _variables.Add(variable);
            return variable.Name;
        }

        internal List<Statement> GetStatements()
        {
            return _variables.Select(variable =>
            {
                Expression literal;
                if (variable.Value is int i)
                {
                    literal = new IntegerLiteral("IntegerLiteral", i.ToString());
                }
                else
                {
                    literal = new StringLiteral("StringLiteral", variable.Value.ToString());
                }

                var assignment = new VariableAssignment("VariableAssignment",
                    new Identifier("Identifier", variable.Name), literal);

                return new OptionStatement("OptionStatement", assignment) as Statement;
            }).ToList();
        }
    }

    internal sealed class NamedVariable
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}