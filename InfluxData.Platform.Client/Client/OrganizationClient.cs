using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Domain;
using Newtonsoft.Json;
using Platform.Common.Platform;
using Platform.Common.Platform.Rest;

namespace InfluxData.Platform.Client.Client
{
    public class OrganizationClient : AbstractClient
    {
        public OrganizationClient(DefaultClientIo client) : base (client)
        {

        }
        
        public async Task<Organization> CreateOrganization(string name) 
        {
            Arguments.CheckNonEmptyString(name, "Organization name");

            Organization organization = new Organization();
            organization.Name = name;

            return await CreateOrganization(organization);
        }
        
        public async Task<Organization> CreateOrganization(Organization organization) 
        {
            Arguments.CheckNotNull(organization, "Organization");
            
            var responseHttp = await Client.DoRequest(PlatformService.CreateOrganization(organization))
                            .ConfigureAwait(false);

            RaiseForInfluxError(responseHttp);

            return JsonConvert.DeserializeObject<Organization>(responseHttp.ToString());
        }
        
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