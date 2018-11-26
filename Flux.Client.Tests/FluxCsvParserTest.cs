using System;
using System.Collections.Generic;
using System.Text;
using NodaTime;
using NodaTime.Text;
using NUnit.Framework;
using Platform.Common.Flux.Domain;
using Platform.Common.Flux.Error;
using Platform.Common.Flux.Parser;
using Platform.Common.Platform.Rest;

namespace Flux.Client.Tests
{
    [TestFixture]
    public class FluxCsvParserTest
    {
        private FluxCsvParser _parser;

        [OneTimeSetUp]
        public void SetUp()
        {
            _parser = new FluxCsvParser();
        }

        [Test]
        public void ResponseWithMultipleValues()
        {
            // curl -i -XPOST --data-urlencode 'q=from(bucket: "ubuntu_test") |> last()
            // |> map(fn: (r) => ({value1: r._value, _value2:r._value * r._value, value_str: "test"}))'
            // --data-urlencode "orgName=0" http://localhost:8093/api/v2/query

            string data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,string,string,string,string,long,long,string\n"
                            + "#group,false,false,true,true,true,true,true,true,false,false,false\n"
                            + "#default,_result,,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_field,_measurement,host,region,_value2,value1,value_str\n"
                            + ",,0,1677-09-21T00:12:43.145224192Z,2018-07-16T11:21:02.547596934Z,free,mem,A,west,121,11,test\n"
                            + ",,1,1677-09-21T00:12:43.145224192Z,2018-07-16T11:21:02.547596934Z,free,mem,B,west,484,22,test\n"
                            + ",,2,1677-09-21T00:12:43.145224192Z,2018-07-16T11:21:02.547596934Z,usage_system,cpu,A,west,1444,38,test\n"
                            + ",,3,1677-09-21T00:12:43.145224192Z,2018-07-16T11:21:02.547596934Z,user_usage,cpu,A,west,2401,49,test";

            List<FluxTable> tables = ParseFluxResponse(data);

            List<FluxColumn> columnHeaders = tables[0].Columns;
            Assert.That(columnHeaders.Count == 11);
            FluxColumn fluxColumn1 = columnHeaders[0];

            Assert.IsFalse(fluxColumn1.Group);
            Assert.IsFalse(columnHeaders[1].Group);
            Assert.IsTrue(columnHeaders[2].Group);
            Assert.IsTrue(columnHeaders[3].Group);
            Assert.IsTrue(columnHeaders[4].Group);
            Assert.IsTrue(columnHeaders[5].Group);
            Assert.IsTrue(columnHeaders[6].Group);
            Assert.IsTrue(columnHeaders[7].Group);
            Assert.IsFalse(columnHeaders[8].Group);
            Assert.IsFalse(columnHeaders[8].Group);
            Assert.IsFalse(columnHeaders[8].Group);

            Assert.That(tables.Count == 4);

            // Record 1
            FluxTable fluxTable1 = tables[0];

            Assert.That(fluxTable1.Records.Count == 1);

            FluxRecord fluxRecord1 = fluxTable1.Records[0];

            Assert.That(0.Equals(fluxRecord1.Table));
            Assert.That("A".Equals(fluxRecord1.GetValueByKey("host")));
            Assert.That("west".Equals(fluxRecord1.GetValueByKey("region")));
            Assert.That(fluxRecord1.Values.Count == 11);
            Assert.IsNull(fluxRecord1.GetValue());
            Assert.That(11L.Equals(fluxRecord1.GetValueByKey("value1")));
            Assert.That(121L.Equals(fluxRecord1.GetValueByKey("_value2")));
            Assert.That("test".Equals(fluxRecord1.GetValueByKey("value_str")));
            Assert.That(121L.Equals(fluxRecord1.GetValueByIndex(8)));
            Assert.That(11L.Equals(fluxRecord1.GetValueByIndex(9)));
            Assert.That("test".Equals(fluxRecord1.GetValueByIndex(10)));

            // Record 2
            FluxTable fluxTable2 = tables[1];

            Assert.That(fluxTable2.Records.Count == 1);

            FluxRecord fluxRecord2 = fluxTable2.Records[0];
            Assert.That(1.Equals(fluxRecord2.Table));
            Assert.That("B".Equals(fluxRecord2.GetValueByKey("host")));
            Assert.That("west".Equals(fluxRecord2.GetValueByKey("region")));
            Assert.That(fluxRecord2.Values.Count == 11);
            Assert.IsNull(fluxRecord2.GetValue());
            Assert.That(22L.Equals(fluxRecord2.GetValueByKey("value1")));
            Assert.That(484L.Equals(fluxRecord2.GetValueByKey("_value2")));
            Assert.That("test".Equals(fluxRecord2.GetValueByKey("value_str")));

            // Record 3
            FluxTable fluxTable3 = tables[2];

            Assert.That(fluxTable3.Records.Count == 1);

            FluxRecord fluxRecord3 = fluxTable3.Records[0];
            Assert.That(2.Equals(fluxRecord3.Table));
            Assert.That("A".Equals(fluxRecord3.GetValueByKey("host")));
            Assert.That("west".Equals(fluxRecord3.GetValueByKey("region")));
            Assert.That(fluxRecord3.Values.Count == 11);
            Assert.IsNull(fluxRecord3.GetValue());
            Assert.That(38L.Equals(fluxRecord3.GetValueByKey("value1")));
            Assert.That(1444L.Equals(fluxRecord3.GetValueByKey("_value2")));
            Assert.That("test".Equals(fluxRecord3.GetValueByKey("value_str")));

            // Record 4
            FluxTable fluxTable4 = tables[3];

            Assert.That(fluxTable4.Records.Count == 1);

            FluxRecord fluxRecord4 = fluxTable4.Records[0];
            Assert.That(3.Equals(fluxRecord4.Table));
            Assert.That("A".Equals(fluxRecord4.GetValueByKey("host")));
            Assert.That("west".Equals(fluxRecord4.GetValueByKey("region")));
            Assert.That(fluxRecord4.Values.Count == 11);
            Assert.IsNull(fluxRecord4.GetValue());
            Assert.That(49L.Equals(fluxRecord4.GetValueByKey("value1")));
            Assert.That(2401L.Equals(fluxRecord4.GetValueByKey("_value2")));
            Assert.That("test".Equals(fluxRecord4.GetValueByKey("value_str")));
        }

