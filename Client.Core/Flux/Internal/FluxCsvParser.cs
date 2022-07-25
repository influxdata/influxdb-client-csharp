using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using CsvHelper;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Exceptions;
using NodaTime;
using NodaTime.Text;

namespace InfluxDB.Client.Core.Flux.Internal
{
    /// <summary>
    /// This class us used to construct <see cref="FluxTable"/> from CSV.
    /// </summary>
    public class FluxCsvParser
    {
        private const string AnnotationDatatype = "#datatype";
        private const string AnnotationGroup = "#group";
        private const string AnnotationDefault = "#default";
        private static readonly string[] Annotations = { AnnotationDatatype, AnnotationGroup, AnnotationDefault };
        private readonly ResponseMode _responseMode;

        private enum ParsingState
        {
            Normal,
            InError
        }

        // The configuration for expected amount of metadata response from InfluxDB.
        internal enum ResponseMode
        {
            // full information about types, default values and groups
            Full,

            // useful for Invokable scripts
            OnlyNames
        }

        public interface IFluxResponseConsumer
        {
            /// <summary>
            /// Add new <see cref="FluxTable"/> to a consumer.
            /// </summary>
            /// <param name="index">index of table</param>
            /// <param name="table">new <see cref="FluxTable"/></param>
            void Accept(int index, FluxTable table);

            /// <summary>
            /// Add new <see cref="FluxRecord"/> to a consumer.
            /// </summary>
            /// <param name="index">index of table</param>
            /// <param name="record">new <see cref="FluxRecord"/></param>
            void Accept(int index, FluxRecord record);
        }

        public class FluxResponseConsumerTable : IFluxResponseConsumer
        {
            public List<FluxTable> Tables { get; } = new List<FluxTable>();

            public void Accept(int index, FluxTable table)
            {
                Tables.Insert(index, table);
            }

            public void Accept(int index, FluxRecord record)
            {
                Tables[index].Records.Add(record);
            }
        }

        internal FluxCsvParser(ResponseMode responseMode = ResponseMode.Full)
        {
            _responseMode = responseMode;
        }

        public void ParseFluxResponse(string source, CancellationToken cancellable, IFluxResponseConsumer consumer)
        {
            Arguments.CheckNonEmptyString(source, "source");

            ParseFluxResponse(ToStream(source), cancellable, consumer);
        }

        /// <summary>
        /// Parse Flux CSV response to <see cref="IFluxResponseConsumer"/>.
        /// </summary>
        /// <param name="source">CSV Data source</param>
        /// <param name="cancellable">to cancel parsing</param>
        /// <param name="consumer">to accept <see cref="FluxTable"/> or <see cref="FluxRecord"/></param>
        public void ParseFluxResponse(Stream source, CancellationToken cancellable, IFluxResponseConsumer consumer)
        {
            Arguments.CheckNotNull(source, "source");

            using var csv = new CsvReader(new StreamReader(source), CultureInfo.InvariantCulture);
            var state = new ParseFluxResponseState { csv = csv };

            while (csv.Read())
            {
                if (cancellable.IsCancellationRequested)
                {
                    return;
                }

                foreach (var (table, record) in ParseNextFluxResponse(state))
                    if (record == null)
                    {
                        consumer.Accept(state.tableIndex, table);
                    }
                    else
                    {
                        consumer.Accept(state.tableIndex - 1, record);
                    }
            }
        }

