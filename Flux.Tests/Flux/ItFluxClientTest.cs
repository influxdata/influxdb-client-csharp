using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Flux.Tests.Flux
{
    public class ItFluxClientTest : AbstractItFluxClientTest
    {
        private static readonly string FromFluxDatabase = String.Format("from(bucket:\"{0}\")", DatabaseName);

        override public void PrepareDara()
        {
            InfluxDbWrite("mem,host=A,region=west free=10i 10000000000", DatabaseName);
            InfluxDbWrite("mem,host=A,region=west free=11i 20000000000", DatabaseName);
            InfluxDbWrite("mem,host=B,region=west free=20i 10000000000", DatabaseName);
            InfluxDbWrite("mem,host=B,region=west free=22i 20000000000", DatabaseName);
            InfluxDbWrite("cpu,host=A,region=west usage_system=35i,user_usage=45i 10000000000", DatabaseName);
            InfluxDbWrite("cpu,host=A,region=west usage_system=38i,user_usage=49i 20000000000", DatabaseName);
            InfluxDbWrite("cpu,host=A,hyper-threading=true,region=west usage_system=38i,user_usage=49i 20000000000", DatabaseName);
        }

        [Test]
        public void ChunkedOneTable() 
        {            
            PrepareChunkRecords();

            string flux = FromFluxDatabase + "\n"
                                             + "\t|> filter(fn: (r) => r[\"_measurement\"] == \"chunked\")\n"
                                             + "\t|> range(start: 1970-01-01T00:00:00.000000000Z)";

            FluxClient.Query(flux, (cancellable, fluxRecord) => 
            {
                // +1 record
                CountdownEvent.Signal();

                if (CountdownEvent.CurrentCount % 100_000 == 0) 
                {
                    Console.WriteLine("Remaining parsed: " + CountdownEvent.CurrentCount + " records");
                }
            }).GetAwaiter().GetResult();

            WaitToCallback(30);
        }
        
        private void PrepareChunkRecords() 
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
                    InfluxDbWrite(String.Join("\n", points), DatabaseName);
                    points.Clear();
                }
            }
        }
    }
}