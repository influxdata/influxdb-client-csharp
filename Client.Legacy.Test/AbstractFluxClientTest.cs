using InfluxDB.Client.Core.Test;
using InfluxDB.Client.Flux;
using NUnit.Framework;
using WireMock.ResponseBuilders;

namespace Client.Legacy.Test
{
    public class AbstractFluxClientTest : AbstractMockServerTest
    {
        protected const string SuccessData =
            "#datatype,string,long,dateTime:RFC3339,dateTime:RFC3339,dateTime:RFC3339,long,string,string,string,string\n"
            + "#group,false,false,false,false,false,false,false,false,false,true\n" + "#default,_result,,,,,,,,,\n"
            + ",result,table,_start,_stop,_time,_value,_field,_measurement,host,region\n"
            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,10,free,mem,A,west\n"
            + ",,0,1970-01-01T00:00:10Z,1970-01-01T00:00:20Z,1970-01-01T00:00:10Z,20,free,mem,B,west\n"
            + ",,0,1970-01-01T00:00:20Z,1970-01-01T00:00:30Z,1970-01-01T00:00:20Z,11,free,mem,A,west\n"
            + ",,0,1970-01-01T00:00:20Z,1970-01-01T00:00:30Z,1970-01-01T00:00:20Z,22,free,mem,B,west";

        protected FluxClient FluxClient;

        [SetUp]
        public new void SetUp()
        {
            FluxClient = FluxClientFactory.Create(MockServerUrl);
        }

        [TearDown]
        protected void After()
        {
            FluxClient.Dispose();
        }

        protected IResponseBuilder CreateResponse()
        {
            return CreateResponse(SuccessData);
        }
    }
}