        /// <summary>
        /// Parse Flux CSV response to <see cref="IAsyncEnumerable{T}"/>.
        /// </summary>
        /// <param name="reader">CSV Data source reader</param>
        /// <param name="cancellationToken">cancellation token</param>
        public async IAsyncEnumerable<(FluxTable, FluxRecord)> ParseFluxResponseAsync(TextReader reader,
            [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(reader, nameof(reader));

            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
            var state = new ParseFluxResponseState { csv = csv };

            while (await csv.ReadAsync().ConfigureAwait(false) && !cancellationToken.IsCancellationRequested)
                foreach (var response in ParseNextFluxResponse(state))
                    yield return response;
        }

        private class ParseFluxResponseState
        {
            public ParsingState parsingState = ParsingState.Normal;
            public int tableIndex;
            public int tableId = -1;
            public bool startNewTable;
            public FluxTable table;
            public string[] groups = new string[0];
            public CsvReader csv;
        }

        private IEnumerable<(FluxTable, FluxRecord)> ParseNextFluxResponse(ParseFluxResponseState state)
        {
            //
            // Response has HTTP status ok, but response is error.
            //
            if ("error".Equals(state.csv[1]) && "reference".Equals(state.csv[2]))
            {
                state.parsingState = ParsingState.InError;
                yield break;
            }

            //
            // Throw InfluxException with error response
            //
            if (ParsingState.InError.Equals(state.parsingState))
            {
                var error = state.csv[1];
                var referenceValue = state.csv[2];

                var reference = 0;

                if (referenceValue != null && !string.IsNullOrEmpty(referenceValue))
                {
                    reference = Convert.ToInt32(referenceValue);
                }

                throw new FluxQueryException(error, reference);
            }

            var token = state.csv[0];

            //// start new table
            if (Annotations.Contains(token) && !state.startNewTable ||
                _responseMode == ResponseMode.OnlyNames && state.table == null)
            {
                state.startNewTable = true;

                state.table = new FluxTable();
                state.groups = new string[0];
                yield return (state.table, null);

                state.tableIndex++;
                state.tableId = -1;
            }
            else if (state.table == null)
            {
                throw new FluxCsvParserException(
                    "Unable to parse CSV response. FluxTable definition was not found.");
            }

            //#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,double,string,string,string
            if (AnnotationDatatype.Equals(token))
            {
                AddDataTypes(state.table, state.csv.Parser.Record);
            }
            else if (AnnotationGroup.Equals(token))
            {
                state.groups = state.csv.Parser.Record;
            }
            else if (AnnotationDefault.Equals(token))
            {
                AddDefaultEmptyValues(state.table, state.csv);
            }
            else
            {
                // parse column names
                if (state.startNewTable)
                {
                    if (_responseMode == ResponseMode.OnlyNames && state.table.Columns.Count == 0)
                    {
                        AddDataTypes(state.table, state.csv.Parser.Record.Select(it => "string").ToArray());
                        state.groups = state.csv.Parser.Record.Select(it => "false").ToArray();
                    }

                    AddGroups(state.table, state.groups);
                    AddColumnNamesAndTags(state.table, state.csv);
                    state.startNewTable = false;
                    yield break;
                }

                int currentId;

                try
                {
                    currentId = Convert.ToInt32(state.csv[1 + 1]);
                }
                catch (Exception e)
                {
                    throw new FluxCsvParserException("Unable to parse CSV response.", e);
                }

                if (state.tableId == -1)
                {
                    state.tableId = currentId;
                }

                if (state.tableId != currentId)
                {
                    //create new table with previous column headers settings
                    var fluxColumns = state.table.Columns;
                    state.table = new FluxTable();
                    state.table.Columns.AddRange(fluxColumns);
                    yield return (state.table, null);

                    state.tableIndex++;
                    state.tableId = currentId;
                }

                yield return (state.table, ParseRecord(state.tableIndex - 1, state.table, state.csv));
            }
        }

        private FluxRecord ParseRecord(int tableIndex, FluxTable table, CsvReader csv)
        {
            var record = new FluxRecord(tableIndex);

            foreach (var fluxColumn in table.Columns)
            {
                var columnName = fluxColumn.Label;

                var strValue = csv[fluxColumn.Index + 1];

                record.Values.Add(columnName, ToValue(strValue, fluxColumn));
            }

            return record;
        }

        private object ToValue(string strValue, FluxColumn column)
        {
            Arguments.CheckNotNull(column, "column");

            // Default value
            if (string.IsNullOrEmpty(strValue))
            {
                var defaultValue = column.DefaultValue;

                return string.IsNullOrEmpty(defaultValue) ? null : ToValue(defaultValue, column);
            }

            try
            {
                switch (column.DataType)
                {
                    case "boolean":
                        return bool.TryParse(strValue, out var value) && value;
                    case "unsignedLong":
                        return Convert.ToUInt64(strValue);
                    case "long":
                        return Convert.ToInt64(strValue);
                    case "double":
                        return strValue switch
                        {
                            "+Inf" => double.PositiveInfinity,
                            "-Inf" => double.NegativeInfinity,
                            _ => Convert.ToDouble(strValue, CultureInfo.InvariantCulture)
                        };
                    case "base64Binary":
                        return Convert.FromBase64String(strValue);
                    case "dateTime:RFC3339":
                    case "dateTime:RFC3339Nano":
                        return InstantPattern.ExtendedIso.Parse(strValue).Value;
                    case "duration":
                        return Duration.FromNanoseconds(Convert.ToDouble(strValue));
                    default:
                        return strValue;
                }
            }
            catch (Exception e)
            {
                throw new FluxCsvParserException("Unable to parse CSV response.", e);
            }
        }

        public static Stream ToStream(string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return new BufferedStream(stream);
        }

        private void AddDataTypes(FluxTable table, string[] dataTypes)
        {
            Arguments.CheckNotNull(table, "table");
            Arguments.CheckNotNull(dataTypes, "dataTypes");

            for (var index = 1; index < dataTypes.Length; index++)
            {
                var dataType = dataTypes[index];

                if (string.IsNullOrEmpty(dataType))
                {
                    continue;
                }

                var columnDef = new FluxColumn
                {
                    DataType = dataType,
                    Index = index - 1
                };

                table.Columns.Add(columnDef);
            }
        }

        private void AddGroups(FluxTable table, string[] groups)
        {
            Arguments.CheckNotNull(table, "table");
            Arguments.CheckNotNull(groups, "groups");

            for (var ii = 0; ii < table.Columns.Count; ii++)
            {
                var fluxColumn = GetFluxColumn(ii, table);
                fluxColumn.Group = Convert.ToBoolean(groups[ii + 1]);
            }
        }

        private void AddDefaultEmptyValues(FluxTable table, CsvReader defaultEmptyValues)
        {
            Arguments.CheckNotNull(table, "table");
            Arguments.CheckNotNull(defaultEmptyValues, "defaultEmptyValues");

            for (var ii = 0; ii < table.Columns.Count; ii++)
            {
                var fluxColumn = GetFluxColumn(ii, table);
                fluxColumn.DefaultValue = defaultEmptyValues[ii + 1];
            }
        }

        private void AddColumnNamesAndTags(FluxTable table, CsvReader columnNames)
        {
            Arguments.CheckNotNull(table, "table");
            Arguments.CheckNotNull(columnNames, "columnNames");

            var size = table.Columns.Count;

            for (var ii = 0; ii < size; ii++)
            {
                var fluxColumn = GetFluxColumn(ii, table);
                fluxColumn.Label = columnNames[ii + 1];
            }
        }

        private FluxColumn GetFluxColumn(int columnIndex, FluxTable table)
        {
            Arguments.CheckNotNull(table, "table");

            return table.Columns[columnIndex];
        }
    }
}