using WireMock.ResponseBuilders;

namespace Flux.Tests
{
    public class AbstractMockServerTest
    {
        protected IResponseBuilder CreateErrorResponse(string influxDbError) 
        {
            string body = "{\"error\":\"" + influxDbError + "\"}";

            return Response.Create().WithStatusCode(500)
                .WithHeader("X-Influx-Error", influxDbError)
                .WithBody(body);
        }
    }
}