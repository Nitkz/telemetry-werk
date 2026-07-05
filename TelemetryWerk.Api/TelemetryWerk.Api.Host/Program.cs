using System.Threading.Channels;
using TelemetryWerk.Api.Host.Extensions;
using TelemetryWerk.Api.Host.Hubs;
using TelemetryWerk.Api.Host.Middlewares;
using TelemetryWerk.Api.Host.Publishers;
using TelemetryWerk.Api.Host.Workers;
using TelemetryWerk.Api.Domain.Interfaces;
using TelemetryWerk.Api.Infrastructure.Repositories;
using TelemetryWerk.Api.Application.Interfaces;
using TelemetryWerk.Api.Application.Services;
using TelemetryWerk.Api.Application.Contracts;
using FluentValidation;
using TelemetryWerk.Api.Application.Validators;
using TelemetryWerk.Api.Host.Filters;

var builder = WebApplication.CreateBuilder(args);
builder.Host.ConfigureServerSettings();

// Create a Pub/Sub Channel for real-time Machine State updates between REST APIs and Background Workers
var updateChannel = Channel.CreateUnbounded<MachineStateUpdateMessage>();

// Register the Channel Writer/Reader
builder.Services.AddSingleton(updateChannel.Writer);
builder.Services.AddSingleton(updateChannel.Reader);

// Add services to the container.
builder.Services.AddApiOptions(builder.Configuration);
builder.Services.AddSingleton<IMachineRepository, InMemoryMachineRepository>();
builder.Services.AddScoped<IMachineService, MachineService>();
builder.Services.AddSingleton<ITelemetryPublisher, SignalRTelemetryPublisher>();
builder.Services.AddScoped<ITelemetryIngestionService, TelemetryIngestionService>();

builder.Services.AddValidatorsFromAssemblyContaining<MachineNodeDtoValidator>();
builder.Services.AddControllers(options => 
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddSignalR();
builder.Services.AddHostedService<PseudoTelemetryGeneratorWorker>();

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

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();
app.MapHub<TelemetryHub>("/hubs/telemetry");

app.Run();
