using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NUnit.Framework;
using Platform.Common.Tests;
using Task = System.Threading.Tasks.Task;

namespace Platform.Client.Tests
{
    public class AbstractItClientTest : AbstractTest
    {
        protected PlatformClient PlatformClient;
        protected string PlatformUrl;
        
        [SetUp]
        public new async Task SetUp()
        {
            PlatformUrl = GetPlatformUrl();
            PlatformClient = PlatformClientFactory.Create(PlatformUrl, "my-user", "my-password".ToCharArray());

            if (!TestContext.CurrentContext.Test.Properties.ContainsKey("basic_auth"))
            {
                var token = await FindMyToken();
                
                PlatformClient.Dispose();
                PlatformClient = PlatformClientFactory.Create(PlatformUrl, token.ToCharArray());
            }
        }

        [TearDown]
        protected void After()
        {
            PlatformClient.Dispose();
        }

        protected string GenerateName(string prefix) 
        {
            Assert.IsNotEmpty(prefix);

            return prefix + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                   CultureInfo.InvariantCulture) + "-IT";
        }

        protected async Task<Organization> FindMyOrg()
        {
            return (await PlatformClient.CreateOrganizationClient().FindOrganizations())
                .First(organization => organization.Name.Equals("my-org"));
        }

        protected async Task<string> FindMyToken()
        {
            var authorizations = await PlatformClient.CreateAuthorizationClient().FindAuthorizations();
            
            return authorizations.Where(authorization =>
            {
                var count = authorization.Permissions.Where(permission =>
                {
                    var resource = permission.Resource;

                    return resource.Id == null && resource.OrgId == null &&
                           resource.Type.Equals(PermissionResourceType.Org);
                }).Count();

                return count > 0;
            }).First().Token;
        }
    }
}