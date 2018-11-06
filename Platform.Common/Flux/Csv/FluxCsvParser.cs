using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using LumenWorks.Framework.IO.Csv;
using NodaTime;
using NodaTime.Text;
using Platform.Common.Flux.Domain;
using Platform.Common.Flux.Error;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;

namespace Platform.Common.Flux.Csv
{
/**
 * This class us used to construct FluxResult from CSV.
 *
 * @see org.influxdata.flux
 */
    public class FluxCsvParser
    {
        private enum ParsingState
        {
            Normal,
            InError
        }

        public interface IFluxResponseConsumer
        {
            /**
             * Add new {@link FluxTable} to consumer.
             *
             * @param index       index of table
             * @param cancellable cancellable
             * @param table       new {@link FluxTable}
             */
            void Accept(int index, ICancellable cancellable, FluxTable table);

            /**
             * Add new {@link FluxRecord} to consumer.
             *
             * @param index       index of table
             * @param cancellable cancellable
             * @param record      new {@link FluxRecord}
             */

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

        /**
         * Parse Flux CSV response to {@link FluxResponseConsumer}.
         *
         * @param source with data
         * @param cancellable    to cancel parsing
         * @param consumer       to accept {@link FluxTable}s and {@link FluxRecord}s
         * @throws IOException If there is a problem with reading CSV
         */
        public void ParseFluxResponse(BufferedStream source, ICancellable cancellable, IFluxResponseConsumer consumer)
        {
            Arguments.CheckNotNull(source, "source");

            var csv = new CsvReader(new StreamReader(source), false, ',', '"', '"', ' ');

            csv.MissingFieldAction = MissingFieldAction.ReplaceByNull;

            ParsingState parsingState = ParsingState.Normal;

            int tableIndex = 0;
            bool startNewTable = false;
            FluxTable table = null;

            while (csv.ReadNextRecord())
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
                    string error = csv[1];
                    string referenceValue = csv[2];

                    int reference = 0;

                    if (referenceValue != null && !String.IsNullOrEmpty(referenceValue))
                    {
                        reference = Convert.ToInt32(referenceValue);
                    }

                    throw new FluxQueryException(error, reference);
                }

                String token = csv[0];

                //// start new table
                if ("#datatype".Equals(token))
                {
                    startNewTable = true;

                    table = new FluxTable();
                    consumer.Accept(tableIndex, cancellable, table);
                    tableIndex++;
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

                    int currentIndex;

                    try
                    {
                        currentIndex = Convert.ToInt32(csv[1 + 1]);
                    }
                    catch (Exception)
                    {
                        throw new FluxCsvParserException("Unable to parse CSV response.");
                    }

                    if (currentIndex > (tableIndex - 1))
                    {
                        //create new table with previous column headers settings
                        List<FluxColumn> fluxColumns = table.Columns;
                        table = new FluxTable();
                        table.Columns.AddRange(fluxColumns);
                        consumer.Accept(tableIndex, cancellable, table);
                        tableIndex++;
                    }

                    FluxRecord fluxRecord = ParseRecord(tableIndex - 1, table, csv);
                    consumer.Accept(tableIndex - 1, cancellable, fluxRecord);
                }
            }
        }

        private FluxRecord ParseRecord(int tableIndex, FluxTable table, CsvReader csv) 
        {
            FluxRecord record = new FluxRecord(tableIndex);

            foreach (FluxColumn fluxColumn in table.Columns) 
            {
                string columnName = fluxColumn.Label;

                string strValue = csv[fluxColumn.Index + 1];

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
                string defaultValue = column.DefaultValue;
                
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
                        return Convert.ToDouble(strValue);
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

        public static BufferedStream ToStream(string str)
        {
            MemoryStream stream = new MemoryStream();
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return new BufferedStream(stream);
        }
        
        private void AddDataTypes(FluxTable table, CsvReader dataTypes) 
        {
            Arguments.CheckNotNull(table, "table");
            Arguments.CheckNotNull(dataTypes, "dataTypes");

            for (int index = 1; index < dataTypes.FieldCount; index++) 
            {
                string dataType = dataTypes[index];

                if (string.IsNullOrEmpty(dataType))
                {
                    continue;
                }

                FluxColumn columnDef = new FluxColumn
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

            for (int ii = 0; ii < table.Columns.Count; ii++) 
            {
                var fluxColumn = GetFluxColumn(ii, table);
                fluxColumn.Group = Convert.ToBoolean(groups[ii + 1]);
            }
        }
        
        private void AddDefaultEmptyValues(FluxTable table, CsvReader defaultEmptyValues) 
        {
            Arguments.CheckNotNull(table, "table");
            Arguments.CheckNotNull(defaultEmptyValues, "defaultEmptyValues");

            for (int ii = 0; ii < table.Columns.Count; ii++) 
            {
                var fluxColumn = GetFluxColumn(ii, table);
                fluxColumn.DefaultValue = defaultEmptyValues[ii + 1];
            }
        }
        
        private void AddColumnNamesAndTags(FluxTable table, CsvReader columnNames) 
        {
            Arguments.CheckNotNull(table, "table");
            Arguments.CheckNotNull(columnNames, "columnNames");

            int size = table.Columns.Count;

            for (int ii = 0; ii < size; ii++) 
            {
                FluxColumn fluxColumn = GetFluxColumn(ii, table);
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