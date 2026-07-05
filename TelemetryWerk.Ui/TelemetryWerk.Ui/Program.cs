using MudBlazor.Services;
using TelemetryWerk.Ui.Core.Configurations;
using TelemetryWerk.Ui.Components;

var builder = WebApplication.CreateBuilder(args);

// Add MudBlazor services
builder.Services.AddMudServices();

// Register API Configuration
builder.Services.Configure<ApiServiceOptions>(
    builder.Configuration.GetSection(ApiServiceOptions.SectionName));

// Register Authenticated HttpClient for REST API calls via Typed Client
builder.Services.AddHttpClient<TelemetryWerk.Api.Client.ITelemetryApiClient, TelemetryWerk.Api.Client.TelemetryApiClient>((sp, client) =>
{
    var options = sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<ApiServiceOptions>>().Value;
    var endpoint = string.IsNullOrWhiteSpace(options.ApiEndpoint) ? ApiServiceOptions.DefaultApiEndpoint : options.ApiEndpoint;
    client.BaseAddress = new Uri(endpoint);
    
    if (!string.IsNullOrEmpty(options.ApiKey))
    {
        client.DefaultRequestHeaders.Add("X-Api-Key", options.ApiKey);
    }
});

// Register Core UI Services
builder.Services.AddScoped<TelemetryWerk.Ui.Core.Interfaces.IMachineApiService, TelemetryWerk.Ui.Infrastructure.Services.MachineApiServiceImpl>();

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
}

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(TelemetryWerk.Ui.Client._Imports).Assembly);

// Endpoint for WASM Client to fetch config
app.MapGet("/config", (Microsoft.Extensions.Options.IOptionsSnapshot<ApiServiceOptions> options) => 
{
    return Results.Ok(options.Value);
});

app.Run();
