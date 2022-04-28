using NUnit.Framework;

namespace InfluxDB.Client.Test
{
    [TestFixture]
    public class ItInvokableScriptsApiTest : AbstractItClientTest
    {
        [Test]
        public void CreateInstance()
        {
            var invokableScriptsApi = Client.GetInvokableScriptsApi();

            Assert.NotNull(invokableScriptsApi);
        }
    }
}