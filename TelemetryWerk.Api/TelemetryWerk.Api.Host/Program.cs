using Serilog;
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
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.Enrich.FromLogContext()
                 .WriteTo.Console());

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
builder.Services.AddSingleton<ISessionRepository, InMemorySessionRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddValidatorsFromAssemblyContaining<MachineNodeDtoValidator>();
builder.Services.AddControllers(options => 
{
    options.Filters.Add<ValidationFilter>();
});

builder.Services.AddSignalR();
builder.Services.AddHostedService<PseudoTelemetryGeneratorWorker>();

var allowedOrigins = builder.Configuration.GetSection("AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCorsPolicy", policy =>
    {
        policy.AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();

        if (builder.Environment.IsDevelopment())
        {
            policy.SetIsOriginAllowed(_ => true);
        }
        else
        {
            policy.WithOrigins(allowedOrigins);
        }
    });
});

builder.Services.AddRateLimiter(options =>
{
    // Helper to get true client IP behind proxy
    string GetClientIp(HttpContext ctx) => 
        ctx.Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? 
        ctx.Connection.RemoteIpAddress?.ToString() ?? 
        ctx.Request.Headers.Host.ToString();

    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetClientIp(httpContext),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));

    // Strict policy for Auth/Login to prevent brute-force attacks
    options.AddPolicy("LoginLimiter", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: GetClientIp(httpContext),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 5, // Allow max 5 login attempts
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1) // per minute per IP
            }));

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.AddSecurityDefinition("SessionKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "Session Key authentication. Enter your Session Key below.",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Name = "X-Session-Key",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "SessionKeyScheme"
    });
    
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "SessionKey"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseCors("DefaultCorsPolicy");
app.UseRateLimiter();

app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<SessionAuthMiddleware>();

app.MapControllers();
app.MapHub<TelemetryHub>("/hubs/telemetry");

app.Run();
