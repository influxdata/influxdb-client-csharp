using System.Collections.Generic;
using System.Linq;
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
        public async Task<Organization> CreateOrganizationAsync(string name)
        {
            Arguments.CheckNonEmptyString(name, nameof(name));

            var organization = new Organization(null, name);

            return await CreateOrganizationAsync(organization);
        }

        /// <summary>
        /// Creates a new organization and sets <see cref="Organization.Id" /> with the new identifier.
        /// </summary>
        /// <param name="organization">the organization to create</param>
        /// <returns>created organization</returns>
        public async Task<Organization> CreateOrganizationAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await _service.PostOrgsAsync(organization);
        }

        /// <summary>
        /// Update an organization.
        /// </summary>
        /// <param name="organization">organization update to apply</param>
        /// <returns>updated organization</returns>
        public async Task<Organization> UpdateOrganizationAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await _service.PatchOrgsIDAsync(organization.Id, organization);
        }

        /// <summary>
        /// Delete an organization.
        /// </summary>
        /// <param name="orgId">ID of organization to delete</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteOrganizationAsync(string orgId)
        {
            Arguments.CheckNotNull(orgId, nameof(orgId));

            await _service.DeleteOrgsIDAsync(orgId);
        }

        /// <summary>
        /// Delete an organization.
        /// </summary>
        /// <param name="organization">organization to delete</param>
        /// <returns>delete has been accepted</returns>
        public async Task DeleteOrganizationAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            await DeleteOrganizationAsync(organization.Id);
        }

        /// <summary>
        /// Clone an organization.
        /// </summary>
        /// <param name="clonedName">name of cloned organization</param>
        /// <param name="bucketId">ID of organization to clone</param>
        /// <returns>cloned organization</returns>
        public async Task<Organization> CloneOrganizationAsync(string clonedName, string bucketId)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNonEmptyString(bucketId, nameof(bucketId));

            return await FindOrganizationByIdAsync(bucketId).ContinueWith(t => 
                            CloneOrganizationAsync(clonedName, t.Result)).Unwrap();
        }

        /// <summary>
        /// Clone an organization.
        /// </summary>
        /// <param name="clonedName">name of cloned organization</param>
        /// <param name="organization">organization to clone</param>
        /// <returns>cloned organization</returns>
        public async Task<Organization> CloneOrganizationAsync(string clonedName, Organization organization)
        {
            Arguments.CheckNonEmptyString(clonedName, nameof(clonedName));
            Arguments.CheckNotNull(organization, nameof(organization));

            var cloned = new Organization(null, clonedName);

            return await CreateOrganizationAsync(cloned);
        }

        /// <summary>
        /// Retrieve an organization.
        /// </summary>
        /// <param name="orgId">ID of organization to get</param>
        /// <returns>organization details</returns>
        public async Task<Organization> FindOrganizationByIdAsync(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return await _service.GetOrgsIDAsync(orgId);
        }

        /// <summary>
        /// List all organizations.
        /// </summary>
        /// <returns>List all organizations</returns>
        public async Task<List<Organization>> FindOrganizationsAsync()
        {
            return (await _service.GetOrgsAsync()).Orgs;
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
        public async Task<List<string>> GetSecretsAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await GetSecretsAsync(organization.Id);
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

            return (await _service.GetOrgsIDSecretsAsync(orgId)).Secrets;
        }

        /// <summary>
        /// Patches all provided secrets and updates any previous values.
        /// </summary>
        /// <param name="secrets">secrets to update/add</param>
        /// <param name="organization">the organization for put secrets</param>
        /// <returns></returns>
        public async Task PutSecretsAsync(Dictionary<string, string> secrets, Organization organization)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNotNull(organization, nameof(organization));

            await PutSecretsAsync(secrets, organization.Id);
        }

        /// <summary>
        /// Patches all provided secrets and updates any previous values.
        /// </summary>
        /// <param name="secrets">secrets to update/add</param>
        /// <param name="orgId">the organization for put secrets</param>
        /// <returns></returns>
        public async Task PutSecretsAsync(Dictionary<string, string> secrets, string orgId)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            await _service.PatchOrgsIDSecretsAsync(orgId, secrets);
        }

        /// <summary>
        /// Delete provided secrets.
        /// </summary>
        /// <param name="secrets">secrets to delete</param>
        /// <param name="organization">the organization for delete secrets</param>
        /// <returns>keys successfully patched</returns>
        public async Task DeleteSecretsAsync(List<string> secrets, Organization organization)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNotNull(organization, nameof(organization));

            await DeleteSecretsAsync(secrets, organization.Id);
        }

        /// <summary>
        /// Delete provided secrets.
        /// </summary>
        /// <param name="secrets">secrets to delete</param>
        /// <param name="orgId">the organization for delete secrets</param>
        /// <returns>keys successfully patched</returns>
        public async Task DeleteSecretsAsync(List<string> secrets, string orgId)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            await DeleteSecretsAsync(new SecretKeys(secrets), orgId);
        }
        
        /// <summary>
        /// Delete provided secrets.
        /// </summary>
        /// <param name="secrets">secrets to delete</param>
        /// <param name="orgId">the organization for delete secrets</param>
        /// <returns>keys successfully patched</returns>
        public async Task DeleteSecretsAsync(SecretKeys secrets, string orgId)
        {
            Arguments.CheckNotNull(secrets, nameof(secrets));
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            await _service.PostOrgsIDSecretsAsync(orgId, secrets);
        }

        /// <summary>
        /// List all members of an organization.
        /// </summary>
        /// <param name="organization">organization of the members</param>
        /// <returns>the List all members of an organization</returns>
        public async Task<List<ResourceMember>> GetMembersAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await GetMembersAsync(organization.Id);
        }

        /// <summary>
        /// List all members of an organization.
        /// </summary>
        /// <param name="orgId">ID of organization to get members</param>
        /// <returns>the List all members of an organization</returns>
        public async Task<List<ResourceMember>> GetMembersAsync(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return (await _service.GetOrgsIDMembersAsync(orgId)).Users;
        }

        /// <summary>
        /// Add organization member.
        /// </summary>
        /// <param name="member">the member of an organization</param>
        /// <param name="organization">the organization of a member</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddMemberAsync(User member, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(member, nameof(member));

            return await AddMemberAsync(member.Id, organization.Id);
        }

        /// <summary>
        /// Add organization member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceMember> AddMemberAsync(string memberId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            return await _service.PostOrgsIDMembersAsync(orgId, new AddResourceMemberRequestBody(memberId));
        }

        /// <summary>
        /// Removes a member from an organization.
        /// </summary>
        /// <param name="member">the member of an organization</param>
        /// <param name="organization">the organization of a member</param>
        /// <returns></returns>
        public async Task DeleteMemberAsync(User member, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(member, nameof(member));

            await DeleteMemberAsync(member.Id, organization.Id);
        }

        /// <summary>
        /// Removes a member from an organization.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns></returns>
        public async Task DeleteMemberAsync(string memberId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(memberId, nameof(memberId));

            await _service.DeleteOrgsIDMembersIDAsync(memberId, orgId);
        }

        /// <summary>
        /// List all owners of an organization.
        /// </summary>
        /// <param name="organization">organization of the owners</param>
        /// <returns>the List all owners of an organization</returns>
        public async Task<List<ResourceOwner>> GetOwnersAsync(Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));

            return await GetOwnersAsync(organization.Id);
        }

        /// <summary>
        /// List all owners of an organization.
        /// </summary>
        /// <param name="orgId">ID of organization to get owners</param>
        /// <returns>the List all owners of an organization</returns>
        public async Task<List<ResourceOwner>> GetOwnersAsync(string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));

            return (await _service.GetOrgsIDOwnersAsync(orgId)).Users;
        }

        /// <summary>
        /// Add organization owner.
        /// </summary>
        /// <param name="owner">the owner of an organization</param>
        /// <param name="organization">the organization of a owner</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceOwner> AddOwnerAsync(User owner, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(owner, nameof(owner));

            return await AddOwnerAsync(owner.Id, organization.Id);
        }

        /// <summary>
        /// Add organization owner.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns>created mapping</returns>
        public async Task<ResourceOwner> AddOwnerAsync(string ownerId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            return await _service.PostOrgsIDOwnersAsync(orgId, new AddResourceMemberRequestBody(ownerId));
        }

        /// <summary>
        /// Removes a owner from an organization.
        /// </summary>
        /// <param name="owner">the owner of an organization</param>
        /// <param name="organization">the organization of a owner</param>
        /// <returns></returns>
        public async Task DeleteOwnerAsync(User owner, Organization organization)
        {
            Arguments.CheckNotNull(organization, nameof(organization));
            Arguments.CheckNotNull(owner, nameof(owner));

            await DeleteOwnerAsync(owner.Id, organization.Id);
        }

        /// <summary>
        /// Removes a owner from an organization.
        /// </summary>
        /// <param name="ownerId">the ID of a owner</param>
        /// <param name="orgId">the ID of an organization</param>
        /// <returns></returns>
        public async Task DeleteOwnerAsync(string ownerId, string orgId)
        {
            Arguments.CheckNonEmptyString(orgId, nameof(orgId));
            Arguments.CheckNonEmptyString(ownerId, nameof(ownerId));

            await _service.DeleteOrgsIDOwnersIDAsync(ownerId, orgId);
        }
    }
}