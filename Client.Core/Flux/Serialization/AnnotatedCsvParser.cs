using System.Collections.Generic;
using System.Threading;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Internal;

namespace InfluxDB.Client.Core.Flux.Serialization
{
    public interface IAnnotatedCsvParser
    {
        /// <summary>
        /// Parsing query results in annotated CSV format into list of <see cref="FluxTable"/>.
        /// </summary>
        /// <param name="annotatedCsv">Query results in annotated CSV format</param>
        /// <param name="cancellationToken">To cancel the parsing</param>
        /// <returns>Parsed Annotated CSV into list of <see cref="FluxTable"/></returns>
        List<FluxTable> Parse(string annotatedCsv, CancellationToken cancellationToken = default);
    }

    /// <summary>
    /// Parser for processing <see href="https://docs.influxdata.com/influxdb/cloud/reference/syntax/annotated-csv/">Annotated CSV</see>.
    /// </summary>
    public class AnnotatedCsvParser : IAnnotatedCsvParser
    {
        private readonly FluxCsvParser _parser;

        /// <summary>
        /// Public constructor.
        /// </summary>
        public AnnotatedCsvParser()
        {
            _parser = new FluxCsvParser();
        }

        /// <summary>
        /// Parsing query results in annotated CSV format into list of <see cref="FluxTable"/>.
        /// </summary>
        /// <param name="annotatedCsv">Query results in annotated CSV format</param>
        /// <param name="cancellationToken">To cancel the parsing</param>
        /// <returns>Parsed Annotated CSV into list of <see cref="FluxTable"/></returns>
        public List<FluxTable> Parse(string annotatedCsv, CancellationToken cancellationToken = default)
        {
            var consumer = new FluxCsvParser.FluxResponseConsumerTable();
            _parser.ParseFluxResponse(annotatedCsv, cancellationToken, consumer);

            return consumer.Tables;
        }
    }
}