using System.Collections.Generic;

namespace InfluxDB.Client.Linq.Internal
{
    internal sealed class VariableAggregator
    {
        private readonly List<NamedVariable> _variables = new List<NamedVariable>();

        public string AddNamedVariable(object value)
        {
            var variable = new NamedVariable
            {
                Value = value,
                Name = $"p{_variables.Count + 1}"
            };
            _variables.Add(variable);
            return variable.Name;
        }

        public NamedVariable[] GetAll()
        {
            return _variables.ToArray();
        }
    }
    
    internal sealed class NamedVariable
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }
}