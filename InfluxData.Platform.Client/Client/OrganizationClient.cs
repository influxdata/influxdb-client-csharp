using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using Newtonsoft.Json;
using Platform.Common.Flux.Error;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;
using Task = System.Threading.Tasks.Task;

namespace InfluxData.Platform.Client.Client
{
    public class OrganizationClient : AbstractClient
    {
        public OrganizationClient(DefaultClientIo client) : base (client)
        {

        }
        
        /**
         * Creates a new organization and sets {@link Organization#id} with the new identifier.
         *
         * @param name name of the organization
         * @return Organization created
         */
        public async Task<Organization> CreateOrganization(string name) 
        {
            Arguments.CheckNonEmptyString(name, "Organization name");

            Organization organization = new Organization();
            organization.Name = name;

            return await CreateOrganization(organization);
        }
        
        /**
         * Creates a new organization and sets {@link Organization#id} with the new identifier.
         *
         * @param organization the organization to create
         * @return Organization created
         */
        public async Task<Organization> CreateOrganization(Organization organization) 
        {
            Arguments.CheckNotNull(organization, "Organization");
            
            var responseHttp = await Client.DoRequest(PlatformService.CreateOrganization(organization))
                            .ConfigureAwait(false);

            RaiseForInfluxError(responseHttp);

            return JsonConvert.DeserializeObject<Organization>(
                            new StreamReader(responseHttp.ResponseContent).ReadToEnd());
        }
        
        /**
         * Update a organization.
         *
         * @param organization organization update to apply
         * @return organization updated
         */
        public async Task<Organization> UpdateOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, "Organization");
            
            var responseHttp = await Client.DoRequest(PlatformService.UpdateOrganization(organization))
                            .ConfigureAwait(false);

            RaiseForInfluxError(responseHttp);

            return JsonConvert.DeserializeObject<Organization>(
                            new StreamReader(responseHttp.ResponseContent).ReadToEnd());  
        }
        
        /**
         * Delete a organization.
         *
         * @param organizationID ID of organization to delete
         */
        public async Task DeleteOrganization(string organizationId)
        {
            Arguments.CheckNotNull(organizationId, "Organization ID");

            var responseHttp = await Client.DoRequest(PlatformService.DeleteOrganization(organizationId))
                            .ConfigureAwait(false);

            RaiseForInfluxError(responseHttp);
        }
        
        /**
         * Delete a organization.
         *
         * @param organization organization to delete
         */
        public async Task DeleteOrganization(Organization organization)
        {
            Arguments.CheckNotNull(organization, "Organization is required");

            await DeleteOrganization(organization.Id);
        }

        /**
         * Retrieve a organization.
         *
         * @param organizationID ID of organization to get
         * @return organization details
         */
        public async Task<Organization> FindOrganizationById(string organizationId)
        {
            Arguments.CheckNonEmptyString(organizationId, "Organization ID");

            var responseHttp = await Client.DoRequest(PlatformService.FindOrganizationById(organizationId))
                            .ConfigureAwait(false);

            RaiseForInfluxError(responseHttp);

            return JsonConvert.DeserializeObject<Organization>(
                            new StreamReader(responseHttp.ResponseContent).ReadToEnd());
        }

        /**
         * List all organizations.
         *
         * @return List all organizations
         */
        
        public async Task<List<Organization>> FindOrganizations() 
        {
            var responseHttp = await Client.DoRequest(PlatformService.FindOrganizations())
                            .ConfigureAwait(false);
            
            RaiseForInfluxError(responseHttp);

            Organizations organizations = JsonConvert.DeserializeObject<Organizations>(new StreamReader(responseHttp.ResponseContent).ReadToEnd());
            
            return organizations?.Orgs;
        }
    }
}