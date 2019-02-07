using InfluxDB.Client.Core.Test;
using InfluxDB.Client.Domain;
using NUnit.Framework;
using WireMock.RequestBuilders;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class InfluxDbClientTest : AbstractMockServerTest
    {
        [SetUp]
        public new void SetUp()
        {
            _client = InfluxDBClientFactory.Create(MockServerUrl);
        }

        private InfluxDBClient _client;

        [Test]
        public async Task ParseKnownEnum()
        {
            MockServer
                .Given(Request.Create().UsingGet())
                .RespondWith(CreateResponse("{\"status\":\"active\"}", "application/json"));

            var authorization = await _client.GetAuthorizationsApi().FindAuthorizationById("id");

            Assert.AreEqual(Status.Active, authorization.Status);
        }

        [Test]
        public async Task ParseUnknownEnumAsNull()
        {
            MockServer
                .Given(Request.Create().UsingGet())
                .RespondWith(CreateResponse("{\"status\":\"unknown\"}", "application/json"));

            var authorization = await _client.GetAuthorizationsApi().FindAuthorizationById("id");

            Assert.IsNull(authorization.Status);
        }
    }
}