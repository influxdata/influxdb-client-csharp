using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Testing;
using NUnit.Framework;

namespace InfluxDB.Client.AspNetCore.Test
{

    public class HealthTest : AbstractTest
    {

        [Test]
        public async Task HealthTest()
        {
            using var httpClient = this.webApplicationFactory.CreateDefaultClient();
            using var httpResponseMessage = await httpClient.GetAsync("/health");
            Assert.Equals(httpResponseMessage.StatusCode, HttpStatusCode.OK);
        }

    }

}
