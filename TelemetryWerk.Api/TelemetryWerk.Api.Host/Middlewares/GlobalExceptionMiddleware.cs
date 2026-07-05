using System.Net;
using System.Text.Json;
using TelemetryWerk.Api.Application.Contracts;
using TelemetryWerk.Api.Application.Exceptions;

namespace TelemetryWerk.Api.Host.Middlewares;

public class GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IHostEnvironment env)
{
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await next(context);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An unhandled exception occurred.");
            await HandleExceptionAsync(context, ex, env);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception, IHostEnvironment env)
    {
        context.Response.ContentType = "application/json";
        
        var statusCode = (int)HttpStatusCode.InternalServerError;
        var message = "An internal server error occurred.";

        switch (exception)
        {
            case NotFoundException notFoundException:
                statusCode = (int)HttpStatusCode.NotFound;
                message = notFoundException.Message;
                break;
            case ValidationException validationException:
                statusCode = (int)HttpStatusCode.BadRequest;
                message = validationException.Message;
                break;
            case ConflictException conflictException:
                statusCode = (int)HttpStatusCode.Conflict;
                message = conflictException.Message;
                break;
            default:
                if (env.IsDevelopment())
                {
                    message = exception.Message; // You might want to leak a bit more in dev, but prompt says "Don't leak stack trace out to production".
                }
                break;
        }

        context.Response.StatusCode = statusCode;

        var response = new UnifiedResponse<object>
        {
            Status = new StatusResponseDto
            {
                Code = statusCode,
                Message = message
            },
            Data = null
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        return context.Response.WriteAsync(json);
    }
}
