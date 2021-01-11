using System.Collections.Generic;
using InfluxDB.Client.Api.Domain;
using Remotion.Linq;

namespace InfluxDB.Client.Linq
{
    public class InfluxDBQueryVisitor : QueryModelVisitorBase
    {
        private readonly string _bucket;

        public InfluxDBQueryVisitor(string bucket)
        {
            _bucket = bucket;
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
            return new File {Imports = null, Package = null, Body = new List<Statement>()};
        }

        public string BuildFluxQuery()
        {
            return $"from(bucket: \"{_bucket}\")" +
                   " |> range(start: 0)" +
                   " |> pivot(rowKey:[\"_time\"], columnKey: [\"_field\"], valueColumn: \"_value\")";
        }
    }
}