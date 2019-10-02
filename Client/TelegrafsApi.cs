using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;
using Task = System.Threading.Tasks.Task;

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
        /// <param name="telegrafRequest">Telegraf Configuration to create</param>
        /// <returns>Telegraf config created</returns>
        public async Task<Telegraf> CreateTelegrafAsync(TelegrafRequest telegrafRequest)
        {
            Arguments.CheckNotNull(telegrafRequest, nameof(telegrafRequest));

            return await _service.PostTelegrafsAsync(telegrafRequest);
        }

        /// <summary>
        /// Create a telegraf config.
        /// </summary>
        /// <param name="name">Telegraf Configuration Name</param>
        /// <param name="description">Telegraf Configuration Description</param>
        /// <param name="org">The organization that owns this config</param>
        /// <param name="collectionInterval">Default data collection interval for all inputs in milliseconds</param>
        /// <param name="plugins">The telegraf plugins config</param>
        /// <returns>Telegraf config created</returns>
        public async Task<Telegraf> CreateTelegrafAsync(string name, string description, Organization org,
            int collectionInterval, List<TelegrafRequestPlugin> plugins)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNotNull(org, nameof(org));
            Arguments.CheckPositiveNumber(collectionInterval, nameof(collectionInterval));

            return await CreateTelegrafAsync(name, description, org.Id, collectionInterval, plugins);
        }

        /// <summary>
        /// Create a telegraf config.
        /// </summary>
        /// <param name="name">Telegraf Configuration Name</param>
        /// <param name="description">Telegraf Configuration Description</param>
        /// <param name="orgId">The ID of the organization that owns this config</param>
        /// <param name="collectionInterval">Default data collection interval for all inputs in milliseconds</param>
        /// <param name="plugins">The telegraf plugins config</param>
        /// <returns>Telegraf config created</returns>
        public async Task<Telegraf> CreateTelegrafAsync(string name, string description, string orgId,
            int collectionInterval, List<TelegrafRequestPlugin> plugins)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckPositiveNumber(collectionInterval, nameof(collectionInterval));

            var request = new TelegrafRequest(name, description, new TelegrafRequestAgent(collectionInterval), plugins,
                orgId);

            return await CreateTelegrafAsync(request);
        }

        /// <summary>
        /// Update a telegraf config.
        /// </summary>
        /// <param name="telegraf">telegraf config update to apply</param>
        /// <returns>An updated telegraf</returns>
        public async Task<Telegraf> UpdateTelegrafAsync(Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            var request = new TelegrafRequest(telegraf.Name, telegraf.Description, telegraf.Agent, telegraf.Plugins,
                telegraf.OrgID);

            return await UpdateTelegrafAsync(telegraf.Id, request);
        }

        /// <summary>
        /// Update a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of telegraf config</param>
        /// <param name="telegrafRequest">telegraf config update to apply</param>
        /// <returns>An updated telegraf</returns>
        public async Task<Telegraf> UpdateTelegrafAsync(string telegrafId, TelegrafRequest telegrafRequest)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNotNull(telegrafRequest, nameof(telegrafRequest));

            return await _service.PutTelegrafsIDAsync(telegrafId, telegrafRequest);
        }

        /// <summary>
        /// Delete a telegraf config.
        /// </summary>
        /// <param name="telegraf">telegraf config to delete</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteTelegrafAsync(Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            await DeleteTelegrafAsync(telegraf.Id);
        }

        /// <summary>
        /// Delete a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of telegraf config to delete</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteTelegrafAsync(string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            await _service.DeleteTelegrafsIDAsync(telegrafId);
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

            return await FindTelegrafByIdAsync(telegrafId).ContinueWith(t => CloneTelegrafAsync(clonedName, t.Result)).Unwrap();
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

            var cloned = new TelegrafRequest(clonedName, telegraf.Description, telegraf.Agent, telegraf.Plugins,
                telegraf.OrgID);

            return await CreateTelegrafAsync(cloned).ContinueWith(created =>
            {
                //
                // Add labels
                //
                return GetLabelsAsync(telegraf)
                    .ContinueWith(labels => { return labels.Result.Select(label => AddLabelAsync(label, created.Result)); })
                    .ContinueWith(async tasks =>
                    {
                        await Task.WhenAll(tasks.Result);
                        return created.Result;
                    })
                    .Unwrap();
            }).Unwrap();
        }

        /// <summary>
        /// Retrieve a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of telegraf config to get</param>
        /// <returns>telegraf config details</returns>
        public async Task<Telegraf> FindTelegrafByIdAsync(string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            return await _service.GetTelegrafsIDWithIRestResponseAsync(telegrafId, null, "application/json")
                .ContinueWith(t => (Telegraf) _service.Configuration.ApiClient.Deserialize(t.Result, typeof(Telegraf)));
        }

        /// <summary>
        /// Returns a list of telegraf configs.
        /// </summary>
        /// <returns>A list of telegraf configs</returns>
        public async Task<List<Telegraf>> FindTelegrafsAsync()
        {
            return await FindTelegrafsByOrgIdAsync(null);
        }

        /// <summary>
        /// Returns a list of telegraf configs for specified organization.
        /// </summary>
        /// <param name="organization">specifies the organization of the telegraf configs</param>
        /// <returns>A list of telegraf configs</returns>
        public async Task<List<Telegraf>> FindTelegrafsByOrgAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await FindTelegrafsByOrgIdAsync(organization.Id);
        }

        /// <summary>
        /// Returns a list of telegraf configs for specified organization.
        /// </summary>
        /// <param name="orgId">specifies the organization of the telegraf configs</param>
        /// <returns>A list of telegraf configs</returns>
        public async Task<List<Telegraf>> FindTelegrafsByOrgIdAsync(string orgId)
        {
            return await _service.GetTelegrafsAsync(orgId).ContinueWith(t => t.Result.Configurations);
        }

        /// <summary>
        /// Retrieve a telegraf config in TOML.
        /// </summary>
        /// <param name="telegraf">telegraf config to get</param>
        /// <returns>telegraf config details in TOML format</returns>
        public async Task<string> GetTOMLAsync(Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            return await GetTOMLAsync(telegraf.Id);
        }

        /// <summary>
        /// Retrieve a telegraf config in TOML.
        /// </summary>
        /// <param name="telegrafId">ID of telegraf config to get</param>
        /// <returns>telegraf config details in TOML format</returns>
        public async Task<string> GetTOMLAsync(string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            return await _service.GetTelegrafsIDstringAsync(telegrafId, null, "application/toml");
        }

        /// <summary>
        /// List all users with member privileges for a telegraf config.
        /// </summary>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>a list of telegraf config members</returns>
        public async Task<List<ResourceMember>> GetMembersAsync(Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            return await GetMembersAsync(telegraf.Id);
        }

        /// <summary>
        /// List all users with member privileges for a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <returns>a list of telegraf config members</returns>
        public async Task<List<ResourceMember>> GetMembersAsync(string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            return await _service.GetTelegrafsIDMembersAsync(telegrafId).ContinueWith(t => t.Result.Users);
        }

        /// <summary>
        /// Add telegraf config member.
        /// </summary>
        /// <param name="member">user to add as member</param>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>member added to telegraf</returns>
        public async Task<ResourceMember> AddMemberAsync(User member, Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(member, nameof(member));

            return await AddMemberAsync(member.Id, telegraf.Id);
        }

        /// <summary>
        /// Add telegraf config member.
        /// </summary>
        /// <param name="memberId">user ID to add as member</param>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <returns>member added to telegraf</returns>
        public async Task<ResourceMember> AddMemberAsync(string memberId, string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return await _service.PostTelegrafsIDMembersAsync(telegrafId, new AddResourceMemberRequestBody(memberId));
        }

        /// <summary>
        /// Removes a member from a telegraf config.
        /// </summary>
        /// <param name="member">member to remove</param>
        /// <param name="telegraf">the telegraf</param>
        /// <returns>member removed</returns>
        public async Task DeleteMemberAsync(User member, Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(member, nameof(member));

            await DeleteMemberAsync(member.Id, telegraf.Id);
        }

        /// <summary>
        /// Removes a member from a telegraf config.
        /// </summary>
        /// <param name="memberId">ID of member to remove</param>
        /// <param name="telegrafId">ID of the telegraf</param>
        /// <returns>member removed</returns>
        public async Task DeleteMemberAsync(string memberId, string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            await _service.DeleteTelegrafsIDMembersIDAsync(memberId, telegrafId);
        }

        /// <summary>
        /// List all owners of a telegraf config.
        /// </summary>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>a list of telegraf config owners</returns>
        public async Task<List<ResourceOwner>> GetOwnersAsync(Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            return await GetOwnersAsync(telegraf.Id);
        }

        /// <summary>
        /// List all owners of a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <returns>a list of telegraf config owners</returns>
        public async Task<List<ResourceOwner>> GetOwnersAsync(string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            return await _service.GetTelegrafsIDOwnersAsync(telegrafId).ContinueWith(t => t.Result.Users);
        }

        /// <summary>
        /// Add telegraf config owner.
        /// </summary>
        /// <param name="owner">user to add as owner</param>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>telegraf config owner added</returns>
        public async Task<ResourceOwner> AddOwnerAsync(User owner, Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(owner, nameof(owner));

            return await AddOwnerAsync(owner.Id, telegraf.Id);
        }

        /// <summary>
        /// Add telegraf config owner.
        /// </summary>
        /// <param name="ownerId">ID of user to add as owner</param>
        /// <param name="telegrafId"> ID of the telegraf config</param>
        /// <returns>telegraf config owner added</returns>
        public async Task<ResourceOwner> AddOwnerAsync(string ownerId, string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            return await _service.PostTelegrafsIDOwnersAsync(telegrafId, new AddResourceMemberRequestBody(ownerId));
        }

        /// <summary>
        /// Removes an owner from a telegraf config.
        /// </summary>
        /// <param name="owner">owner to remove</param>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>owner removed</returns>
        public async Task DeleteOwnerAsync(User owner, Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(owner, nameof(owner));

            await DeleteOwnerAsync(owner.Id, telegraf.Id);
        }

        /// <summary>
        /// Removes an owner from a telegraf config.
        /// </summary>
        /// <param name="ownerId">ID of owner to remove</param>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <returns>owner removed</returns>
        public async Task DeleteOwnerAsync(string ownerId, string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            await _service.DeleteTelegrafsIDOwnersIDAsync(ownerId, telegrafId);
        }

        /// <summary>
        /// List all labels for a telegraf config.
        /// </summary>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>a list of all labels for a telegraf config</returns>
        public async Task<List<Label>> GetLabelsAsync(Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));

            return await GetLabelsAsync(telegraf.Id);
        }

        /// <summary>
        /// List all labels for a telegraf config.
        /// </summary>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <returns>a list of all labels for a telegraf config</returns>
        public async Task<List<Label>> GetLabelsAsync(string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));

            return await _service.GetTelegrafsIDLabelsAsync(telegrafId).ContinueWith(t => t.Result.Labels);
        }

        /// <summary>
        /// Add a label to a telegraf config.
        /// </summary>
        /// <param name="label">label to add</param>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabelAsync(Label label, Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(label, nameof(label));

            return await AddLabelAsync(label.Id, telegraf.Id);
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

            return await _service.PostTelegrafsIDLabelsAsync(telegrafId, new LabelMapping(labelId)).ContinueWith(t => t.Result.Label);
        }

        /// <summary>
        /// Delete a label from a telegraf config.
        /// </summary>
        /// <param name="label">label to delete</param>
        /// <param name="telegraf">the telegraf config</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteLabelAsync(Label label, Telegraf telegraf)
        {
            Arguments.CheckNotNull(telegraf, nameof(telegraf));
            Arguments.CheckNotNull(label, nameof(label));

            await DeleteLabelAsync(label.Id, telegraf.Id);
        }

        /// <summary>
        /// Delete a label from a telegraf config.
        /// </summary>
        /// <param name="labelId">ID of label to delete</param>
        /// <param name="telegrafId">ID of the telegraf config</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteLabelAsync(string labelId, string telegrafId)
        {
            Arguments.CheckNonEmptyString(telegrafId, nameof(telegrafId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            await _service.DeleteTelegrafsIDLabelsIDAsync(telegrafId, labelId);
        }
    }
}