        [Test]
        public void Shortcut()
        {
            string data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,boolean\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,true\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,true\n";

            List<FluxTable> tables = ParseFluxResponse(data);

            Assert.That(tables.Count == 1);
            Assert.That(tables[0].Records.Count == 1);

            FluxRecord fluxRecord = tables[0].Records[0];

            Assert.That(InstantPattern.ExtendedIso.Parse("1970-01-01T00:00:10Z").Value.Equals(fluxRecord.GetStart()));
            Assert.That(InstantPattern.ExtendedIso.Parse("1970-01-01T00:00:20Z").Value.Equals(fluxRecord.GetStop()));
            Assert.That(InstantPattern.ExtendedIso.Parse("1970-01-01T00:00:10Z").Value.Equals(fluxRecord.GetTime()));
            Assert.That(fluxRecord.GetValue().Equals(10L));
            Assert.That(fluxRecord.GetField().Equals("free"));
            Assert.That(fluxRecord.GetMeasurement().Equals("mem"));
        }

        [Test]
        public void MappingBoolean()
        {
            string data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,boolean\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,true\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,true\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,false\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,x\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            List<FluxTable> tables = ParseFluxResponse(data);
            
            Assert.IsNotNull(tables.Count == 1);

            List<FluxRecord> records = tables[0].Records;

            Assert.That(records.Count == 4);
            
            Assert.That(true.Equals(records[0].GetValueByKey("value")));
            Assert.That(false.Equals(records[1].GetValueByKey("value")));
            Assert.That(false.Equals(records[2].GetValueByKey("value")));
            Assert.That(true.Equals(records[3].GetValueByKey("value")));
        }

        [Test]
        public void MappingUnsignedLong()
        {
            string data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,unsignedLong\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,17916881237904312345\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            ulong expected = Convert.ToUInt64("17916881237904312345");

            List<FluxTable> tables = ParseFluxResponse(data);
            
            Assert.IsNotNull(tables.Count == 1);

            List<FluxRecord> records = tables[0].Records;

            Assert.That(records.Count == 2);
            
            Assert.That(expected.Equals(records[0].GetValueByKey("value")));
            Assert.IsNull(records[1].GetValueByKey("value"));
        }

