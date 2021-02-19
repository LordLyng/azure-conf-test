using System.Security.Principal;
using System.IO;
using System;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;

namespace AzureConfTest
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        // https://docs.microsoft.com/en-us/azure/azure-app-configuration/quickstart-aspnet-core-app?tabs=core5x     
                        var appName = AppDomain.CurrentDomain.FriendlyName;
                        var settings = config.Build();
                        var connection = settings.GetConnectionString("AppConfig");
                        config.AddAzureAppConfiguration(options =>
                        {
                            // https://docs.microsoft.com/en-us/azure/azure-app-configuration/enable-dynamic-configuration-aspnet-core?tabs=core5x
                            options.Connect(connection)
                                .ConfigureRefresh(refresh =>
                                {
                                    refresh.Register($"{appName}:Settings:Sentinel", refreshAll: true)
                                        .SetCacheExpiration(new TimeSpan(0, 5, 0));
                                })
                                .UseFeatureFlags(ffOptions =>
                                {
                                    ffOptions.CacheExpirationInterval = TimeSpan.FromSeconds(30);
                                    ffOptions.Label = hostingContext.HostingEnvironment.EnvironmentName;
                                })
                                // https://docs.microsoft.com/en-us/azure/azure-app-configuration/howto-labels-aspnet-core
                                // Without label => general config, valid for all environments
                                .Select(KeyFilter.Any, LabelFilter.Null)
                                // Label == EnvironmentName => config relevant for specific environment
                                .Select(KeyFilter.Any, hostingContext.HostingEnvironment.EnvironmentName)
                                // https://github.com/Azure/AppConfiguration/issues/45
                                .SetOfflineCache(OfflineCacheFactory.GetOfflineFileCache())
                                .TrimKeyPrefix($"{appName}:");
                        })
                            .AddJsonFile("local.overrides.json", optional: true, reloadOnChange: true);
                    })
                    .UseStartup<Startup>();
                });
    }
}
