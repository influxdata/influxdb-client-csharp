using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;

namespace InfluxDB.Client
{
    /// <summary>
    /// The client of the InfluxDB 2.x that implement Telegrafs HTTP API endpoint.
    /// </summary>
    public class TelegrafsApi
    {
        private readonly TelegrafsService _service;

        protected internal TelegrafsApi(TelegrafsService service)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
        }

        /// <summary>
        /// Create a telegraf config.
        /// </summary>
        /// <param name="name">Telegraf Configuration Name</param>
        /// <param name="description">Telegraf Configuration Description</param>
        /// <param name="org">The organization that owns this config</param>
        /// <param name="plugins">The telegraf plugins config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Telegraf config created</returns>
        public Task<Telegraf> CreateTelegrafAsync(string name, string description, Organization org,
            List<TelegrafPlugin> plugins, CancellationToken cancellationToken = default)
        {
            return CreateTelegrafAsync(name, description, org, CreateAgentConfiguration(), plugins, cancellationToken);
        }

        /// <summary>
        /// Create a telegraf config.
        /// </summary>
        /// <param name="name">Telegraf Configuration Name</param>
        /// <param name="description">Telegraf Configuration Description</param>
        /// <param name="org">The organization that owns this config</param>
        /// <param name="agentConfiguration">The telegraf agent config</param>
        /// <param name="plugins">The telegraf plugins config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Telegraf config created</returns>
        public Task<Telegraf> CreateTelegrafAsync(string name, string description, Organization org,
            Dictionary<string, object> agentConfiguration, List<TelegrafPlugin> plugins,
            CancellationToken cancellationToken = default)
        {
            return CreateTelegrafAsync(name, description, org.Id, agentConfiguration, plugins, cancellationToken);
        }

        /// <summary>
        /// Create a telegraf config.
        /// </summary>
        /// <param name="name">Telegraf Configuration Name</param>
        /// <param name="description">Telegraf Configuration Description</param>
        /// <param name="orgId">The organization that owns this config</param>
        /// <param name="plugins">The telegraf plugins config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Telegraf config created</returns>
        public Task<Telegraf> CreateTelegrafAsync(string name, string description, string orgId,
            List<TelegrafPlugin> plugins, CancellationToken cancellationToken = default)
        {
            return CreateTelegrafAsync(name, description, orgId, CreateAgentConfiguration(), plugins,
                cancellationToken);
        }

        /// <summary>
        /// Create a telegraf config.
        /// </summary>
        /// <param name="name">Telegraf Configuration Name</param>
        /// <param name="description">Telegraf Configuration Description</param>
        /// <param name="orgId">The organization that owns this config</param>
        /// <param name="agentConfiguration">The telegraf agent config</param>
        /// <param name="plugins">The telegraf plugins config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Telegraf config created</returns>
        public Task<Telegraf> CreateTelegrafAsync(string name, string description, string orgId,
            Dictionary<string, object> agentConfiguration, List<TelegrafPlugin> plugins,
            CancellationToken cancellationToken = default)
        {
            var config = new StringBuilder();

            // append agent configuration
            config.Append("[agent]").Append("\n");
            foreach (var pair in agentConfiguration) AppendConfiguration(config, pair.Key, pair.Value);

            config.Append("\n");

            // append plugins configuration
            foreach (var plugin in plugins)
            {
                if (!string.IsNullOrEmpty(plugin.Description))
                {
                    config.Append("#").Append(plugin.Description).Append("\n");
                }

                config
                    .Append("[[")
                    .Append(plugin.Type.ToString().ToLower())
                    .Append(".")
                    .Append(plugin.Name)
                    .Append("]]")
                    .Append("\n");

                foreach (var pair in plugin.Config) AppendConfiguration(config, pair.Key, pair.Value);

                config.Append("\n");
            }

            var pluginsList = plugins
                .Select(it => new TelegrafPluginRequestPlugins(
                    it.Type.ToString().ToLower(),
                    it.Name,
                    description: it.Description,
                    config: it.Config)
                )
                .ToList();

            var request = new TelegrafPluginRequest(name, description, orgID: orgId, config: config.ToString(),
                plugins: pluginsList);

            return CreateTelegrafAsync(request, cancellationToken);
        }

        /// <summary>
        /// Create a telegraf config.
        /// </summary>
        /// <param name="name">Telegraf Configuration Name</param>
        /// <param name="description">Telegraf Configuration Description</param>
        /// <param name="org">The organization that owns this config</param>
        /// <param name="config">ConfigTOML contains the raw toml config</param>
        /// <param name="metadata">Metadata for the config</param>
        /// <param name="plugins">Plugins to use.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Telegraf config created</returns>
        public Task<Telegraf> CreateTelegrafAsync(string name, string description, Organization org,
            string config, TelegrafRequestMetadata metadata, List<TelegrafPluginRequestPlugins> plugins = null,
            CancellationToken cancellationToken = default)
        {
            return CreateTelegrafAsync(name, description, org.Id, config, metadata, plugins, cancellationToken);
        }

