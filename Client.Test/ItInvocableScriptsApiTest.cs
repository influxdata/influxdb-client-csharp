using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItInvocableScriptsApiTest: AbstractItClientTest
    {
        [Test]
        public void CreateInstance()
        {
            var invocableScriptsApi = Client.GetInvocableScriptsApi();
            
            Assert.NotNull(invocableScriptsApi);
        }
    }
}