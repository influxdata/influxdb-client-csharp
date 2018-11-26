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
    }
}