using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace InfluxDB.Client.Test
{
    public class Startup
    {
        public static void ConfigureServices(IServiceCollection services)
        {
        }

        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/ping",
                    async context =>
                    {
                        context.Response.Headers.Append("x-influxdb-version", "2.0.0");
                        await context.Response.WriteAsync("pong");
                    });
            });
        }
    }
}