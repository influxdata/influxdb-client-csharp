using WireMock.ResponseBuilders;

namespace Flux.Tests
{
    public class AbstractTest
    {
        protected IResponseBuilder CreateErrorResponse(string influxDBError) 
        {
            string body = "{\"error\":\"" + influxDBError + "\"}";

            return Response.Create().WithStatusCode(500)
                .WithHeader("X-Influx-Error", influxDBError)
                .WithBody(body);
        }
    }
}