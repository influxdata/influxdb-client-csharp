using System.Threading.Tasks;
using InfluxDB.Client.Core.Test;
using InfluxDB.Client.Flux;
using NUnit.Framework;

namespace Client.Legacy.Test
{
    public abstract class AbstractItFluxClientTest : AbstractTest
    {
        protected const string DatabaseName = "flux_database";

        protected FluxClient FluxClient;

        [SetUp]
        public new void SetUp()
        {
            SetUpAsync().Wait();
        }

        private async Task SetUpAsync()
        {
            var influxUrl = GetInfluxDbUrl();

            var options = new FluxConnectionOptions(influxUrl);

            FluxClient = FluxClientFactory.Create(options);

            await InfluxDbQuery("CREATE DATABASE " + DatabaseName, DatabaseName);
        }

        [TearDown]
        protected void After()
        {
            InfluxDbQuery("DROP DATABASE " + DatabaseName, DatabaseName).ConfigureAwait(false).GetAwaiter().GetResult();

            FluxClient.Dispose();
        }
    }
}