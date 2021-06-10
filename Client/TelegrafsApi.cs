using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;

namespace InfluxDB.Client
{
    /// <summary>
    /// The client of the InfluxDB 2.0 that implement Telegrafs HTTP API endpoint.
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
        /// <returns>Telegraf config created</returns>
        public Task<Telegraf> CreateTelegrafAsync(string name, string description, Organization org,
            List<TelegrafPlugin> plugins)
        {
            return CreateTelegrafAsync(name, description, org, CreateAgentConfiguration(), plugins);
        }

        /// <summary>
        /// Create a telegraf config.
        /// </summary>
        /// <param name="name">Telegraf Configuration Name</param>
        /// <param name="description">Telegraf Configuration Description</param>
        /// <param name="org">The organization that owns this config</param>
        /// <param name="agentConfiguration">The telegraf agent config</param>
        /// <param name="plugins">The telegraf plugins config</param>
        /// <returns>Telegraf config created</returns>
        public Task<Telegraf> CreateTelegrafAsync(string name, string description, Organization org,
            Dictionary<string, object> agentConfiguration, List<TelegrafPlugin> plugins)
        {
            return CreateTelegrafAsync(name, description, org.Id, agentConfiguration, plugins);
        }

        /// <summary>
        /// Create a telegraf config.
        /// </summary>
        /// <param name="name">Telegraf Configuration Name</param>
        /// <param name="description">Telegraf Configuration Description</param>
        /// <param name="orgId">The organization that owns this config</param>
        /// <param name="plugins">The telegraf plugins config</param>
        /// <returns>Telegraf config created</returns>
        public Task<Telegraf> CreateTelegrafAsync(string name, string description, string orgId,
            List<TelegrafPlugin> plugins)
        {
            return CreateTelegrafAsync(name, description, orgId, CreateAgentConfiguration(), plugins);
        }

        /// <summary>
        /// Create a telegraf config.
        /// </summary>
        /// <param name="name">Telegraf Configuration Name</param>
        /// <param name="description">Telegraf Configuration Description</param>
        /// <param name="orgId">The organization that owns this config</param>
        /// <param name="agentConfiguration">The telegraf agent config</param>
        /// <param name="plugins">The telegraf plugins config</param>
        /// <returns>Telegraf config created</returns>
        public Task<Telegraf> CreateTelegrafAsync(string name, string description, string orgId,
            Dictionary<string, object> agentConfiguration, List<TelegrafPlugin> plugins)
        {
            var config = new StringBuilder();

            // append agent configuration
            config.Append("[agent]").Append("\n");
            foreach (var pair in agentConfiguration)
            {
                AppendConfiguration(config, pair.Key, pair.Value);
            }

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

                foreach (var pair in plugin.Config)
                {
                    AppendConfiguration(config, pair.Key, pair.Value);
                }

                config.Append("\n");
            }

            var request = new TelegrafRequest(name: name, description: description, orgID: orgId,
                config: config.ToString());

