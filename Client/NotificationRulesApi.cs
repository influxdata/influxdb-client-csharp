using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using InfluxDB.Client.Domain;

namespace InfluxDB.Client
{
    public class NotificationRulesApi
    {
        private readonly NotificationRulesService _service;

        protected internal NotificationRulesApi(NotificationRulesService service)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
        }

        /// <summary>
        /// Add a Slack notification rule.
        /// </summary>
        /// <param name="name">Human-readable name describing the notification rule.</param>
        /// <param name="every">The notification repetition interval.</param>
        /// <param name="messageTemplate">The template used to generate notification.</param>
        /// <param name="status">Status rule the notification rule attempts to match.</param>
        /// <param name="endpoint">The endpoint to use for notification.</param>
        /// <param name="orgId">The ID of the organization that owns this notification rule.</param>
        /// <returns>Notification rule created</returns>
        public Task<SlackNotificationRule> CreateSlackRuleAsync(string name, string every, string messageTemplate,
            RuleStatusLevel status, SlackNotificationEndpoint endpoint, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(every, nameof(every));
            Arguments.CheckNonEmptyString(messageTemplate, nameof(messageTemplate));
            Arguments.CheckNotNull(status, nameof(status));
            Arguments.CheckNotNull(endpoint, nameof(endpoint));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return CreateSlackRuleAsync(name, every, messageTemplate, status, new List<TagRule>(), endpoint,
                orgId);
        }

        /**
         * Add a Slack notification rule.
         *
         * @param name            Human-readable name describing the notification rule.
         * @param every           The notification repetition interval.
         * @param messageTemplate The template used to generate notification.
         * @param status          Status rule the notification rule attempts to match.
         * @param tagRules        List of tag rules the notification rule attempts to match.
         * @param endpoint        The endpoint to use for notification.
         * @param orgID           The ID of the organization that owns this notification rule.
         * @return Notification rule created
         */

        /// <summary>
        /// Add a Slack notification rule.
        /// </summary>
        /// <param name="name">Human-readable name describing the notification rule.</param>
        /// <param name="every">The notification repetition interval.</param>
        /// <param name="messageTemplate">The template used to generate notification.</param>
        /// <param name="status">Status rule the notification rule attempts to match.</param>
        /// <param name="tagRules">List of tag rules the notification rule attempts to match.</param>
        /// <param name="endpoint">The endpoint to use for notification.</param>
        /// <param name="orgId">The ID of the organization that owns this notification rule.</param>
        /// <returns>Notification rule created</returns>
        public async Task<SlackNotificationRule> CreateSlackRuleAsync(string name, string every, string messageTemplate,
            RuleStatusLevel status, List<TagRule> tagRules, SlackNotificationEndpoint endpoint, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(every, nameof(every));
            Arguments.CheckNonEmptyString(messageTemplate, nameof(messageTemplate));
            Arguments.CheckNotNull(status, nameof(status));
            Arguments.CheckNotNull(endpoint, nameof(endpoint));
            Arguments.CheckNotNull(tagRules, nameof(tagRules));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var rule = new SlackNotificationRule(SlackNotificationRuleBase.TypeEnum.Slack,
                messageTemplate: messageTemplate);

            return (SlackNotificationRule) await CreateRuleAsync(name, every, status, tagRules, endpoint, orgId, rule).ConfigureAwait(false);
        }

        /**
         * Add a PagerDuty notification rule.
         *
         * @param name            Human-readable name describing the notification rule.
         * @param every           The notification repetition interval.
         * @param messageTemplate The template used to generate notification.
         * @param status          Status rule the notification rule attempts to match.
         * @param tagRules        List of tag rules the notification rule attempts to match.
         * @param endpoint        The endpoint to use for notification.
         * @param orgID           The ID of the organization that owns this notification rule.
         * @return Notification rule created
         */

        /// <summary>
        /// Add a PagerDuty notification rule. 
        /// </summary>
        /// <param name="name">Human-readable name describing the notification rule.</param>
        /// <param name="every">The notification repetition interval.</param>
        /// <param name="messageTemplate">The template used to generate notification.</param>
        /// <param name="status">Status rule the notification rule attempts to match.</param>
        /// <param name="tagRules">List of tag rules the notification rule attempts to match.</param>
        /// <param name="endpoint">The endpoint to use for notification.</param>
        /// <param name="orgId">The ID of the organization that owns this notification rule</param>
        /// <returns>Notification rule created</returns>
        public async Task<PagerDutyNotificationRule> CreatePagerDutyRuleAsync(string name, string every,
            string messageTemplate, RuleStatusLevel status, List<TagRule> tagRules,
            PagerDutyNotificationEndpoint endpoint, string orgId)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(every, nameof(every));
            Arguments.CheckNonEmptyString(messageTemplate, nameof(messageTemplate));
            Arguments.CheckNotNull(status, nameof(status));
            Arguments.CheckNotNull(endpoint, nameof(endpoint));
            Arguments.CheckNotNull(tagRules, nameof(tagRules));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var rule = new PagerDutyNotificationRule(PagerDutyNotificationRuleBase.TypeEnum.Pagerduty,
                messageTemplate);

