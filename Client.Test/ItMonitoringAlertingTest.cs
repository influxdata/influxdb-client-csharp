using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Test;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Writes;
using Newtonsoft.Json.Linq;
using NUnit.Framework;
using WireMock.RequestBuilders;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItMonitoringAlertingTest : AbstractMockServerTest
    {
        private InfluxDBClient _client;

        [SetUp]
        public new void SetUp()
        {
            _client = InfluxDBClientFactory.Create(GetInfluxDb2Url(), "my-token");
        }

        [TearDown]
        protected void After()
        {
            _client.Dispose();
        }

        [Test]
        [Ignore("TODO fix CI")]
        public async Task CreateMonitoringAndAlerting()
        {
            MockServer
                .Given(Request.Create().UsingGet())
                .RespondWith(CreateResponse("{}", "application/json"));

            MockServer
                .Given(Request.Create().UsingPost())
                .RespondWith(CreateResponse("{}", "application/json"));

            var org = (await _client.GetOrganizationsApi().FindOrganizationsAsync())
                .First(organization => organization.Name.Equals("my-org"));

            var checksApi = _client.GetChecksApi();
            var notificationEndpointsApi = _client.GetNotificationEndpointsApi();
            var notificationRulesApi = _client.GetNotificationRulesApi();

            var checks = await checksApi.FindChecksAsync(org.Id, new FindOptions());
            foreach (var delete in checks._Checks.Where(ne => ne.Name.EndsWith("-IT")))
                await checksApi.DeleteCheckAsync(delete);

            var rules = await notificationRulesApi.FindNotificationRulesAsync(org.Id, new FindOptions());
            foreach (var delete in rules._NotificationRules.Where(ne => ne.Name.EndsWith("-IT")))
                await notificationRulesApi.DeleteNotificationRuleAsync(delete);

            var endpoints = await notificationEndpointsApi.FindNotificationEndpointsAsync(org.Id, new FindOptions());
            foreach (var delete in endpoints._NotificationEndpoints.Where(ne => ne.Name.EndsWith("-IT")))
                await notificationEndpointsApi.DeleteNotificationEndpointAsync(delete);

            //
            // Create Threshold Check
            //
            // Set status to 'Critical' if the 'current' value for 'stock' measurement is lesser than '35'
            //
            var query = "from(bucket: \"my-bucket\") "
                        + "|> range(start: v.timeRangeStart, stop: v.timeRangeStop)  "
                        + "|> filter(fn: (r) => r._measurement == \"stock\")  "
                        + "|> filter(fn: (r) => r.company == \"zyz\")  "
                        + "|> aggregateWindow(every: 5s, fn: mean)  "
                        + "|> filter(fn: (r) => r._field == \"current\")  "
                        + "|> yield(name: \"mean\")";

            var threshold = new LesserThreshold(value: 35F, level: CheckStatusLevel.CRIT,
                type: LesserThreshold.TypeEnum.Lesser);

            var message = "The Stock price for XYZ is on: ${ r._level } level!";

            await checksApi.CreateThresholdCheckAsync(AbstractItClientTest.GenerateName("XYZ Stock value"), query, "5s",
                message, threshold, org.Id);


            //
            // Create Slack Notification endpoint
            //
            var url = MockServerUrl;
            var endpoint =
                await notificationEndpointsApi.CreateSlackEndpointAsync(
                    AbstractItClientTest.GenerateName("Slack Endpoint"), url, org.Id);

            //
            // Create Notification Rule
            //
            // Send message if the status is 'Critical'
            //
            await notificationRulesApi.CreateSlackRuleAsync(
                AbstractItClientTest.GenerateName("Critical status to Slack"), "10s", "${ r._message }",
                RuleStatusLevel.CRIT, endpoint, org.Id);


            //
            // Write Data
            //
            var now = DateTime.UtcNow;
            var point1 = PointData
                .Measurement("stock")
                .Tag("company", "zyz")
                .Field("current", 33.65)
                .Timestamp(now, WritePrecision.Ns);

            var writeApi = _client.GetWriteApi();
            var listener = new WriteApiTest.EventListener(writeApi);
            writeApi.WritePoint(point1, "my-bucket", "my-org");
            writeApi.Flush();
            listener.WaitToSuccess();


            var start = DateTime.UtcNow;
            while (!MockServer.LogEntries.Any() && (DateTime.UtcNow - start).TotalSeconds < 30) Thread.Sleep(100);

            var requestEntry = MockServer.LogEntries.Last();
            Assert.AreEqual($"{MockServerUrl}/", requestEntry.RequestMessage.Url);

            var json = (JObject)requestEntry.RequestMessage.BodyAsJson;
            Assert.IsNotNull(json.GetValue("attachments"));

            var attachments = (JArray)json.GetValue("attachments");
            Assert.AreEqual(1, attachments.Count);

            Assert.AreEqual("The Stock price for XYZ is on: crit level!", attachments[0]["text"].ToString());
        }
    }
}