using System;
using System.Globalization;
using InfluxData.Platform.Client.Client;
using NUnit.Framework;
using Platform.Common.Platform.Rest;
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
            PlatformClient.Close().Wait();
        }
        
        protected string GenerateName(string prefix) 
        {
            Assert.IsNotEmpty(prefix);

            return prefix + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss.fff",
                                   CultureInfo.InvariantCulture);
        }
    }
}