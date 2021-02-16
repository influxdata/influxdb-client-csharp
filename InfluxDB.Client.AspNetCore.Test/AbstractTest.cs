using Microsoft.AspNetCore.Mvc.Testing;

namespace InfluxDB.Client.AspNetCore.Test
{

    public abstract class AbstractTest : global::InfluxDB.Client.Core.Test.AbstractTest
    {

        protected readonly WebApplicationFactory<Startup> webApplicationFactory = new WebApplicationFactory<Startup>();

    }

}
