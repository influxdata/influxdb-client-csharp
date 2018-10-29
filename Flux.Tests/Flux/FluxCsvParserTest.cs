using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using NLog;
using NodaTime;
using NodaTime.Text;
using NUnit.Framework;
using Platform.Common.Flux.Csv;
using Platform.Common.Flux.Domain;
using Platform.Common.Platform.Rest;

namespace Flux.Tests.Flux
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

            String data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,string,string,string,string,long,long,string\n"
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
            String data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,boolean\n"
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
        public void MappingDuration()
        {
            String data = "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,duration\n"
                            + "#group,false,false,false,false,false,false,false,false,false,true\n"
                            + "#default,_result,,,,,,,,,\n"
                            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,value\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,125\n"
                            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,\n";

            List<FluxTable> tables = ParseFluxResponse(data);

            Assert.That(tables[0].Records[0].GetValueByKey("value").Equals(Duration.FromNanoseconds(125)));
            Assert.That(tables[0].Records[1].GetValueByKey("value") == null);
        }

        private List<FluxTable> ParseFluxResponse(string data)
        {
            FluxCsvParser.FluxResponseConsumerTable consumer = new FluxCsvParser.FluxResponseConsumerTable();
            _parser.ParseFluxResponse(data, new DefaultCancellable(), consumer);

            return consumer.Tables;
        }

        private class DefaultCancellable : ICancellable
        {
            private bool _cancelled = false;

            public void Cancel()
            {
                _cancelled = true;
            }

            public bool IsCancelled()
            {
                return _cancelled;
            }
        }
    }
}