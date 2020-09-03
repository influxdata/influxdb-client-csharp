using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
        private enum ParsingState
        {
            Normal,
            InError
        }

        public interface IFluxResponseConsumer
        {
            /// <summary>
            /// Add new <see cref="FluxTable"/> to a consumer.
            /// </summary>
            /// <param name="index">index of table</param>
            /// <param name="cancellable">cancellable</param>
            /// <param name="table">new <see cref="FluxTable"/></param>
            void Accept(int index, ICancellable cancellable, FluxTable table);

            /// <summary>
            /// Add new <see cref="FluxRecord"/> to a consumer.
            /// </summary>
            /// <param name="index">index of table</param>
            /// <param name="cancellable">cancellable</param>
            /// <param name="record">new <see cref="FluxRecord"/></param>
            void Accept(int index, ICancellable cancellable, FluxRecord record);
        }

        public class FluxResponseConsumerTable : IFluxResponseConsumer
        {
            public List<FluxTable> Tables { get; } = new List<FluxTable>();

            public void Accept(int index, ICancellable cancellable, FluxTable table)
            {
                Tables.Insert(index, table);
            }

            public void Accept(int index, ICancellable cancellable, FluxRecord record)
            {
                Tables[index].Records.Add(record);
            }
        }

        public void ParseFluxResponse(string source, ICancellable cancellable, IFluxResponseConsumer consumer)
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
        public void ParseFluxResponse(Stream source, ICancellable cancellable, IFluxResponseConsumer consumer)
        {
            Arguments.CheckNotNull(source, "source");

            var parsingState = ParsingState.Normal;

            var tableIndex = 0;
            var tableId = -1;
            var startNewTable = false;
            FluxTable table = null;

            using (var csv = new CsvReader(new StreamReader(source), CultureInfo.InvariantCulture))
            {
                while (csv.Read())
                {
                    if (cancellable != null && cancellable.IsCancelled())
                    {
                        return;
                    }

                    //
                    // Response has HTTP status ok, but response is error.
                    //
                    if ("error".Equals(csv[1]) && "reference".Equals(csv[2]))
                    {
                        parsingState = ParsingState.InError;
                        continue;
                    }

                    //
                    // Throw InfluxException with error response
                    //
                    if (ParsingState.InError.Equals(parsingState))
                    {
                        var error = csv[1];
                        var referenceValue = csv[2];

                        var reference = 0;

                        if (referenceValue != null && !String.IsNullOrEmpty(referenceValue))
                        {
                            reference = Convert.ToInt32(referenceValue);
                        }

                        throw new FluxQueryException(error, reference);
                    }

                    var token = csv[0];

                    //// start new table
                    if ("#datatype".Equals(token))
                    {
                        startNewTable = true;

                        table = new FluxTable();
                        consumer.Accept(tableIndex, cancellable, table);
                        tableIndex++;
                        tableId = -1;
                    }
                    else if (table == null)
                    {
                        throw new FluxCsvParserException(
                            "Unable to parse CSV response. FluxTable definition was not found.");
                    }

                    //#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,double,string,string,string
                    if ("#datatype".Equals(token))
                    {
                        AddDataTypes(table, csv);
                    }
                    else if ("#group".Equals(token))
                    {
                        AddGroups(table, csv);
                    }
                    else if ("#default".Equals(token))
                    {
                        AddDefaultEmptyValues(table, csv);
                    }
                    else
                    {
                        // parse column names
                        if (startNewTable)
                        {
                            AddColumnNamesAndTags(table, csv);
                            startNewTable = false;
                            continue;
                        }

                        int currentId;

                        try
                        {
                            currentId = Convert.ToInt32(csv[1 + 1]);
                        }
                        catch (Exception)
                        {
                            throw new FluxCsvParserException("Unable to parse CSV response.");
                        }
                        if (tableId == -1) {
                            tableId = currentId;
                        }

                        if (tableId != currentId)
                        {
                            //create new table with previous column headers settings
                            var fluxColumns = table.Columns;
                            table = new FluxTable();
                            table.Columns.AddRange(fluxColumns);
                            consumer.Accept(tableIndex, cancellable, table);
                            tableIndex++;
                            tableId = currentId;
                        }

                        var fluxRecord = ParseRecord(tableIndex - 1, table, csv);
                        consumer.Accept(tableIndex - 1, cancellable, fluxRecord);
                    }
                }
            }
        }