        [Test]
        public void MappingDouble()
        {
            string data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,double\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,12.25\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            List<FluxTable> tables = ParseFluxResponse(data);
            
            Assert.IsNotNull(tables.Count == 1);

            List<FluxRecord> records = tables[0].Records;

            Assert.That(records.Count == 2);
            
            Assert.That(12.25D.Equals(records[0].GetValueByKey("value")));
            Assert.IsNull(records[1].GetValueByKey("value"));
        }

        [Test]
        public void MappingBase64Binary()
        {
            string binaryData = "test value";
            string encodedString = Convert.ToBase64String(Encoding.UTF8.GetBytes(binaryData));

            string data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,base64Binary\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A," +
                            encodedString + "\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            List<FluxTable> tables = ParseFluxResponse(data);
            
            Assert.IsNotNull(tables.Count == 1);

            List<FluxRecord> records = tables[0].Records;

            Assert.That(records.Count == 2);

            byte[] value = (byte[]) records[0].GetValueByKey("value");
            
            Assert.IsNotEmpty(value);
            Assert.That(binaryData.Equals(Encoding.UTF8.GetString(value)));

            Assert.IsNull(records[1].GetValueByKey("value"));
        }

        [Test]
        public void MappingRfc3339()
        {
            string data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,dateTime:RFC3339\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,1970-01-01T00:00:10Z\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            List<FluxTable> tables = ParseFluxResponse(data);

            Assert.IsNotNull(tables.Count == 1);

            List<FluxRecord> records = tables[0].Records;

            Assert.That(records.Count == 2);

            Assert.That(Instant.Add(new Instant(), Duration.FromSeconds(10L))
                            .Equals(records[0].GetValueByKey("value")));
            Assert.IsNull(records[1].GetValueByKey("value"));
        }

        [Test]
        public void MappingRfc3339Nano()
        {
            string data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,dateTime:RFC3339Nano\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,1970-01-01T00:00:10.999999999Z\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            List<FluxTable> tables = ParseFluxResponse(data);
            
            Assert.IsNotNull(tables.Count == 1);

            List<FluxRecord> records = tables[0].Records;

            Assert.That(records.Count == 2);

            Instant timeSeconds = Instant.Add(new Instant(), Duration.FromSeconds(10L));
            Instant timeNanos = Instant.Add(timeSeconds, Duration.FromNanoseconds(999999999L));

            Assert.That(timeNanos.Equals(records[0].GetValueByKey("value")));
            Assert.IsNull(records[1].GetValueByKey("value"));
        }

        [Test]
        public void MappingDuration()
        {
            string data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,duration\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,125\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            List<FluxTable> tables = ParseFluxResponse(data);
            
            Assert.IsNotNull(tables.Count == 1);

            List<FluxRecord> records = tables[0].Records;

            Assert.That(records.Count == 2);

            Assert.That(records[0].GetValueByKey("value").Equals(Duration.FromNanoseconds(125)));
            Assert.That(records[1].GetValueByKey("value") == null);
        }

        [Test]
        public void GroupKey()
        {
            string data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,duration\n"
                            + "#group,false,false,false,false,true,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,125\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            List<FluxTable> tables = ParseFluxResponse(data);

            Assert.IsNotNull(tables.Count == 1);

            Assert.That(tables[0].Columns.Count == 10);
            Assert.That(tables[0].GetGroupKey().Count == 2);
        }

        [Test]
        public void UnknownTypeAsString()
        {
            string data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,unknown\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,12.25\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            List<FluxTable> tables = ParseFluxResponse(data);
            
            Assert.IsNotNull(tables.Count == 1);
            
            List<FluxRecord> records = tables[0].Records;

            Assert.That(records.Count == 2);
            
            Assert.That("12.25".Equals(records[0].GetValueByKey("value")));
            Assert.IsNull(records[1].GetValueByKey("value"));
        }
        
