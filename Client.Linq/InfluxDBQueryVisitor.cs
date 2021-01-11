using System.Collections.Generic;
using System.Linq;
using InfluxDB.Client.Api.Domain;
using Remotion.Linq;

namespace InfluxDB.Client.Linq
{
    public class InfluxDBQueryVisitor : QueryModelVisitorBase
    {
        private readonly Dictionary<string, object> _properties = new Dictionary<string, object>();

        public InfluxDBQueryVisitor(string bucket)
        {
            AddProperty(bucket);
        }

        public Query GenerateQuery()
        {
            var query = new Query(BuildFluxAST(), BuildFluxQuery())
            {
                Dialect = new Dialect
                {
                    Header = true,
                    Delimiter = ",",
                    CommentPrefix = "#",
                    Annotations = new List<Dialect.AnnotationsEnum>
                    {
                        Dialect.AnnotationsEnum.Datatype,
                        Dialect.AnnotationsEnum.Group,
                        Dialect.AnnotationsEnum.Default
                    }
                }
            };

            return query;
        }

        public File BuildFluxAST()
        {
            var results = _properties.Select(pair =>
            {
                var assignment = new VariableAssignment("VariableAssignment",
                    new Identifier("Identifier", "p1"),
                    new StringLiteral("StringLiteral", "my-bucket"));

                return new OptionStatement("OptionStatement", assignment) as Statement;
            }).ToList();

            return new File {Imports = null, Package = null, Body = results};
        }

        public string BuildFluxQuery()
        {
            return "from(bucket: p1)" +
                   " |> range(start: 0)" +
                   " |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";
        }

        private void AddProperty(object bucket)
        {
            _properties.Add($"p{_properties.Count + 1}", bucket);
        }
    }
}