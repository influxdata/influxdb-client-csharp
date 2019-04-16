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
        private InfluxDBClient _client;
        
        [SetUp]
        public new void SetUp()
        {
            _client = InfluxDBClientFactory.Create(MockServerUrl);
        }

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

        [Test]
        public void ParseDate()
        {
            const string data = "{\"links\":{\"self\":\"/api/v2/buckets/0376298868765000/log\"},\"logs\":[" +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Created\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T07:33:44.390263749Z\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.252492+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.334601+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.437055+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.568936+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.64818+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.749176+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.82996+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.908611+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:41.9828+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.090233+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.193205+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.271324+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.338836+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.446591+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.549676+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.631707+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.714726+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.806946+01:00\"}," +
                                "{\"links\":{\"user\":\"/api/v2/users/037624e8d440e000\"},\"description\":\"Bucket Updated\",\"userID\":\"037624e8d440e000\",\"time\":\"2019-02-26T08:15:42.889206+01:00\"}]}";
            
            MockServer
                .Given(Request.Create().UsingGet())
                .RespondWith(CreateResponse(data, "application/json"));

            var logs = _client.GetBucketsApi().FindBucketLogs("id");
            
            Assert.AreEqual(20, logs.Count);
        }
    }
}