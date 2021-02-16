using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InfluxDB.Client.AspNetCore.Test
{

    public class Startup
    {

        private readonly IConfiguration configuration;

        public Startup(IConfiguration configuration)
        {
            this.configuration = configuration;
        }

        public void Configure(IApplicationBuilder applicationBuilder) {
            applicationBuilder.UseEndpoints(endpointRouterBuilder => {
                endpointRouterBuilder.MapHealthChecks("/health");
            });
        }

        public void ConfigureServices(IServiceCollection serviceCollection) {
            serviceCollection.AddInfluxDBClient(this.configuration.GetConnectionString(""));
            serviceCollection.AddHealthChecks().AddInfluxDBCheck();
        }

    }

}
