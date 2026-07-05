using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Threading.Tasks;
using TelemetryWerk.Api.Domain.Interfaces;

namespace TelemetryWerk.Api.Host.Middlewares;

public class SessionAuthMiddleware(RequestDelegate next)
{
    private const string API_KEY_HEADER = "X-Session-Key";
    private const string API_KEY_QUERY = "access_token"; // Used by SignalR

    public async Task InvokeAsync(HttpContext context, ILogger<SessionAuthMiddleware> logger, ISessionRepository sessionRepository)
    {
        // Bypass login endpoint
        if (context.Request.Path.StartsWithSegments("/api/v1/auth/login", StringComparison.OrdinalIgnoreCase))
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

        // Validate Key
        bool isValid = false;
        if (!string.IsNullOrWhiteSpace(providedKey))
        {
            isValid = await sessionRepository.ValidateSessionAsync(providedKey);
        }

        if (!isValid)
        {
            logger.LogWarning("Session validation: Failed. Client IP: {IpAddress}", context.Connection.RemoteIpAddress);
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized: Invalid Session Key.");
            return;
        }

        logger.LogDebug("Session validation: Success");

        await next(context);
    }
}
