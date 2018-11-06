using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flux.Client;
using Flux.Client.Options;
using NodaTime;
using NUnit.Framework;
using Platform.Common.Flux.Domain;
using Platform.Common.Flux.Error;

namespace Flux.Tests.Flux
{
    public class ItFluxClientTest : AbstractItFluxClientTest
    {
        private static readonly string FromFluxDatabase = String.Format("from(bucket:\"{0}\")", DatabaseName);

        public override async Task PrepareDara()
        {
            await InfluxDbWrite("mem,host=A,region=west free=10i 10000000000", DatabaseName);
            await InfluxDbWrite("mem,host=A,region=west free=11i 20000000000", DatabaseName);
            await InfluxDbWrite("mem,host=B,region=west free=20i 10000000000", DatabaseName);
            await InfluxDbWrite("mem,host=B,region=west free=22i 20000000000", DatabaseName);
            await InfluxDbWrite("cpu,host=A,region=west usage_system=35i,user_usage=45i 10000000000", DatabaseName);
            await InfluxDbWrite("cpu,host=A,region=west usage_system=38i,user_usage=49i 20000000000", DatabaseName);
            await InfluxDbWrite("cpu,host=A,hyper-threading=true,region=west usage_system=38i,user_usage=49i 20000000000", DatabaseName);
        }

        [Test]
        public async Task ChunkedOneTable() 
        {            
            await PrepareChunkRecords();

            string flux = FromFluxDatabase + "\n"
                                           + "\t|> filter(fn: (r) => r[\"_measurement\"] == \"chunked\")\n"
                                           + "\t|> range(start: 1970-01-01T00:00:00.000000000Z)";

            await FluxClient.Query(flux, (cancellable, fluxRecord) => 
            {
                // +1 record
                CountdownEvent.Signal();

                if (CountdownEvent.CurrentCount % 100_000 == 0) 
                {
                    Console.WriteLine("Remaining parsed: " + CountdownEvent.CurrentCount + " records");
                }
            });

            WaitToCallback(30);
        }

        [Test]
        public async Task ChunkedMoreTables()
        {
            await PrepareChunkRecords();

            string flux = FromFluxDatabase + "\n"
                                           + "\t|> filter(fn: (r) => r[\"_measurement\"] == \"chunked\")\n"
                                           + "\t|> range(start: 1970-01-01T00:00:00.000000000Z)\n"
                                           + "\t|> window(every: 10m)";

            await FluxClient.Query(flux, (cancellable, fluxRecord) =>
            {
                // +1 record
                CountdownEvent.Signal();

                if (CountdownEvent.CurrentCount % 100_000 == 0)
                {
                    Console.WriteLine("Remaining parsed: {0} records", CountdownEvent.CurrentCount);
                }
            });

            WaitToCallback(30);
        }

        [Test]
        public async Task ChunkedCancel() 
        {
            await PrepareChunkRecords();

            string flux = FromFluxDatabase + "\n"
                                             + "\t|> filter(fn: (r) => r[\"_measurement\"] == \"chunked\")\n"
                                             + "\t|> range(start: 1970-01-01T00:00:00.000000000Z)\n"
                                             + "\t|> window(every: 10m)";

            CountdownEvent = new CountdownEvent(10_000);
            CountdownEvent cancelCountDown = new CountdownEvent(1);

            await FluxClient.Query(flux, (cancellable, fluxRecord) =>
            {
                // +1 record
                CountdownEvent.Signal();

                if (CountdownEvent.CurrentCount % 1_000 == 0 && CountdownEvent.CurrentCount != 0)
                {
                    Console.WriteLine("Remaining parsed: {0} records", CountdownEvent.CurrentCount);
                }

                if (CountdownEvent.CurrentCount == 9_000)
                {
                    cancellable.Cancel();
                    cancelCountDown.Signal();
                }
            });

            // wait to cancel
            WaitToCallback(cancelCountDown, 30);

            //
            Assert.That(CountdownEvent.CurrentCount.Equals(9_000));
        }

        [Test]
        public async Task Query()
        {
            string flux = FromFluxDatabase + "\n"
                                           + "\t|> range(start: 1970-01-01T00:00:00.000000000Z)\n"
                                           + "\t|> filter(fn: (r) => (r[\"_measurement\"] == \"mem\" AND r[\"_field\"] == \"free\"))\n"
                                           + "\t|> sum()";

            List<FluxTable> fluxTables = await FluxClient.Query(flux);

            AssertFluxResult(fluxTables);
        }

        [Test]
        public async Task QueryWithTime()
        {
            string flux = FromFluxDatabase + "\n"
                                           + "\t|> range(start: 1970-01-01T00:00:00.000000000Z)\n"
                                           + "\t|> filter(fn: (r) => (r[\"_measurement\"] == \"mem\" AND r[\"_field\"] == \"free\"))";

            List<FluxTable> fluxTables = await FluxClient.Query(flux);

            AssertFluxResultWithTime(fluxTables);
        }

        [Test]
        public async Task QueryDifferentSchemas()
        {
            String flux = FromFluxDatabase + "\n"
                                           + "\t|> range(start: 1970-01-01T00:00:00.000000000Z)";

            List<FluxTable> fluxTables = await FluxClient.Query(flux);

            Assert.That(fluxTables.Count == 6);
        }
        
        [Test]
        public async Task Error() 
        {
            try
            {
                await FluxClient.Query("from(bucket:\"telegraf\")");
                
                Assert.Fail();
            }
            catch (InfluxException e)
            {
                Assert.That(e.Errors.Count == 1);
                Assert.That(e.Errors[0].Contains("failed to create physical plan:"));
                Assert.That(e.Errors[0].Contains("Add a 'range' call to bound the query"));
            }
        }
        
