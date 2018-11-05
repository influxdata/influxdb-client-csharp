using System;
using System.Threading.Tasks;
using Flux.Client;
using Flux.Flux.Options;
using NUnit.Framework;
using WireMock.Server;

namespace Flux.Tests.Flux
{
    public abstract class AbstractItFluxClientTest : AbstractTest
    {
        protected const string DatabaseName = "flux_database";

        protected FluxClient FluxClient;

        [OneTimeSetUp]
        public new void SetUp()
        {
            string influxUrl = GetInfluxDbUrl();
            
            var options = new FluxConnectionOptions(influxUrl);
            
            FluxClient = FluxClientFactory.Create(options);
            
            InfluxDbQuery("CREATE DATABASE " + DatabaseName, DatabaseName);
            
            PrepareDara();            
        }

        [TearDown]
        protected void After() 
        {
            InfluxDbQuery("DROP DATABASE " + DatabaseName, DatabaseName);
        }

        public abstract void PrepareDara();
    }
}