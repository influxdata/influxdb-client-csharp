using System;
using System.Collections.Generic;
using System.Linq;
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

        private List<FluxTable> ParseFluxResponse(string data)
        {
            FluxCsvParser.FluxResponseConsumerTable consumer = new FluxCsvParser.FluxResponseConsumerTable();
            _parser.ParseFluxResponse(data, new DefaultCancellable(), consumer).GetAwaiter().GetResult();

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