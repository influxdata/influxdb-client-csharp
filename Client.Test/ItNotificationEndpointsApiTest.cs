using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Exceptions;
using InfluxDB.Client.Domain;
using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItNotificationEndpointsApiTest : AbstractItClientTest
    {
        private string _orgId;
        private NotificationEndpointsApi _notificationEndpointsApi;

        [SetUp]
        public new async Task SetUp()
        {
            _orgId = (await FindMyOrg()).Id;
            _notificationEndpointsApi = Client.GetNotificationEndpointsApi();

            var endpoints = await _notificationEndpointsApi.FindNotificationEndpointsAsync(_orgId, new FindOptions());
            foreach (var endpoint in endpoints._NotificationEndpoints.Where(ne => ne.Name.EndsWith("-IT")))
                await _notificationEndpointsApi.DeleteNotificationEndpointAsync(endpoint);
        }

        [Test]
        public async Task CreateSlackEndpoint()
        {
            var now = DateTime.UtcNow;

            var name = GenerateName("slack");
            var endpoint = await _notificationEndpointsApi
                .CreateSlackEndpointAsync(name, "https://hooks.slack.com/services/x/y/z", null, _orgId);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("https://hooks.slack.com/services/x/y/z", endpoint.Url);
            Assert.IsEmpty(endpoint.Token);
            Assert.IsNotEmpty(endpoint.Id);
            Assert.AreEqual(_orgId, endpoint.OrgID);
            Assert.Greater(endpoint.CreatedAt, now);
            Assert.Greater(endpoint.UpdatedAt, now);
            Assert.AreEqual(name, endpoint.Name);
            Assert.AreEqual(NotificationEndpointBase.StatusEnum.Active, endpoint.Status);
            Assert.IsEmpty(endpoint.Labels);
            Assert.AreEqual(NotificationEndpointType.Slack, endpoint.Type);
            Assert.IsNotNull(endpoint.Links);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}", endpoint.Links.Self);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}/labels", endpoint.Links.Labels);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}/members", endpoint.Links.Members);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}/owners", endpoint.Links.Owners);
        }

        [Test]
        public async Task CreateSlackEndpointSecret()
        {
            var endpoint = await _notificationEndpointsApi
                .CreateSlackEndpointAsync(GenerateName("slack"), "https://hooks.slack.com/services/x/y/z",
                    "slack-secret", _orgId);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual($"secret: {endpoint.Id}-token", endpoint.Token);
        }

        [Test]
        public async Task CreateSlackEndpointWithDescription()
        {
            var endpoint = new SlackNotificationEndpoint(type: NotificationEndpointType.Slack,
                url: "https://hooks.slack.com/services/x/y/z", description: "my production slack channel",
                orgID: _orgId, name: GenerateName("slack"), status: NotificationEndpointBase.StatusEnum.Active);

            endpoint = (SlackNotificationEndpoint)await _notificationEndpointsApi.CreateEndpointAsync(endpoint);

            Assert.AreEqual("my production slack channel", endpoint.Description);
        }

        [Test]
        public async Task CreatePagerDutyEndpoint()
        {
            var now = DateTime.UtcNow;

            var name = GenerateName("pager-duty");
            var endpoint = await _notificationEndpointsApi
                .CreatePagerDutyEndpointAsync(name, "https://events.pagerduty.com/v2/enqueue", "secret-key", _orgId);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("https://events.pagerduty.com/v2/enqueue", endpoint.ClientURL);
            Assert.AreEqual($"secret: {endpoint.Id}-routing-key", endpoint.RoutingKey);
            Assert.IsNotEmpty(endpoint.Id);
            Assert.AreEqual(_orgId, endpoint.OrgID);
            Assert.Greater(endpoint.CreatedAt, now);
            Assert.Greater(endpoint.UpdatedAt, now);
            Assert.AreEqual(name, endpoint.Name);
            Assert.AreEqual(NotificationEndpointBase.StatusEnum.Active, endpoint.Status);
            Assert.IsEmpty(endpoint.Labels);
            Assert.AreEqual(NotificationEndpointType.Pagerduty, endpoint.Type);
            Assert.IsNotNull(endpoint.Links);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}", endpoint.Links.Self);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}/labels", endpoint.Links.Labels);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}/members", endpoint.Links.Members);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}/owners", endpoint.Links.Owners);
        }

        [Test]
        public async Task SlackUrlShouldBeDefined()
        {
            await _notificationEndpointsApi
                .CreateSlackEndpointAsync(GenerateName("slack"), "https://hooks.slack.com/services/x/y/z", "token",
                    _orgId);

            var ioe = Assert.ThrowsAsync<ArgumentException>(async () => await _notificationEndpointsApi
                .CreateSlackEndpointAsync(GenerateName("slack"), null, null, _orgId));

            Assert.AreEqual("Expecting a non-empty string for url", ioe.Message);
        }

        [Test]
        public async Task CreateHttpEndpoint()
        {
            var now = DateTime.UtcNow;

            var name = GenerateName("http");
            var endpoint = await _notificationEndpointsApi
                .CreateHttpEndpointAsync(name, "http://localhost:1234/mock",
                    HTTPNotificationEndpoint.MethodEnum.POST, _orgId);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("http://localhost:1234/mock", endpoint.Url);
            Assert.AreEqual(HTTPNotificationEndpoint.AuthMethodEnum.None, endpoint.AuthMethod);
            Assert.AreEqual(HTTPNotificationEndpoint.MethodEnum.POST, endpoint.Method);
            Assert.IsNotEmpty(endpoint.Id);
            Assert.IsEmpty(endpoint.Username);
            Assert.IsEmpty(endpoint.Password);
            Assert.IsEmpty(endpoint.Token);
            Assert.IsNull(endpoint.Headers);
            Assert.AreEqual(_orgId, endpoint.OrgID);
            Assert.Greater(endpoint.CreatedAt, now);
            Assert.Greater(endpoint.UpdatedAt, now);
            Assert.AreEqual(name, endpoint.Name);
            Assert.AreEqual(NotificationEndpointBase.StatusEnum.Active, endpoint.Status);
            Assert.IsEmpty(endpoint.Labels);
            Assert.AreEqual(NotificationEndpointType.Http, endpoint.Type);
            Assert.IsNotNull(endpoint.Links);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}", endpoint.Links.Self);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}/labels", endpoint.Links.Labels);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}/members", endpoint.Links.Members);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}/owners", endpoint.Links.Owners);
        }

        [Test]
        public async Task CreateHttpEndpointBasic()
        {
            var now = DateTime.UtcNow;

            var name = GenerateName("http");
            var endpoint = await _notificationEndpointsApi
                .CreateHttpEndpointBasicAuthAsync(name, "http://localhost:1234/mock",
                    HTTPNotificationEndpoint.MethodEnum.PUT, "my-user", "my-password", _orgId);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("http://localhost:1234/mock", endpoint.Url);
            Assert.AreEqual(HTTPNotificationEndpoint.AuthMethodEnum.Basic, endpoint.AuthMethod);
            Assert.AreEqual(HTTPNotificationEndpoint.MethodEnum.PUT, endpoint.Method);
            Assert.IsNotEmpty(endpoint.Id);
            Assert.AreEqual($"secret: {endpoint.Id}-username", endpoint.Username);
            Assert.AreEqual($"secret: {endpoint.Id}-password", endpoint.Password);
            Assert.IsEmpty(endpoint.Token);
            Assert.IsNull(endpoint.Headers);
            Assert.AreEqual(_orgId, endpoint.OrgID);
            Assert.Greater(endpoint.CreatedAt, now);
            Assert.Greater(endpoint.UpdatedAt, now);
            Assert.AreEqual(name, endpoint.Name);
            Assert.AreEqual(NotificationEndpointBase.StatusEnum.Active, endpoint.Status);
            Assert.IsEmpty(endpoint.Labels);
            Assert.AreEqual(NotificationEndpointType.Http, endpoint.Type);
            Assert.IsNotNull(endpoint.Links);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}", endpoint.Links.Self);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}/labels", endpoint.Links.Labels);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}/members", endpoint.Links.Members);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}/owners", endpoint.Links.Owners);
        }

        [Test]
        public async Task CreateHttpEndpointBearer()
        {
            var now = DateTime.UtcNow;

            var name = GenerateName("http");
            var endpoint = await _notificationEndpointsApi
                .CreateHttpEndpointBearerAsync(name, "http://localhost:1234/mock",
                    HTTPNotificationEndpoint.MethodEnum.GET, "my-token", _orgId);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual("http://localhost:1234/mock", endpoint.Url);
            Assert.AreEqual(HTTPNotificationEndpoint.AuthMethodEnum.Bearer, endpoint.AuthMethod);
            Assert.AreEqual(HTTPNotificationEndpoint.MethodEnum.GET, endpoint.Method);
            Assert.IsNotEmpty(endpoint.Id);
            Assert.AreEqual($"secret: {endpoint.Id}-token", endpoint.Token);
            Assert.IsEmpty(endpoint.Username);
            Assert.IsEmpty(endpoint.Password);
            Assert.IsNull(endpoint.Headers);
            Assert.AreEqual(_orgId, endpoint.OrgID);
            Assert.Greater(endpoint.CreatedAt, now);
            Assert.Greater(endpoint.UpdatedAt, now);
            Assert.AreEqual(name, endpoint.Name);
            Assert.AreEqual(NotificationEndpointBase.StatusEnum.Active, endpoint.Status);
            Assert.IsEmpty(endpoint.Labels);
            Assert.AreEqual(NotificationEndpointType.Http, endpoint.Type);
            Assert.IsNotNull(endpoint.Links);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}", endpoint.Links.Self);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}/labels", endpoint.Links.Labels);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}/members", endpoint.Links.Members);
            Assert.AreEqual($"/api/v2/notificationEndpoints/{endpoint.Id}/owners", endpoint.Links.Owners);
        }

        [Test]
        public async Task CreateHttpEndpointHeadersTemplate()
        {
            var name = GenerateName("http");

            var headers = new Dictionary<string, string> { { "custom-header", "123" }, { "client", "InfluxDB" } };

            var endpoint = new HTTPNotificationEndpoint(type: NotificationEndpointType.Http,
                method: HTTPNotificationEndpoint.MethodEnum.POST, url: "http://localhost:1234/mock",
                orgID: _orgId,
                name: name, authMethod: HTTPNotificationEndpoint.AuthMethodEnum.None,
                status: NotificationEndpointBase.StatusEnum.Active, headers: headers);

            endpoint = (HTTPNotificationEndpoint)await _notificationEndpointsApi.CreateEndpointAsync(endpoint);

            Assert.IsNotNull(endpoint);
            Assert.AreEqual(2, endpoint.Headers.Count);
            Assert.AreEqual("123", endpoint.Headers["custom-header"]);
            Assert.AreEqual("InfluxDB", endpoint.Headers["client"]);
        }

        [Test]
        public async Task UpdateEndpoint()
        {
            var endpoint = await _notificationEndpointsApi
                .CreateHttpEndpointAsync(GenerateName("http"), "http://localhost:1234/mock",
                    HTTPNotificationEndpoint.MethodEnum.POST, _orgId);

            var name = GenerateName("updated name");
            endpoint.Name = name;
            endpoint.Description = "updated description";
            endpoint.Status = NotificationEndpointBase.StatusEnum.Inactive;

            endpoint = (HTTPNotificationEndpoint)await _notificationEndpointsApi.UpdateEndpointAsync(endpoint);

            Assert.AreEqual(name, endpoint.Name);
            Assert.AreEqual("updated description", endpoint.Description);
            Assert.AreEqual(NotificationEndpointBase.StatusEnum.Inactive, endpoint.Status);
        }

        [Test]
        public void UpdateEndpointNotExists()
        {
            var update = new NotificationEndpointUpdate("not exists name",
                "not exists description", NotificationEndpointUpdate.StatusEnum.Active);

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _notificationEndpointsApi
                .UpdateEndpointAsync("020f755c3c082000", update));

            Assert.AreEqual("notification endpoint not found for key \"020f755c3c082000\"", ioe.Message);
        }

        [Test]
        public async Task DeleteEndpoint()
        {
            var created = await _notificationEndpointsApi
                .CreatePagerDutyEndpointAsync(GenerateName("pager-duty"), "https://events.pagerduty.com/v2/enqueue",
                    "secret-key", _orgId);

            var found = (PagerDutyNotificationEndpoint)await _notificationEndpointsApi
                .FindNotificationEndpointByIdAsync(created.Id);

            await _notificationEndpointsApi.DeleteNotificationEndpointAsync(found);

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _notificationEndpointsApi
                .FindNotificationEndpointByIdAsync(found.Id));

            Assert.AreEqual($"notification endpoint not found for key \"{found.Id}\"", ioe.Message);
        }

        [Test]
        public void DeleteEndpointNotFound()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _notificationEndpointsApi
                .DeleteNotificationEndpointAsync("020f755c3c082000"));

            Assert.AreEqual("notification endpoint not found for key \"020f755c3c082000\"", ioe.Message);
        }

        [Test]
        public async Task FindNotificationEndpointById()
        {
            var endpoint = await _notificationEndpointsApi
                .CreatePagerDutyEndpointAsync(GenerateName("pager-duty"), "https://events.pagerduty.com/v2/enqueue",
                    "secret-key", _orgId);

            var found = (PagerDutyNotificationEndpoint)await _notificationEndpointsApi
                .FindNotificationEndpointByIdAsync(endpoint.Id);

            Assert.AreEqual(endpoint.Id, found.Id);
        }

        [Test]
        public void FindNotificationEndpointByIdNotFound()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _notificationEndpointsApi
                .FindNotificationEndpointByIdAsync("020f755c3c082000"));

            Assert.AreEqual("notification endpoint not found for key \"020f755c3c082000\"", ioe.Message);
        }

        [Test]
        public async Task ClonePagerDuty()
        {
            var endpoint = await _notificationEndpointsApi
                .CreatePagerDutyEndpointAsync(GenerateName("pager-duty"), "https://events.pagerduty.com/v2/enqueue",
                    "secret-key", _orgId);

            var name = GenerateName("cloned-pager-duty");
            var cloned = await _notificationEndpointsApi
                .ClonePagerDutyEndpointAsync(name, "routing-key", endpoint);

            Assert.AreNotEqual(endpoint.Id, cloned.Id);
            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual("https://events.pagerduty.com/v2/enqueue", cloned.ClientURL);
            Assert.AreEqual($"secret: {cloned.Id}-routing-key", cloned.RoutingKey);
        }

        [Test]
        public async Task CloneSlack()
        {
            var endpoint = new SlackNotificationEndpoint(name: GenerateName("slack"),
                url: "https://hooks.slack.com/services/x/y/z");
            endpoint.Type = NotificationEndpointType.Slack;
            endpoint.OrgID = _orgId;
            endpoint.Token = "my-slack-token";
            endpoint.Description = "my production slack channel";
            endpoint.Status = NotificationEndpointBase.StatusEnum.Active;

            endpoint = (SlackNotificationEndpoint)await _notificationEndpointsApi.CreateEndpointAsync(endpoint);

            var name = GenerateName("cloned-slack");
            var cloned = await _notificationEndpointsApi
                .CloneSlackEndpointAsync(name, "my-slack-token", endpoint);

            Assert.AreNotEqual(endpoint.Id, cloned.Id);
            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual("https://hooks.slack.com/services/x/y/z", cloned.Url);
            Assert.AreEqual($"secret: {cloned.Id}-token", cloned.Token);
            Assert.AreEqual("my production slack channel", cloned.Description);
            Assert.AreEqual(NotificationEndpointBase.StatusEnum.Active, cloned.Status);
            Assert.AreEqual(NotificationEndpointType.Slack, cloned.Type);
        }

        [Test]
        public async Task CloneHttpWithoutAuth()
        {
            var endpoint = await _notificationEndpointsApi
                .CreateHttpEndpointBearerAsync(GenerateName("http"), "http://localhost:1234/mock",
                    HTTPNotificationEndpoint.MethodEnum.GET, "my-token", _orgId);

            var name = GenerateName("cloned-http");
            var cloned = await _notificationEndpointsApi.CloneHttpEndpoint(name, endpoint);

            Assert.AreNotEqual(endpoint.Id, cloned.Id);
            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual(HTTPNotificationEndpoint.MethodEnum.GET, cloned.Method);
            Assert.AreEqual(HTTPNotificationEndpoint.AuthMethodEnum.None, cloned.AuthMethod);
            Assert.IsEmpty(cloned.Token);
        }

        [Test]
        public async Task CloneHttpBearerToken()
        {
            var headers = new Dictionary<string, string> { { "custom-header", "123" }, { "client", "InfluxDB" } };

            var endpoint = new HTTPNotificationEndpoint(type: NotificationEndpointType.Http,
                method: HTTPNotificationEndpoint.MethodEnum.POST, url: "http://localhost:1234/mock",
                orgID: _orgId,
                name: GenerateName("http"), authMethod: HTTPNotificationEndpoint.AuthMethodEnum.Bearer,
                token: "bearer-token",
                contentTemplate: "content - template",
                status: NotificationEndpointBase.StatusEnum.Active, headers: headers);

            endpoint = (HTTPNotificationEndpoint)await _notificationEndpointsApi.CreateEndpointAsync(endpoint);

            var name = GenerateName("cloned-http");
            var cloned = await _notificationEndpointsApi
                .CloneHttpEndpointBearerAsync(name, "bearer-token", endpoint);

            Assert.AreNotEqual(endpoint.Id, cloned.Id);
            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual(HTTPNotificationEndpoint.MethodEnum.POST, cloned.Method);
            Assert.AreEqual(HTTPNotificationEndpoint.AuthMethodEnum.Bearer, cloned.AuthMethod);
            Assert.AreEqual($"secret: {cloned.Id}-token", cloned.Token);
            Assert.AreEqual(2, endpoint.Headers.Count);
            Assert.AreEqual("123", endpoint.Headers["custom-header"]);
            Assert.AreEqual("InfluxDB", endpoint.Headers["client"]);
            Assert.AreEqual("content - template", cloned.ContentTemplate);
        }

        [Test]
        public async Task CloneHttpBearerBasicAuth()
        {
            var endpoint = await _notificationEndpointsApi
                .CreateHttpEndpointBasicAuthAsync(GenerateName("http"), "http://localhost:1234/mock",
                    HTTPNotificationEndpoint.MethodEnum.PUT, "my-user", "my-password", _orgId);

            var name = GenerateName("cloned-http");
            var cloned =
                await _notificationEndpointsApi.CloneHttpEndpointBasicAuthAsync(name,
                    "basic-username", "basic-password", endpoint);

            Assert.AreNotEqual(endpoint.Id, cloned.Id);
            Assert.AreEqual(name, cloned.Name);
            Assert.AreEqual(HTTPNotificationEndpoint.MethodEnum.PUT, cloned.Method);
            Assert.AreEqual(HTTPNotificationEndpoint.AuthMethodEnum.Basic, cloned.AuthMethod);
            Assert.AreEqual($"secret: {cloned.Id}-username", cloned.Username);
            Assert.AreEqual($"secret: {cloned.Id}-password", cloned.Password);
            Assert.IsNull(cloned.Headers);
        }

        [Test]
        public void CloneNotFound()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _notificationEndpointsApi
                .CloneSlackEndpointAsync("not-found-cloned", "token", "020f755c3c082000"));

            Assert.AreEqual("notification endpoint not found for key \"020f755c3c082000\"", ioe.Message);

            ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _notificationEndpointsApi
                .ClonePagerDutyEndpointAsync("not-found-cloned", "token", "020f755c3c082000"));

            Assert.AreEqual("notification endpoint not found for key \"020f755c3c082000\"", ioe.Message);

            ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _notificationEndpointsApi
                .CloneHttpEndpointAsync("not-found-cloned", "020f755c3c082000"));

            Assert.AreEqual("notification endpoint not found for key \"020f755c3c082000\"", ioe.Message);

            ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _notificationEndpointsApi
                .CloneHttpEndpointBearerAsync("not-found-cloned", "token", "020f755c3c082000"));

            Assert.AreEqual("notification endpoint not found for key \"020f755c3c082000\"", ioe.Message);

            ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _notificationEndpointsApi
                .CloneHttpEndpointBasicAuthAsync("not-found-cloned", "username", "password", "020f755c3c082000"));

            Assert.AreEqual("notification endpoint not found for key \"020f755c3c082000\"", ioe.Message);
        }

        [Test]
        public async Task Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var endpoint = await _notificationEndpointsApi
                .CreateHttpEndpointBasicAuthAsync(GenerateName("http"), "http://localhost:1234/mock",
                    HTTPNotificationEndpoint.MethodEnum.PUT, "my-user", "my-password", _orgId);

            var properties = new Dictionary<string, string> { { "color", "green" }, { "location", "west" } };

            var label = await labelClient.CreateLabelAsync(GenerateName("Cool Resource"), properties, _orgId);

            var labels = await _notificationEndpointsApi.GetLabelsAsync(endpoint);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = await _notificationEndpointsApi.AddLabelAsync(label, endpoint);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = await _notificationEndpointsApi.GetLabelsAsync(endpoint);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            await _notificationEndpointsApi.DeleteLabelAsync(label, endpoint);

            labels = await _notificationEndpointsApi.GetLabelsAsync(endpoint);
            Assert.AreEqual(0, labels.Count);
        }

        [Test]
        public async Task FindNotifications()
        {
            var size = (await _notificationEndpointsApi.FindNotificationEndpointsAsync(_orgId)).Count;

            await _notificationEndpointsApi
                .CreateHttpEndpointBasicAuthAsync(GenerateName("http"), "http://localhost:1234/mock",
                    HTTPNotificationEndpoint.MethodEnum.PUT, "my-user", "my-password", _orgId);


            var endpoints = await _notificationEndpointsApi.FindNotificationEndpointsAsync(_orgId);
            Assert.AreEqual(size + 1, endpoints.Count);
        }

        [Test]
        public async Task FindNotificationsPaging()
        {
            foreach (var unused in Enumerable.Range(0,
                         20 - (await _notificationEndpointsApi.FindNotificationEndpointsAsync(_orgId,
                             new FindOptions()))
                         ._NotificationEndpoints.Count))
                await _notificationEndpointsApi
                    .CreateHttpEndpointBasicAuthAsync(GenerateName("http"), "http://localhost:1234/mock",
                        HTTPNotificationEndpoint.MethodEnum.PUT, "my-user", "my-password", _orgId);

            var findOptions = new FindOptions { Limit = 5 };

            var endpoints = await _notificationEndpointsApi.FindNotificationEndpointsAsync(_orgId, findOptions);
            Assert.AreEqual(5, endpoints._NotificationEndpoints.Count);
            Assert.AreEqual($"/api/v2/notificationEndpoints?descending=false&limit=5&offset=5&orgID={_orgId}",
                endpoints.Links.Next);

            endpoints = await _notificationEndpointsApi.FindNotificationEndpointsAsync(_orgId,
                FindOptions.GetFindOptions(endpoints.Links.Next));
            Assert.AreEqual(5, endpoints._NotificationEndpoints.Count);
            Assert.AreEqual($"/api/v2/notificationEndpoints?descending=false&limit=5&offset=10&orgID={_orgId}",
                endpoints.Links.Next);

            endpoints = await _notificationEndpointsApi.FindNotificationEndpointsAsync(_orgId,
                FindOptions.GetFindOptions(endpoints.Links.Next));
            Assert.AreEqual(5, endpoints._NotificationEndpoints.Count);
            Assert.AreEqual($"/api/v2/notificationEndpoints?descending=false&limit=5&offset=15&orgID={_orgId}",
                endpoints.Links.Next);

            endpoints = await _notificationEndpointsApi.FindNotificationEndpointsAsync(_orgId,
                FindOptions.GetFindOptions(endpoints.Links.Next));
            Assert.AreEqual(5, endpoints._NotificationEndpoints.Count);
            Assert.AreEqual($"/api/v2/notificationEndpoints?descending=false&limit=5&offset=20&orgID={_orgId}",
                endpoints.Links.Next);

            endpoints = await _notificationEndpointsApi.FindNotificationEndpointsAsync(_orgId,
                FindOptions.GetFindOptions(endpoints.Links.Next));
            Assert.AreEqual(0, endpoints._NotificationEndpoints.Count);
            Assert.IsNull(endpoints.Links.Next);
        }
    }
}