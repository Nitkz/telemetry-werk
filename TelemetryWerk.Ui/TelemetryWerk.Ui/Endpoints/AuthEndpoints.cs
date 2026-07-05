using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using TelemetryWerk.Ui.Core.Configurations;

namespace TelemetryWerk.Ui.Endpoints;

public static class AuthEndpoints
{
    public static void MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/auth").DisableAntiforgery();

        group.MapPost("/login", async (HttpContext ctx, TelemetryWerk.Api.Client.ITelemetryApiClient telemetryApiClient) =>
        {
            var form = await ctx.Request.ReadFormAsync();
            var password = form["password"].ToString();
            
            try
            {
                // Use the typed client (ITelemetryApiClient) instead of a raw HttpClient.
                // The attached ServerSessionAuthenticationHandler automatically handles:
                // 1. Injecting the X-Forwarded-For header for backend rate limiting.
                // 2. Omitting the X-Session-Key header (since it's null during login).
                var result = await telemetryApiClient.LoginAsync(password);
                
                if (result != null && !string.IsNullOrEmpty(result.Token))
                {
                    var claims = new List<Claim> 
                    { 
                        new Claim(ClaimTypes.Name, "Admin"),
                        new Claim("SessionKey", result.Token)
                    };
                    var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var principal = new ClaimsPrincipal(identity);
                    
                    await ctx.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);
                    return Results.Redirect("/");
                }
            }
            catch (TelemetryWerk.Api.Client.ApiException)
            {
                // NSwag throws ApiException for non-2xx responses (like 401 Unauthorized)
                return Results.Redirect("/login?error=Invalid password");
            }
            catch (Exception)
            {
                // Fallback for network errors etc.
                return Results.Redirect("/login?error=Service unavailable");
            }
            
            return Results.Redirect("/login?error=Invalid password");
        });

        group.MapPost("/logout", async (HttpContext ctx) =>
        {
            await ctx.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Redirect("/login");
        });
    }
}
