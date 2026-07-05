using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Threading.Tasks;
using TelemetryWerk.Api.Application.Configurations;

namespace TelemetryWerk.Api.Host.Middlewares;

public class ApiKeyMiddleware(RequestDelegate next)
{
    private const string API_KEY_HEADER = "X-Api-Key";
    private const string API_KEY_QUERY = "access_token"; // Used by SignalR

    public async Task InvokeAsync(HttpContext context, IOptions<ApiServiceOptions> options, ILogger<ApiKeyMiddleware> logger)
    {
        var expectedKey = options.Value.ApiKey;

        // If no API key is configured on the server, bypass validation
        if (string.IsNullOrWhiteSpace(expectedKey))
        {
            await next(context);
            return;
        }

        string? providedKey = null;

        // 1. Try to get the API Key from the Header (Standard REST)
        if (context.Request.Headers.TryGetValue(API_KEY_HEADER, out var extractedHeaderKey))
        {
            providedKey = extractedHeaderKey.FirstOrDefault();
        }

        // 2. Try to get the API Key from Query String (SignalR WebSockets)
        if (string.IsNullOrWhiteSpace(providedKey) && context.Request.Query.TryGetValue(API_KEY_QUERY, out var extractedQueryKey))
        {
            providedKey = extractedQueryKey.FirstOrDefault();
        }

        // 3. Try to get the API Key from Authorization Header (SignalR HTTP requests like /negotiate)
        if (string.IsNullOrWhiteSpace(providedKey) && context.Request.Headers.TryGetValue("Authorization", out var authHeader))
        {
            var token = authHeader.FirstOrDefault();
            if (token != null && token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            {
                providedKey = token.Substring("Bearer ".Length).Trim();
            }
        }

        logger.LogDebug("[API Key Middleware] Expected: '{ExpectedKey}'", expectedKey);
        logger.LogDebug("[API Key Middleware] Provided: '{ProvidedKey}'", providedKey);
        logger.LogDebug("[API Key Middleware] Request Path: {Path}", context.Request.Path);

        // Validate Key
        if (string.IsNullOrWhiteSpace(providedKey) || providedKey != expectedKey)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Invalid API Key.");
            return;
        }

        await next(context);
    }
}
