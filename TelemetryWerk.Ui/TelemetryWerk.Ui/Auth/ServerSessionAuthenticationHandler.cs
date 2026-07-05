using Microsoft.AspNetCore.Http;

namespace TelemetryWerk.Ui.Auth;

public class ServerSessionAuthenticationHandler : DelegatingHandler
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public ServerSessionAuthenticationHandler(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var sessionKey = _httpContextAccessor.HttpContext?.User?.FindFirst("SessionKey")?.Value;

        if (!string.IsNullOrEmpty(sessionKey))
        {
            request.Headers.Add("X-Session-Key", sessionKey);
        }

        // Global IP Forwarding for BFF (Backend-For-Frontend) Architecture
        var clientIp = _httpContextAccessor.HttpContext?.Connection?.RemoteIpAddress?.ToString();
        if (!string.IsNullOrEmpty(clientIp))
        {
            request.Headers.Add("X-Forwarded-For", clientIp);
        }

        return base.SendAsync(request, cancellationToken);
    }
}