        [Test]
        public async Task ErrorWithStatusOk() 
        {
            try
            {
                await FluxClient.Query(FromFluxDatabase);
                
                Assert.Fail();
            }
            catch (InfluxException e)
            {
                Assert.That(e.Errors.Count == 1);
                Assert.That(e.Errors[0].Contains("failed to create physical plan:"));
                Assert.That(e.Errors[0].Contains("Add a 'range' call to bound the query"));
            }
        }

        [Test]
        public async Task Callback()
        {
            CountdownEvent = new CountdownEvent(3);
            List<FluxRecord> records = new List<FluxRecord>();

            string flux = FromFluxDatabase + "\n"
                                           + "\t|> range(start: 1970-01-01T00:00:00.000000000Z)\n"
                                           + "\t|> filter(fn: (r) => (r[\"_measurement\"] == \"mem\" AND r[\"_field\"] == \"free\"))\n"
                                           + "\t|> sum()";

            await FluxClient.Query(flux, (cancellable, record) =>
                            {
                                records.Add(record);
                                CountdownEvent.Signal();
                            },
                            error => Assert.Fail("Unreachable"),
                            () => CountdownEvent.Signal());

            WaitToCallback();
            AssertFluxRecords(records);
        }

        [Test]
        public async Task CallbackWhenConnectionRefuse()
        {
            FluxConnectionOptions options = new FluxConnectionOptions("http://localhost:8003");

            FluxClient fluxClient = FluxClientFactory.Create(options);

            await fluxClient.Query(FromFluxDatabase + " |> last()",
                            (cancellable, record) => { },
                            error => CountdownEvent.Signal());

            WaitToCallback();
        }
        
        //TODO - CallbackToMeasurement Test

        [Test]
        public async Task Ping() 
        {
            Assert.IsTrue(await FluxClient.Ping());
        }
        
        [Test]
        public async Task Version()
        {
            string version = await FluxClient.Version();
            
            Assert.IsNotEmpty(version);
        }

        private async Task PrepareChunkRecords() 
        {
            int totalRecords = 500_000;
            CountdownEvent = new CountdownEvent(totalRecords);

            List<string> points = new List<string>();

            for (int ii = 1; ii <= totalRecords + 1; ii++)
            {
                string value = String.Format("chunked,host=A,region=west free={0}i {0}", ii);
                points.Add(value);
                
                if (ii % 100_000 == 0) 
                {
                    await InfluxDbWrite(String.Join("\n", points), DatabaseName);
                    points.Clear();
                }
            }
        }
        
        private void AssertFluxResult(List<FluxTable> tables) 
        {
            Assert.IsNotNull(tables);

            Assert.That(tables.Count == 2);

            FluxTable table1 = tables[0];
            
            // Data types
            Assert.That(table1.Columns.Count == 9);
            List<string> expected = new List<string>{"string", "long", "dateTime:RFC3339", "dateTime:RFC3339", "long", "string", "string", "string", "string"};
            CollectionAssert.AreEquivalent(table1.Columns.Select(c => c.DataType).ToList(), expected);

            // Columns
            List<string> expected2 = new List<string>{"result", "table", "_start", "_stop", "_value", "_field", "_measurement", "host", "region"};
            CollectionAssert.AreEquivalent(table1.Columns.Select(c => c.Label).ToList(), expected2);

            // Records
            Assert.That(table1.Records.Count == 1);

            List<FluxRecord> records = new List<FluxRecord>();
            records.Add(table1.Records[0]);
            records.Add(tables[1].Records[0]);
            AssertFluxRecords(records);
        }
        
        private void AssertFluxResultWithTime(List<FluxTable> tables) 
        {
            Assert.IsNotNull(tables);

            Assert.That(tables.Count == 2);

            FluxTable table1 = tables[0];
            
            // Data types
            Assert.That(table1.Columns.Count == 10);
            List<string> expected = new List<string>{"string", "long", "dateTime:RFC3339", "dateTime:RFC3339", "dateTime:RFC3339", "long", "string", "string", "string", "string"};
            CollectionAssert.AreEquivalent(table1.Columns.Select(c => c.DataType).ToList(), expected);

            // Columns
            List<string> expected2 = new List<string>{"result", "table", "_start", "_stop", "_time", "_value", "_field", "_measurement", "host", "region"};
            CollectionAssert.AreEquivalent(table1.Columns.Select(c => c.Label).ToList(), expected2);

            // Records
            Assert.That(table1.Records.Count == 2);
            Assert.That(tables[1].Records.Count == 2);
        }
        
        private void AssertFluxRecords(List<FluxRecord> records) 
        {
            Assert.NotNull(records);
            Assert.That(records.Count == 2);

            // Record 1
            FluxRecord record1 = records[0];
            Assert.AreEqual(record1.GetMeasurement(), "mem");
            Assert.AreEqual(record1.GetField(), "free");

            Assert.AreEqual(record1.GetStart(), new Instant());
            Assert.NotNull(record1.GetStop());
            Assert.IsNull(record1.GetTime());
            
            Assert.AreEqual(record1.GetValue(), 21L);

            Assert.AreEqual("A", record1.GetValueByKey("host"));
            Assert.AreEqual("west", record1.GetValueByKey("region"));
            
            // Record 2
            FluxRecord record2 = records[1];
            Assert.AreEqual(record2.GetMeasurement(), "mem");
            Assert.AreEqual(record2.GetField(), "free");

            Assert.AreEqual(record2.GetStart(), new Instant());
            Assert.NotNull(record2.GetStop());
            Assert.IsNull(record2.GetTime());

            Assert.AreEqual(record2.GetValue(), 42L);

            Assert.AreEqual("B", record2.GetValueByKey("host"));
            Assert.AreEqual("west", record2.GetValueByKey("region"));
        }
    }
}