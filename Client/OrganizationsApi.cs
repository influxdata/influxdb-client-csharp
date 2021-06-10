using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Api.Service;
using InfluxDB.Client.Core;

namespace InfluxDB.Client
{
    public class OrganizationsApi
    {
        private readonly OrganizationsService _service;

        protected internal OrganizationsApi(OrganizationsService service)
        {
            Arguments.CheckNotNull(service, nameof(service));

            _service = service;
        }

        /// <summary>
        /// Creates a new organization and sets <see cref="InfluxDB.Client.Api.Domain.Organization.Id" /> with the new identifier.
        /// </summary>
        /// <param name="name"></param>
        /// <returns>Created organization</returns>
        public Task<Organization> CreateOrganizationAsync(string name)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));

            var organization = new Organization(null, name);

            return CreateOrganizationAsync(organization);
        }

        /// <summary>
        /// Creates a new organization and sets <see cref="Organization.Id" /> with the new identifier.
        /// </summary>
        /// <param name="organization">the organization to create</param>
        /// <returns>created organization</returns>
        public Task<Organization> CreateOrganizationAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            var request = new PostOrganizationRequest(organization.Name, organization.Description);
            return _service.PostOrgsAsync(request);
        }

        /// <summary>
        /// Update an organization.
        /// </summary>
        /// <param name="organization">organization update to apply</param>
        /// <returns>updated organization</returns>
        public Task<Organization> UpdateOrganizationAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            var request = new PatchOrganizationRequest(organization.Name, organization.Description);
            return _service.PatchOrgsIDAsync(organization.Id, request);
        }

        /// <summary>
        /// Delete an organization.
        /// </summary>
        /// <param name="orgId">ID of organization to delete</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteOrganizationAsync(string orgId)
        {
            Arguments.CheckNotNull(orgId, nameof(orgId));

            return _service.DeleteOrgsIDAsync(orgId);
        }

        /// <summary>
        /// Delete an organization.
        /// </summary>
        /// <param name="organization">organization to delete</param>
        /// <returns>delete has been accepted</returns>
        public Task DeleteOrganizationAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return DeleteOrganizationAsync(organization.Id);
        }

        /// <summary>
        /// Clone an organization.
        /// </summary>
        /// <param name="clonedName">name of cloned organization</param>
        /// <param name="orgId">ID of organization to clone</param>
        /// <returns>cloned organization</returns>
        public async Task<Organization> CloneOrganizationAsync(string clonedName, string orgId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var org = await FindOrganizationByIdAsync(orgId).ConfigureAwait(false);
            return await CloneOrganizationAsync(clonedName, org).ConfigureAwait(false);
        }

        /// <summary>
        /// Clone an organization.
        /// </summary>
        /// <param name="clonedName">name of cloned organization</param>
        /// <param name="organization">organization to clone</param>
        /// <returns>cloned organization</returns>
        public Task<Organization> CloneOrganizationAsync(string clonedName, Organization organization)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(organization, nameof(organization));

            var cloned = new Organization(null, clonedName);

            return CreateOrganizationAsync(cloned);
        }

        /// <summary>
        /// Retrieve an organization.
        /// </summary>
        /// <param name="orgId">ID of organization to get</param>
        /// <returns>organization details</returns>
        public Task<Organization> FindOrganizationByIdAsync(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return _service.GetOrgsIDAsync(orgId);
        }

        /// <summary>
        /// List all organizations.
        /// </summary>
        /// <param name="limit"> (optional, default to 20)</param>
        /// <param name="offset"> (optional)</param>
        /// <param name="descending"> (optional, default to false)</param>
        /// <param name="org">Filter organizations to a specific organization name. (optional)</param>
        /// <param name="orgID">Filter organizations to a specific organization ID. (optional)</param>
        /// <param name="userID">Filter organizations to a specific user ID. (optional)</param>
        /// <returns>List all organizations</returns>
        public async Task<List<Organization>> FindOrganizationsAsync(int? limit = null, int? offset = null,
            bool? descending = null, string org = null, string orgID = null, string userID = null)
        {
            var response = await _service.GetOrgsAsync(limit: limit, offset: offset, descending: descending, org: org,
                orgID: orgID, userID: userID).ConfigureAwait(false);

            return response.Orgs;
        }

        /// <summary>
        /// List of secret keys the are stored for Organization. For example:
        /// <code>
        /// github_api_key,
        /// some_other_key,
        /// a_secret_key
        /// </code>
        /// </summary>
        /// <param name="organization">the organization for get secrets</param>
        /// <returns>the secret keys</returns>
        public Task<List<string>> GetSecretsAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return GetSecretsAsync(organization.Id);
        }

        /// <summary>
        /// List of secret keys the are stored for Organization. For example:
        /// <code>
        /// github_api_key,
        /// some_other_key,
        /// a_secret_key
        /// </code>
        /// </summary>
        /// <param name="orgId">the organization for get secrets</param>
        /// <returns>the secret keys</returns>
        public async Task<List<string>> GetSecretsAsync(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var response = await _service.GetOrgsIDSecretsAsync(orgId).ConfigureAwait(false);
            return response.Secrets;
        }

        /// <summary>
        /// Patches all provided secrets and updates any previous values.
        /// </summary>
        /// <param name="secrets">secrets to update/add</param>
        /// <param name="organization">the organization for put secrets</param>
        /// <returns></returns>
        public Task PutSecretsAsync(Dictionary<string, string> secrets, Organization organization)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNotNull(organization, nameof(organization));

            return PutSecretsAsync(secrets, organization.Id);
        }

        /// <summary>
        /// Patches all provided secrets and updates any previous values.
        /// </summary>
        /// <param name="secrets">secrets to update/add</param>
        /// <param name="orgId">the organization for put secrets</param>
        /// <returns></returns>
        public Task PutSecretsAsync(Dictionary<string, string> secrets, string orgId)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return _service.PatchOrgsIDSecretsAsync(orgId, secrets);
        }

        /// <summary>
        /// Delete provided secrets.
        /// </summary>
        /// <param name="secrets">secrets to delete</param>
        /// <param name="organization">the organization for delete secrets</param>
        /// <returns>keys successfully patched</returns>
        public Task DeleteSecretsAsync(List<string> secrets, Organization organization)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNotNull(organization, nameof(organization));

            return DeleteSecretsAsync(secrets, organization.Id);
        }

        /// <summary>
        /// Delete provided secrets.
        /// </summary>
        /// <param name="secrets">secrets to delete</param>
        /// <param name="orgId">the organization for delete secrets</param>
        /// <returns>keys successfully patched</returns>
        public Task DeleteSecretsAsync(List<string> secrets, string orgId)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return DeleteSecretsAsync(new SecretKeys(secrets), orgId);
        }
        
        /// <summary>
        /// Delete provided secrets.
        /// </summary>
        /// <param name="secrets">secrets to delete</param>
        /// <param name="orgId">the organization for delete secrets</param>
        /// <returns>keys successfully patched</returns>
        public Task DeleteSecretsAsync(SecretKeys secrets, string orgId)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return _service.PostOrgsIDSecretsAsync(orgId, secrets);
        }

        /// <summary>
        /// List all members of an organization.
        /// </summary>
        /// <param name="organization">organization of the members</param>
        /// <returns>the List all members of an organization</returns>
        public Task<List<ResourceMember>> GetMembersAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return GetMembersAsync(organization.Id);
        }

        /// <summary>
        /// List all members of an organization.
        /// </summary>
        /// <param name="orgId">ID of organization to get members</param>
        /// <returns>the List all members of an organization</returns>
        public async Task<List<ResourceMember>> GetMembersAsync(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var response = await _service.GetOrgsIDMembersAsync(orgId).ConfigureAwait(false);
            return response.Users;
        }

        /// <summary>
        /// Add organization member.
        /// </summary>
        /// <param name="member">the member of an organization</param>
        /// <param name="organization">the organization of a member</param>
        /// <returns>created mapping</returns>
        public Task<ResourceMember> AddMemberAsync(User member, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(member, nameof(member));

            return AddMemberAsync(member.Id, organization.Id);
        }

        /// <summary>
        /// Add organization member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns>created mapping</returns>
        public Task<ResourceMember> AddMemberAsync(string memberId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return _service.PostOrgsIDMembersAsync(orgId, new AddResourceMemberRequestBody(memberId));
        }

        /// <summary>
        /// Removes a member from an organization.
        /// </summary>
        /// <param name="member">the member of an organization</param>
        /// <param name="organization">the organization of a member</param>
        /// <returns></returns>
        public Task DeleteMemberAsync(User member, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(member, nameof(member));

            return DeleteMemberAsync(member.Id, organization.Id);
        }

        /// <summary>
        /// Removes a member from an organization.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns></returns>
        public Task DeleteMemberAsync(string memberId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return _service.DeleteOrgsIDMembersIDAsync(memberId, orgId);
        }

        /// <summary>
        /// List all owners of an organization.
        /// </summary>
        /// <param name="organization">organization of the owners</param>
        /// <returns>the List all owners of an organization</returns>
        public Task<List<ResourceOwner>> GetOwnersAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return GetOwnersAsync(organization.Id);
        }

        /// <summary>
        /// List all owners of an organization.
        /// </summary>
        /// <param name="orgId">ID of organization to get owners</param>
        /// <returns>the List all owners of an organization</returns>
        public async Task<List<ResourceOwner>> GetOwnersAsync(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            var response = await _service.GetOrgsIDOwnersAsync(orgId).ConfigureAwait(false);
            return response.Users;
        }

        /// <summary>
        /// Add organization owner.
        /// </summary>
        /// <param name="owner">the owner of an organization</param>
        /// <param name="organization">the organization of a owner</param>
        /// <returns>created mapping</returns>
        public Task<ResourceOwner> AddOwnerAsync(User owner, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(owner, nameof(owner));

            return AddOwnerAsync(owner.Id, organization.Id);
        }

        /// <summary>
        /// Add organization owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns>created mapping</returns>
        public Task<ResourceOwner> AddOwnerAsync(string ownerId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            return _service.PostOrgsIDOwnersAsync(orgId, new AddResourceMemberRequestBody(ownerId));
        }

        /// <summary>
        /// Removes a owner from an organization.
        /// </summary>
        /// <param name="owner">the owner of an organization</param>
        /// <param name="organization">the organization of a owner</param>
        /// <returns></returns>
        public Task DeleteOwnerAsync(User owner, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(owner, nameof(owner));

            return DeleteOwnerAsync(owner.Id, organization.Id);
        }

        /// <summary>
        /// Removes a owner from an organization.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns></returns>
        public Task DeleteOwnerAsync(string ownerId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            return _service.DeleteOrgsIDOwnersIDAsync(ownerId, orgId);
        }
    }
}