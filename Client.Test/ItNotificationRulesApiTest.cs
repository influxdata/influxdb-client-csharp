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
    public class ItNotificationRulesApiTest : AbstractItClientTest
    {
        private string _orgId;
        private NotificationRulesApi _notificationRulesApi;
        private NotificationEndpointsApi _notificationEndpointsApi;

        [SetUp]
        public new async Task SetUp()
        {
            _orgId = (await FindMyOrg()).Id;
            _notificationRulesApi = Client.GetNotificationRulesApi();
            _notificationEndpointsApi = Client.GetNotificationEndpointsApi();

            var rules = await _notificationRulesApi.FindNotificationRulesAsync(_orgId, new FindOptions());
            foreach (var rule in rules._NotificationRules.Where(ne => ne.Name.EndsWith("-IT")))
                await _notificationRulesApi.DeleteNotificationRuleAsync(rule);

            var endpoints = await _notificationEndpointsApi.FindNotificationEndpointsAsync(_orgId, new FindOptions());
            foreach (var endpoint in endpoints._NotificationEndpoints.Where(ne => ne.Name.EndsWith("-IT")))
                await _notificationEndpointsApi.DeleteNotificationEndpointAsync(endpoint);
        }

        [Test]
        public async Task CreateSlackRule()
        {
            var now = DateTime.UtcNow;

            var endpoint = await _notificationEndpointsApi
                .CreateSlackEndpointAsync(GenerateName("slack"), "https://hooks.slack.com/services/x/y/z", null,
                    _orgId);

            var tagRules = new List<TagRule>
                { new TagRule("tag_key", "tag_value", TagRule.OperatorEnum.Equal) };

            var name = GenerateName("slack-rule");
            var rule = await _notificationRulesApi.CreateSlackRuleAsync(
                name,
                "10s",
                "my-template", RuleStatusLevel.CRIT, tagRules, endpoint, _orgId);

            Assert.IsNotNull(rule);
            Assert.IsNotEmpty(rule.Id);
            Assert.AreEqual(SlackNotificationRuleBase.TypeEnum.Slack, rule.Type);
            Assert.IsEmpty(rule.Channel);
            Assert.AreEqual("my-template", rule.MessageTemplate);
            Assert.AreEqual(rule.EndpointID, endpoint.Id);
            Assert.AreEqual(_orgId, rule.OrgID);
            Assert.Greater(rule.CreatedAt, now);
            Assert.Greater(rule.UpdatedAt, now);
            Assert.AreEqual(TaskStatusType.Active, rule.Status);
            Assert.AreEqual(name, rule.Name);
            Assert.IsNull(rule.SleepUntil);
            Assert.AreEqual("10s", rule.Every);
            Assert.IsNull(rule.Offset);
            Assert.IsEmpty(rule.RunbookLink);
            Assert.IsNull(rule.LimitEvery);
            Assert.IsNull(rule.Limit);
            Assert.AreEqual(1, rule.TagRules.Count);
            Assert.AreEqual("tag_key", rule.TagRules[0].Key);
            Assert.AreEqual("tag_value", rule.TagRules[0].Value);
            Assert.AreEqual(TagRule.OperatorEnum.Equal, rule.TagRules[0].Operator);
            Assert.IsNull(rule.Description);
            Assert.AreEqual(1, rule.StatusRules.Count);
            Assert.IsNull(rule.StatusRules[0].Count);
            Assert.IsNull(rule.StatusRules[0].Period);
            Assert.IsNull(rule.StatusRules[0].PreviousLevel);
            Assert.AreEqual(RuleStatusLevel.CRIT, rule.StatusRules[0].CurrentLevel);
            Assert.IsEmpty(rule.Labels);
            Assert.AreEqual($"/api/v2/notificationRules/{rule.Id}", rule.Links.Self);
            Assert.AreEqual($"/api/v2/notificationRules/{rule.Id}/labels", rule.Links.Labels);
            Assert.AreEqual($"/api/v2/notificationRules/{rule.Id}/members", rule.Links.Members);
            Assert.AreEqual($"/api/v2/notificationRules/{rule.Id}/owners", rule.Links.Owners);
        }

        [Test]
        public async Task CreatePagerDutyRule()
        {
            var now = DateTime.UtcNow;

            var endpoint = await _notificationEndpointsApi
                .CreatePagerDutyEndpointAsync(GenerateName("pager-duty"), "https://events.pagerduty.com/v2/enqueue",
                    "secret-key", _orgId);

            var tagRules = new List<TagRule>
                { new TagRule("tag_key", "tag_value", TagRule.OperatorEnum.Notequal) };

            var name = GenerateName("pagerduty-rule");
            var rule = await _notificationRulesApi.CreatePagerDutyRuleAsync(
                name,
                "10s",
                "my-template", RuleStatusLevel.CRIT, tagRules, endpoint, _orgId);

            Assert.IsNotNull(rule);
            Assert.IsNotEmpty(rule.Id);
            Assert.AreEqual(PagerDutyNotificationRuleBase.TypeEnum.Pagerduty, rule.Type);
            Assert.AreEqual("my-template", rule.MessageTemplate);
            Assert.AreEqual(rule.EndpointID, endpoint.Id);
            Assert.AreEqual(_orgId, rule.OrgID);
            Assert.Greater(rule.CreatedAt, now);
            Assert.Greater(rule.UpdatedAt, now);
            Assert.AreEqual(TaskStatusType.Active, rule.Status);
            Assert.AreEqual(name, rule.Name);
            Assert.IsNull(rule.SleepUntil);
            Assert.AreEqual("10s", rule.Every);
            Assert.IsNull(rule.Offset);
            Assert.IsEmpty(rule.RunbookLink);
            Assert.IsNull(rule.LimitEvery);
            Assert.IsNull(rule.Limit);
            Assert.AreEqual(1, rule.TagRules.Count);
            Assert.AreEqual("tag_key", rule.TagRules[0].Key);
            Assert.AreEqual("tag_value", rule.TagRules[0].Value);
            Assert.AreEqual(TagRule.OperatorEnum.Notequal, rule.TagRules[0].Operator);
            Assert.IsNull(rule.Description);
            Assert.AreEqual(1, rule.StatusRules.Count);
            Assert.IsNull(rule.StatusRules[0].Count);
            Assert.IsNull(rule.StatusRules[0].Period);
            Assert.IsNull(rule.StatusRules[0].PreviousLevel);
            Assert.AreEqual(RuleStatusLevel.CRIT, rule.StatusRules[0].CurrentLevel);
            Assert.IsEmpty(rule.Labels);
            Assert.AreEqual($"/api/v2/notificationRules/{rule.Id}", rule.Links.Self);
            Assert.AreEqual($"/api/v2/notificationRules/{rule.Id}/labels", rule.Links.Labels);
            Assert.AreEqual($"/api/v2/notificationRules/{rule.Id}/members", rule.Links.Members);
            Assert.AreEqual($"/api/v2/notificationRules/{rule.Id}/owners", rule.Links.Owners);
        }

        [Test]
        public async Task CreateHttpRule()
        {
            var now = DateTime.UtcNow;

            var endpoint = await _notificationEndpointsApi
                .CreateHttpEndpointAsync(GenerateName("http"), "http://localhost:1234/mock",
                    HTTPNotificationEndpoint.MethodEnum.POST, _orgId);

            var tagRules = new List<TagRule>
                { new TagRule("tag_key", "tag_value", TagRule.OperatorEnum.Equal) };

            var name = GenerateName("http-rule");
            var rule = await _notificationRulesApi.CreateHttpRuleAsync(name,
                "10s",
                RuleStatusLevel.CRIT, tagRules, endpoint, _orgId);

            Assert.IsNotNull(rule);
            Assert.IsNotEmpty(rule.Id);
            Assert.AreEqual(HTTPNotificationRuleBase.TypeEnum.Http, rule.Type);
            Assert.AreEqual(rule.EndpointID, endpoint.Id);
            Assert.AreEqual(_orgId, rule.OrgID);
            Assert.Greater(rule.CreatedAt, now);
            Assert.Greater(rule.UpdatedAt, now);
            Assert.AreEqual(TaskStatusType.Active, rule.Status);
            Assert.AreEqual(name, rule.Name);
            Assert.IsNull(rule.SleepUntil);
            Assert.AreEqual("10s", rule.Every);
            Assert.IsNull(rule.Offset);
            Assert.IsEmpty(rule.RunbookLink);
            Assert.IsNull(rule.LimitEvery);
            Assert.IsNull(rule.Limit);
            Assert.AreEqual(1, rule.TagRules.Count);
            Assert.AreEqual("tag_key", rule.TagRules[0].Key);
            Assert.AreEqual("tag_value", rule.TagRules[0].Value);
            Assert.AreEqual(TagRule.OperatorEnum.Equal, rule.TagRules[0].Operator);
            Assert.IsNull(rule.Description);
            Assert.AreEqual(1, rule.StatusRules.Count);
            Assert.IsNull(rule.StatusRules[0].Count);
            Assert.IsNull(rule.StatusRules[0].Period);
            Assert.IsNull(rule.StatusRules[0].PreviousLevel);
            Assert.AreEqual(RuleStatusLevel.CRIT, rule.StatusRules[0].CurrentLevel);
            Assert.IsEmpty(rule.Labels);
            Assert.AreEqual($"/api/v2/notificationRules/{rule.Id}", rule.Links.Self);
            Assert.AreEqual($"/api/v2/notificationRules/{rule.Id}/labels", rule.Links.Labels);
            Assert.AreEqual($"/api/v2/notificationRules/{rule.Id}/members", rule.Links.Members);
            Assert.AreEqual($"/api/v2/notificationRules/{rule.Id}/owners", rule.Links.Owners);
        }

        [Test]
        public async Task UpdateRule()
        {
            var endpoint = await _notificationEndpointsApi
                .CreateHttpEndpointAsync(GenerateName("http"), "http://localhost:1234/mock",
                    HTTPNotificationEndpoint.MethodEnum.POST, _orgId);

            var tagRules = new List<TagRule>
                { new TagRule("tag_key", "tag_value", TagRule.OperatorEnum.Equal) };


            var rule = await _notificationRulesApi.CreateHttpRuleAsync(GenerateName("http-rule"),
                "10s",
                RuleStatusLevel.CRIT, tagRules, endpoint, _orgId);


            var updatedName = GenerateName("updated name");
            rule.Name = updatedName;
            rule.Description = "updated description";
            rule.Status = TaskStatusType.Inactive;

            rule = (HTTPNotificationRule)await _notificationRulesApi.UpdateNotificationRuleAsync(rule);

            Assert.AreEqual(updatedName, rule.Name);
            Assert.AreEqual("updated description", rule.Description);
            Assert.AreEqual(TaskStatusType.Inactive, rule.Status);
        }

        [Test]
        public void UpdateRuleNotExists()
        {
            var update = new NotificationRuleUpdate("not exists name", "not exists update",
                NotificationRuleUpdate.StatusEnum.Active);

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _notificationRulesApi
                .UpdateNotificationRuleAsync("020f755c3c082000", update));

            Assert.AreEqual("notification rule not found", ioe.Message);
        }

        [Test]
        public async Task DeleteRule()
        {
            var endpoint = await _notificationEndpointsApi
                .CreateSlackEndpointAsync(GenerateName("slack"), "https://hooks.slack.com/services/x/y/z", null,
                    _orgId);

            var tagRules = new List<TagRule>
                { new TagRule("tag_key", "tag_value", TagRule.OperatorEnum.Equal) };

            var name = GenerateName("slack-rule");
            var created = await _notificationRulesApi.CreateSlackRuleAsync(
                name,
                "10s",
                "my-template", RuleStatusLevel.CRIT, tagRules, endpoint, _orgId);

            await _notificationRulesApi.DeleteNotificationRuleAsync(created);

            var ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _notificationRulesApi
                .FindNotificationRuleByIdAsync(created.Id));

            Assert.AreEqual("notification rule not found", ioe.Message);
        }

        [Test]
        public void DeleteRuleNotFound()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _notificationRulesApi
                .DeleteNotificationRuleAsync("020f755c3c082000"));

            Assert.AreEqual("notification rule not found", ioe.Message);
        }

        [Test]
        public async Task FindRuleById()
        {
            var endpoint = await _notificationEndpointsApi
                .CreatePagerDutyEndpointAsync(GenerateName("pager-duty"), "https://events.pagerduty.com/v2/enqueue",
                    "secret-key", _orgId);

            var tagRules = new List<TagRule>
                { new TagRule("tag_key", "tag_value", TagRule.OperatorEnum.Notequal) };

            var name = GenerateName("pagerduty-rule");
            var rule = await _notificationRulesApi.CreatePagerDutyRuleAsync(
                name,
                "10s",
                "my-template", RuleStatusLevel.CRIT, tagRules, endpoint, _orgId);

            var found =
                (PagerDutyNotificationRule)await _notificationRulesApi.FindNotificationRuleByIdAsync(rule.Id);

            Assert.AreEqual(rule.Id, found.Id);
        }

        [Test]
        public void FindRuleByIdNotFound()
        {
            var ioe = Assert.ThrowsAsync<NotFoundException>(async () => await _notificationRulesApi
                .FindNotificationRuleByIdAsync("020f755c3c082000"));

            Assert.AreEqual("notification rule not found", ioe.Message);
        }

        [Test]
        public async Task FindRules()
        {
            var size = (await _notificationRulesApi.FindNotificationRulesAsync(_orgId)).Count;

            var endpoint = await _notificationEndpointsApi
                .CreatePagerDutyEndpointAsync(GenerateName("pager-duty"), "https://events.pagerduty.com/v2/enqueue",
                    "secret-key", _orgId);

            var tagRules = new List<TagRule>
                { new TagRule("tag_key", "tag_value", TagRule.OperatorEnum.Notequal) };

            var name = GenerateName("pagerduty-rule");
            await _notificationRulesApi.CreatePagerDutyRuleAsync(
                name,
                "10s",
                "my-template", RuleStatusLevel.CRIT, tagRules, endpoint, _orgId);

            var rules = await _notificationRulesApi.FindNotificationRulesAsync(_orgId);
            Assert.AreEqual(size + 1, rules.Count);
        }

        [Test]
        public async Task FindRulesPaging()
        {
            var endpoint = await _notificationEndpointsApi
                .CreateHttpEndpointAsync(GenerateName("http"), "http://localhost:1234/mock",
                    HTTPNotificationEndpoint.MethodEnum.POST, _orgId);

            var tagRules = new List<TagRule>
                { new TagRule("tag_key", "tag_value", TagRule.OperatorEnum.Notequal) };

            foreach (var unused in Enumerable.Range(0,
                         20 - (await _notificationRulesApi.FindNotificationRulesAsync(_orgId, new FindOptions()))
                         ._NotificationRules.Count))
                await _notificationRulesApi.CreateHttpRuleAsync(GenerateName("rule"),
                    "10s",
                    RuleStatusLevel.CRIT, tagRules, endpoint, _orgId);

            var findOptions = new FindOptions { Limit = 5 };

            var rules = await _notificationRulesApi.FindNotificationRulesAsync(_orgId, findOptions);
            Assert.AreEqual(5, rules._NotificationRules.Count);
            Assert.AreEqual($"/api/v2/notificationRules?descending=false&limit=5&offset=5&orgID={_orgId}",
                rules.Links.Next);

            rules = await _notificationRulesApi.FindNotificationRulesAsync(_orgId,
                FindOptions.GetFindOptions(rules.Links.Next));
            Assert.AreEqual(5, rules._NotificationRules.Count);
            Assert.AreEqual($"/api/v2/notificationRules?descending=false&limit=5&offset=10&orgID={_orgId}",
                rules.Links.Next);

            rules = await _notificationRulesApi.FindNotificationRulesAsync(_orgId,
                FindOptions.GetFindOptions(rules.Links.Next));
            Assert.AreEqual(5, rules._NotificationRules.Count);
            Assert.AreEqual($"/api/v2/notificationRules?descending=false&limit=5&offset=15&orgID={_orgId}",
                rules.Links.Next);

            rules = await _notificationRulesApi.FindNotificationRulesAsync(_orgId,
                FindOptions.GetFindOptions(rules.Links.Next));
            Assert.AreEqual(5, rules._NotificationRules.Count);
            Assert.AreEqual($"/api/v2/notificationRules?descending=false&limit=5&offset=20&orgID={_orgId}",
                rules.Links.Next);

            rules = await _notificationRulesApi.FindNotificationRulesAsync(_orgId,
                FindOptions.GetFindOptions(rules.Links.Next));
            Assert.AreEqual(0, rules._NotificationRules.Count);
            Assert.IsNull(rules.Links.Next);
        }

        [Test]
        public async Task Labels()
        {
            var labelClient = Client.GetLabelsApi();

            var endpoint = await _notificationEndpointsApi
                .CreatePagerDutyEndpointAsync(GenerateName("pager-duty"), "https://events.pagerduty.com/v2/enqueue",
                    "secret-key", _orgId);

            var tagRules = new List<TagRule>
                { new TagRule("tag_key", "tag_value", TagRule.OperatorEnum.Notequal) };

            var name = GenerateName("pagerduty-rule");
            var rule = await _notificationRulesApi.CreatePagerDutyRuleAsync(
                name,
                "10s",
                "my-template", RuleStatusLevel.CRIT, tagRules, endpoint, _orgId);

            var properties = new Dictionary<string, string> { { "color", "green" }, { "location", "west" } };

            var label = await labelClient.CreateLabelAsync(GenerateName("Cool Resource"), properties, _orgId);

            var labels = await _notificationRulesApi.GetLabelsAsync(rule);
            Assert.AreEqual(0, labels.Count);

            var addedLabel = await _notificationRulesApi.AddLabelAsync(label, rule);
            Assert.IsNotNull(addedLabel);
            Assert.AreEqual(label.Id, addedLabel.Id);
            Assert.AreEqual(label.Name, addedLabel.Name);
            Assert.AreEqual(label.Properties, addedLabel.Properties);

            labels = await _notificationRulesApi.GetLabelsAsync(rule);
            Assert.AreEqual(1, labels.Count);
            Assert.AreEqual(label.Id, labels[0].Id);
            Assert.AreEqual(label.Name, labels[0].Name);

            await _notificationRulesApi.DeleteLabelAsync(label, rule);

            labels = await _notificationRulesApi.GetLabelsAsync(rule);
            Assert.AreEqual(0, labels.Count);
        }
    }
}