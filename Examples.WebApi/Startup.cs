using System;
using InfluxDB.Client;
using InfluxDB.Client.Api.Domain;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Examples.WebApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllers();
            
            var client = InfluxDBClientFactory.Create("http://localhost:9999", "my-token");
            var converter = new DomainEntityConverter();
            PrepareData(client, converter);
            
            
            services.Add(new ServiceDescriptor(typeof(InfluxDBClient), client));   
            services.Add(new ServiceDescriptor(typeof(DomainEntityConverter), converter));   
        }

        private void PrepareData(InfluxDBClient client, DomainEntityConverter converter)
        {
            var time = new DateTimeOffset(2020, 11, 15, 8, 20, 15,
                new TimeSpan(3, 0, 0));

            var entity1 = new DomainEntity
            {
                Timestamp = time,
                SeriesId = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"),
                Value = 15
            };
            var entity2 = new DomainEntity
            {
                Timestamp = time.AddHours(1),
                SeriesId = Guid.Parse("0f8fad5b-d9cb-469f-a165-70867728950e"),
                Value = 16
            };
            var entity3 = new DomainEntity
            {
                Timestamp = time.AddHours(2),
                SeriesId = Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"),
                Value = 17
            };
            var entity4 = new DomainEntity
            {
                Timestamp = time.AddHours(3),
                SeriesId = Guid.Parse("7c9e6679-7425-40de-944b-e07fc1f90ae7"),
                Value = 18
            };

            client.GetWriteApiAsync(converter)
                .WriteMeasurementsAsync("my-bucket", "my-org", WritePrecision.S, 
                    entity1, entity2, entity3, entity4);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
        }
    }
}