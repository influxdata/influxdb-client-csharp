using System.Linq;
using System.Threading.Tasks;
using InfluxDB.Client.Api.Domain;
using InfluxDB.Client.Core.Test;
using NUnit.Framework;

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

            if (!TestContext.CurrentContext.Test.Properties.ContainsKey("basic_auth"))
            {
                Client = new InfluxDBClient(InfluxDbUrl, "my-token");
            }
            else
            {
                Client = new InfluxDBClient(InfluxDbUrl, "my-user", "my-password");
            }
        }

        [TearDown]
        protected void After()
        {
            Client.Dispose();
        }

        protected async Task<Organization> FindMyOrg()
        {
            var org = (await Client.GetOrganizationsApi().FindOrganizationsAsync(100))
                .FirstOrDefault(organization => organization.Name.Equals("my-org"));

            Assert.IsNotNull(org, "my-org is required for integration tests");

            return org;
        }
    }
}