        [Test]
        public void Error() 
        {
            string message = "failed to create physical plan: invalid time bounds from procedure from: bounds contain zero time";
            string data = "#datatype,string,string\n"
                            + "#group,true,true\n"
                            + "#default,,\n"
                            + ",error,reference\n"
                            + "," + message + ",897";

            try
            {
                ParseFluxResponse(data);
                
                Assert.Fail();
            }
            catch (FluxQueryException e)
            {
                Assert.That(e.Error.Equals(message));
                Assert.That(e.Reference.Equals(897));
            }
        }

        [Test]
        public void ErrorWithoutReference()
        {
            string message = "failed to create physical plan: invalid time bounds from procedure from: bounds contain zero time";
            string data = "#datatype,string,string\n"
                            + "#group,true,true\n"
                            + "#default,,\n"
                            + ",error,reference\n"
                            + "," + message + ",";

            try
            {
                ParseFluxResponse(data);
                
                Assert.Fail();
            }
            catch (FluxQueryException e)
            {
                Assert.That(e.Error.Equals(message));
                Assert.That(e.Reference.Equals(0));
            }
        }

        [Test]
        public void ParsingToConsumer()
        {
            string data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,unknown\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,12.25\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            List<FluxRecord> records = new List<FluxRecord>();

            var consumer = new TestConsumer
            (
                            acceptTable: (table) => { },
                            acceptRecord: (record) => { records.Add(record); }
            );

            _parser.ParseFluxResponse(FluxCsvParser.ToStream(data), new DefaultCancellable(), consumer);
            Assert.That(records.Count == 2);
        }

        [Test]
        public void CancelParsing()
        {
            string data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,unknown\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,12.25\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            List<FluxRecord> records = new List<FluxRecord>();

            DefaultCancellable defaultCancellable = new DefaultCancellable();

            var consumer = new TestConsumer
            (
                            acceptTable: (table) => { },
                            acceptRecord: (record) =>
                            {
                                defaultCancellable.Cancel();
                                records.Add(record);
                            }
            );

            _parser.ParseFluxResponse(FluxCsvParser.ToStream(data), defaultCancellable, consumer);
            Assert.That(records.Count == 1);
        }

        [Test]
        public void ParseDifferentSchemas()
        {
            string data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string\n"
                            + "#group,false,false,false,false,false,false,false,false\n"
                            + "#default,_result,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem\n"
                            + "\n"
                            + "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,unknown\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,12.25\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";
            
            List<FluxTable> tables = ParseFluxResponse(data);
            
            Assert.That(tables.Count == 2);
            
            Assert.That(tables[0].Columns.Count == 8);
            Assert.That(tables[1].Columns.Count == 10);
        }

        [Test]
        public void ParsingWithoutTableDefinition() 
        {
            string data = ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                          + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,12.25\n"
                          + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            try
            {
                ParseFluxResponse(data);
                
                Assert.Fail();
            }
            catch (FluxCsvParserException e)
            {
                Assert.That(e.Error.Equals("Unable to parse CSV response. FluxTable definition was not found."));
            }
        }

        private List<FluxTable> ParseFluxResponse(string data)
        {
            FluxCsvParser.FluxResponseConsumerTable consumer = new FluxCsvParser.FluxResponseConsumerTable();
            _parser.ParseFluxResponse(data, new DefaultCancellable(), consumer);

            return consumer.Tables;
        }

        private class DefaultCancellable : ICancellable
        {
            private bool _cancelled;

            public void Cancel()
            {
                _cancelled = true;
            }

            public bool IsCancelled()
            {
                return _cancelled;
            }
        }

        public class TestConsumer : FluxCsvParser.IFluxResponseConsumer
        {
            public readonly Action<FluxTable> AcceptTable;
            public readonly Action<FluxRecord> AcceptRecord;

            public TestConsumer(Action<FluxTable> acceptTable, Action<FluxRecord> acceptRecord)
            {
                AcceptTable = acceptTable;
                AcceptRecord = acceptRecord;
            }

            public void Accept(int index, ICancellable cancellable, FluxTable table)
            {
                AcceptTable(table);
            }

            public void Accept(int index, ICancellable cancellable, FluxRecord record)
            {
                AcceptRecord(record);
            }
        }
    }
}