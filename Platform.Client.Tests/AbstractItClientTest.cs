using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using InfluxData.Platform.Client.Client;
using InfluxData.Platform.Client.Domain;
using NUnit.Framework;
using Platform.Common.Tests;

namespace Platform.Client.Tests
{
    public class AbstractItClientTest : AbstractTest
    {
        protected PlatformClient PlatformClient;
        protected string PlatformUrl;
        
        [SetUp]
        public new void SetUp()
        {
            PlatformUrl = GetPlatformUrl();
            PlatformClient = PlatformClientFactory.Create(PlatformUrl, "my-user", "my-password".ToCharArray());
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
                                   CultureInfo.InvariantCulture);
        }

        protected async Task<Organization> FindMyOrg()
        {
            return (await PlatformClient.CreateOrganizationClient().FindOrganizations())
                .First(organization => organization.Name.Equals("my-org"));
        }
    }
}