using TelemetryWerk.Api.Host.Extensions;
using TelemetryWerk.Api.Host.Hubs;
using TelemetryWerk.Api.Host.Middlewares;
using TelemetryWerk.Api.Host.Workers;
using TelemetryWerk.Api.Domain.Interfaces;
using TelemetryWerk.Api.Infrastructure.Repositories;
using TelemetryWerk.Api.Application.Interfaces;
using TelemetryWerk.Api.Application.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureServerSettings();

// Add services to the container.
builder.Services.AddApiOptions(builder.Configuration);
builder.Services.AddSingleton<IMachineRepository, InMemoryMachineRepository>();
builder.Services.AddScoped<IMachineService, MachineService>();
builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddHostedService<TelemetryBackgroundIngester>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(_ => true)
              .AllowCredentials();
    });
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "API Key authentication. Enter your API Key below.\nExample: DEV_MODE_PLEASE_CHANGE_THIS_KEY_IN_PRODUCTION",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "X-Api-Key",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowAll");

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();
app.MapHub<TelemetryHub>("/hubs/telemetry");

app.Run();
