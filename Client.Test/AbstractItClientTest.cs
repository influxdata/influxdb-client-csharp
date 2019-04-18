using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Core.Test;
using InfluxDB.Client.Domain;
using InfluxDB.Client.Api.Domain;
using NUnit.Framework;
using Task = System.Threading.Tasks.Task;

namespace InfluxDB.Client.Test
{
    public class AbstractItClientTest : AbstractTest
    {
        protected InfluxDBClient Client;
        protected string InfluxDbUrl;
        
        [SetUp]
        public new void SetUp()
        {
            InfluxDbUrl = GetInfluxDb2Url();
            Client = InfluxDBClientFactory.Create(InfluxDbUrl, "my-user", "my-password".ToCharArray());

            if (!TestContext.CurrentContext.Test.Properties.ContainsKey("basic_auth"))
            {
                var token = FindMyToken();
                
                Client.Dispose();
                Client = InfluxDBClientFactory.Create(InfluxDbUrl, token.ToCharArray());
            }
        }

        [TearDown]
        protected void After()
        {
            Client.Dispose();
        }

        protected string GenerateName(string prefix) 
        {
            Assert.IsNotEmpty(prefix);

            return prefix + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                   CultureInfo.InvariantCulture) + "-IT";
        }

        protected Organization FindMyOrg()
        {
            return Client.GetOrganizationsApi().FindOrganizations()
                .First(organization => organization.Name.Equals("my-org"));
        }

        protected string FindMyToken()
        {
            var authorizations = Client.GetAuthorizationsApi().FindAuthorizations();
            
            return authorizations.Where(authorization =>
            {
                var count = authorization.Permissions.Where(permission =>
                {
                    var resource = permission.Resource;

                    return resource.Id == null && resource.OrgID == null &&
                           resource.Type.Equals(PermissionResource.TypeEnum.Orgs);
                }).Count();

                return count > 0;
            }).First().Token;
        }
    }
}