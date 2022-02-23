using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using InfluxDB.Client.Domain;

namespace InfluxDB.Client
{
    public class NotificationEndpointsApi
    {
        private readonly NotificationEndpointsService _service;

        protected internal NotificationEndpointsApi(NotificationEndpointsService service)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
        }

        /// <summary>
        /// Add new Slack notification endpoint. The 'url' should be defined.
        /// </summary>
        /// <param name="name">Endpoint name</param>
        /// <param name="url">Slack WebHook URL</param>
        /// <param name="orgId">Owner of an endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created Slack notification endpoint</returns>
        public Task<SlackNotificationEndpoint> CreateSlackEndpointAsync(string name, string url, string orgId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(url, nameof(url));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return CreateSlackEndpointAsync(name, url, null, orgId, cancellationToken);
        }

        /// <summary>
        ///  Add new Slack notification endpoint. The 'url' should be defined.
        /// </summary>
        /// <param name="name">Endpoint name</param>
        /// <param name="url">Slack WebHook URL</param>
        /// <param name="token">Slack WebHook Token</param>
        /// <param name="orgId">Owner of an endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created Slack notification endpoint</returns>
        public async Task<SlackNotificationEndpoint> CreateSlackEndpointAsync(string name, string url, string token,
            string orgId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(url, nameof(url));

            var endpoint = new SlackNotificationEndpoint(type: NotificationEndpointType.Slack,
                url: url, token: token, orgID: orgId, name: name, status: NotificationEndpointBase.StatusEnum.Active);

            return (SlackNotificationEndpoint)await CreateEndpointAsync(endpoint, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Add new PagerDuty notification endpoint.
        /// </summary>
        /// <param name="name">Endpoint name</param>
        /// <param name="clientUrl">Client URL</param>
        /// <param name="routingKey">Routing Key</param>
        /// <param name="orgId">Owner of an endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created PagerDuty notification endpoint</returns>
        public async Task<PagerDutyNotificationEndpoint> CreatePagerDutyEndpointAsync(string name, string clientUrl,
            string routingKey, string orgId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(clientUrl, nameof(clientUrl));
            Arguments.CheckNonEmptyString(routingKey, nameof(clientUrl));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var endpoint = new PagerDutyNotificationEndpoint(type: NotificationEndpointType.Pagerduty,
                clientURL: clientUrl, routingKey: routingKey, orgID: orgId, name: name,
                status: NotificationEndpointBase.StatusEnum.Active);

            return (PagerDutyNotificationEndpoint)await CreateEndpointAsync(endpoint, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Add new HTTP notification endpoint without authentication.
        /// </summary>
        /// <param name="name">Endpoint name</param>
        /// <param name="url">URL</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="orgId">Owner of an endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created HTTP notification endpoint</returns>
        public async Task<HTTPNotificationEndpoint> CreateHttpEndpointAsync(string name, string url,
            HTTPNotificationEndpoint.MethodEnum method, string orgId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(url, nameof(url));
            Arguments.CheckNotNull(method, nameof(method));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var endpoint = new HTTPNotificationEndpoint(type: NotificationEndpointType.Http, method: method, url: url,
                orgID: orgId,
                name: name, authMethod: HTTPNotificationEndpoint.AuthMethodEnum.None,
                status: NotificationEndpointBase.StatusEnum.Active);

            return (HTTPNotificationEndpoint)await CreateEndpointAsync(endpoint, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Endpoint name</param>
        /// <param name="url">URL</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="username">HTTP Basic Username</param>
        /// <param name="password">HTTP Basic Password</param>
        /// <param name="orgId">Owner of an endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created HTTP notification endpoint</returns>
        public async Task<HTTPNotificationEndpoint> CreateHttpEndpointBasicAuthAsync(string name, string url,
            HTTPNotificationEndpoint.MethodEnum method, string username, string password, string orgId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(url, nameof(url));
            Arguments.CheckNotNull(method, nameof(method));
            Arguments.CheckNonEmptyString(username, nameof(username));
            Arguments.CheckNonEmptyString(password, nameof(password));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var endpoint = new HTTPNotificationEndpoint(type: NotificationEndpointType.Http, method: method, url: url,
                orgID: orgId,
                name: name, authMethod: HTTPNotificationEndpoint.AuthMethodEnum.Basic, username: username,
                password: password,
                status: NotificationEndpointBase.StatusEnum.Active);

            return (HTTPNotificationEndpoint)await CreateEndpointAsync(endpoint, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">Endpoint name</param>
        /// <param name="url">URL</param>
        /// <param name="method">HTTP Method</param>
        /// <param name="token">Bearer token</param>
        /// <param name="orgId">Owner of an endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>created HTTP notification endpoint</returns>
        public async Task<HTTPNotificationEndpoint> CreateHttpEndpointBearerAsync(string name, string url,
            HTTPNotificationEndpoint.MethodEnum method, string token, string orgId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(url, nameof(url));
            Arguments.CheckNotNull(method, nameof(method));
            Arguments.CheckNonEmptyString(token, nameof(token));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var endpoint = new HTTPNotificationEndpoint(type: NotificationEndpointType.Http, method: method, url: url,
                orgID: orgId,
                name: name, authMethod: HTTPNotificationEndpoint.AuthMethodEnum.Bearer, token: token,
                status: NotificationEndpointBase.StatusEnum.Active);

            return (HTTPNotificationEndpoint)await CreateEndpointAsync(endpoint, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Add new notification endpoint.
        /// </summary>
        /// <param name="notificationEndpoint">notificationEndpoint to create</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Notification endpoint created</returns>
        public Task<NotificationEndpoint> CreateEndpointAsync(NotificationEndpoint notificationEndpoint,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(notificationEndpoint, nameof(notificationEndpoint));

            return _service.CreateNotificationEndpointAsync(notificationEndpoint, cancellationToken);
        }

        /// <summary>
        /// Update a notification endpoint. The updates is used for fields from <see cref="NotificationEndpointUpdate"/>.
        /// </summary>
        /// <param name="notificationEndpoint">update to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An updated notification endpoint</returns>
        public Task<NotificationEndpoint> UpdateEndpointAsync(NotificationEndpoint notificationEndpoint,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(notificationEndpoint, nameof(notificationEndpoint));

            Enum.TryParse(notificationEndpoint.Status.ToString(), true,
                out NotificationEndpointUpdate.StatusEnum status);

            return UpdateEndpointAsync(notificationEndpoint.Id,
                new NotificationEndpointUpdate(notificationEndpoint.Name,
                    notificationEndpoint.Description, status), cancellationToken);
        }

        /// <summary>
        /// Update a notification endpoint.
        /// </summary>
        /// <param name="endpointId">ID of notification endpoint</param>
        /// <param name="notificationEndpointUpdate">update to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An updated notification endpoint</returns>
        public Task<NotificationEndpoint> UpdateEndpointAsync(string endpointId,
            NotificationEndpointUpdate notificationEndpointUpdate, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(endpointId, nameof(endpointId));
            Arguments.CheckNotNull(notificationEndpointUpdate, nameof(notificationEndpointUpdate));

            return _service.PatchNotificationEndpointsIDAsync(endpointId, notificationEndpointUpdate,
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Delete a notification endpoint.
        /// </summary>
        /// <param name="notificationEndpoint">notification endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted></returns>
        public Task DeleteNotificationEndpointAsync(NotificationEndpoint notificationEndpoint,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(notificationEndpoint, nameof(notificationEndpoint));

            return DeleteNotificationEndpointAsync(notificationEndpoint.Id, cancellationToken);
        }

        /// <summary>
        /// Delete a notification endpoint.
        /// </summary>
        /// <param name="endpointId">ID of notification endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteNotificationEndpointAsync(string endpointId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(endpointId, nameof(endpointId));

            return _service.DeleteNotificationEndpointsIDAsync(endpointId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Get notification endpoints.
        /// </summary>
        /// <param name="orgId">only show notification endpoints belonging to specified organization</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of notification endpoint</returns>
        public async Task<List<NotificationEndpoint>> FindNotificationEndpointsAsync(string orgId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var response = await FindNotificationEndpointsAsync(orgId, new FindOptions(), cancellationToken)
                .ConfigureAwait(false);
            return response._NotificationEndpoints;
        }

        /// <summary>
        /// Get all notification endpoints.
        /// </summary>
        /// <param name="orgId">only show notification endpoints belonging to specified organization</param>
        /// <param name="findOptions">the find options</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public Task<NotificationEndpoints> FindNotificationEndpointsAsync(string orgId, FindOptions findOptions,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNotNull(findOptions, nameof(findOptions));

            return _service.GetNotificationEndpointsAsync(orgId, offset: findOptions.Offset,
                limit: findOptions.Limit, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Get a notification endpoint.
        /// </summary>
        /// <param name="endpointId">ID of notification endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>the notification endpoint requested</returns>
        public Task<NotificationEndpoint> FindNotificationEndpointByIdAsync(string endpointId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(endpointId, nameof(endpointId));

            return _service.GetNotificationEndpointsIDAsync(endpointId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Clone a Slack Notification endpoint.
        /// </summary>
        /// <param name="name">name of cloned endpoint</param>
        /// <param name="token">Slack WebHook Token</param>
        /// <param name="endpointId">ID of endpoint to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Notification endpoint cloned</returns>
        public async Task<SlackNotificationEndpoint> CloneSlackEndpointAsync(string name, string token,
            string endpointId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(endpointId, nameof(endpointId));

            var endpoint =
                (SlackNotificationEndpoint)await FindNotificationEndpointByIdAsync(endpointId, cancellationToken)
                    .ConfigureAwait(false);
            return await CloneSlackEndpointAsync(name, token, endpoint).ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a Slack Notification endpoint.
        /// </summary>
        /// <param name="name">name of cloned endpoint</param>
        /// <param name="token"></param>
        /// <param name="endpoint">endpoint to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Notification endpoint cloned</returns>
        public async Task<SlackNotificationEndpoint> CloneSlackEndpointAsync(string name, string token,
            SlackNotificationEndpoint endpoint, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNotNull(endpoint, nameof(endpoint));

            var cloned = new SlackNotificationEndpoint(endpoint.Url, token, name: name);

            return (SlackNotificationEndpoint)await CloneEndpointAsync(name, endpoint, cloned, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a PagerDuty Notification endpoint.
        /// </summary>
        /// <param name="name">name of cloned endpoint</param>
        /// <param name="routingKey">Routing Key</param>
        /// <param name="endpointId">ID of endpoint to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Notification endpoint cloned</returns>
        public async Task<PagerDutyNotificationEndpoint> ClonePagerDutyEndpointAsync(string name, string routingKey,
            string endpointId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(routingKey, nameof(routingKey));
            Arguments.CheckNonEmptyString(endpointId, nameof(endpointId));

            var endpoint =
                (PagerDutyNotificationEndpoint)await FindNotificationEndpointByIdAsync(endpointId, cancellationToken)
                    .ConfigureAwait(false);
            return await ClonePagerDutyEndpointAsync(name, routingKey, endpoint).ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a PagerDuty Notification endpoint.
        /// </summary>
        /// <param name="name">name of cloned endpoint</param>
        /// <param name="routingKey">Routing Key</param>
        /// <param name="endpoint">endpoint to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Notification endpoint cloned</returns>
        public async Task<PagerDutyNotificationEndpoint> ClonePagerDutyEndpointAsync(string name, string routingKey,
            PagerDutyNotificationEndpoint endpoint, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(routingKey, nameof(routingKey));
            Arguments.CheckNotNull(endpoint, nameof(endpoint));

            var cloned = new PagerDutyNotificationEndpoint(endpoint.ClientURL, routingKey, name: name);

            return (PagerDutyNotificationEndpoint)await CloneEndpointAsync(name, endpoint, cloned, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a Http Notification endpoint without authentication.
        /// </summary>
        /// <param name="name">name of cloned endpoint</param>
        /// <param name="endpointId">ID of endpoint to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Notification endpoint cloned</returns>
        public async Task<HTTPNotificationEndpoint> CloneHttpEndpointAsync(string name, string endpointId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(endpointId, nameof(endpointId));

            var endpoint =
                (HTTPNotificationEndpoint)await FindNotificationEndpointByIdAsync(endpointId, cancellationToken)
                    .ConfigureAwait(false);
            return await CloneHttpEndpoint(name, endpoint).ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a Http Notification endpoint without authentication.
        /// </summary>
        /// <param name="name">name of cloned endpoint</param>
        /// <param name="endpoint">endpoint to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Notification endpoint cloned</returns>
        public async Task<HTTPNotificationEndpoint> CloneHttpEndpoint(string name, HTTPNotificationEndpoint endpoint,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNotNull(endpoint, nameof(endpoint));

            var cloned = new HTTPNotificationEndpoint(endpoint.Url, method: endpoint.Method, name: name,
                authMethod: HTTPNotificationEndpoint.AuthMethodEnum.None, contentTemplate: endpoint.ContentTemplate,
                headers: endpoint.Headers);

            return (HTTPNotificationEndpoint)await CloneEndpointAsync(name, endpoint, cloned, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a Http Notification endpoint with Http Basic authentication.
        /// </summary>
        /// <param name="name">name of cloned endpoint</param>
        /// <param name="username">HTTP Basic Username</param>
        /// <param name="password">HTTP Basic Password</param>
        /// <param name="endpointId">ID of endpoint to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Notification endpoint cloned</returns>
        public async Task<HTTPNotificationEndpoint> CloneHttpEndpointBasicAuthAsync(string name, string username,
            string password, string endpointId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(username, nameof(username));
            Arguments.CheckNonEmptyString(password, nameof(password));
            Arguments.CheckNonEmptyString(endpointId, nameof(endpointId));

            var endpoint =
                (HTTPNotificationEndpoint)await FindNotificationEndpointByIdAsync(endpointId, cancellationToken)
                    .ConfigureAwait(false);
            return await CloneHttpEndpointBasicAuthAsync(name, username, password, endpoint, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a Http Notification endpoint with Http Basic authentication.
        /// </summary>
        /// <param name="name">name of cloned endpoint</param>
        /// <param name="username">HTTP Basic Username</param>
        /// <param name="password">HTTP Basic Password</param>
        /// <param name="endpoint">endpoint to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Notification endpoint cloned</returns>
        public async Task<HTTPNotificationEndpoint> CloneHttpEndpointBasicAuthAsync(string name, string username,
            string password, HTTPNotificationEndpoint endpoint, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(username, nameof(username));
            Arguments.CheckNonEmptyString(password, nameof(password));
            Arguments.CheckNotNull(endpoint, nameof(endpoint));

            var cloned = new HTTPNotificationEndpoint(endpoint.Url, username, password,
                name: name,
                method: endpoint.Method,
                authMethod: HTTPNotificationEndpoint.AuthMethodEnum.Basic, contentTemplate: endpoint.ContentTemplate,
                headers: endpoint.Headers);

            return (HTTPNotificationEndpoint)await CloneEndpointAsync(name, endpoint, cloned, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a Http Notification endpoint with Bearer authentication.
        /// </summary>
        /// <param name="name">name of cloned endpoint</param>
        /// <param name="token">Bearer token</param>
        /// <param name="endpointId">ID of endpoint to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Notification endpoint cloned</returns>
        public async Task<HTTPNotificationEndpoint> CloneHttpEndpointBearerAsync(string name, string token,
            string endpointId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(token, nameof(token));
            Arguments.CheckNonEmptyString(endpointId, nameof(endpointId));

            var endpoint =
                (HTTPNotificationEndpoint)await FindNotificationEndpointByIdAsync(endpointId, cancellationToken)
                    .ConfigureAwait(false);
            return await CloneHttpEndpointBearerAsync(name, token, endpoint, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a Http Notification endpoint with Bearer authentication.
        /// </summary>
        /// <param name="name">name of cloned endpoint</param>
        /// <param name="token">Bearer token</param>
        /// <param name="endpoint">endpoint to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Notification endpoint cloned</returns>
        public async Task<HTTPNotificationEndpoint> CloneHttpEndpointBearerAsync(string name, string token,
            HTTPNotificationEndpoint endpoint, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(token, nameof(token));
            Arguments.CheckNotNull(endpoint, nameof(endpoint));

            var cloned = new HTTPNotificationEndpoint(endpoint.Url, token: token, name: name,
                method: endpoint.Method,
                authMethod: HTTPNotificationEndpoint.AuthMethodEnum.Bearer, contentTemplate: endpoint.ContentTemplate,
                headers: endpoint.Headers);

            return (HTTPNotificationEndpoint)await CloneEndpointAsync(name, endpoint, cloned, cancellationToken)
                .ConfigureAwait(false);
        }

        /// <summary>
        /// List all labels for a notification endpoint.
        /// </summary>
        /// <param name="endpoint">the notification endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>a list of all labels for a notification endpoint</returns>
        public Task<List<Label>> GetLabelsAsync(NotificationEndpoint endpoint,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(endpoint, nameof(endpoint));

            return GetLabelsAsync(endpoint.Id, cancellationToken);
        }

        /// <summary>
        /// List all labels for a notification endpoint.
        /// </summary>
        /// <param name="endpointId">ID of the notification endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>a list of all labels for a notification endpoint</returns>
        public async Task<List<Label>> GetLabelsAsync(string endpointId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(endpointId, nameof(endpointId));

            var response = await _service
                .GetNotificationEndpointsIDLabelsAsync(endpointId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Labels;
        }

        /// <summary>
        /// Add a label to a notification endpoint.
        /// </summary>
        /// <param name="label">label to add</param>
        /// <param name="endpoint">the notification endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public Task<Label> AddLabelAsync(Label label, NotificationEndpoint endpoint,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(endpoint, nameof(endpoint));
            Arguments.CheckNotNull(label, nameof(label));

            return AddLabelAsync(label.Id, endpoint.Id, cancellationToken);
        }

        /// <summary>
        /// Add a label to a notification endpoint.
        /// </summary>
        /// <param name="labelId">the ID of label to add</param>
        /// <param name="endpointId">the ID of the notification endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public async Task<Label> AddLabelAsync(string labelId, string endpointId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(endpointId, nameof(endpointId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var mapping = new LabelMapping(labelId);

            var response = await _service
                .PostNotificationEndpointIDLabelsAsync(endpointId, mapping, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Label;
        }

        /// <summary>
        /// Delete label from a notification endpoint.
        /// </summary>
        /// <param name="label">the label to delete</param>
        /// <param name="endpoint">the notification endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public Task DeleteLabelAsync(Label label, NotificationEndpoint endpoint,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(endpoint, nameof(endpoint));
            Arguments.CheckNotNull(label, nameof(label));

            return DeleteLabelAsync(label.Id, endpoint.Id, cancellationToken);
        }

        /// <summary>
        /// Delete label from a notification endpoint.
        /// </summary>
        /// <param name="labelId">the label id to delete</param>
        /// <param name="endpointId">ID of the notification endpoint</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns></returns>
        public Task DeleteLabelAsync(string labelId, string endpointId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(endpointId, nameof(endpointId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return _service.DeleteNotificationEndpointsIDLabelsIDAsync(endpointId, labelId,
                cancellationToken: cancellationToken);
        }

        private async Task<NotificationEndpoint> CloneEndpointAsync(string name, NotificationEndpoint toCloneEndpoint,
            NotificationEndpoint clonedEndpoint, CancellationToken cancellationToken = default)
        {
            clonedEndpoint.OrgID = toCloneEndpoint.OrgID;
            clonedEndpoint.Description = toCloneEndpoint.Description;
            clonedEndpoint.Status = toCloneEndpoint.Status;
            clonedEndpoint.Type = toCloneEndpoint.Type;

            var created = await CreateEndpointAsync(clonedEndpoint, cancellationToken).ConfigureAwait(false);
            var labels = await GetLabelsAsync(toCloneEndpoint, cancellationToken).ConfigureAwait(false);

            foreach (var label in labels) await AddLabelAsync(label, created, cancellationToken).ConfigureAwait(false);

            return created;
        }
    }
}