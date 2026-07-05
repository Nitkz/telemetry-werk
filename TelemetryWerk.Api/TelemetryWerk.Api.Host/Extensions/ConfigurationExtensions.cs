using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TelemetryWerk.Api.Application.Configurations;

namespace TelemetryWerk.Api.Host.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// For the Server side (ASP.NET Core / API Host).
    /// Supports changing the JSON file name and overwriting via Environment Variables.
    /// </summary>
    public static IHostBuilder ConfigureServerSettings(this IHostBuilder host)
    {
        return host.ConfigureAppConfiguration((context, config) =>
        {
            // 1. Read file name from ENV (if any), e.g., for use in Docker
            var customConfigName = Environment.GetEnvironmentVariable("CONFIG_FILE_NAME") ?? "appsettings.json";

            config.SetBasePath(Directory.GetCurrentDirectory())
                  .AddJsonFile(customConfigName, optional: true, reloadOnChange: true)
                  .AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", optional: true)
                  .AddEnvironmentVariables(); // ENV has the highest priority
        });
    }

    /// <summary>
    /// Registers ApiServiceOptions into the DI Container
    /// </summary>
    public static IServiceCollection AddApiOptions(this IServiceCollection services, IConfiguration configuration)
    {
        return services.Configure<ApiServiceOptions>(
            configuration.GetSection(ApiServiceOptions.SectionName));
    }
}
