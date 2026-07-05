using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using System.Net.Http.Json;
using TelemetryWerk.Ui.Core.Configurations;

namespace TelemetryWerk.Ui.Client.Extensions;

public static class ConfigurationExtensions
{
    /// <summary>
    /// For WASM side: Fetch Config from API Endpoint
    /// </summary>
    public static async Task ConfigureWasmSettings(this WebAssemblyHostBuilder builder, string configApiUrl = "config")
    {
        var apiEndpoint = ApiServiceOptions.DefaultApiEndpoint; // Fallback default

        try
        {
            using var http = new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) };
            var serverConfig = await http.GetFromJsonAsync<ApiServiceOptions>(configApiUrl);

            if (serverConfig != null)
            {
                apiEndpoint = serverConfig.ApiEndpoint ?? apiEndpoint;
            }
        }
        catch (Exception ex)
        {
            // During WASM initialization, we must build a temporary ServiceProvider to get an ILogger
            using var sp = builder.Services.BuildServiceProvider();
            var logger = sp.GetService<Microsoft.Extensions.Logging.ILoggerFactory>()?.CreateLogger("ConfigurationExtensions");
            
            if (logger != null)
            {
                logger.LogWarning(ex, "[Config Error]: Could not fetch remote config, using fallback.");
            }
            else
            {
                Console.WriteLine($"[Config Error]: Could not fetch remote config, using fallback. {ex.Message}");
            }
        }

        // Map the fetched (or fallback) data to .NET Configuration system
        var configDict = new Dictionary<string, string?>
        {
            [$"{ApiServiceOptions.SectionName}:ApiEndpoint"] = apiEndpoint
        };

        builder.Configuration.AddInMemoryCollection(configDict);
    }
}
