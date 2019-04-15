using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;
using InfluxDB.Client.Core;
using InfluxDB.Client.Core.Internal;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Generated.Domain;
using InfluxDB.Client.Internal;
using Organization = InfluxDB.Client.Domain.Organization;
using ResourceMember = InfluxDB.Client.Domain.ResourceMember;
using ResourceMembers = InfluxDB.Client.Domain.ResourceMembers;
using Task = System.Threading.Tasks.Task;
using User = InfluxDB.Client.Domain.User;

namespace InfluxDB.Client
{
    /// <summary>
    ///     The client of the InfluxDB 2.0 that implement Telegrafs HTTP API endpoint.
    /// </summary>
    public class TelegrafsApi : AbstractInfluxDBClient
    {
        protected internal TelegrafsApi(DefaultClientIo client) : base(client)
        {
        }

        /// <summary>
        ///     Create a telegraf config.
        /// </summary>
        /// <param name="telegrafConfig">Telegraf Configuration to create</param>
        /// <returns>Telegraf config created</returns>
        public async Task<TelegrafConfig> CreateTelegrafConfig(TelegrafConfig telegrafConfig)
        {
            Arguments.CheckNotNull(telegrafConfig, nameof(telegrafConfig));

            var response = await Post(telegrafConfig, "/api/v2/telegrafs");

            return Call<TelegrafConfig>(response);
        }

        /// <summary>
        ///     Create a telegraf config.
        /// </summary>
        /// <param name="name">Telegraf Configuration Name</param>
        /// <param name="description">Telegraf Configuration Description</param>
        /// <param name="org">The organization that owns this config</param>
        /// <param name="collectionInterval">Default data collection interval for all inputs in milliseconds</param>
        /// <param name="plugins">The telegraf plugins config</param>
        /// <returns>Telegraf config created</returns>
        public async Task<TelegrafConfig> CreateTelegrafConfig(string name, string description, Organization org,
            int collectionInterval, IEnumerable<TelegrafPlugin> plugins)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNotNull(org, nameof(org));
            Arguments.CheckPositiveNumber(collectionInterval, nameof(collectionInterval));

            return await CreateTelegrafConfig(name, description, org.Id, collectionInterval, plugins);
        }

        /// <summary>
        ///     Create a telegraf config.
        /// </summary>
        /// <param name="name">Telegraf Configuration Name</param>
        /// <param name="description">Telegraf Configuration Description</param>
        /// <param name="orgId">The ID of the organization that owns this config</param>
        /// <param name="collectionInterval">Default data collection interval for all inputs in milliseconds</param>
        /// <param name="plugins">The telegraf plugins config</param>
        /// <returns>Telegraf config created</returns>
        public async Task<TelegrafConfig> CreateTelegrafConfig(string name, string description, string orgId,
            int collectionInterval, IEnumerable<TelegrafPlugin> plugins)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckPositiveNumber(collectionInterval, nameof(collectionInterval));

            var telegrafAgent = new TelegrafAgent {CollectionInterval = collectionInterval};

            var telegrafConfig = new TelegrafConfig
                {Name = name, Description = description, OrgId = orgId, Agent = telegrafAgent};
            telegrafConfig.Plugins.AddRange(plugins);