#if NETSTANDARD2_1
        /// <summary>
        /// Parse Flux CSV response to <see cref="IFluxResponseConsumer"/>.
        /// </summary>
        /// <param name="reader">CSV Data source reader</param>
        /// <param name="cancellationToken">cancellation token</param>
        public async IAsyncEnumerable<(FluxTable, FluxRecord)> ParseFluxResponseAsync(StringReader reader, [EnumeratorCancellation] CancellationToken cancellationToken)
        {
            Arguments.CheckNotNull(reader, nameof(reader));

            var parsingState = ParsingState.Normal;

            var tableIndex = 0;
            var tableId = -1;
            var startNewTable = false;
            FluxTable table = null;

            using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);

            while (await csv.ReadAsync() && !cancellationToken.IsCancellationRequested)
            {
                //
                // Response has HTTP status ok, but response is error.
                //
                if ("error".Equals(csv[1]) && "reference".Equals(csv[2]))
                {
                    parsingState = ParsingState.InError;
                    continue;
                }

                //
                // Throw InfluxException with error response
                //
                if (ParsingState.InError.Equals(parsingState))
                {
                    var error = csv[1];
                    var referenceValue = csv[2];

                    var reference = 0;

                    if (referenceValue != null && !String.IsNullOrEmpty(referenceValue))
                    {
                        reference = Convert.ToInt32(referenceValue);
                    }

                    throw new FluxQueryException(error, reference);
                }

                var token = csv[0];

                //// start new table
                if ("#datatype".Equals(token))
                {
                    startNewTable = true;

                    table = new FluxTable();
                    yield return (table, null);

                    tableIndex++;
                    tableId = -1;
                }
                else if (table == null)
                {
                    throw new FluxCsvParserException(
                        "Unable to parse CSV response. FluxTable definition was not found.");
                }

                //#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,double,string,string,string
                if ("#datatype".Equals(token))
                {
                    AddDataTypes(table, csv);
                }
                else if ("#group".Equals(token))
                {
                    AddGroups(table, csv);
                }
                else if ("#default".Equals(token))
                {
                    AddDefaultEmptyValues(table, csv);
                }
                else
                {
                    // parse column names
                    if (startNewTable)
                    {
                        AddColumnNamesAndTags(table, csv);
                        startNewTable = false;
                        continue;
                    }

                    int currentId;

                    try
                    {
                        currentId = Convert.ToInt32(csv[1 + 1]);
                    }
                    catch (Exception)
                    {
                        throw new FluxCsvParserException("Unable to parse CSV response.");
                    }
                    if (tableId == -1)
                    {
                        tableId = currentId;
                    }

                    if (tableId != currentId)
                    {
                        //create new table with previous column headers settings
                        var fluxColumns = table.Columns;
                        table = new FluxTable();
                        table.Columns.AddRange(fluxColumns);
                        yield return (table, null);

                        tableIndex++;
                        tableId = currentId;
                    }

                    yield return (table, ParseRecord(tableIndex - 1, table, csv));
                }
            }
        }
#endif

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

        private Object ToValue(string strValue, FluxColumn column)
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
                        return Convert.ToDouble(strValue, CultureInfo.InvariantCulture);
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
            catch (Exception)
            {
                throw new FluxCsvParserException("Unable to parse CSV response.");
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

        private void AddDataTypes(FluxTable table, CsvReader dataTypes)
        {
            Arguments.CheckNotNull(table, "table");
            Arguments.CheckNotNull(dataTypes, "dataTypes");

            for (var index = 1; index < dataTypes.Context.Record.Length; index++)
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

        private void AddGroups(FluxTable table, CsvReader groups)
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