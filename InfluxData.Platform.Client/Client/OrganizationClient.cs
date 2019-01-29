using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;
using Task = System.Threading.Tasks.Task;

namespace InfluxData.Platform.Client.Client
{
    public class OrganizationClient : AbstractPlatformClient
    {
        protected internal OrganizationClient(DefaultClientIo client) : base(client)
        {
        }

        /// <summary>
        /// Creates a new organization and sets <see cref="Organization.Id"/> with the new identifier.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Created organization</returns>
        public async Task<Organization> CreateOrganization(string name)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));

            var organization = new Organization {Name = name};

            return await CreateOrganization(organization);
        }

        /// <summary>
        /// Creates a new organization and sets <see cref="Organization.Id"/> with the new identifier.
        /// </summary>
        /// <param name="organization">the organization to create</param>
        /// <returns>created organization</returns>
        public async Task<Organization> CreateOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            var request = await Post(organization, "/api/v2/orgs");

            return Call<Organization>(request);
        }

        /// <summary>
        /// Update a organization.
        /// </summary>
        /// <param name="organization">organization update to apply</param>
        /// <returns>updated organization</returns>
        public async Task<Organization> UpdateOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            var request = await Patch(organization, $"/api/v2/orgs/{organization.Id}");

            return Call<Organization>(request);
        }

        /// <summary>
        /// Delete a organization.
        /// </summary>
        /// <param name="orgId">ID of organization to delete</param>
        /// <returns></returns>
        public async Task DeleteOrganization(string orgId)
        {
            Arguments.CheckNotNull(orgId, nameof(orgId));

            var request = await Delete($"/api/v2/orgs/{orgId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        /// Delete a organization.
        /// </summary>
        /// <param name="organization">organization to delete</param>
        /// <returns></returns>
        public async Task DeleteOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            await DeleteOrganization(organization.Id);
        }

        /// <summary>
        /// Retrieve a organization.
        /// </summary>
        /// <param name="orgId">ID of organization to get</param>
        /// <returns>organization details</returns>
        public async Task<Organization> FindOrganizationById(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var request = await Get($"/api/v2/orgs/{orgId}");

            return Call<Organization>(request, "organization not found");
        }

        /// <summary>
        /// List all organizations.
        /// </summary>
        /// <returns>List all organizations</returns>
        public async Task<List<Organization>> FindOrganizations()
        {
            var request = await Get("/api/v2/orgs");

            var organizations = Call<Organizations>(request);

            return organizations?.Orgs;
        }

        /// <summary>
        /// List of secret keys the are stored for Organization. For example:
        ///
        /// <code>
        ///     github_api_key,
        ///     some_other_key,
        ///     a_secret_key
        /// </code>
        /// </summary>
        /// <param name="organization">the organization for get secrets</param>
        /// <returns>the secret keys</returns>
        public async Task<List<string>> GetSecrets(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await GetSecrets(organization.Id);
        }

        /// <summary>
        /// List of secret keys the are stored for Organization. For example:
        ///
        /// <code>
        ///     github_api_key,
        ///     some_other_key,
        ///     a_secret_key
        /// </code>
        /// </summary>
        /// <param name="orgId">the organization for get secrets</param>
        /// <returns>the secret keys</returns>
        public async Task<List<string>> GetSecrets(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var request = await Get($"/api/v2/orgs/{orgId}/secrets");

            var response = Call<Secrets>(request);

            return response?.SecretList;
        }

        /// <summary>
        /// Patches all provided secrets and updates any previous values.
        /// </summary>
        /// <param name="secrets">secrets to update/add</param>
        /// <param name="organization">the organization for put secrets</param>
        /// <returns>async task</returns>
        public async Task PutSecrets(Dictionary<string, string> secrets, Organization organization)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNotNull(organization, nameof(organization));

            await PutSecrets(secrets, organization.Id);
        }

        /// <summary>
        /// Patches all provided secrets and updates any previous values.
        /// </summary>
        /// <param name="secrets">secrets to update/add</param>
        /// <param name="orgId">the organization for put secrets</param>
        /// <returns>async task</returns>
        public async Task PutSecrets(Dictionary<string, string> secrets, string orgId)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var request = await Patch(secrets, $"/api/v2/orgs/{orgId}/secrets");

            RaiseForInfluxError(request);
        }

        /// <summary>
        /// Delete provided secrets.
        /// </summary>
        /// <param name="secrets">secrets to delete</param>
        /// <param name="organization">the organization for delete secrets</param>
        /// <returns>async task</returns>
        public async Task DeleteSecrets(List<string> secrets, Organization organization)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNotNull(organization, nameof(organization));

            await DeleteSecrets(secrets, organization.Id);
        }

        /// <summary>
        /// Delete provided secrets.
        /// </summary>
        /// <param name="secrets">secrets to delete</param>
        /// <param name="orgId">the organization for delete secrets</param>
        /// <returns>async task</returns>
        public async Task DeleteSecrets(List<string> secrets, string orgId)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var request = await Post(secrets, $"/api/v2/orgs/{orgId}/secrets/delete");

            RaiseForInfluxError(request);
        }

        /// <summary>
        /// List all members of an organization.
        /// </summary>
        /// <param name="organization">organization of the members</param>
        /// <returns>the List all members of an organization</returns>
        public async Task<List<ResourceMember>> GetMembers(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await GetMembers(organization.Id);
        }

        /// <summary>
        /// List all members of an organization.
        /// </summary>
        /// <param name="orgId">ID of organization to get members</param>
        /// <returns>the List all members of an organization</returns>
        public async Task<List<ResourceMember>> GetMembers(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var request = await Get($"/api/v2/orgs/{orgId}/members");

            var response = Call<ResourceMembers>(request);

            return response?.Users;
        }

        /// <summary>
        /// Add organization member.
        /// </summary>
        /// <param name="member">the member of an organization</param>
        /// <param name="organization">the organization of a member</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddMember(User member, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(member, nameof(member));

            return await AddMember(member.Id, organization.Id);
        }

        /// <summary>
        /// Add organization member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddMember(string memberId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            var user = new User {Id = memberId};

            var request = await Post(user, $"/api/v2/orgs/{orgId}/members");

            return Call<ResourceMember>(request);
        }

        /// <summary>
        /// Removes a member from an organization.
        /// </summary>
        /// <param name="member">the member of an organization</param>
        /// <param name="organization">the organization of a member</param>
        /// <returns>async task</returns>
        public async Task DeleteMember(User member, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(member, nameof(member));

            await DeleteMember(member.Id, organization.Id);
        }

        /// <summary>
        /// Removes a member from an organization.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns>async task</returns>
        public async Task DeleteMember(string memberId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            var request = await Delete($"/api/v2/orgs/{orgId}/members/{memberId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        /// List all owners of an organization.
        /// </summary>
        /// <param name="organization">organization of the owners</param>
        /// <returns>the List all owners of an organization</returns>
        public async Task<List<ResourceMember>> GetOwners(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await GetOwners(organization.Id);
        }

        /// <summary>
        /// List all owners of an organization.
        /// </summary>
        /// <param name="orgId">ID of organization to get owners</param>
        /// <returns>the List all owners of an organization</returns>
        public async Task<List<ResourceMember>> GetOwners(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var request = await Get($"/api/v2/orgs/{orgId}/owners");

            var response = Call<ResourceMembers>(request);

            return response?.Users;
        }

        /// <summary>
        /// Add organization owner.
        /// </summary>
        /// <param name="owner">the owner of an organization</param>
        /// <param name="organization">the organization of a owner</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddOwner(User owner, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(owner, nameof(owner));

            return await AddOwner(owner.Id, organization.Id);
        }

        /// <summary>
        /// Add organization owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddOwner(string ownerId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var user = new User {Id = ownerId};

            var request = await Post(user, $"/api/v2/orgs/{orgId}/owners");

            return Call<ResourceMember>(request);
        }

        /// <summary>
        /// Removes a owner from an organization.
        /// </summary>
        /// <param name="owner">the owner of an organization</param>
        /// <param name="organization">the organization of a owner</param>
        /// <returns>async task</returns>
        public async Task DeleteOwner(User owner, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(owner, nameof(owner));

            await DeleteOwner(owner.Id, organization.Id);
        }

        /// <summary>
        /// Removes a owner from an organization.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns>async task</returns>
        public async Task DeleteOwner(string ownerId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            var request = await Delete($"/api/v2/orgs/{orgId}/owners/{ownerId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        /// List all labels of an organization.
        /// </summary>
        /// <param name="organization">organization of the labels</param>
        /// <returns>the List all labels of an organization</returns>
        public async Task<List<Label>> GetLabels(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await GetLabels(organization.Id);
        }

        /// <summary>
        /// List all labels of an organization.
        /// </summary>
        /// <param name="orgId">ID of an organization to get labels</param>
        /// <returns>the List all labels of an organization</returns>
        public async Task<List<Label>> GetLabels(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return await GetLabels(orgId, "orgs");
        }

        /// <summary>
        /// Add an organization label.
        /// </summary>
        /// <param name="label">the label of an organization</param>
        /// <param name="organization">an organization of a label</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabel(Label label, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(label, nameof(label));

            return await AddLabel(label.Id, organization.Id);
        }

        /// <summary>
        /// Add an organization label.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns>added label</returns>
        public async Task<Label> AddLabel(string labelId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            return await AddLabel(labelId, orgId, "orgs", ResourceType.Orgs);
        }

        /// <summary>
        /// Removes a label from an organization.
        /// </summary>
        /// <param name="label">the label of an organization</param>
        /// <param name="organization">an organization of a owner</param>
        /// <returns>async task</returns>
        public async Task DeleteLabel(Label label, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(label, nameof(label));

            await DeleteLabel(label.Id, organization.Id);
        }

        /// <summary>
        /// Removes a label from an organization.
        /// </summary>
        /// <param name="labelId">the ID of a label</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns>async task</returns>
        public async Task DeleteLabel(string labelId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(labelId, nameof(labelId));

            await DeleteLabel(labelId, orgId, "orgs");
        }
    }
}