            return await CreateTelegrafConfig(telegrafConfig);
        }

        /// <summary>
        ///     Update a telegraf config.
        /// </summary>
        /// <param name="telegrafConfig">telegraf config update to apply</param>
        /// <returns>An updated telegraf</returns>
        public async Task<TelegrafConfig> UpdateTelegrafConfig(TelegrafConfig telegrafConfig)
        {
            Arguments.CheckNotNull(telegrafConfig, nameof(telegrafConfig));

            var result = await Put(telegrafConfig, $"/api/v2/telegrafs/{telegrafConfig.Id}");

            return Call<TelegrafConfig>(result);
        }

        /// <summary>
        ///     Delete a telegraf config.
        /// </summary>
        /// <param name="telegrafConfig">telegraf config to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteTelegrafConfig(TelegrafConfig telegrafConfig)
        {
            Arguments.CheckNotNull(telegrafConfig, nameof(telegrafConfig));

            await DeleteTelegrafConfig(telegrafConfig.Id);
        }

        /// <summary>
        ///     Delete a telegraf config.
        /// </summary>
        /// <param name="telegrafConfigId">ID of telegraf config to delete</param>
        /// <returns>async task</returns>
        public async Task DeleteTelegrafConfig(string telegrafConfigId)
        {
            Arguments.CheckNonEmptyString(telegrafConfigId, nameof(telegrafConfigId));

            var request = await Delete($"/api/v2/telegrafs/{telegrafConfigId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        ///     Clone a telegraf config.
        /// </summary>
        /// <param name="clonedName">name of cloned telegraf config</param>
        /// <param name="telegrafConfigId">ID of telegraf config to clone</param>
        /// <returns>cloned telegraf config</returns>
        public async Task<TelegrafConfig> CloneTelegrafConfig(string clonedName, string telegrafConfigId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(telegrafConfigId, nameof(telegrafConfigId));

            var telegrafConfig = await FindTelegrafConfigById(telegrafConfigId);
            if (telegrafConfig == null)
                throw new InvalidOperationException($"NotFound TelegrafConfig with ID: {telegrafConfigId}");

            return await CloneTelegrafConfig(clonedName, telegrafConfig);
        }

        /// <summary>
        ///     Clone a telegraf config.
        /// </summary>
        /// <param name="clonedName">name of cloned telegraf config</param>
        /// <param name="telegrafConfig">telegraf config to clone></param>
        /// <returns>cloned telegraf config</returns>
        public async Task<TelegrafConfig> CloneTelegrafConfig(string clonedName, TelegrafConfig telegrafConfig)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(telegrafConfig, nameof(telegrafConfig));

            var cloned = new TelegrafConfig
            {
                Name = clonedName,
                OrgId = telegrafConfig.OrgId,
                Description = telegrafConfig.Description,
                Agent = new TelegrafAgent {CollectionInterval = telegrafConfig.Agent.CollectionInterval}
            };
            cloned.Plugins.AddRange(telegrafConfig.Plugins);

            var created = await CreateTelegrafConfig(cloned);

            foreach (var label in await GetLabels(telegrafConfig)) await AddLabel(label, created);

            return created;
        }

        /// <summary>
        ///     Retrieve a telegraf config.
        /// </summary>
        /// <param name="telegrafConfigId">ID of telegraf config to get</param>
        /// <returns>telegraf config details</returns>
        public async Task<TelegrafConfig> FindTelegrafConfigById(string telegrafConfigId)
        {
            Arguments.CheckNonEmptyString(telegrafConfigId, nameof(telegrafConfigId));

            var request = await Get($"/api/v2/telegrafs/{telegrafConfigId}");

            return Call<TelegrafConfig>(request, 404);
        }

        /// <summary>
        ///     Returns a list of telegraf configs.
        /// </summary>
        /// <returns>A list of telegraf configs</returns>
        public async Task<List<TelegrafConfig>> FindTelegrafConfigs()
        {
            return await FindTelegrafConfigsByOrgId(null);
        }

        /// <summary>
        ///     Returns a list of telegraf configs for specified organization.
        /// </summary>
        /// <param name="organization">specifies the organization of the telegraf configs</param>
        /// <returns>A list of telegraf configs</returns>
        public async Task<List<TelegrafConfig>> FindTelegrafConfigsByOrg(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await FindTelegrafConfigsByOrgId(organization.Id);
        }

        /// <summary>
        ///     Returns a list of telegraf configs for specified organization.
        /// </summary>
        /// <param name="orgId">specifies the organization of the telegraf configs</param>
        /// <returns>A list of telegraf configs</returns>
        public async Task<List<TelegrafConfig>> FindTelegrafConfigsByOrgId(string orgId)
        {
            var request = await Get($"/api/v2/telegrafs?orgID={orgId}");

            var telegrafConfigs = Call<TelegrafConfigs>(request);

            return telegrafConfigs?.Configs;
        }

        /// <summary>
        ///     Retrieve a telegraf config in TOML.
        /// </summary>
        /// <param name="telegrafConfig">telegraf config to get</param>
        /// <returns>telegraf config details in TOML format</returns>
        public async Task<string> GetTOML(TelegrafConfig telegrafConfig)
        {
            Arguments.CheckNotNull(telegrafConfig, nameof(telegrafConfig));

            return await GetTOML(telegrafConfig.Id);
        }

        /// <summary>
        ///     Retrieve a telegraf config in TOML.
        /// </summary>
        /// <param name="telegrafConfigId">ID of telegraf config to get</param>
        /// <returns>telegraf config details in TOML format</returns>
        public async Task<string> GetTOML(string telegrafConfigId)
        {
            var request = new HttpRequestMessage(new HttpMethod(HttpMethodKind.Get.Name()),
                $"/api/v2/telegrafs/{telegrafConfigId}");

            request.Headers.Add("accept", "application/toml");

            var result = await Client.DoRequest(request).ConfigureAwait(false);

            RaiseForInfluxError(result);

            return new StreamReader(result.ResponseContent).ReadToEnd();
        }

        /// <summary>
        ///     List all users with member privileges for a telegraf config.
        /// </summary>
        /// <param name="telegrafConfig">the telegraf config</param>
        /// <returns>a list of telegraf config members</returns>
        public async Task<List<ResourceMember>> GetMembers(TelegrafConfig telegrafConfig)
        {
            Arguments.CheckNotNull(telegrafConfig, nameof(telegrafConfig));

            return await GetMembers(telegrafConfig.Id);
        }

        /// <summary>
        ///     List all users with member privileges for a telegraf config.
        /// </summary>
        /// <param name="telegrafConfigId">ID of the telegraf config</param>
        /// <returns>a list of telegraf config members</returns>
        public async Task<List<ResourceMember>> GetMembers(string telegrafConfigId)
        {
            Arguments.CheckNonEmptyString(telegrafConfigId, nameof(telegrafConfigId));

            var request = await Get($"/api/v2/telegrafs/{telegrafConfigId}/members");

            var response = Call<ResourceMembers>(request);

            return response?.Users;
        }

        /// <summary>
        ///     Add telegraf config member.
        /// </summary>
        /// <param name="member">user to add as member</param>
        /// <param name="telegrafConfig">the telegraf config</param>
        /// <returns>member added to telegraf</returns>
        public async Task<ResourceMember> AddMember(User member, TelegrafConfig telegrafConfig)
        {
            Arguments.CheckNotNull(telegrafConfig, nameof(telegrafConfig));
            Arguments.CheckNotNull(member, nameof(member));

            return await AddMember(member.Id, telegrafConfig.Id);
        }

        /// <summary>
        ///     Add telegraf config member.
        /// </summary>
        /// <param name="memberId">user ID to add as member</param>
        /// <param name="telegrafConfigId">ID of the telegraf config</param>
        /// <returns>member added to telegraf</returns>
        public async Task<ResourceMember> AddMember(string memberId, string telegrafConfigId)
        {
            Arguments.CheckNonEmptyString(telegrafConfigId, nameof(telegrafConfigId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            var user = new User {Id = memberId};

            var request = await Post(user, $"/api/v2/telegrafs/{telegrafConfigId}/members");

            return Call<ResourceMember>(request);
        }

        /// <summary>
        ///     Removes a member from a telegraf config.
        /// </summary>
        /// <param name="member">member to remove</param>
        /// <param name="telegrafConfig">the telegraf</param>
        /// <returns>async task</returns>
        public async Task DeleteMember(User member, TelegrafConfig telegrafConfig)
        {
            Arguments.CheckNotNull(telegrafConfig, nameof(telegrafConfig));
            Arguments.CheckNotNull(member, nameof(member));

            await DeleteMember(member.Id, telegrafConfig.Id);
        }

        /// <summary>
        ///     Removes a member from a telegraf config.
        /// </summary>
        /// <param name="memberId">ID of member to remove</param>
        /// <param name="telegrafConfigId">ID of the telegraf</param>
        /// <returns>async task</returns>
        public async Task DeleteMember(string memberId, string telegrafConfigId)
        {
            Arguments.CheckNonEmptyString(telegrafConfigId, nameof(telegrafConfigId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            var request = await Delete($"/api/v2/telegrafs/{telegrafConfigId}/members/{memberId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        ///     List all owners of a telegraf config.
        /// </summary>
        /// <param name="telegrafConfig">the telegraf config</param>
        /// <returns>a list of telegraf config owners</returns>
        public async Task<List<ResourceMember>> GetOwners(TelegrafConfig telegrafConfig)
        {
            Arguments.CheckNotNull(telegrafConfig, nameof(telegrafConfig));

            return await GetOwners(telegrafConfig.Id);
        }

        /// <summary>
        ///     List all owners of a telegraf config.
        /// </summary>
        /// <param name="telegrafConfigId">ID of the telegraf config</param>
        /// <returns>a list of telegraf config owners</returns>
        public async Task<List<ResourceMember>> GetOwners(string telegrafConfigId)
        {
            Arguments.CheckNonEmptyString(telegrafConfigId, nameof(telegrafConfigId));

            var request = await Get($"/api/v2/telegrafs/{telegrafConfigId}/owners");

            var response = Call<ResourceMembers>(request);

            return response?.Users;
        }

        /// <summary>
        ///     Add telegraf config owner.
        /// </summary>
        /// <param name="owner">user to add as owner</param>
        /// <param name="telegrafConfig">the telegraf config</param>
        /// <returns>telegraf config owner added</returns>
        public async Task<ResourceMember> AddOwner(User owner, TelegrafConfig telegrafConfig)
        {
            Arguments.CheckNotNull(telegrafConfig, nameof(telegrafConfig));
            Arguments.CheckNotNull(owner, nameof(owner));

            return await AddOwner(owner.Id, telegrafConfig.Id);
        }

        /// <summary>
        ///     Add telegraf config owner.
        /// </summary>
        /// <param name="ownerId">ID of user to add as owner</param>
        /// <param name="telegrafConfigId"> ID of the telegraf config</param>
        /// <returns>telegraf config owner added</returns>
        public async Task<ResourceMember> AddOwner(string ownerId, string telegrafConfigId)
        {
            Arguments.CheckNonEmptyString(telegrafConfigId, nameof(telegrafConfigId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var user = new User {Id = ownerId};

            var request = await Post(user, $"/api/v2/telegrafs/{telegrafConfigId}/owners");

            return Call<ResourceMember>(request);
        }

        /// <summary>
        ///     Removes an owner from a telegraf config.
        /// </summary>
        /// <param name="owner">owner to remove</param>
        /// <param name="telegrafConfig">the telegraf config</param>
        /// <returns>async task</returns>
        public async Task DeleteOwner(User owner, TelegrafConfig telegrafConfig)
        {
            Arguments.CheckNotNull(telegrafConfig, nameof(telegrafConfig));
            Arguments.CheckNotNull(owner, nameof(owner));

            await DeleteOwner(owner.Id, telegrafConfig.Id);
        }

        /// <summary>
        ///     Removes an owner from a telegraf config.
        /// </summary>
        /// <param name="ownerId">ID of owner to remove</param>
        /// <param name="telegrafConfigId">ID of the telegraf config</param>
        /// <returns>async task</returns>
        public async Task DeleteOwner(string ownerId, string telegrafConfigId)
        {
            Arguments.CheckNonEmptyString(telegrafConfigId, nameof(telegrafConfigId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var request = await Delete($"/api/v2/telegrafs/{telegrafConfigId}/owners/{ownerId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        ///     List all labels for a telegraf config.
        /// </summary>
        /// <param name="telegrafConfig">the telegraf config</param>
        /// <returns>a list of all labels for a telegraf config</returns>
        public async Task<List<Label>> GetLabels(TelegrafConfig telegrafConfig)
        {
            Arguments.CheckNotNull(telegrafConfig, nameof(telegrafConfig));

            return await GetLabels(telegrafConfig.Id);
        }

        /// <summary>
        ///     List all labels for a telegraf config.
        /// </summary>
        /// <param name="telegrafConfigId">ID of the telegraf config</param>
        /// <returns>a list of all labels for a telegraf config</returns>
        public async Task<List<Label>> GetLabels(string telegrafConfigId)
        {
            Arguments.CheckNonEmptyString(telegrafConfigId, nameof(telegrafConfigId));

            return await GetLabels(telegrafConfigId, "telegrafs");
        }

        /// <summary>
        ///     Add a label to a telegraf config.
        /// </summary>
        /// <param name="label">label to add</param>
        /// <param name="telegrafConfig">the telegraf config</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabel(Label label, TelegrafConfig telegrafConfig)
        {
            Arguments.CheckNotNull(telegrafConfig, nameof(telegrafConfig));
            Arguments.CheckNotNull(label, nameof(label));

            return await AddLabel(label.Id, telegrafConfig.Id);
        }

        /// <summary>
        ///     Add a label to a telegraf config.
        /// </summary>
        /// <param name="labelId">ID of label to add</param>
        /// <param name="telegrafConfigId">ID of the telegraf config</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabel(string labelId, string telegrafConfigId)
        {
            Arguments.CheckNonEmptyString(telegrafConfigId, nameof(telegrafConfigId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return await AddLabel(labelId, telegrafConfigId, "telegrafs", ResourceType.Telegrafs);
        }

        /// <summary>
        ///     Delete a label from a telegraf config.
        /// </summary>
        /// <param name="label">label to delete</param>
        /// <param name="telegrafConfig">the telegraf config</param>
        /// <returns>async task</returns>
        public async Task DeleteLabel(Label label, TelegrafConfig telegrafConfig)
        {
            Arguments.CheckNotNull(telegrafConfig, nameof(telegrafConfig));
            Arguments.CheckNotNull(label, nameof(label));

            await DeleteLabel(label.Id, telegrafConfig.Id);
        }

        /// <summary>
        ///     Delete a label from a telegraf config.
        /// </summary>
        /// <param name="labelId">ID of label to delete</param>
        /// <param name="telegrafConfigId">ID of the telegraf config</param>
        /// <returns>async task</returns>
        public async Task DeleteLabel(string labelId, string telegrafConfigId)
        {
            Arguments.CheckNonEmptyString(telegrafConfigId, nameof(telegrafConfigId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            await DeleteLabel(labelId, telegrafConfigId, "telegrafs");
        }
    }
}