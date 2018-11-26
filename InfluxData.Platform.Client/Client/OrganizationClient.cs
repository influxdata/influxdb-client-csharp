using System.Collections.Generic;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;
using Task = System.Threading.Tasks.Task;

namespace InfluxData.Platform.Client.Client
{
    public class OrganizationClient : AbstractClient
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
            Arguments.CheckNonEmptyString(name, "Organization name");

            Organization organization = new Organization {Name = name};

            return await CreateOrganization(organization);
        }

        /// <summary>
        /// Creates a new organization and sets <see cref="Organization.Id"/> with the new identifier.
        /// </summary>
        /// <param name="organization">the organization to create</param>
        /// <returns>created organization</returns>
        public async Task<Organization> CreateOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, "Organization");

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
            Arguments.CheckNotNull(organization, "Organization");

            var request = await Patch(organization, $"/api/v2/orgs/{organization.Id}");

            return Call<Organization>(request);
        }

        /// <summary>
        /// Delete a organization.
        /// </summary>
        /// <param name="organizationId">ID of organization to delete</param>
        /// <returns></returns>
        public async Task DeleteOrganization(string organizationId)
        {
            Arguments.CheckNotNull(organizationId, "Organization ID");

            var request = await Delete($"/api/v2/orgs/{organizationId}");

            RaiseForInfluxError(request);
        }

        /// <summary>
        /// Delete a organization.
        /// </summary>
        /// <param name="organization">organization to delete</param>
        /// <returns></returns>
        public async Task DeleteOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, "Organization is required");

            await DeleteOrganization(organization.Id);
        }

        /// <summary>
        /// Retrieve a organization.
        /// </summary>
        /// <param name="organizationId">ID of organization to get</param>
        /// <returns>organization details</returns>
        public async Task<Organization> FindOrganizationById(string organizationId)
        {
            Arguments.CheckNonEmptyString(organizationId, "Organization ID");

            var request = await Get($"/api/v2/orgs/{organizationId}");

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
        /// List all members of an organization.
        /// </summary>
        /// <param name="organization">organization of the members</param>
        /// <returns>the List all members of an organization</returns>
        public async Task<List<UserResourceMapping>> GetMembers(Organization organization)
        {
            Arguments.CheckNotNull(organization, "Organization is required");

            return await GetMembers(organization.Id);
        }

        /// <summary>
        /// List all members of an organization.
        /// </summary>
        /// <param name="organizationId">ID of organization to get members</param>
        /// <returns>the List all members of an organization</returns>
        public async Task<List<UserResourceMapping>> GetMembers(string organizationId)
        {
            Arguments.CheckNonEmptyString(organizationId, "Organization ID");

            var request = await Get($"/api/v2/orgs/{organizationId}/members");

            var response = Call<UserResourcesResponse>(request);

            return response?.UserResourceMappings;
        }

        /// <summary>
        /// Add organization member.
        /// </summary>
        /// <param name="member">the member of an organization</param>
        /// <param name="organization">the organization of a member</param>
        /// <returns>created mapping</returns>
        public async Task<UserResourceMapping> AddMember(User member, Organization organization)
        {
            Arguments.CheckNotNull(organization, "organization");
            Arguments.CheckNotNull(member, "member");

            return await AddMember(member.Id, organization.Id);
        }

        /// <summary>
        /// Add organization member.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="organizationId">the ID of an organization</param>
        /// <returns>created mapping</returns>
        public async Task<UserResourceMapping> AddMember(string memberId, string organizationId)
        {
            Arguments.CheckNonEmptyString(organizationId, "Organization ID");
            Arguments.CheckNonEmptyString(memberId, "Member ID");

            User user = new User {Id = memberId};

            var request = await Post(user, $"/api/v2/orgs/{organizationId}/members");

            return Call<UserResourceMapping>(request);
        }

        /// <summary>
        /// Removes a member from an organization.
        /// </summary>
        /// <param name="member">the member of an organization</param>
        /// <param name="organization">the organization of a member</param>
        /// <returns>async task</returns>
        public async Task DeleteMember(User member, Organization organization)
        {
            Arguments.CheckNotNull(organization, "organization");
            Arguments.CheckNotNull(member, "member");

            await DeleteMember(member.Id, organization.Id);
        }

        /// <summary>
        /// Removes a member from an organization.
        /// </summary>
        /// <param name="memberId">the ID of a member</param>
        /// <param name="organizationId">the ID of an organization</param>
        /// <returns>async tasy</returns>
        public async Task DeleteMember(string memberId, string organizationId)
        {
            Arguments.CheckNonEmptyString(organizationId, "Organization ID");
            Arguments.CheckNonEmptyString(memberId, "Member ID");
            
            var request = await Delete($"/api/v2/orgs/{organizationId}/members/{memberId}");

            RaiseForInfluxError(request);
        }
    }
}