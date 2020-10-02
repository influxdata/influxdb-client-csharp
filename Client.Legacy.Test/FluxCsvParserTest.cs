using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Flux.Domain;
using InfluxDB.Client.Core.Flux.Exceptions;
using InfluxDB.Client.Core.Flux.Internal;
using NodaTime;
using NodaTime.Text;
using NUnit.Framework;

namespace Client.Legacy.Test
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

            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,string,string,string,string,long,long,string\n"
                            + "#group,false,false,true,true,true,true,true,true,false,false,false\n"
                            + "#default,_result,,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_field,_measurement,host,region,_value2,value1,value_str\n"
                            + ",,0,1677-09-21T00:12:43.145224192Z,2018-07-16T11:21:02.547596934Z,free,mem,A,west,121,11,test\n"
                            + ",,1,1677-09-21T00:12:43.145224192Z,2018-07-16T11:21:02.547596934Z,free,mem,B,west,484,22,test\n"
                            + ",,2,1677-09-21T00:12:43.145224192Z,2018-07-16T11:21:02.547596934Z,usage_system,cpu,A,west,1444,38,test\n"
                            + ",,3,1677-09-21T00:12:43.145224192Z,2018-07-16T11:21:02.547596934Z,user_usage,cpu,A,west,2401,49,test";

            var tables = ParseFluxResponse(data);

            var columnHeaders = tables[0].Columns;
            Assert.That(columnHeaders.Count == 11);
            var fluxColumn1 = columnHeaders[0];

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
            var fluxTable1 = tables[0];

            Assert.That(fluxTable1.Records.Count == 1);

            var fluxRecord1 = fluxTable1.Records[0];

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
            var fluxTable2 = tables[1];

            Assert.That(fluxTable2.Records.Count == 1);

            var fluxRecord2 = fluxTable2.Records[0];
            Assert.That(1.Equals(fluxRecord2.Table));
            Assert.That("B".Equals(fluxRecord2.GetValueByKey("host")));
            Assert.That("west".Equals(fluxRecord2.GetValueByKey("region")));
            Assert.That(fluxRecord2.Values.Count == 11);
            Assert.IsNull(fluxRecord2.GetValue());
            Assert.That(22L.Equals(fluxRecord2.GetValueByKey("value1")));
            Assert.That(484L.Equals(fluxRecord2.GetValueByKey("_value2")));
            Assert.That("test".Equals(fluxRecord2.GetValueByKey("value_str")));

            // Record 3
            var fluxTable3 = tables[2];

            Assert.That(fluxTable3.Records.Count == 1);

            var fluxRecord3 = fluxTable3.Records[0];
            Assert.That(2.Equals(fluxRecord3.Table));
            Assert.That("A".Equals(fluxRecord3.GetValueByKey("host")));
            Assert.That("west".Equals(fluxRecord3.GetValueByKey("region")));
            Assert.That(fluxRecord3.Values.Count == 11);
            Assert.IsNull(fluxRecord3.GetValue());
            Assert.That(38L.Equals(fluxRecord3.GetValueByKey("value1")));
            Assert.That(1444L.Equals(fluxRecord3.GetValueByKey("_value2")));
            Assert.That("test".Equals(fluxRecord3.GetValueByKey("value_str")));

            // Record 4
            var fluxTable4 = tables[3];

            Assert.That(fluxTable4.Records.Count == 1);

            var fluxRecord4 = fluxTable4.Records[0];
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
            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,boolean\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,true\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,true\n";

            var tables = ParseFluxResponse(data);

            Assert.That(tables.Count == 1);
            Assert.That(tables[0].Records.Count == 1);

            var fluxRecord = tables[0].Records[0];

            Assert.That(InstantPattern.ExtendedIso.Parse("1970-01-01T00:00:10Z").Value.Equals(fluxRecord.GetStart()));
            Assert.That(InstantPattern.ExtendedIso.Parse("1970-01-01T00:00:20Z").Value.Equals(fluxRecord.GetStop()));
            Assert.That(InstantPattern.ExtendedIso.Parse("1970-01-01T00:00:10Z").Value.Equals(fluxRecord.GetTime()));
            Assert.That(new DateTime(1970,1,1, 0, 0, 10).Equals(fluxRecord.GetTimeInDateTime()));
            Assert.That(fluxRecord.GetValue().Equals(10L));
            Assert.That(fluxRecord.GetField().Equals("free"));
            Assert.That(fluxRecord.GetMeasurement().Equals("mem"));
        }

        [Test]
        public void MappingBoolean()
        {
            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,boolean\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,true\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,true\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,false\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,x\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            var tables = ParseFluxResponse(data);
            
            Assert.IsNotNull(tables.Count == 1);

            var records = tables[0].Records;

            Assert.That(records.Count == 4);
            
            Assert.That(true.Equals(records[0].GetValueByKey("value")));
            Assert.That(false.Equals(records[1].GetValueByKey("value")));
            Assert.That(false.Equals(records[2].GetValueByKey("value")));
            Assert.That(true.Equals(records[3].GetValueByKey("value")));
        }

        [Test]
        public void MappingUnsignedLong()
        {
            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,unsignedLong\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,17916881237904312345\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            var expected = Convert.ToUInt64("17916881237904312345");

            var tables = ParseFluxResponse(data);
            
            Assert.IsNotNull(tables.Count == 1);

            var records = tables[0].Records;

            Assert.That(records.Count == 2);
            
            Assert.That(expected.Equals(records[0].GetValueByKey("value")));
            Assert.IsNull(records[1].GetValueByKey("value"));
        }

        [Test]
        public void MappingDouble()
        {
            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,double\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,12.25\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            var tables = ParseFluxResponse(data);
            
            Assert.IsNotNull(tables.Count == 1);

            var records = tables[0].Records;

            Assert.That(records.Count == 2);
            
            Assert.That(12.25D.Equals(records[0].GetValueByKey("value")));
            Assert.IsNull(records[1].GetValueByKey("value"));
        }

        [Test]
        public void MappingBase64Binary()
        {
            var binaryData = "test value";
            var encodedString = Convert.ToBase64String(Encoding.UTF8.GetBytes(binaryData));

            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,base64Binary\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A," +
                            encodedString + "\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            var tables = ParseFluxResponse(data);
            
            Assert.IsNotNull(tables.Count == 1);

            var records = tables[0].Records;

            Assert.That(records.Count == 2);

            var value = (byte[]) records[0].GetValueByKey("value");
            
            Assert.IsNotEmpty(value);
            Assert.That(binaryData.Equals(Encoding.UTF8.GetString(value)));

            Assert.IsNull(records[1].GetValueByKey("value"));
        }

        [Test]
        public void MappingRfc3339()
        {
            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,dateTime:RFC3339\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,1970-01-01T00:00:10Z\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            var tables = ParseFluxResponse(data);

            Assert.IsNotNull(tables.Count == 1);

            var records = tables[0].Records;

            Assert.That(records.Count == 2);

            Assert.That(Instant.Add(new Instant(), Duration.FromSeconds(10L))
                            .Equals(records[0].GetValueByKey("value")));
            Assert.IsNull(records[1].GetValueByKey("value"));
        }

        [Test]
        public void MappingRfc3339Nano()
        {
            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,dateTime:RFC3339Nano\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,1970-01-01T00:00:10.999999999Z\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            var tables = ParseFluxResponse(data);
            
            Assert.IsNotNull(tables.Count == 1);

            var records = tables[0].Records;

            Assert.That(records.Count == 2);

            var timeSeconds = Instant.Add(new Instant(), Duration.FromSeconds(10L));
            var timeNanos = Instant.Add(timeSeconds, Duration.FromNanoseconds(999999999L));

            Assert.That(timeNanos.Equals(records[0].GetValueByKey("value")));
            Assert.IsNull(records[1].GetValueByKey("value"));
        }

        [Test]
        public void MappingDuration()
        {
            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,duration\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,125\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            var tables = ParseFluxResponse(data);
            
            Assert.IsNotNull(tables.Count == 1);

            var records = tables[0].Records;

            Assert.That(records.Count == 2);

            Assert.That(records[0].GetValueByKey("value").Equals(Duration.FromNanoseconds(125)));
            Assert.That(records[1].GetValueByKey("value") == null);
        }

        [Test]
        public void GroupKey()
        {
            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,duration\n"
                            + "#group,false,false,false,false,true,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,125\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            var tables = ParseFluxResponse(data);

            Assert.IsNotNull(tables.Count == 1);

            Assert.That(tables[0].Columns.Count == 10);
            Assert.That(tables[0].GetGroupKey().Count == 2);
        }

        [Test]
        public void UnknownTypeAsString()
        {
            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,unknown\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,12.25\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            var tables = ParseFluxResponse(data);
            
            Assert.IsNotNull(tables.Count == 1);
            
            var records = tables[0].Records;

            Assert.That(records.Count == 2);
            
            Assert.That("12.25".Equals(records[0].GetValueByKey("value")));
            Assert.IsNull(records[1].GetValueByKey("value"));
        }
        
        [Test]
        public void Error() 
        {
            var message = "failed to create physical plan: invalid time bounds from procedure from: bounds contain zero time";
            var data = "#datatype,string,string\n"
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
                Assert.That(e.Message.Equals(message));
                Assert.That(e.Reference.Equals(897));
            }
        }

        [Test]
        public void ErrorWithoutReference()
        {
            var message = "failed to create physical plan: invalid time bounds from procedure from: bounds contain zero time";
            var data = "#datatype,string,string\n"
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
                Assert.That(e.Message.Equals(message));
                Assert.That(e.Reference.Equals(0));
            }
        }

        [Test]
        public void ParsingToConsumer()
        {
            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,unknown\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,12.25\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            var records = new List<FluxRecord>();

            var consumer = new TestConsumer
            (
                            acceptTable: (table) => { },
                            acceptRecord: (record) => { records.Add(record); }
            );

            _parser.ParseFluxResponse(FluxCsvParser.ToStream(data), new DefaultCancellable(), consumer);
            Assert.That(records.Count == 2);
        }

        [Test]
        public async Task ParsingToAsyncEnumerable()
        {
            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,unknown\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,12.25\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            var records = new List<FluxRecord>();

            await foreach (var (table, record) in _parser.ParseFluxResponseAsync(new StringReader(data), CancellationToken.None))
            {
                // table with null record is "new table" indicator
                if (!(record is null))
                    records.Add(record);
            }

            Assert.That(records.Count == 2);
        }

        [Test]
        public void CancelParsing()
        {
            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,unknown\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,12.25\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            var records = new List<FluxRecord>();

            var defaultCancellable = new DefaultCancellable();

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
        public async Task CancelParsingAsync()
        {
            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,unknown\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,12.25\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            var records = new List<FluxRecord>();

            var cancellationSource = new CancellationTokenSource();

            await foreach (var (table, record) in _parser.ParseFluxResponseAsync(new StringReader(data), cancellationSource.Token))
            {
                // table with null record is "new table" indicator
                if (!(record is null))
                {
                    cancellationSource.Cancel();
                    records.Add(record);
                }
            }

            Assert.That(records.Count == 1);
        }

        [Test]
        public void ParseDifferentSchemas()
        {
            var data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string\n"
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
            
            var tables = ParseFluxResponse(data);
            
            Assert.That(tables.Count == 2);
            
            Assert.That(tables[0].Columns.Count == 8);
            Assert.That(tables[1].Columns.Count == 10);
        }

        [Test]
        public void ParsingWithoutTableDefinition() 
        {
            var data = ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                          + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,12.25\n"
                          + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            try
            {
                ParseFluxResponse(data);
                
                Assert.Fail();
            }
            catch (FluxCsvParserException e)
            {
                Assert.That(e.Message.Equals("Unable to parse CSV response. FluxTable definition was not found."));
            }
        }

        [Test]
        [SetCulture("de-DE")]
        public void CustomCultureInfo()
        {
            var data =
                "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,double,long,unsignedLong\n"
                + "#group,false,false,false,false,false,false,false,false,false,true,true,true\n"
                + "#default,_result,,,,,,,,,,,\n"
                + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value-double,value-long,value-unsignedLong\n"
                + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,6.1949943235120708,61949943235120708,61949943235120708\n";

            var tables = ParseFluxResponse(data);

            Assert.IsNotNull(tables.Count == 1);

            var records = tables[0].Records;

            Assert.That(records.Count == 1);

            Assert.AreEqual(6.1949943235120708D, records[0].GetValueByKey("value-double"));
            Assert.AreEqual(61949943235120708L, records[0].GetValueByKey("value-long"));
            Assert.AreEqual(61949943235120708UL, records[0].GetValueByKey("value-unsignedLong"));
        }

        [Test]
        public void MultipleQueries()
        {
            var data =
                "#datatype,string,long,string,string,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,double,string\n"
                + "#group,false,false,true,true,true,true,false,false,true\n"
                + "#default,t1,,,,,,,,\n"
                + ",result,table,_field,_measurement,_start,_stop,_time,_value,tag\n"
                + ",,0,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:20:00Z,2,test1\n"
                + ",,0,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:21:40Z,2,test1\n"
                + ",,0,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:23:20Z,2,test1\n"
                + ",,0,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:25:00Z,2,test1\n"
                + ",,0,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:26:40Z,2,test1\n"
                + ",,0,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:28:20Z,2,test1\n"
                + ",,0,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:30:00Z,2,test1\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:20:00Z,2,test2\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:21:40Z,2,test2\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:23:20Z,2,test2\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:25:00Z,2,test2\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:26:40Z,2,test2\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:28:20Z,2,test2\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:30:00Z,2,test2\n"
                + "\n"
                + "#datatype,string,long,string,string,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,double,string\n"
                + "#group,false,false,true,true,true,true,false,false,true\n"
                + "#default,t2,,,,,,,,\n"
                + ",result,table,_field,_measurement,_start,_stop,_time,_value,tag\n"
                + ",,0,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:20:00Z,2,test1\n"
                + ",,0,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:21:40Z,2,test1\n"
                + ",,0,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:23:20Z,2,test1\n"
                + ",,0,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:25:00Z,2,test1\n"
                + ",,0,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:26:40Z,2,test1\n"
                + ",,0,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:28:20Z,2,test1\n"
                + ",,0,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:30:00Z,2,test1\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:20:00Z,2,test2\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:21:40Z,2,test2\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:23:20Z,2,test2\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:25:00Z,2,test2\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:26:40Z,2,test2\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:28:20Z,2,test2\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:30:00Z,2,test2";

            var tables = ParseFluxResponse(data);
            Assert.AreEqual(4, tables.Count);
            Assert.AreEqual(7, tables[0].Records.Count);
            Assert.AreEqual(7, tables[1].Records.Count);
            Assert.AreEqual(7, tables[2].Records.Count);
            Assert.AreEqual(7, tables[3].Records.Count);
        }

        [Test]
        public void TableIndexNotStartAtZero()
        {
            var data =
                "#datatype,string,long,string,string,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,double,string\n"
                + "#group,false,false,true,true,true,true,false,false,true\n"
                + "#default,t1,,,,,,,,\n"
                + ",result,table,_field,_measurement,_start,_stop,_time,_value,tag\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:20:00Z,2,test1\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:21:40Z,2,test1\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:23:20Z,2,test1\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:25:00Z,2,test1\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:26:40Z,2,test1\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:28:20Z,2,test1\n"
                + ",,1,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:30:00Z,2,test1\n"
                + ",,2,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:20:00Z,2,test2\n"
                + ",,2,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:21:40Z,2,test2\n"
                + ",,2,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:23:20Z,2,test2\n"
                + ",,2,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:25:00Z,2,test2\n"
                + ",,2,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:26:40Z,2,test2\n"
                + ",,2,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:28:20Z,2,test2\n"
                + ",,2,value,python_client_test,2010-02-27T04:48:32.752600083Z,2020-02-27T16:48:32.752600083Z,2020-02-27T16:30:00Z,2,test2\n";
            
            var tables = ParseFluxResponse(data);
            Assert.AreEqual(2, tables.Count);
            Assert.AreEqual(7, tables[0].Records.Count);
            Assert.AreEqual(7, tables[1].Records.Count);
        }

        private List<FluxTable> ParseFluxResponse(string data)
        {
            var consumer = new FluxCsvParser.FluxResponseConsumerTable();
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