            return CreateTelegrafAsync(request);
        }

        /// <summary>
        /// Create a telegraf config.
        /// </summary>
        /// <param name="name">Telegraf Configuration Name</param>
        /// <param name="description">Telegraf Configuration Description</param>
        /// <param name="org">The organization that owns this config</param>
        /// <param name="config">ConfigTOML contains the raw toml config</param>
        /// <param name="metadata">Metadata for the config</param>
        /// <returns>Telegraf config created</returns>
        public Task<Telegraf> CreateTelegrafAsync(string name, string description, Organization org,
            string config, TelegrafRequestMetadata metadata)
        {
            return CreateTelegrafAsync(name, description, org.Id, config, metadata);
        }

        /// <summary>
        /// Create a telegraf config.
        /// </summary>
        /// <param name="name">Telegraf Configuration Name</param>
        /// <param name="description">Telegraf Configuration Description</param>
        /// <param name="orgId">The organization that owns this config</param>
        /// <param name="config">ConfigTOML contains the raw toml config</param>
        /// <param name="metadata">Metadata for the config</param>
        /// <returns>Telegraf config created</returns>
        public Task<Telegraf> CreateTelegrafAsync(string name, string description, string orgId,
            string config, TelegrafRequestMetadata metadata)
        {
            var request = new TelegrafRequest(name, description, metadata, config, orgId);

            return CreateTelegrafAsync(request);
        }

        /// <summary>
        /// Create a telegraf config.
        /// </summary>
        /// <param name="telegrafRequest">Telegraf Configuration to create</param>
        /// <returns>Telegraf config created</returns>
        public Task<Telegraf> CreateTelegrafAsync(TelegrafRequest telegrafRequest)
        {
            Arguments.CheckNotNull(telegrafRequest, nameof(telegrafRequest));

            return _service.PostTelegrafsAsync(telegrafRequest);
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
                {"interval", "10s"},
                {"round_interval", true},
                {"metric_batch_size", 1000},
                {"metric_buffer_limit", 10000},
                {"collection_jitter", "0s"},
                {"flush_jitter", "0s"},
                {"precision", ""},
                {"omit_hostname", false}
            };
        }

        /// <summary>
        /// Update a telegraf config.
        /// </summary>
        /// <param name="telegraf">telegraf config update to apply</param>
        /// <returns>An updated telegraf</returns>
        public Task<Telegraf> UpdateTelegrafAsync(Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            var request = new TelegrafRequest(telegraf.Name, telegraf.Description, telegraf.Metadata, telegraf.Config,
                telegraf.OrgID);

            return UpdateTelegrafAsync(telegraf.Id, request);
        }

        /// <summary>
        /// Update a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of telegraf config</param>
        /// <param name="telegrafRequest">telegraf config update to apply</param>
        /// <returns>An updated telegraf</returns>
        public Task<Telegraf> UpdateTelegrafAsync(string telegrafId, TelegrafRequest telegrafRequest)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNotNull(telegrafRequest, nameof(telegrafRequest));

            return _service.PutTelegrafsIDAsync(telegrafId, telegrafRequest);
        }

        /// <summary>
        /// Delete a telegraf config.
        /// </summary>
        /// <param name="telegraf">telegraf config to delete</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteTelegrafAsync(Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            return DeleteTelegrafAsync(telegraf.Id);
        }

        /// <summary>
        /// Delete a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of telegraf config to delete</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteTelegrafAsync(string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            return _service.DeleteTelegrafsIDAsync(telegrafId);
        }

        /// <summary>
        /// Clone a telegraf config.
        /// </summary>
        /// <param name="clonedName">name of cloned telegraf config</param>
        /// <param name="telegrafId">ID of telegraf config to clone</param>
        /// <returns>cloned telegraf config</returns>
        public async Task<Telegraf> CloneTelegrafAsync(string clonedName, string telegrafId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            var telegraf = await FindTelegrafByIdAsync(telegrafId).ConfigureAwait(false);
            
            return await CloneTelegrafAsync(clonedName, telegraf).ConfigureAwait(false);
        }

        /// <summary>
        /// Clone a telegraf config.
        /// </summary>
        /// <param name="clonedName">name of cloned telegraf config</param>
        /// <param name="telegraf">telegraf config to clone></param>
        /// <returns>cloned telegraf config</returns>
        public async Task<Telegraf> CloneTelegrafAsync(string clonedName, Telegraf telegraf)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            var cloned = new TelegrafRequest(clonedName, telegraf.Description, telegraf.Metadata, telegraf.Config,
                telegraf.OrgID);

            var created = await CreateTelegrafAsync(cloned).ConfigureAwait(false);
            var labels = await GetLabelsAsync(telegraf).ConfigureAwait(false);
            foreach (var label in labels)
            {
                await AddLabelAsync(label, created).ConfigureAwait(false);
            }

            return created;
        }

        /// <summary>
        /// Retrieve a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of telegraf config to get</param>
        /// <returns>telegraf config details</returns>
        public async Task<Telegraf> FindTelegrafByIdAsync(string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            var response = await _service.GetTelegrafsIDWithIRestResponseAsync(telegrafId, null, "application/json").ConfigureAwait(false);
            
            return (Telegraf) _service.Configuration.ApiClient.Deserialize(response, typeof(Telegraf));
        }

        /// <summary>
        /// Returns a list of telegraf configs.
        /// </summary>
        /// <returns>A list of telegraf configs</returns>
        public Task<List<Telegraf>> FindTelegrafsAsync()
        {
            return FindTelegrafsByOrgIdAsync(null);
        }

        /// <summary>
        /// Returns a list of telegraf configs for specified organization.
        /// </summary>
        /// <param name="organization">specifies the organization of the telegraf configs</param>
        /// <returns>A list of telegraf configs</returns>
        public Task<List<Telegraf>> FindTelegrafsByOrgAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return FindTelegrafsByOrgIdAsync(organization.Id);
        }

        /// <summary>
        /// Returns a list of telegraf configs for specified organization.
        /// </summary>
        /// <param name="orgId">specifies the organization of the telegraf configs</param>
        /// <returns>A list of telegraf configs</returns>
        public async Task<List<Telegraf>> FindTelegrafsByOrgIdAsync(string orgId)
        {
            var response = await _service.GetTelegrafsAsync(orgId).ConfigureAwait(false);
            return response.Configurations;
        }

        /// <summary>
        /// Retrieve a telegraf config in TOML.
        /// </summary>
        /// <param name="telegraf">telegraf config to get</param>
        /// <returns>telegraf config details in TOML format</returns>
        public Task<string> GetTOMLAsync(Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            return GetTOMLAsync(telegraf.Id);
        }

        /// <summary>
        /// Retrieve a telegraf config in TOML.
        /// </summary>
        /// <param name="telegrafId">ID of telegraf config to get</param>
        /// <returns>telegraf config details in TOML format</returns>
        public Task<string> GetTOMLAsync(string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            return _service.GetTelegrafsIDAsync(telegrafId, null, "application/toml");
        }

        /// <summary>
        /// List all users with member privileges for a telegraf config.
        /// </summary>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>a list of telegraf config members</returns>
        public Task<List<ResourceMember>> GetMembersAsync(Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            return GetMembersAsync(telegraf.Id);
        }

        /// <summary>
        /// List all users with member privileges for a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <returns>a list of telegraf config members</returns>
        public async Task<List<ResourceMember>> GetMembersAsync(string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            var response = await _service.GetTelegrafsIDMembersAsync(telegrafId).ConfigureAwait(false);
            return response.Users;
        }

        /// <summary>
        /// Add telegraf config member.
        /// </summary>
        /// <param name="member">user to add as member</param>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>member added to telegraf</returns>
        public Task<ResourceMember> AddMemberAsync(User member, Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(member, nameof(member));

            return AddMemberAsync(member.Id, telegraf.Id);
        }

        /// <summary>
        /// Add telegraf config member.
        /// </summary>
        /// <param name="memberId">user ID to add as member</param>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <returns>member added to telegraf</returns>
        public Task<ResourceMember> AddMemberAsync(string memberId, string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return _service.PostTelegrafsIDMembersAsync(telegrafId, new AddResourceMemberRequestBody(memberId));
        }

        /// <summary>
        /// Removes a member from a telegraf config.
        /// </summary>
        /// <param name="member">member to remove</param>
        /// <param name="telegraf">the telegraf</param>
        /// <returns>member removed</returns>
        public Task DeleteMemberAsync(User member, Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(member, nameof(member));

            return DeleteMemberAsync(member.Id, telegraf.Id);
        }

        /// <summary>
        /// Removes a member from a telegraf config.
        /// </summary>
        /// <param name="memberId">ID of member to remove</param>
        /// <param name="telegrafId">ID of the telegraf</param>
        /// <returns>member removed</returns>
        public Task DeleteMemberAsync(string memberId, string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return _service.DeleteTelegrafsIDMembersIDAsync(memberId, telegrafId);
        }

        /// <summary>
        /// List all owners of a telegraf config.
        /// </summary>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>a list of telegraf config owners</returns>
        public Task<List<ResourceOwner>> GetOwnersAsync(Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            return GetOwnersAsync(telegraf.Id);
        }

        /// <summary>
        /// List all owners of a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <returns>a list of telegraf config owners</returns>
        public async Task<List<ResourceOwner>> GetOwnersAsync(string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            var response = await _service.GetTelegrafsIDOwnersAsync(telegrafId).ConfigureAwait(false);
            return response.Users;
        }

        /// <summary>
        /// Add telegraf config owner.
        /// </summary>
        /// <param name="owner">user to add as owner</param>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>telegraf config owner added</returns>
        public Task<ResourceOwner> AddOwnerAsync(User owner, Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(owner, nameof(owner));

            return AddOwnerAsync(owner.Id, telegraf.Id);
        }

        /// <summary>
        /// Add telegraf config owner.
        /// </summary>
        /// <param name="ownerId">ID of user to add as owner</param>
        /// <param name="telegrafId"> ID of the telegraf config</param>
        /// <returns>telegraf config owner added</returns>
        public Task<ResourceOwner> AddOwnerAsync(string ownerId, string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            return _service.PostTelegrafsIDOwnersAsync(telegrafId, new AddResourceMemberRequestBody(ownerId));
        }

        /// <summary>
        /// Removes an owner from a telegraf config.
        /// </summary>
        /// <param name="owner">owner to remove</param>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>owner removed</returns>
        public Task DeleteOwnerAsync(User owner, Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(owner, nameof(owner));

            return DeleteOwnerAsync(owner.Id, telegraf.Id);
        }

        /// <summary>
        /// Removes an owner from a telegraf config.
        /// </summary>
        /// <param name="ownerId">ID of owner to remove</param>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <returns>owner removed</returns>
        public Task DeleteOwnerAsync(string ownerId, string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            return _service.DeleteTelegrafsIDOwnersIDAsync(ownerId, telegrafId);
        }

        /// <summary>
        /// List all labels for a telegraf config.
        /// </summary>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>a list of all labels for a telegraf config</returns>
        public Task<List<Label>> GetLabelsAsync(Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            return GetLabelsAsync(telegraf.Id);
        }

        /// <summary>
        /// List all labels for a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <returns>a list of all labels for a telegraf config</returns>
        public async Task<List<Label>> GetLabelsAsync(string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            var response = await _service.GetTelegrafsIDLabelsAsync(telegrafId).ConfigureAwait(false);
            return response.Labels;
        }

        /// <summary>
        /// Add a label to a telegraf config.
        /// </summary>
        /// <param name="label">label to add</param>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>added label</returns>
        public Task<Label> AddLabelAsync(Label label, Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(label, nameof(label));

            return AddLabelAsync(label.Id, telegraf.Id);
        }

        /// <summary>
        /// Add a label to a telegraf config.
        /// </summary>
        /// <param name="labelId">ID of label to add</param>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabelAsync(string labelId, string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            var response = await _service.PostTelegrafsIDLabelsAsync(telegrafId, new LabelMapping(labelId)).ConfigureAwait(false);
            return response.Label;
        }

        /// <summary>
        /// Delete a label from a telegraf config.
        /// </summary>
        /// <param name="label">label to delete</param>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteLabelAsync(Label label, Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(label, nameof(label));

            return DeleteLabelAsync(label.Id, telegraf.Id);
        }

        /// <summary>
        /// Delete a label from a telegraf config.
        /// </summary>
        /// <param name="labelId">ID of label to delete</param>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteLabelAsync(string labelId, string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return _service.DeleteTelegrafsIDLabelsIDAsync(telegrafId, labelId);
        }

        private void AppendConfiguration(StringBuilder config, string key, object value)
        {
            if (value == null) return;

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