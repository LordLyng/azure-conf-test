using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.FeatureManagement;

namespace AzureConfTest
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // https://docs.microsoft.com/en-us/azure/azure-app-configuration/enable-dynamic-configuration-aspnet-core?tabs=core5x
            services.AddAzureAppConfiguration();
            services.AddFeatureManagement();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // https://docs.microsoft.com/en-us/azure/azure-app-configuration/enable-dynamic-configuration-aspnet-core?tabs=core5x
            app.UseAzureAppConfiguration();

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/config", async (requestContext) =>
                {
                    requestContext.Response.ContentType = "text/plain";
                    var featureManager = requestContext.RequestServices.GetRequiredService<IFeatureManager>();
                    if (!await featureManager.IsEnabledAsync(AzureConfTestFeatureFlags.ConfigDump))
                    {
                        await requestContext.Response.WriteAsync("Feature disabled");
                        return;
                    }

                    var debugView = (Configuration as IConfigurationRoot).GetDebugView();
                    await requestContext.Response.WriteAsync(debugView);
                });
            });
        }
    }
}