        /// <summary>
        /// Create a telegraf config.
        /// </summary>
        /// <param name="name">Telegraf Configuration Name</param>
        /// <param name="description">Telegraf Configuration Description</param>
        /// <param name="orgId">The organization that owns this config</param>
        /// <param name="config">ConfigTOML contains the raw toml config</param>
        /// <param name="metadata">Metadata for the config</param>
        /// <param name="plugins">Plugins to use.</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Telegraf config created</returns>
        public Task<Telegraf> CreateTelegrafAsync(string name, string description, string orgId,
            string config, TelegrafRequestMetadata metadata, List<TelegrafPluginRequestPlugins> plugins = null,
            CancellationToken cancellationToken = default)
        {
            var request = new TelegrafPluginRequest(name, description, plugins, metadata, config, orgId);

            return CreateTelegrafAsync(request, cancellationToken);
        }

        /// <summary>
        /// Create a telegraf config.
        /// </summary>
        /// <param name="telegrafRequest">Telegraf Configuration to create</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>Telegraf config created</returns>
        public Task<Telegraf> CreateTelegrafAsync(TelegrafPluginRequest telegrafRequest,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(telegrafRequest, nameof(telegrafRequest));

            return _service.PostTelegrafsAsync(telegrafRequest, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Created default Telegraf Agent configuration.
        /// <example>
        /// [agent]
        ///    interval = "10s"
        ///    round_interval = true
        ///    metric_batch_size = 1000
        ///    metric_buffer_limit = 10000
        ///    collection_jitter = "0s"
        ///    flush_jitter = "0s"
        ///    precision = ""
        ///    omit_hostname = false
        /// </example>
        /// </summary>
        /// <returns>default configuration</returns>
        public Dictionary<string, object> CreateAgentConfiguration()
        {
            return new Dictionary<string, object>
            {
                { "interval", "10s" },
                { "round_interval", true },
                { "metric_batch_size", 1000 },
                { "metric_buffer_limit", 10000 },
                { "collection_jitter", "0s" },
                { "flush_jitter", "0s" },
                { "precision", "" },
                { "omit_hostname", false }
            };
        }

        /// <summary>
        /// Update a telegraf config.
        /// </summary>
        /// <param name="telegraf">telegraf config update to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An updated telegraf</returns>
        public Task<Telegraf> UpdateTelegrafAsync(Telegraf telegraf, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            var request = new TelegrafPluginRequest(telegraf.Name, telegraf.Description, default, telegraf.Metadata,
                telegraf.Config,
                telegraf.OrgID);

            return UpdateTelegrafAsync(telegraf.Id, request, cancellationToken);
        }

        /// <summary>
        /// Update a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of telegraf config</param>
        /// <param name="telegrafRequest">telegraf config update to apply</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>An updated telegraf</returns>
        public Task<Telegraf> UpdateTelegrafAsync(string telegrafId, TelegrafPluginRequest telegrafRequest,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNotNull(telegrafRequest, nameof(telegrafRequest));

            return _service.PutTelegrafsIDAsync(telegrafId, telegrafRequest, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Delete a telegraf config.
        /// </summary>
        /// <param name="telegraf">telegraf config to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteTelegrafAsync(Telegraf telegraf, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            return DeleteTelegrafAsync(telegraf.Id, cancellationToken);
        }

        /// <summary>
        /// Delete a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of telegraf config to delete</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteTelegrafAsync(string telegrafId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            return _service.DeleteTelegrafsIDAsync(telegrafId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Clone a telegraf config.
        /// </summary>
        /// <param name="clonedName">name of cloned telegraf config</param>
        /// <param name="telegrafId">ID of telegraf config to clone</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned telegraf config</returns>
        public async Task<Telegraf> CloneTelegrafAsync(string clonedName, string telegrafId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            var telegraf = await FindTelegrafByIdAsync(telegrafId, cancellationToken).ConfigureAwait(false);

            return await CloneTelegrafAsync(clonedName, telegraf, cancellationToken).ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a telegraf config.
        /// </summary>
        /// <param name="clonedName">name of cloned telegraf config</param>
        /// <param name="telegraf">telegraf config to clone></param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>cloned telegraf config</returns>
        public async Task<Telegraf> CloneTelegrafAsync(string clonedName, Telegraf telegraf,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            var cloned = new TelegrafPluginRequest(clonedName, telegraf.Description, default, telegraf.Metadata,
                telegraf.Config,
                telegraf.OrgID);

            var created = await CreateTelegrafAsync(cloned, cancellationToken).ConfigureAwait(false);
            var labels = await GetLabelsAsync(telegraf, cancellationToken).ConfigureAwait(false);
            foreach (var label in labels) await AddLabelAsync(label, created).ConfigureAwait(false);

            return created;
        }

        /// <summary>
        /// Retrieve a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of telegraf config to get</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>telegraf config details</returns>
        public async Task<Telegraf> FindTelegrafByIdAsync(string telegrafId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            var response = await _service
                .GetTelegrafsIDWithIRestResponseAsync(telegrafId, null, "application/json", cancellationToken)
                .ConfigureAwait(false);

            return (Telegraf)_service.Configuration.ApiClient.Deserialize(response, typeof(Telegraf));
        }

        /// <summary>
        /// Returns a list of telegraf configs.
        /// </summary>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of telegraf configs</returns>
        public Task<List<Telegraf>> FindTelegrafsAsync(CancellationToken cancellationToken = default)
        {
            return FindTelegrafsByOrgIdAsync(null, cancellationToken);
        }

        /// <summary>
        /// Returns a list of telegraf configs for specified organization.
        /// </summary>
        /// <param name="organization">specifies the organization of the telegraf configs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of telegraf configs</returns>
        public Task<List<Telegraf>> FindTelegrafsByOrgAsync(Organization organization,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return FindTelegrafsByOrgIdAsync(organization.Id, cancellationToken);
        }

        /// <summary>
        /// Returns a list of telegraf configs for specified organization.
        /// </summary>
        /// <param name="orgId">specifies the organization of the telegraf configs</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>A list of telegraf configs</returns>
        public async Task<List<Telegraf>> FindTelegrafsByOrgIdAsync(string orgId,
            CancellationToken cancellationToken = default)
        {
            var response = await _service.GetTelegrafsAsync(orgId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Configurations;
        }

        /// <summary>
        /// Retrieve a telegraf config in TOML.
        /// </summary>
        /// <param name="telegraf">telegraf config to get</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>telegraf config details in TOML format</returns>
        public Task<string> GetTOMLAsync(Telegraf telegraf, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            return GetTOMLAsync(telegraf.Id, cancellationToken);
        }

        /// <summary>
        /// Retrieve a telegraf config in TOML.
        /// </summary>
        /// <param name="telegrafId">ID of telegraf config to get</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>telegraf config details in TOML format</returns>
        public Task<string> GetTOMLAsync(string telegrafId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            return _service.GetTelegrafsIDAsync(telegrafId, null, "application/toml", cancellationToken);
        }

        /// <summary>
        /// List all users with member privileges for a telegraf config.
        /// </summary>
        /// <param name="telegraf">the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>a list of telegraf config members</returns>
        public Task<List<ResourceMember>> GetMembersAsync(Telegraf telegraf,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            return GetMembersAsync(telegraf.Id, cancellationToken);
        }

        /// <summary>
        /// List all users with member privileges for a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>a list of telegraf config members</returns>
        public async Task<List<ResourceMember>> GetMembersAsync(string telegrafId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            var response = await _service.GetTelegrafsIDMembersAsync(telegrafId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Users;
        }

        /// <summary>
        /// Add telegraf config member.
        /// </summary>
        /// <param name="member">user to add as member</param>
        /// <param name="telegraf">the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>member added to telegraf</returns>
        public Task<ResourceMember> AddMemberAsync(User member, Telegraf telegraf,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(member, nameof(member));

            return AddMemberAsync(member.Id, telegraf.Id, cancellationToken);
        }

        /// <summary>
        /// Add telegraf config member.
        /// </summary>
        /// <param name="memberId">user ID to add as member</param>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>member added to telegraf</returns>
        public Task<ResourceMember> AddMemberAsync(string memberId, string telegrafId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return _service.PostTelegrafsIDMembersAsync(telegrafId, new AddResourceMemberRequestBody(memberId),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Removes a member from a telegraf config.
        /// </summary>
        /// <param name="member">member to remove</param>
        /// <param name="telegraf">the telegraf</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>member removed</returns>
        public Task DeleteMemberAsync(User member, Telegraf telegraf, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(member, nameof(member));

            return DeleteMemberAsync(member.Id, telegraf.Id, cancellationToken);
        }

        /// <summary>
        /// Removes a member from a telegraf config.
        /// </summary>
        /// <param name="memberId">ID of member to remove</param>
        /// <param name="telegrafId">ID of the telegraf</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>member removed</returns>
        public Task DeleteMemberAsync(string memberId, string telegrafId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return _service.DeleteTelegrafsIDMembersIDAsync(memberId, telegrafId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// List all owners of a telegraf config.
        /// </summary>
        /// <param name="telegraf">the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>a list of telegraf config owners</returns>
        public Task<List<ResourceOwner>> GetOwnersAsync(Telegraf telegraf,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            return GetOwnersAsync(telegraf.Id, cancellationToken);
        }

        /// <summary>
        /// List all owners of a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>a list of telegraf config owners</returns>
        public async Task<List<ResourceOwner>> GetOwnersAsync(string telegrafId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            var response = await _service.GetTelegrafsIDOwnersAsync(telegrafId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Users;
        }

        /// <summary>
        /// Add telegraf config owner.
        /// </summary>
        /// <param name="owner">user to add as owner</param>
        /// <param name="telegraf">the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>telegraf config owner added</returns>
        public Task<ResourceOwner> AddOwnerAsync(User owner, Telegraf telegraf,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(owner, nameof(owner));

            return AddOwnerAsync(owner.Id, telegraf.Id, cancellationToken);
        }

        /// <summary>
        /// Add telegraf config owner.
        /// </summary>
        /// <param name="ownerId">ID of user to add as owner</param>
        /// <param name="telegrafId"> ID of the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>telegraf config owner added</returns>
        public Task<ResourceOwner> AddOwnerAsync(string ownerId, string telegrafId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            return _service.PostTelegrafsIDOwnersAsync(telegrafId, new AddResourceMemberRequestBody(ownerId),
                cancellationToken: cancellationToken);
        }

        /// <summary>
        /// Removes an owner from a telegraf config.
        /// </summary>
        /// <param name="owner">owner to remove</param>
        /// <param name="telegraf">the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>owner removed</returns>
        public Task DeleteOwnerAsync(User owner, Telegraf telegraf, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(owner, nameof(owner));

            return DeleteOwnerAsync(owner.Id, telegraf.Id, cancellationToken);
        }

        /// <summary>
        /// Removes an owner from a telegraf config.
        /// </summary>
        /// <param name="ownerId">ID of owner to remove</param>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>owner removed</returns>
        public Task DeleteOwnerAsync(string ownerId, string telegrafId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            return _service.DeleteTelegrafsIDOwnersIDAsync(ownerId, telegrafId, cancellationToken: cancellationToken);
        }

        /// <summary>
        /// List all labels for a telegraf config.
        /// </summary>
        /// <param name="telegraf">the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>a list of all labels for a telegraf config</returns>
        public Task<List<Label>> GetLabelsAsync(Telegraf telegraf, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            return GetLabelsAsync(telegraf.Id, cancellationToken);
        }

        /// <summary>
        /// List all labels for a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>a list of all labels for a telegraf config</returns>
        public async Task<List<Label>> GetLabelsAsync(string telegrafId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            var response = await _service.GetTelegrafsIDLabelsAsync(telegrafId, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Labels;
        }

        /// <summary>
        /// Add a label to a telegraf config.
        /// </summary>
        /// <param name="label">label to add</param>
        /// <param name="telegraf">the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>added label</returns>
        public Task<Label> AddLabelAsync(Label label, Telegraf telegraf, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(label, nameof(label));

            return AddLabelAsync(label.Id, telegraf.Id, cancellationToken);
        }

        /// <summary>
        /// Add a label to a telegraf config.
        /// </summary>
        /// <param name="labelId">ID of label to add</param>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabelAsync(string labelId, string telegrafId,
            CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var response = await _service.PostTelegrafsIDLabelsAsync(telegrafId, new LabelMapping(labelId),
                    cancellationToken: cancellationToken)
                .ConfigureAwait(false);
            return response.Label;
        }

        /// <summary>
        /// Delete a label from a telegraf config.
        /// </summary>
        /// <param name="label">label to delete</param>
        /// <param name="telegraf">the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteLabelAsync(Label label, Telegraf telegraf, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(label, nameof(label));

            return DeleteLabelAsync(label.Id, telegraf.Id, cancellationToken);
        }

        /// <summary>
        /// Delete a label from a telegraf config.
        /// </summary>
        /// <param name="labelId">ID of label to delete</param>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteLabelAsync(string labelId, string telegrafId, CancellationToken cancellationToken = default)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return _service.DeleteTelegrafsIDLabelsIDAsync(telegrafId, labelId, cancellationToken: cancellationToken);
        }

        private void AppendConfiguration(StringBuilder config, string key, object value)
        {
            if (value == null)
            {
                return;
            }

            config.Append("  ").Append(key).Append(" = ");
            if (value is IEnumerable<object> enumerable)
            {
                var values = enumerable.Select(it =>
                {
                    if (it is string str)
                    {
                        return $"\"{str}\"";
                    }

                    return it.ToString();
                });
                config.Append("[");
                config.Append(string.Join(", ", values));
                config.Append("]");
            }
            else if (value is string)
            {
                config.Append('"');
                config.Append(value);
                config.Append('"');
            }
            else if (value is bool b)
            {
                config.Append(b.ToString().ToLower());
            }
            else
            {
                config.Append(value);
            }

            config.Append("\n");
        }
    }
}