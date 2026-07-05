using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;
using TelemetryWerk.Ui.Core.Configurations;
using TelemetryWerk.Ui.Client.Extensions;
using TelemetryWerk.Ui.Client.Auth;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddMudServices();

builder.Services.AddAuthorizationCore();
builder.Services.AddSingleton<PersistentAuthenticationStateProvider>();
builder.Services.AddSingleton<AuthenticationStateProvider>(sp => sp.GetRequiredService<PersistentAuthenticationStateProvider>());
builder.Services.AddTransient<SessionAuthenticationHandler>();

// Fetch settings from UI Server pattern
await builder.ConfigureWasmSettings();

// Register options
builder.Services.Configure<ApiServiceOptions>(
    builder.Configuration.GetSection(ApiServiceOptions.SectionName));

// Register Authenticated HttpClient for REST API calls via Typed Client
builder.Services.AddHttpClient<TelemetryWerk.Api.Client.ITelemetryApiClient, TelemetryWerk.Api.Client.TelemetryApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiServiceOptions>>().Value;
    var endpoint = string.IsNullOrWhiteSpace(options.ApiEndpoint) ? ApiServiceOptions.DefaultApiEndpoint : options.ApiEndpoint;
    client.BaseAddress = new Uri(endpoint);
}).AddHttpMessageHandler<SessionAuthenticationHandler>();

// Register Core UI Services
builder.Services.AddScoped<TelemetryWerk.Ui.Core.Interfaces.IMachineApiService, TelemetryWerk.Ui.Infrastructure.Services.MachineApiServiceImpl>();

await builder.Build().RunAsync();