            return (PagerDutyNotificationRule) await CreateRuleAsync(name, every, status, tagRules, endpoint, orgId,
                rule).ConfigureAwait(false);
        }

        /// <summary>
        /// Add a HTTP notification rule.
        /// </summary>
        /// <param name="name">Human-readable name describing the notification rule.</param>
        /// <param name="every">The notification repetition interval.</param>
        /// <param name="status">Status rule the notification rule attempts to match.</param>
        /// <param name="tagRules">List of tag rules the notification rule attempts to match.</param>
        /// <param name="endpoint">The endpoint to use for notification.</param>
        /// <param name="orgId">The ID of the organization that owns this notification rule.</param>
        /// <returns>Notification rule created</returns>
        public async Task<HTTPNotificationRule> CreateHttpRuleAsync(string name, string every, RuleStatusLevel status,
            List<TagRule> tagRules,
            HTTPNotificationEndpoint endpoint, string orgId)
        {
            var rule = new HTTPNotificationRule(HTTPNotificationRuleBase.TypeEnum.Http);

            return (HTTPNotificationRule) await CreateRuleAsync(name, every, status, tagRules, endpoint, orgId,
                rule).ConfigureAwait(false);
        }

        /// <summary>
        /// Add a notification rule.
        /// </summary>
        /// <param name="rule">Notification rule to create</param>
        /// <returns>Notification rule created</returns>
        public Task<NotificationRule> CreateRuleAsync(NotificationRule rule)
        {
            Arguments.CheckNotNull(rule, nameof(rule));

            return _service.CreateNotificationRuleAsync(rule);
        }

        /// <summary>
        /// Update a notification rule.
        /// </summary>
        /// <param name="rule">Notification rule update to apply</param>
        /// <returns>An updated notification rule</returns>
        public Task<NotificationRule> UpdateNotificationRuleAsync(NotificationRule rule)
        {
            Arguments.CheckNotNull(rule, nameof(rule));

            Enum.TryParse(rule.Status.ToString(), true,
                out NotificationRuleUpdate.StatusEnum status);

            return UpdateNotificationRuleAsync(rule.Id,
                new NotificationRuleUpdate(rule.Name, rule.Description, status));
        }

        /// <summary>
        /// Update a notification rule.
        /// </summary>
        /// <param name="ruleId">The notification rule ID.</param>
        /// <param name="update">Notification rule update to apply</param>
        /// <returns>An updated notification rule</returns>
        public Task<NotificationRule> UpdateNotificationRuleAsync(string ruleId, NotificationRuleUpdate update)
        {
            Arguments.CheckNonEmptyString(ruleId, nameof(ruleId));
            Arguments.CheckNotNull(update, nameof(update));

            return _service.PatchNotificationRulesIDAsync(ruleId, update);
        }

        /// <summary>
        /// Delete a notification rule.
        /// </summary>
        /// <param name="rule">The notification rule</param>
        /// <returns></returns>
        public Task DeleteNotificationRuleAsync(NotificationRule rule)
        {
            Arguments.CheckNotNull(rule, nameof(rule));

            return DeleteNotificationRuleAsync(rule.Id);
        }

        /// <summary>
        /// Delete a notification rule.
        /// </summary>
        /// <param name="ruleId">The notification rule ID</param>
        /// <returns></returns>
        public Task DeleteNotificationRuleAsync(string ruleId)
        {
            Arguments.CheckNonEmptyString(ruleId, nameof(ruleId));

            return _service.DeleteNotificationRulesIDAsync(ruleId);
        }

        /// <summary>
        /// Get a notification rule.
        /// </summary>
        /// <param name="ruleId">The notification rule ID</param>
        /// <returns>The notification rule requested</returns>
        public Task<NotificationRule> FindNotificationRuleByIdAsync(string ruleId)
        {
            Arguments.CheckNonEmptyString(ruleId, nameof(ruleId));

            return _service.GetNotificationRulesIDAsync(ruleId);
        }

        /// <summary>
        /// Get notification rules.
        /// </summary>
        /// <param name="orgId">Only show notification rules that belong to a specific organization ID.</param>
        /// <returns>A list of notification rules</returns>
        public async Task<List<NotificationRule>> FindNotificationRulesAsync(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var response = await FindNotificationRulesAsync(orgId, new FindOptions()).ConfigureAwait(false);
            return response._NotificationRules;
        }

        /// <summary>
        /// Get all notification rules.
        /// </summary>
        /// <param name="orgId">Only show notification rules that belong to a specific organization ID.</param>
        /// <param name="findOptions">find options</param>
        /// <returns></returns>
        public Task<NotificationRules> FindNotificationRulesAsync(string orgId, FindOptions findOptions)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return _service.GetNotificationRulesAsync(orgId, offset: findOptions.Offset,
                limit: findOptions.Limit);
        }

        /// <summary>
        /// List all labels for a notification rule.
        /// </summary>
        /// <param name="rule">The notification rule.</param>
        /// <returns>A list of all labels for a notification rule</returns>
        public Task<List<Label>> GetLabelsAsync(NotificationRule rule)
        {
            Arguments.CheckNotNull(rule, nameof(rule));

            return GetLabelsAsync(rule.Id);
        }

        /**
         * List all labels for a notification rule.
         *
         * @param ruleID The notification rule ID.
         * @return A list of all labels for a notification rule
         */

        /// <summary>
        /// List all labels for a notification rule
        /// </summary>
        /// <param name="ruleId"> The notification rule ID.</param>
        /// <returns>A list of all labels for a notification rule</returns>
        public async Task<List<Label>> GetLabelsAsync(string ruleId)
        {
            Arguments.CheckNonEmptyString(ruleId, nameof(ruleId));

            var response = await _service.GetNotificationRulesIDLabelsAsync(ruleId).ConfigureAwait(false);
            return response.Labels;
        }

        /// <summary>
        /// Add a label to a notification rule.
        /// </summary>
        /// <param name="label">Label to add</param>
        /// <param name="rule">The notification rule.</param>
        /// <returns>The label was added to the notification rule</returns>
        public Task<Label> AddLabelAsync(Label label, NotificationRule rule)
        {
            Arguments.CheckNotNull(rule, nameof(rule));
            Arguments.CheckNotNull(label, nameof(label));

            return AddLabelAsync(label.Id, rule.Id);
        }

        /// <summary>
        /// Add a label to a notification rule.
        /// </summary>
        /// <param name="labelId">Label to add</param>
        /// <param name="ruleId">The notification rule ID.</param>
        /// <returns>The label was added to the notification rule</returns>
        public async Task<Label> AddLabelAsync(string labelId, string ruleId)
        {
            Arguments.CheckNonEmptyString(ruleId, nameof(ruleId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var mapping = new LabelMapping(labelId);

            var response = await _service.PostNotificationRuleIDLabelsAsync(ruleId, mapping).ConfigureAwait(false);
            return response.Label;
        }

        /// <summary>
        /// Delete label from a notification rule.
        /// </summary>
        /// <param name="label">The label to delete.</param>
        /// <param name="rule">The notification rule.</param>
        public Task DeleteLabelAsync(Label label, NotificationRule rule)
        {
            Arguments.CheckNotNull(rule, nameof(rule));
            Arguments.CheckNotNull(label, nameof(label));

            return DeleteLabelAsync(label.Id, rule.Id);
        }

        /// <summary>
        /// Delete label from a notification rule.
        /// </summary>
        /// <param name="labelId">The ID of the label to delete.</param>
        /// <param name="ruleId">The notification rule ID.</param>
        public Task DeleteLabelAsync(string labelId, string ruleId)
        {
            Arguments.CheckNonEmptyString(ruleId, nameof(ruleId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return _service.DeleteNotificationRulesIDLabelsIDAsync(ruleId, labelId);
        }

        private Task<NotificationRule> CreateRuleAsync(string name, string every, RuleStatusLevel status,
            List<TagRule> tagRules, NotificationEndpoint notificationEndpoint, string orgId, NotificationRule rule)
        {
            Arguments.CheckNotNull(rule, "rule");

            rule.Name = name;
            rule.Every = every;
            rule.OrgID = orgId;
            rule.TagRules = tagRules;
            rule.StatusRules = new List<StatusRule> {new StatusRule(status)};
            rule.EndpointID = notificationEndpoint.Id;
            rule.Status = TaskStatusType.Active;

            return CreateRuleAsync(rule);
        }
    }
}